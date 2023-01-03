using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NetRabbitmq.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NetRabbitmq.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoCollection<MessageModel> _messageCollection;
        private readonly RabbitMQSettings _rabbitmqSettings;

        public Worker(ILogger<Worker> logger, IOptions<Shared.MongoSettings> dabaseSettings, IOptions<RabbitMQSettings> rabbitmqSettings)
        {
            _logger = logger;
            _rabbitmqSettings = rabbitmqSettings.Value;

            var mongoClient = new MongoClient(dabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dabaseSettings.Value.DatabaseName);
            _messageCollection = mongoDatabase.GetCollection<MessageModel>(_rabbitmqSettings.Queue);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = _rabbitmqSettings.Host };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _rabbitmqSettings.Queue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation(" [*] Waiting for messages.");

                
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var obj = JsonSerializer.Deserialize<MessageModel>(message);
                        if (obj is not null)
                            await _messageCollection.InsertOneAsync(obj);

                        _logger.LogInformation(" [x] Received {0}", JsonSerializer.Serialize(obj));
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: _rabbitmqSettings.Queue,
                                         autoAck: false,
                                         consumer: consumer);

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}