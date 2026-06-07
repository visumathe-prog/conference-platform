using Microsoft.EntityFrameworkCore;
using Event.Service.Infrastructure.Data;
using Event.Service.Infrastructure.Repositories;
using Event.Service.Application.Commands;
using Event.Service.Application.Queries;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("PostgreSQL");

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEventCommandHandler).Assembly));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
