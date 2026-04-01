using MassTransit;
using RankCalculator.Messages;

namespace EventsLogger.Events;

public class RankCalculatedEvent : IConsumer<IRankCalculated>
{
    public Task Consume(ConsumeContext<IRankCalculated> context)
    {
        string result = $"""
                        Id: {context.Message.Id}
                        Rank: {context.Message.Rank} 
                        """;
        
        Console.WriteLine(result);
        
        return Task.CompletedTask;
    }
}