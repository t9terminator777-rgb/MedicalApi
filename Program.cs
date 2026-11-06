using MedicalApi.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger доступен при локальном запуске проекта.
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();