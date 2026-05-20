using EventsLogger.Events;
using MassTransit;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<RankCalculatedEvent>();
            x.AddConsumer<SimilarityCalculatedEvent>();

            x.UsingRabbitMq((context, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.Host(
                    new Uri(builder.Configuration["RabbitMq:RabbitServer"]!),
                    h =>
                    {
                        h.Username(builder.Configuration["RabbitMq:RabbitUsername"]!);
                        h.Password(builder.Configuration["RabbitMq:RabbitPassword"]!);
                    });

                rabbitMqBusFactoryConfigurator.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();
        await app.RunAsync();
    }
}
