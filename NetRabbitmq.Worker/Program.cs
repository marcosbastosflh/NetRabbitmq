using NetRabbitmq.Worker;
using NetRabbitmq.Worker.Models;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.Configure<MessageDatabaseSettings>(configuration.GetSection(nameof(MessageDatabaseSettings)));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
