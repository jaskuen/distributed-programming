using MassTransit;
using StackExchange.Redis;
using Valuator.Consumers;
using Valuator.Hubs;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<RankCalculatedConsumer>();

            x.UsingRabbitMq((context, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    $"rank-calculated-notifications-{Guid.NewGuid():N}",
                    endpoint =>
                    {
                        endpoint.AutoDelete = true;
                        endpoint.Durable = false;
                        endpoint.ConfigureConsumer<RankCalculatedConsumer>(context);
                    });
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapHub<RankHub>("/rankHub");

        app.Run();
    }
}
