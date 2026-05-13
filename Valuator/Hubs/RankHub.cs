using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Utils;

namespace Valuator.Hubs;

public class RankHub : Hub
{
    public const string RankCalculatedMethod = "RankCalculated";

    private readonly IDatabase _redis;

    public RankHub(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public static string BuildGroupName(string id)
    {
        return $"rank-{id}";
    }

    public override async Task OnConnectedAsync()
    {
        string? id = Context.GetHttpContext()?.Request.Query["textId"].ToString();

        if (!string.IsNullOrWhiteSpace(id))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(id), Context.ConnectionAborted);
            await SendRankIfReady(id);
        }

        await base.OnConnectedAsync();
    }

    private async Task SendRankIfReady(string id)
    {
        RedisValue stringRank = _redis.StringGet(KeyBuilder.BuildRankKey(id));

        if (!stringRank.HasValue)
        {
            return;
        }

        if (double.TryParse(stringRank.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double rank))
        {
            await Clients.Caller.SendAsync(RankCalculatedMethod, rank, Context.ConnectionAborted);
        }
    }
}
