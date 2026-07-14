# CICD_Testing_Project

![CI](https://github.com/khaiisian/CICD_Testing_Project/actions/workflows/ci.yml/badge.svg)

A learning project for practising **CI/CD** with .NET 10 and GitHub Actions.

## What's inside

A small ASP.NET Core Web API with a clean, layered architecture:

```
CICD_Testing_Project.Api/        ← Web API (controllers, business + data layers)
CICD_Testing_Project.Database/   ← EF Core entities / DbContext
CICD_Testing_Project.Testing/    ← xUnit unit tests (Moq)
```

The `Item` feature demonstrates full CRUD (`Create`, `GetAll`, `GetById`, `Update`, `Patch`, `Delete`)
split across layers behind interfaces:

```
Controller → IBL_Item (BL_Item) → IDA_Item (DA_Item) → AppDbContext → SQL Server
```

## Running locally

```bash
dotnet restore
dotnet build
dotnet run --project CICD_Testing_Project.Api
```

Swagger UI: `https://localhost:7125/swagger`

## Running the tests

```bash
dotnet test
```

Tests use **Moq** to fake the data-access layer, so they run fast with **no database required**.

## CI pipeline

Every push and pull request triggers `.github/workflows/ci.yml`, which runs on a clean
Ubuntu runner:

1. Checkout code
2. Install the .NET 10 SDK
3. Restore NuGet packages (cached between runs)
4. **Security scan** — fails the build if any package has a known vulnerability
5. Build in `Release`
6. Run all tests and collect code coverage

---

# 🐳 Docker — Study Notes

My personal notes from learning Docker on this project, written the way I actually
came to understand it (question → plain-English answer).

## Why Docker at all?

**Goal: get my API off my laptop and onto a server, without it breaking.**

My app needs .NET 10, the right config, files in the right place, etc. Copying code to
another machine means setting all that up again by hand — the classic *"but it works on
my machine!"* problem. Docker packs my app **and everything it needs to run** into one
sealed box that runs the same everywhere.

```
Without Docker:  copy code → install .NET → configure → often breaks
With Docker:     "run the box" → works the same everywhere
```

In the CI/CD pipeline, Docker is the **"package"** step: `build → test → package → deploy`.

## The 3 words (the mental model that made it click)

| Term | What it is | My metaphor |
|------|-----------|-------------|
| **Dockerfile** | Instructions to pack the app into a box | the **recipe** |
| **Image** | The built, frozen box (app + runtime) | the **frozen meal** |
| **Container** | A *running* instance of an image | the **hot meal** |

Flow: `Dockerfile → (docker build) → Image → (docker run) → Container`

## Multi-stage build (why the Dockerfile has two `FROM`s)

- **Build** needs the big **SDK** image (~800 MB).
- **Running** only needs the slim **runtime** image (~220 MB).

So the Dockerfile builds in the SDK stage, then copies *only the published output* into a
slim runtime stage. The build tools are left behind → small final image (mine was ~103 MB).

## One Dockerfile per *runnable* app

I only made a Dockerfile for the **API**, not for the Database class library — and that's
correct:

- **API** = a runnable app (it starts and listens) → **gets a Dockerfile**
- **Database class library** = just code the API uses (can't run on its own) → **no Dockerfile**
- The library is a *dependency*, so `dotnet publish` compiles it **into** the API automatically.

> Rule I learned: **one Dockerfile per thing that actually runs.** Libraries ride along inside.

## Ports (this took a few questions to fully get)

- A **port = a numbered door** that lets network traffic reach a specific app.
- My image exposes two: **8080 = HTTP** (use it) and **8081 = HTTPS** (needs a certificate, skip it).
- The command `-p 8080:8080` maps **`HOST:CONTAINER`**:

```
-p 8085:8080
   │     │
   │     └─ port INSIDE the container (must match what the app listens on)
   └─────── port on MY PC (I choose it; visit this one in the browser)
```

Key insight: **the two numbers don't have to match.** `-p 8085:8080` → I visit
`localhost:8085`, and it reaches the API on `8080` inside the container.

## The database is NOT in the container

When I called `/api/Item` in the container I got a **500 (SQL Server not found)** — and that's
**expected**:

- **SQL Server + the database live on my Windows machine**, not in the container.
- Inside a container, `localhost` means *the container itself*, not my PC — so it can't
  reach my host's SQL Server.
- The container only holds the **API + .NET runtime**. Nothing else.

To make the DB reachable you have to **host it somewhere the container can see** — its own
container (via Docker Compose), or a cloud database. (Next lesson.)

## Environment matters (why Swagger 404'd at first)

Containers run in **Production** mode by default, but Swagger is only enabled in
**Development** (`if (app.Environment.IsDevelopment())`). Same image, different behaviour
depending on the environment variable:

```bash
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development cicd-testing-api
```

> Lesson: **one image, configured per environment with variables** — you don't rebuild to
> switch between Development and Production.

## Three ways to build & run (all do the same thing)

| Task | Command line | Visual Studio ▶ | Docker Desktop GUI |
|------|-------------|-----------------|--------------------|
| Build | `docker build ...` | one-click (auto) | ❌ (still needs CLI) |
| Run | `docker run ...` | one-click (auto) | ▶ Run button + dialog |
| Logs / Stop / Delete | `docker logs/stop/rm` | — | buttons |

- **Visual Studio ▶** does build + run + port-mapping + Development mode + browser + debugger,
  all in one click. It picks a **random host port** (e.g. `32769`) and uses HTTPS.
- **Docker Desktop** is for *running and managing* (buttons); building still needs the CLI.
- **CLI** is the universal one — **pipelines have no buttons**, so CI/CD uses commands.

> "Add → Docker Support" in Visual Studio generated **both** the `Dockerfile` **and** the
> `Container (Dockerfile)` profile in `launchSettings.json`.

## The commands I actually use

```bash
# BUILD (Dockerfile is in the API folder, so -f points to it; "." = build context)
docker build -t cicd-testing-api -f CICD_Testing_Project.Api/Dockerfile .

# RUN (map port 8080, enable Swagger via Development)
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development cicd-testing-api

# REDO after a code change: just build again (it replaces the old image), then run.
```

Everyday loop: **create Dockerfile once → build when code changes → run to start it.**

---

# 🐳🐳 Docker Compose — Study Notes

Where Docker runs **one** container, Compose runs **many together** with one command.
I used it to run my API **and** a SQL Server side by side so `/api/Item` returns real data.

## Why Compose (and not just `docker run`)?

`docker build`/`docker run` handle **one** container by hand. I needed **two** (API + database)
that start together, share a network, and talk to each other. Doing that manually = a pile of
commands (create network, run db, build api, run api on the same network...). Compose does all
of it from **one file + one command**.

> `docker build/run` = one container by hand. **Compose = many containers together, automatically** — using the same Dockerfile I already wrote.

## The `compose.yaml`, line-by-line (why each part exists)

```yaml
services:                                    # the list of containers to run
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest   # run SQL Server without installing it
    environment:
      ACCEPT_EULA: "Y"                       # SQL Server won't start without accepting the licence
      MSSQL_SA_PASSWORD: "Str0ng_Passw0rd!"  # 'sa' password — MUST be strong or the container won't start
    ports:
      - "1433:1433"                          # so I can connect with SSMS to create the table

  api:
    build:
      context: .                                        # build using the whole solution folder
      dockerfile: CICD_Testing_Project.Api/Dockerfile   # my API must be built from my Dockerfile
    ports:
      - "8080:8080"                          # so I can open the API at localhost:8080
    environment:
      ASPNETCORE_ENVIRONMENT: "Development"  # containers default to Production; this turns Swagger on
      # THE line that fixed the 500 — point the API at the 'db' container instead of my PC:
      ConnectionStrings__DefaultConnection: "Server=db;Database=Workspace;User ID=sa;Password=Str0ng_Passw0rd!;TrustServerCertificate=True;"
    depends_on:
      - db                                   # start the database before the API
```

**The magic line:** `ConnectionStrings__DefaultConnection` with `Server=db`. Compose puts both
containers on a shared network where each is reachable **by its service name**, so the API finds
the database at `db` — no IP, no PC name. (.NET reads the `__` env var and it overrides the
`DefaultConnection` in `appsettings.json`, so I didn't edit the file.)

## The two database errors I hit (and what each taught me)

| Error | What it really meant | Lesson |
|-------|----------------------|--------|
| *"SQL Server not found"* (500) | Couldn't **connect** to a database | Container networking — fixed by `Server=db` |
| *"Invalid object name 'Tbl_Item'"* | Connected, but the **table didn't exist** | A fresh DB container starts **empty** — I must create the schema |

Reading *which* error I got told me *which* problem to fix: first couldn't reach SQL Server,
then reached it but the table was missing. (The real table name is `Tbl_Item` — the entity maps
to it via `entity.ToTable("Tbl_Item")`.)

## The commands

```bash
docker compose up --build   # build the API image + start BOTH containers together
docker compose down         # stop and remove both containers
```

If I delete the images/containers, `docker compose up --build` recreates everything automatically
(rebuilds my API, re-downloads SQL Server). **But** without a **volume**, the database data is
wiped when the SQL container is deleted — so I'd re-run the CREATE TABLE script. A volume
(`volumes: - sqldata:/var/opt/mssql`) makes the data survive.

## Bonus lesson — Swagger UI and OpenAPI 3.0 vs 3.1

The `GET /api/Item/{id}` box in Swagger kept saying *"Required field is not provided"* even with a
value typed. **Not my code** — a tooling mismatch:

- .NET 10's built-in `AddOpenApi()` generates **OpenAPI 3.1**, where an `int` path param becomes
  `type: ["integer","string"]` (a multi-type array).
- **Swagger UI doesn't fully support OpenAPI 3.1 yet**, so it mis-validates that and falsely rejects input.

Calling the endpoint directly (`curl http://localhost:8080/api/Item/1`) returned `200 OK` — proving
the API was fine. Fix: use **Swashbuckle's `AddSwaggerGen()` + `UseSwagger()` + `UseSwaggerUI()`**,
which serves an **OpenAPI 3.0** doc (single-type `integer`) that Swagger UI validates correctly.

> Lesson: same packages, different **wiring** → different behaviour. Point Swagger UI at the 3.0
> doc and `{id}` works.
