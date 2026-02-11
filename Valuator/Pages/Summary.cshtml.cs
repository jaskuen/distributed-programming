using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
        _logger.LogDebug(id);

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)

        string? val = _redis.StringGet(KeyBuilder.BuildRankKey(id)).ToString().Replace('.', ',');

        if (!double.TryParse(val, out var rank))
        {
            throw new Exception("Could not read rank from Redis");
        }

        Rank = rank;

        if (!double.TryParse(_redis.StringGet(KeyBuilder.BuildSimilarityKey(id)), out var similatiry))
        {
            throw new Exception("Could not read similarity from Redis");
        }

        Similarity = similatiry;
    }
}