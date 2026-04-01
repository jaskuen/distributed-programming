using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Valuator.Utils;

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

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        try
        {
            _logger.LogDebug(id);

            Rank = double.Parse(_redis.StringGet(KeyBuilder.BuildRankKey(id)).ToString(), CultureInfo.InvariantCulture);

            Similarity = double.Parse(_redis.StringGet(KeyBuilder.BuildSimilarityKey(id)).ToString(),
                CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}