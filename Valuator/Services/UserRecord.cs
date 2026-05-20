namespace Valuator.Services;

public sealed class UserRecord
{
    public required string Id { get; init; }
    public required string Login { get; init; }
    public required string PasswordHash { get; init; }
}
