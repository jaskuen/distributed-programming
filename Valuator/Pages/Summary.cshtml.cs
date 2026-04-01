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

    public bool IsLoading { get; set; }
    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        try
        {
            _logger.LogDebug(id);
            
            string? stringRank = _redis.StringGet(KeyBuilder.BuildRankKey(id));
            if (stringRank is null)
            {
                IsLoading = true;
            }

            Rank = double.Parse(stringRank, CultureInfo.InvariantCulture);

            Similarity = double.Parse(_redis.StringGet(KeyBuilder.BuildSimilarityKey(id)).ToString(),
                CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}