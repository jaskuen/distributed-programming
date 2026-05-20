using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.Redirect(new Uri(context.RedirectUri).PathAndQuery);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
        builder.Services.AddAuthorization();

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
        builder.Services.AddSingleton<UserStore>();

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((_, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.Host(
                    new Uri(builder.Configuration["RabbitMq:RabbitServer"]!),
                    h =>
                    {
                        h.Username(builder.Configuration["RabbitMq:RabbitUsername"]!);
                        h.Password(builder.Configuration["RabbitMq:RabbitPassword"]!);
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
