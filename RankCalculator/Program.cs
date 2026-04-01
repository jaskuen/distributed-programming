using MassTransit;
using RankCalculator.Services;
using StackExchange.Redis;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

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