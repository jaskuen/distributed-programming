using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using Utils;

namespace Valuator.Services;

public sealed class UserStore
{
    private readonly IDatabase _redis;
    private readonly PasswordHasher<UserRecord> _passwordHasher = new();

    public UserStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<(bool Success, string? Error, UserRecord? User)> RegisterAsync(string login, string password)
    {
        login = login.Trim();
        if (login.Length < 3)
        {
            return (false, "Логин должен содержать минимум 3 символа.", null);
        }

        if (password.Length < 6)
        {
            return (false, "Пароль должен содержать минимум 6 символов.", null);
        }

        string id = Guid.NewGuid().ToString();
        string loginKey = KeyBuilder.BuildUserLoginKey(login);
        bool loginReserved = await _redis.StringSetAsync(loginKey, id, when: When.NotExists);
        if (!loginReserved)
        {
            return (false, "Пользователь с таким логином уже существует.", null);
        }

        var user = new UserRecord
        {
            Id = id,
            Login = login,
            PasswordHash = string.Empty,
        };

        user = new UserRecord
        {
            Id = user.Id,
            Login = user.Login,
            PasswordHash = _passwordHasher.HashPassword(user, password),
        };

        await _redis.StringSetAsync(KeyBuilder.BuildUserKey(id), JsonSerializer.Serialize(user));
        return (true, null, user);
    }

    public async Task<UserRecord?> ValidateCredentialsAsync(string login, string password)
    {
        UserRecord? user = await FindByLoginAsync(login);
        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    private async Task<UserRecord?> FindByLoginAsync(string login)
    {
        RedisValue userId = await _redis.StringGetAsync(KeyBuilder.BuildUserLoginKey(login));
        if (userId.IsNullOrEmpty)
        {
            return null;
        }

        return await FindByIdAsync(userId!);
    }

    private async Task<UserRecord?> FindByIdAsync(string id)
    {
        RedisValue json = await _redis.StringGetAsync(KeyBuilder.BuildUserKey(id));
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<UserRecord>(json!);
    }
}
