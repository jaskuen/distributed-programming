using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Utils;

namespace Valuator.Pages;

[Authorize]
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

    public IActionResult OnGet(string id)
    {
        try
        {
            _logger.LogDebug(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            string? ownerId = _redis.StringGet(KeyBuilder.BuildTextOwnerKey(id));
            if (ownerId is null)
            {
                return NotFound();
            }

            if (ownerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            string? stringRank = _redis.StringGet(KeyBuilder.BuildRankKey(id));
            if (stringRank is null)
            {
                IsLoading = true;
                return Page();
            }

            Rank = double.Parse(stringRank, CultureInfo.InvariantCulture);

            Similarity = double.Parse(_redis.StringGet(KeyBuilder.BuildSimilarityKey(id)).ToString(),
                CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Page();
    }
}
