using Microsoft.AspNetCore.SignalR;

namespace WebsocketService.Hubs
{
    /// <summary>
    /// Hub específico para gestión de comentarios en tiempo real
    /// </summary>
    public class CommentHub : Hub
    {
        private readonly ILogger<CommentHub> _logger;

        public CommentHub(ILogger<CommentHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se conecta al hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation($"Cliente conectado a CommentHub: {connectionId}");
            
            await Clients.Caller.SendAsync("Connected", new
            {
                ConnectionId = connectionId,
                Message = "Conectado exitosamente al CommentHub",
                Timestamp = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se desconecta del hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation($"Cliente desconectado de CommentHub: {connectionId}");
            
            if (exception != null)
            {
                _logger.LogError(exception, $"Error durante la desconexión: {connectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Suscribir al cliente a comentarios de una imagen específica
        /// </summary>
        public async Task SubscribeToImage(string imageId)
        {
            var groupName = $"comments_image_{imageId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation($"Cliente {Context.ConnectionId} suscrito a comentarios de imagen {imageId}");
            
            await Clients.Caller.SendAsync("SubscriptionConfirmed", new
            {
                ImageId = imageId,
                GroupName = groupName,
                Message = $"Suscrito a comentarios de la imagen {imageId}"
            });
        }

        /// <summary>
        /// Desuscribir al cliente de comentarios de una imagen
        /// </summary>
        public async Task UnsubscribeFromImage(string imageId)
        {
            var groupName = $"comments_image_{imageId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation($"Cliente {Context.ConnectionId} desuscrito de comentarios de imagen {imageId}");
            
            await Clients.Caller.SendAsync("UnsubscriptionConfirmed", new
            {
                ImageId = imageId,
                Message = $"Desuscrito de comentarios de la imagen {imageId}"
            });
        }

        /// <summary>
        /// Notificar que el usuario está escribiendo un comentario
        /// </summary>
        public async Task UserTyping(string imageId, string userId, string username)
        {
            var groupName = $"comments_image_{imageId}";
            
            // Notificar a todos en el grupo excepto al que está escribiendo
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new
            {
                ImageId = imageId,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Notificar que el usuario dejó de escribir
        /// </summary>
        public async Task UserStoppedTyping(string imageId, string userId)
        {
            var groupName = $"comments_image_{imageId}";
            
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", new
            {
                ImageId = imageId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Obtener el conteo de usuarios viendo una imagen
        /// </summary>
        public async Task GetViewersCount(string imageId)
        {
            // Esta funcionalidad requeriría un tracking más avanzado
            // Por ahora, retornamos un placeholder
            await Clients.Caller.SendAsync("ViewersCount", new
            {
                ImageId = imageId,
                Count = 0, // Implementar conteo real con ConnectionManager
                Message = "Funcionalidad de conteo en desarrollo"
            });
        }

        /// <summary>
        /// Notificar reacción a un comentario (like, etc)
        /// </summary>
        public async Task ReactToComment(string imageId, string commentId, string userId, string reactionType)
        {
            var groupName = $"comments_image_{imageId}";
            
            await Clients.Group(groupName).SendAsync("CommentReaction", new
            {
                CommentId = commentId,
                UserId = userId,
                ReactionType = reactionType,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation($"Reacción {reactionType} al comentario {commentId} por usuario {userId}");
        }

        /// <summary>
        /// Notificar eliminación de comentario
        /// </summary>
        public async Task NotifyCommentDeleted(string imageId, string commentId)
        {
            var groupName = $"comments_image_{imageId}";
            
            await Clients.Group(groupName).SendAsync("CommentDeleted", new
            {
                CommentId = commentId,
                ImageId = imageId,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation($"Notificada eliminación del comentario {commentId}");
        }

        /// <summary>
        /// Notificar edición de comentario
        /// </summary>
        public async Task NotifyCommentEdited(string imageId, string commentId, string newContent)
        {
            var groupName = $"comments_image_{imageId}";
            
            await Clients.Group(groupName).SendAsync("CommentEdited", new
            {
                CommentId = commentId,
                ImageId = imageId,
                NewContent = newContent,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation($"Notificada edición del comentario {commentId}");
        }
    }
}
