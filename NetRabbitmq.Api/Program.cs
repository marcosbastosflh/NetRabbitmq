using MongoDB.Bson;
using MongoDB.Driver;
using NetRabbitmq.Shared;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapPost("/send", (string bodydto) =>
{
    var rabbitMQSettings = builder.Configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

    var factory = new ConnectionFactory() { HostName = rabbitMQSettings.Host };
    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: rabbitMQSettings.Queue,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        var message = new MessageModel(bodydto);
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: "",
                             routingKey: rabbitMQSettings.Queue,
                             basicProperties: properties,
                             body: body);
    }
    return Results.Ok();
})
.WithName("SendMessage");

app.MapGet("/", async () =>
{
    var rabbitmqSettings = builder.Configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();
    var dabaseSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
    var mongoClient = new MongoClient(dabaseSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(dabaseSettings.DatabaseName);
    var messageCollection = mongoDatabase.GetCollection<MessageModel>(rabbitmqSettings.Queue);

    var documents = await messageCollection.Find<MessageModel>(new BsonDocument()).ToListAsync();

    return Results.Ok(documents);
})
.WithName("GetMessages");

app.Run();