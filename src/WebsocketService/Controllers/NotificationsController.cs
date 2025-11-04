using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebsocketService.Hubs;
using WebsocketService.Services;

namespace WebsocketService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IHubContext<NotificationHub> hubContext,
            ConnectionManager connectionManager,
            ILogger<NotificationsController> logger)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        [HttpGet("stats")]
        public IActionResult GetConnectionStats()
        {
            var stats = _connectionManager.GetStats();
            return Ok(stats);
        }

        [HttpPost("test-broadcast")]
        public async Task<IActionResult> TestBroadcast([FromBody] TestMessageRequest request)
        {
            await _hubContext.Clients.All.SendAsync("TestMessage", new
            {
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                Type = "broadcast"
            });

            _logger.LogInformation($"Mensaje de prueba enviado: {request.Message}");
            return Ok(new { Success = true, Message = "Mensaje enviado a todos los usuarios conectados" });
        }

        [HttpPost("test-user")]
        public async Task<IActionResult> TestUserMessage([FromBody] TestUserMessageRequest request)
        {
            await _hubContext.Clients.Group($"user_{request.UserId}").SendAsync("TestMessage", new
            {
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                Type = "user_specific",
                TargetUser = request.UserId
            });

            _logger.LogInformation($"Mensaje de prueba enviado al usuario {request.UserId}: {request.Message}");
            return Ok(new { Success = true, Message = $"Mensaje enviado al usuario {request.UserId}" });
        }

        [HttpPost("test-image-group")]
        public async Task<IActionResult> TestImageGroupMessage([FromBody] TestImageGroupMessageRequest request)
        {
            await _hubContext.Clients.Group($"image_{request.ImageId}").SendAsync("TestMessage", new
            {
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                Type = "image_group",
                ImageId = request.ImageId
            });

            _logger.LogInformation($"Mensaje de prueba enviado al grupo imagen {request.ImageId}: {request.Message}");
            return Ok(new { Success = true, Message = $"Mensaje enviado al grupo de imagen {request.ImageId}" });
        }
    }

    public record TestMessageRequest(string Message);
    public record TestUserMessageRequest(string UserId, string Message);
    public record TestImageGroupMessageRequest(string ImageId, string Message);
}