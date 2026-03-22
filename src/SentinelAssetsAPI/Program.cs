using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SentinelAssetsAPI.Data;
using SentinelAssetsAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURAÇÃO DO CONTAINER DE DEPENDÊNCIAS (DI - Dependency Injection)
// Aqui registramos serviços, controllers, banco, auth etc. para injetar automaticamente
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// 🐘 BANCO POSTGRESQL - EF Core + Npgsql (Prod Ready, Docker/Local)
// Connection string em appsettings.json → localhost:5433/sentinel_assets
// Logs SQL detalhados para debug (dev only)
// ========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string PostgreSQL não configurada em appsettings.json"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()); // VER SQL executado no terminal!

// ========================================
// 🔐 AUTENTICAÇÃO JWT - Protege TODOS endpoints AssetsController
// Valida issuer, audience, expiração, assinatura (appsettings.json)
// ========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "super-secret-key-min-32-chars-long!!!"))
        };
    });

builder.Services.AddAuthorization();

// ========================================
// ☁️ SERVIÇOS AWS SNS - Scoped (nova instância por HTTP request)
// Injetado automaticamente no AssetsController
// ========================================
builder.Services.AddScoped<NotificationService>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

var app = builder.Build();

// ========================================
// 🗄️ MIGRATIONS AUTOMÁTICAS - Aplica pendentes na inicialização (Docker/local)
// ========================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ========================================
// 🔄 PIPELINE HTTP MIDDLEWARE - Ordem CRÍTICA!
// Auth ANTES Authorization, Swagger só DEV
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  // Lê/Valida JWT
app.UseAuthorization();   // Checa [Authorize]
app.MapControllers();

app.Run();
