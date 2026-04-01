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
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _redis;
    private readonly ILogger<IndexModel> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer connectionMultiplexer,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _publishEndpoint = publishEndpoint;
        _redis = connectionMultiplexer.GetDatabase();
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string? text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("Text is empty");
            }

            string id = Guid.NewGuid().ToString();

            _logger.LogDebug(text);

            string textKey = KeyBuilder.BuildTextKey(id);

            _redis.StringSet(textKey, text.ToString(CultureInfo.InvariantCulture));
            await _publishEndpoint.Publish<ITextCreated>(new
            {
                Id = id,
            });
            
            string similarityKey = KeyBuilder.BuildSimilarityKey(id);

            var keys =
                _redis
                    .Multiplexer
                    .GetServer(_connectionMultiplexer.GetEndPoints().First()).Keys(database: 0, pattern: "TEXT-*");

            int similarity = 0;

            foreach (var key in keys)
            {
                if (text == _redis.StringGet(key).ToString())
                {
                    similarity = 1;
                    break;
                }
            }
        
            _redis.StringSet(similarityKey, similarity.ToString(CultureInfo.InvariantCulture));

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