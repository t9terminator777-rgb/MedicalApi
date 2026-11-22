using MedicalApi.Data;
using MedicalApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IGosReestr, FakeGosReestr>();
builder.Services.AddDbContext<MedicalDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("MedicalDb")
    ?? "Host=localhost;Database=medical_api;Username=postgres"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<MedicalDbContext>().Database.MigrateAsync();
    return;
}

app.MapControllers();
app.Run();
