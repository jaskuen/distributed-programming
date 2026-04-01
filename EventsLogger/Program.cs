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
            x.AddConsumer<RankCalculatedEvent>();
            x.AddConsumer<SimilarityCalculatedEvent>();

            x.UsingRabbitMq((context, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    "save-text-event",
                    e =>
                    {
                        e.ConfigureConsumer<RankCalculatedEvent>(context);
                        e.ConfigureConsumer<SimilarityCalculatedEvent>(context);
                    });
            });
        });

        var app = builder.Build();
        await app.RunAsync();
    }
}