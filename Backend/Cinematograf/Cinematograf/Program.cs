using Microsoft.EntityFrameworkCore;
using Cinematograf.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configurare DbContext pentru SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adaugă controller-e
builder.Services.AddControllers();

// Adaugă Swagger/OpenAPI (pentru testarea API-ului)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Permite CORS pentru React frontend (localhost)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurare pipeline HTTP

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

