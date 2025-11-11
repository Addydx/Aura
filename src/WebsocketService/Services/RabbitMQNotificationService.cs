using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WebsocketService.Hubs;
using Shared.Contracts.Events;

namespace WebsocketService.Services
{
    public class RabbitMQNotificationService : BackgroundService
    {
        private readonly ILogger<RabbitMQNotificationService> _logger;
        private readonly IConfiguration _config;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IHubContext<CommentHub> _commentHub;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQNotificationService(
            ILogger<RabbitMQNotificationService> logger, 
            IConfiguration config,
            IHubContext<NotificationHub> notificationHub,
            IHubContext<CommentHub> commentHub)
        {
            _logger = logger;
            _config = config;
            _notificationHub = notificationHub;
            _commentHub = commentHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeRabbitMQ();
                
                if (_channel != null)
                {
                    // Escuchar eventos de imágenes subidas
                    await SetupImageUploadConsumer();
                    
                    // Escuchar eventos de comentarios creados
                    await SetupCommentCreatedConsumer();
                }

                // Mantener el servicio corriendo
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMQ notification service");
            }
        }

        private async Task SetupImageUploadConsumer()
        {
            if (_channel == null) return;

            await _channel.QueueDeclareAsync(queue: "image-uploads",
                                           durable: true,
                                           exclusive: false,
                                           autoDelete: false,
                                           arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"📸 Imagen subida recibida: {message}");

                    var imageEvent = JsonSerializer.Deserialize<ImageUploaderEvent>(message);
                    if (imageEvent != null)
                    {
                        // Notificar a todos los usuarios conectados a través del NotificationHub
                        await _notificationHub.Clients.All.SendAsync("ImageUploaded", new
                        {
                            imageEvent.ImageId,
                            imageEvent.Url,
                            imageEvent.OwnerId,
                            Message = "¡Nueva imagen subida!",
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image upload event");
                }
            };

            await _channel.BasicConsumeAsync(queue: "image-uploads",
                                           autoAck: false,
                                           consumer: consumer);
        }

        private async Task SetupCommentCreatedConsumer()
        {
            if (_channel == null) return;

            await _channel.QueueDeclareAsync(queue: "comment-created",
                                           durable: true,
                                           exclusive: false,
                                           autoDelete: false,
                                           arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"💬 Comentario creado recibido: {message}");

                    var commentEvent = JsonSerializer.Deserialize<CommentCreatedEvent>(message);
                    if (commentEvent != null)
                    {
                        // Notificar a través del NotificationHub (grupo general de la imagen)
                        await _notificationHub.Clients.Group($"image_{commentEvent.ImageId}")
                            .SendAsync("CommentAdded", new
                            {
                                commentEvent.CommentId,
                                commentEvent.ImageId,
                                commentEvent.UserId,
                                commentEvent.Content,
                                commentEvent.CreatedAt,
                                Message = "¡Nuevo comentario agregado!"
                            });

                        // Notificar a través del CommentHub (usuarios suscritos a comentarios de esa imagen)
                        await _commentHub.Clients.Group($"image_{commentEvent.ImageId}")
                            .SendAsync("NewComment", new
                            {
                                CommentId = commentEvent.CommentId,
                                ImageId = commentEvent.ImageId,
                                UserId = commentEvent.UserId,
                                Content = commentEvent.Content,
                                CreatedAt = commentEvent.CreatedAt,
                                Timestamp = DateTime.UtcNow
                            });
                    }

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing comment created event");
                }
            };

            await _channel.BasicConsumeAsync(queue: "comment-created",
                                           autoAck: false,
                                           consumer: consumer);
        }

        private async Task InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMQ:HostName"] ?? "localhost",
                UserName = _config["RabbitMQ:UserName"] ?? "guest",
                Password = _config["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _logger.LogInformation("RabbitMQ connection established for notifications");
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
            _connection?.CloseAsync();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}