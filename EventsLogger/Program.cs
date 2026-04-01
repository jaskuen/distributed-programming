using EventsLogger.Events;
using MassTransit;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddScoped<RankCalculatedEvent>();
        builder.Services.AddScoped<SimilarityCalculatedEvent>();

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    "save-text-event",
                    e =>
                    {
                        e.Consumer(typeof(RankCalculatedEvent), context.GetRequiredService);
                        e.Consumer(typeof(SimilarityCalculatedEvent), context.GetRequiredService);
                    });
            });
        });

        var app = builder.Build();
        await app.RunAsync();
    }
}