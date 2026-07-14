using CICD_Testing_Project.Api.Domain.Features.Item;
using CICD_Testing_Project.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Swashbuckle's generator produces an OpenAPI 3.0 doc that Swagger UI validates correctly
// (so {id} path parameters work in the UI).
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<IDA_Item, DA_Item>();
builder.Services.AddScoped<IBL_Item, BL_Item>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();        // Swashbuckle serves /swagger/v1/swagger.json (OpenAPI 3.0)
    app.UseSwaggerUI();      // Swagger UI reads that 3.0 doc by default
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
