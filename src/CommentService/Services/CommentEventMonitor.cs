using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommentService.Services
{
    public class CommentEventMonitor : BackgroundService
    {
        private readonly ILogger<CommentEventMonitor> _logger;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IChannel? _channel;

        public CommentEventMonitor(ILogger<CommentEventMonitor> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeRabbitMQ();
                
                if (_channel != null)
                {
                    // Monitor comment-created queue
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
                            
                            _logger.LogInformation("🔔 COMMENT EVENT RECEIVED:");
                            _logger.LogInformation($"📨 Queue: comment-created");
                            _logger.LogInformation($"📄 Message: {message}");
                            _logger.LogInformation("-----------------------------------");
                            
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error monitoring comment event");
                        }
                    };

                    await _channel.BasicConsumeAsync(queue: "comment-created",
                                                   autoAck: false,
                                                   consumer: consumer);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in comment event monitor");
            }
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