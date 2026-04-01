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
    private readonly IDatabase _redis;
    private readonly ILogger<IndexModel> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer connectionMultiplexer,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
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

            return Redirect($"summary?id={id}");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Page();
    }
}