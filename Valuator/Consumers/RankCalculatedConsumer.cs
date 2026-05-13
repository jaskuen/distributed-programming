using MassTransit;
using Microsoft.AspNetCore.SignalR;
using RankCalculator.Messages;
using Valuator.Hubs;

namespace Valuator.Consumers;

public class RankCalculatedConsumer : IConsumer<IRankCalculated>
{
    private readonly IHubContext<RankHub> _hubContext;

    public RankCalculatedConsumer(IHubContext<RankHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task Consume(ConsumeContext<IRankCalculated> context)
    {
        return _hubContext.Clients
            .Group(RankHub.BuildGroupName(context.Message.Id))
            .SendAsync(RankHub.RankCalculatedMethod, context.Message.Rank, context.CancellationToken);
    }
}
