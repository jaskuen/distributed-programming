using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Utils;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly IDatabase _redis;
    private readonly ILogger<SummaryModel> _logger;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _redis = connectionMultiplexer.GetDatabase();
    }

    public string? Id { get; set; }
    public bool IsLoading { get; set; }
    public double? Rank { get; set; }
    public double? Similarity { get; set; }

    public void OnGet(string id)
    {
        try
        {
            Id = id;
            _logger.LogDebug("{Id}", id);

            RedisValue stringRank = _redis.StringGet(KeyBuilder.BuildRankKey(id));
            if (stringRank.HasValue &&
                double.TryParse(stringRank.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double rank))
            {
                Rank = rank;
            }
            else
            {
                IsLoading = true;
            }

            RedisValue stringSimilarity = _redis.StringGet(KeyBuilder.BuildSimilarityKey(id));
            if (stringSimilarity.HasValue &&
                double.TryParse(stringSimilarity.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double similarity))
            {
                Similarity = similarity;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}
