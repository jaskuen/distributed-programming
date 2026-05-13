using System.Globalization;
using MassTransit;
using RankCalculator.Messages;
using StackExchange.Redis;
using Utils;
using Valuator.Messages;

namespace RankCalculator.Services;

public class CalculatorService : IConsumer<ITextCreated>
{
    private readonly RedisShardStore _redisShardStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CalculatorService(RedisShardStore redisShardStore, IPublishEndpoint publishEndpoint)
    {
        _redisShardStore = redisShardStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ITextCreated> context)
    {
        string id = context.Message.Id;

        await Calculate(id);

        await Task.CompletedTask;
    }

    private async Task Calculate(string id)
    {
        string region = _redisShardStore.GetShardKey(id);
        IDatabase shardDatabase = _redisShardStore.GetShardDatabase(region);
        Console.WriteLine($"LOOKUP: {id}, {region}");

        string text = shardDatabase.StringGet(KeyBuilder.BuildTextKey(id))!;

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

        shardDatabase.StringSet(rankKey, rank.ToString(CultureInfo.InvariantCulture));

        await _publishEndpoint.Publish<IRankCalculated>(new
        {
            Id = id,
            Rank = rank,
        });
    }
}
