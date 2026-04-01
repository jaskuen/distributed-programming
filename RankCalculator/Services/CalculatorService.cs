using System.Globalization;
using MassTransit;
using RankCalculator.Messages;
using StackExchange.Redis;
using Utils;
using Valuator.Messages;

namespace RankCalculator.Services;

public class CalculatorService : IConsumer<ITextCreated>
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _redis;
    private readonly IPublishEndpoint _publishEndpoint;

    public CalculatorService(IConnectionMultiplexer connectionMultiplexer, IPublishEndpoint publishEndpoint)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _publishEndpoint = publishEndpoint;
        _redis = _connectionMultiplexer.GetDatabase();
    }

    public async Task Consume(ConsumeContext<ITextCreated> context)
    {
        string id = context.Message.Id;

        await Calculate(id);

        await Task.CompletedTask;
    }

    private async Task Calculate(string id)
    {
        string text = _redis.StringGet(KeyBuilder.BuildTextKey(id))!;

        Console.WriteLine($"Got text: {text}; by id: {id}");

        if (text is null)
        {
            throw new Exception("No text found for id: " + id);
        }

        int notAlphabetCount = 0;

        foreach (char c in text)
        {
            if (!char.IsAsciiLetter(c))
            {
                notAlphabetCount++;
            }
        }

        string rankKey = KeyBuilder.BuildRankKey(id);
        double rank = (double)notAlphabetCount / text.Length;

        _redis.StringSet(rankKey, rank.ToString(CultureInfo.InvariantCulture));

        await _publishEndpoint.Publish<IRankCalculated>(new
        {
            Id = id,
            Rank = rank,
        });
    }
}