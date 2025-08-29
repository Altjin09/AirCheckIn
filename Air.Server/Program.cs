using Air.Server.Data;
using Air.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// DB (SQLite)
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS – зөвхөн Display руу зөвшөөрнө
const string CorsPolicy = "AllowDisplay";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:5031")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// ---------- Pipeline ORDER ЧУХАЛ ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// 1) Routing эхлүүлнэ
app.UseRouting();

// 2) CORS яг энд!
app.UseCors(CorsPolicy);

// (хэрвээ ашиглавал) auth/authorization
// app.UseAuthentication();
app.UseAuthorization();

// 3) Endpoint-уудаа map хийхдээ CORS-оо шаард
app.MapControllers().RequireCors(CorsPolicy);
app.MapHub<FlightsHub>("/hubs/flights").RequireCors(CorsPolicy);

// Миграци (seed хийхгүй)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
}

app.Run();
