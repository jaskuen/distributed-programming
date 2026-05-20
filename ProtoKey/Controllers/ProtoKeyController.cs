using Microsoft.AspNetCore.Mvc;
using ProtoKey.Http;
using ProtoKey.Store;

namespace ProtoKey.Controllers;

[ApiController]
public sealed class ProtoKeyController(StoreClient store) : ControllerBase
{
    [HttpPost("/set")]
    public async Task<IActionResult> SetAsync(
        [FromBody] SetRequest request,
        CancellationToken cancellationToken)
    {
        if (!ProtoKey.Validation.InputValidator.IsValidKey(request.Key))
        {
            return BadRequest("Key must be 1..1000 chars: a-z, A-Z, 0-9, _, -, .");
        }

        StoreResponse response = await store.SetAsync(request.Key, request.Value, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("/get/{key}")]
    public async Task<IActionResult> GetAsync(
        string key,
        CancellationToken cancellationToken)
    {
        if (!ProtoKey.Validation.InputValidator.IsValidKey(key))
        {
            return BadRequest("Key must be 1..1000 chars: a-z, A-Z, 0-9, _, -, .");
        }

        StoreResponse response = await store.GetAsync(key, cancellationToken);
        return response.Success ? Ok(response.Value) : BadRequest(response.Error);
    }

    [HttpGet("/keys")]
    public async Task<IActionResult> KeysAsync(
        [FromQuery] string? prefix,
        CancellationToken cancellationToken)
    {
        prefix ??= string.Empty;

        if (!ProtoKey.Validation.InputValidator.IsValidPrefix(prefix))
        {
            return BadRequest("Prefix must contain only: a-z, A-Z, 0-9, _, -, .");
        }

        StoreResponse response = await store.KeysAsync(prefix, cancellationToken);
        return response.Success ? Ok(response.Keys) : BadRequest(response.Error);
    }

    private IActionResult ToActionResult(StoreResponse response) =>
        response.Success ? Ok() : BadRequest(response.Error);
}