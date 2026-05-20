using System.Threading.Channels;
using ProtoKey.Persistence;
using ProtoKey.Store;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:7777");

builder.Services.AddControllers();
builder.Services.AddSingleton(Channel.CreateUnbounded<StoreCommand>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
}));
builder.Services.AddSingleton(Channel.CreateUnbounded<PersistedCommand>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
}));
builder.Services.AddSingleton<StoreClient>();
builder.Services.AddHostedService<StoreWorker>();
builder.Services.AddHostedService<PersistenceWorker>();

WebApplication app = builder.Build();

app.MapControllers();

await app.RunAsync();
