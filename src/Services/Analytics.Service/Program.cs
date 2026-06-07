using Analytics.Service.Services;
using Analytics.Service.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAnalyticsService, ClickHouseService>();
builder.Services.AddHostedService<EventConsumer>();
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

app.Run();
