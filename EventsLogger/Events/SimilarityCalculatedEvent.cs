using MassTransit;
using Valuator.Messages;

namespace EventsLogger.Events;

public class SimilarityCalculatedEvent : IConsumer<ISimilarityCalculated>
{
    public Task Consume(ConsumeContext<ISimilarityCalculated> context)
    {
        string result = $"""
                         Id: {context.Message.Id}
                         Similarity: {context.Message.Similarity} 
                         """;
        
        Console.WriteLine(result);
        
        return Task.CompletedTask;
    }
}