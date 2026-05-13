using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Utils;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly RedisShardStore _redisShardStore;
    private readonly ILogger<SummaryModel> _logger;

    public SummaryModel(ILogger<SummaryModel> logger, RedisShardStore redisShardStore)
    {
        _logger = logger;
        _redisShardStore = redisShardStore;
    }

    public bool IsLoading { get; set; }
    public double Rank { get; set; }
    public double Similarity { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }

    public void OnGet(string id)
    {
        try
        {
            _logger.LogDebug(id);
            Region = _redisShardStore.GetShardKey(id);
            _logger.LogInformation("LOOKUP: {Id}, {Region}", id, Region);

            IDatabase shardDatabase = _redisShardStore.GetShardDatabase(Region);
            
            RedisValue stringRank = shardDatabase.StringGet(KeyBuilder.BuildRankKey(id));
            if (stringRank.IsNullOrEmpty)
            {
                IsLoading = true;
                return;
            }

            Rank = double.Parse(stringRank.ToString(), CultureInfo.InvariantCulture);

            Similarity = double.Parse(shardDatabase.StringGet(KeyBuilder.BuildSimilarityKey(id)).ToString(),
                CultureInfo.InvariantCulture);
            Country = shardDatabase.StringGet(KeyBuilder.BuildCountryKey(id)).ToString();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}
