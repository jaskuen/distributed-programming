using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Utils;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _redis;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _redis = connectionMultiplexer.GetDatabase();
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string textKey = KeyBuilder.BuildTextKey(id);
        _redis.StringSet(textKey, text);

        string rankKey = KeyBuilder.BuildRankKey(id);

        string regex = "[A-Za-zА-Яа-я]";
        int lettersCount = Regex.Count(text, regex, RegexOptions.None);

        double rank = (double)lettersCount / text.Length;

        _redis.StringSet(rankKey, rank.ToString(CultureInfo.InvariantCulture));

        string similarityKey = KeyBuilder.BuildSimilarityKey(id);

        var keys =
            _redis
                .Multiplexer
                .GetServer(_connectionMultiplexer.GetEndPoints().First()).Keys(database: 0, pattern: "TEXT-*");

        int similarity = 0;

        foreach (var key in keys)
        {
            string stringKey = key.ToString();
            
            if (stringKey == textKey)
            {
                continue;
            }

            if (text == _redis.StringGet(key).ToString())
            {
                similarity = 1;
                break;
            }
        }

        _redis.StringSet(similarityKey, similarity);

        return Redirect($"summary?id={id}");
    }
}