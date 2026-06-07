using Notification.Service.Services;
using Notification.Service.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationService, EmailService>();
builder.Services.AddHostedService<RegistrationNotificationConsumer>();
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
