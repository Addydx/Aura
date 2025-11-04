using System.Collections.Concurrent;

namespace WebsocketService.Services
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _userConnections = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _imageGroups = new();
        private readonly ILogger<ConnectionManager> _logger;

        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
        }

        // Asociar userId con connectionId
        public void AddUserConnection(string userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
            _logger.LogInformation($"Usuario {userId} conectado con ID: {connectionId}");
        }

        // Remover conexión de usuario
        public void RemoveUserConnection(string userId)
        {
            if (_userConnections.TryRemove(userId, out var connectionId))
            {
                _logger.LogInformation($"Usuario {userId} desconectado. ConnectionId: {connectionId}");
            }
        }

        // Obtener connectionId por userId
        public string? GetConnectionId(string userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        // Agregar usuario a grupo de imagen
        public void AddToImageGroup(string imageId, string userId)
        {
            var groupKey = $"image_{imageId}";
            _imageGroups.AddOrUpdate(groupKey, 
                new HashSet<string> { userId }, 
                (key, existing) => 
                {
                    existing.Add(userId);
                    return existing;
                });
            
            _logger.LogInformation($"Usuario {userId} agregado al grupo {groupKey}");
        }

        // Remover usuario de grupo de imagen
        public void RemoveFromImageGroup(string imageId, string userId)
        {
            var groupKey = $"image_{imageId}";
            if (_imageGroups.TryGetValue(groupKey, out var users))
            {
                users.Remove(userId);
                if (users.Count == 0)
                {
                    _imageGroups.TryRemove(groupKey, out _);
                }
                _logger.LogInformation($"Usuario {userId} removido del grupo {groupKey}");
            }
        }

        // Obtener usuarios en grupo de imagen
        public IEnumerable<string> GetUsersInImageGroup(string imageId)
        {
            var groupKey = $"image_{imageId}";
            return _imageGroups.TryGetValue(groupKey, out var users) ? users : Enumerable.Empty<string>();
        }

        // Obtener estadísticas
        public object GetStats()
        {
            return new
            {
                ConnectedUsers = _userConnections.Count,
                ActiveImageGroups = _imageGroups.Count,
                TotalUsersInGroups = _imageGroups.Values.Sum(g => g.Count)
            };
        }
    }
}