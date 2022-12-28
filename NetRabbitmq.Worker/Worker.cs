using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NetRabbitmq.Shared;
using NetRabbitmq.Worker.Models;
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

        public Worker(ILogger<Worker> logger, IOptions<MessageDatabaseSettings> dabaseSettings)
        {
            _logger = logger;
            // TODO: fix localhost on docker
            var mongoClient = new MongoClient(dabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dabaseSettings.Value.DatabaseName);
            _messageCollection = mongoDatabase.GetCollection<MessageModel>("task_queue");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string? isDOcker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
            var host = String.IsNullOrEmpty(isDOcker) ? "localhost" : "rabbit_srv";

            var factory = new ConnectionFactory() { HostName = host };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
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
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var obj = JsonSerializer.Deserialize<MessageModel>(message);

                        _messageCollection.InsertOneAsync(obj);

                        _logger.LogInformation(" [x] Received {0}", JsonSerializer.Serialize(obj));
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: "task_queue",
                                         autoAck: false,
                                         consumer: consumer);

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}