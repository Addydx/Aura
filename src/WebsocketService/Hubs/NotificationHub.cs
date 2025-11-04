using Microsoft.AspNetCore.SignalR;

namespace WebsocketService.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // Cuando un usuario se conecta
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Usuario conectado: {Context.ConnectionId}");
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        // Cuando un usuario se desconecta
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Usuario desconectado: {Context.ConnectionId}");
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Método para que el cliente se una a un grupo (ej: imagen específica)
        public async Task JoinImageGroup(string imageId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"image_{imageId}");
            _logger.LogInformation($"Usuario {Context.ConnectionId} se unió al grupo image_{imageId}");
        }

        // Método para que el cliente salga de un grupo
        public async Task LeaveImageGroup(string imageId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"image_{imageId}");
            _logger.LogInformation($"Usuario {Context.ConnectionId} salió del grupo image_{imageId}");
        }

        // Método para asociar userId con connectionId
        public async Task RegisterUser(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"Usuario {userId} registrado con conexión {Context.ConnectionId}");
        }

        // Método para enviar mensaje de prueba
        public async Task SendTestMessage(string message)
        {
            await Clients.All.SendAsync("TestMessage", $"Echo: {message}");
        }
    }
}