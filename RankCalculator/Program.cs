using MassTransit;
using RankCalculator.Services;
using Utils;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSingleton(_ =>
            RedisShardStore.Create(builder.Configuration.GetConnectionString("Redis")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<CalculatorService>();
            
            x.UsingRabbitMq((context, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    "save-text-event",
                    e =>
                    {
                        e.ConfigureConsumer<CalculatorService>(context);
                    });
            });
        });

        var app = builder.Build();
        await app.RunAsync();
    }
}
