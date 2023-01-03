using NetRabbitmq.Shared;
using NetRabbitmq.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
        services.Configure<RabbitMQSettings>(configuration.GetSection(nameof(RabbitMQSettings)));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
