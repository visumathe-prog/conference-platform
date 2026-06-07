using Microsoft.EntityFrameworkCore;
using Identity.Service.Infrastructure.Data;
using Identity.Service.Infrastructure.Repositories;
using Identity.Service.Infrastructure.Services;
using Identity.Service.Application.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Confluent.Kafka;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. PostgreSQL (реальная БД)
// ============================================
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("PostgreSQL");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
        npgsqlOptions.EnableRetryOnFailure(5);
    }));

// ============================================
// 2. Redis (реальный кеш)
// ============================================
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration["Redis:ConnectionString"];

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

// ============================================
// 3. Kafka (реальный event bus)
// ============================================
var kafkaServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
    ?? builder.Configuration["Kafka:BootstrapServers"];

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig { BootstrapServers = kafkaServers };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddScoped<IEventBus, KafkaEventBus>();

// ============================================
// 4. JWT Authentication
// ============================================
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:SecretKey"];

var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ============================================
// 5. Dependency Injection
// ============================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// ============================================
// 6. Controllers + Swagger
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================
// 7. CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ============================================
// 8. Apply migrations automatically (production)
// ============================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ============================================
// 9. Middleware
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
