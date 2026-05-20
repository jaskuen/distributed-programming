using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProtoCli;

internal sealed class ProtoKeyClient(HttpClient httpClient, string host)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CommandResult> SetAsync(string key, int value)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{host}/set",
            new SetRequest(key, value));

        return await ToResultAsync(response);
    }

    public async Task<CommandResult> GetAsync(string key)
    {
        string? encodedKey = WebUtility.UrlEncode(key);
        HttpResponseMessage response = await httpClient.GetAsync($"{host}/get/{encodedKey}");

        if (!response.IsSuccessStatusCode)
        {
            return await ToResultAsync(response);
        }

        return CommandResult.Ok(await response.Content.ReadAsStringAsync());
    }

    public async Task<CommandResult> KeysAsync(string prefix)
    {
        string? encodedPrefix = WebUtility.UrlEncode(prefix);
        HttpResponseMessage response = await httpClient.GetAsync($"{host}/keys?prefix={encodedPrefix}");

        if (!response.IsSuccessStatusCode)
        {
            return await ToResultAsync(response);
        }

        string[]? keys = await response.Content.ReadFromJsonAsync<string[]>(JsonOptions);
        return CommandResult.Ok(keys ?? []);
    }

    private static async Task<CommandResult> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return CommandResult.Ok();
        }

        string body = await response.Content.ReadAsStringAsync();
        string error = string.IsNullOrWhiteSpace(body)
            ? $"HTTP {(int)response.StatusCode}"
            : body;

        return CommandResult.Fail(error);
    }

    private sealed record SetRequest(string Key, int Value);
}