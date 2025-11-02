using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ImageService.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQService(IConfiguration config)
        {
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQ:HostName"] ?? "localhost",
                UserName = config["RabbitMQ:UserName"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest"
            };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
        }

        public async Task PublishAsync(object message, string queueName = "image-uploads")
        {
            await _channel.QueueDeclareAsync(queue: queueName,
                                           durable: true,
                                           exclusive: false,
                                           autoDelete: false,
                                           arguments: null);
            
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(exchange: "",
                                           routingKey: queueName,
                                           body: body);
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