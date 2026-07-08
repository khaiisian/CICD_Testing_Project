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

The `Item` feature demonstrates full CRUD (`GetAll`, `GetById`, `Update`, `Patch`, `Delete`)
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
