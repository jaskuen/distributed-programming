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

        builder.Services.AddScoped<CalculatorService>();
        
        IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.ReceiveEndpoint(
                "save-text-event",
                e =>
                {
                    e.Consumer(typeof(CalculatorService), serviceProvider.GetRequiredService);
                });
        });

        await busControl.StartAsync(CancellationToken.None);

        var app = builder.Build();
        await app.RunAsync();
    }
}