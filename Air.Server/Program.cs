using Air.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext → SQLite
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

// Миграцийг автоматаар ажиллуулна (seed ХИЙХГҮЙ)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS порт тодорхойгүй анхааруулга байвал алгасъя
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
