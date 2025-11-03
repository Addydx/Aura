using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CommentService.Services
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration config, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQ:HostName"] ?? "localhost",
                UserName = config["RabbitMQ:UserName"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest"
            };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
        }

        public async Task PublishCommentCreatedAsync(object commentEvent, string queueName = "comment-created")
        {
            try
            {
                await _channel.QueueDeclareAsync(queue: queueName,
                                               durable: true,
                                               exclusive: false,
                                               autoDelete: false,
                                               arguments: null);
                
                var json = JsonSerializer.Serialize(commentEvent);
                var body = Encoding.UTF8.GetBytes(json);

                await _channel.BasicPublishAsync(exchange: "",
                                               routingKey: queueName,
                                               body: body);

                _logger.LogInformation($"Published comment event to queue {queueName}: {json}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing comment event to RabbitMQ");
            }
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
            _connection?.CloseAsync();
            _connection?.Dispose();
        }
    }
}