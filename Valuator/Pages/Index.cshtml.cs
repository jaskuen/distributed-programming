using System.Globalization;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Utils;
using Valuator.Messages;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly RedisShardStore _redisShardStore;
    private readonly ILogger<IndexModel> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public IndexModel(ILogger<IndexModel> logger, RedisShardStore redisShardStore,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _redisShardStore = redisShardStore;
        _publishEndpoint = publishEndpoint;
    }

    public IReadOnlyList<CountryOption> Countries => CountryRegions.Countries;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string? text, string? country)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("Text is empty");
            }

            if (string.IsNullOrWhiteSpace(country))
            {
                throw new Exception("Country is empty");
            }

            string region = CountryRegions.GetRegionForCountry(country);
            string id = Guid.NewGuid().ToString();
            IDatabase shardDatabase = _redisShardStore.GetShardDatabase(region);

            _logger.LogDebug(text);
            _logger.LogInformation("LOOKUP: {Id}, {Region}", id, region);

            string textKey = KeyBuilder.BuildTextKey(id);
            string countryKey = KeyBuilder.BuildCountryKey(id);

            _redisShardStore.SaveShardKey(id, region);
            shardDatabase.StringSet(textKey, text.ToString(CultureInfo.InvariantCulture));
            shardDatabase.StringSet(countryKey, country);
            await _publishEndpoint.Publish<ITextCreated>(new
            {
                Id = id,
            });
            
            string similarityKey = KeyBuilder.BuildSimilarityKey(id);

            var keys =
                shardDatabase
                    .Multiplexer
                    .GetServer(_redisShardStore.GetShardConnection(region).GetEndPoints().First())
                    .Keys(database: 0, pattern: "TEXT-*");

            int similarity = 0;

            foreach (var key in keys)
            {
                if (key == textKey)
                {
                    continue;
                }

                if (text == shardDatabase.StringGet(key).ToString())
                {
                    similarity = 1;
                    break;
                }
            }
        
            shardDatabase.StringSet(similarityKey, similarity.ToString(CultureInfo.InvariantCulture));

            await _publishEndpoint.Publish<ISimilarityCalculated>(new
            {
                Id = id,
                Similarity = similarity,
            });

            return Redirect($"summary?id={id}");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Page();
    }
}
