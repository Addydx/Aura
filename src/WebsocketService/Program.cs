using WebsocketService.Hubs;
using WebsocketService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configurar SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Solo para desarrollo
});

// Registrar servicios personalizados
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddHostedService<RabbitMQNotificationService>();

// Configurar CORS para SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Puertos comunes de React/Vue
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Importante para SignalR
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

// Habilitar CORS
app.UseCors();

// Mapear controladores
app.MapControllers();

// Mapear el Hub de SignalR
app.MapHub<NotificationHub>("/notificationHub");

// Endpoint de prueba para verificar que el servicio está funcionando
app.MapGet("/", () => "WebSocket Service is running! 🚀\nSignalR Hub available at: /notificationHub");

app.Run();
