using StackExchange.Redis;

namespace Utils;

public sealed class RedisShardStore : IDisposable
{
    private static readonly IReadOnlyDictionary<string, string> DefaultShardConnections =
        new Dictionary<string, string>
        {
            ["RU"] = "localhost:6001",
            ["EU"] = "localhost:6002",
            ["ASIA"] = "localhost:6003"
        };

    private readonly Dictionary<string, IConnectionMultiplexer> _shardConnections;

    private RedisShardStore(
        IConnectionMultiplexer mainConnection,
        Dictionary<string, IConnectionMultiplexer> shardConnections)
    {
        MainConnection = mainConnection;
        _shardConnections = shardConnections;
    }

    public IConnectionMultiplexer MainConnection { get; }

    public IDatabase MainDatabase => MainConnection.GetDatabase();

    public static RedisShardStore Create(string? mainFallbackConnectionString = null)
    {
        string mainConnectionString =
            ReadConnectionString("DB_MAIN", mainFallbackConnectionString ?? "localhost:6000");

        var shardConnections = new Dictionary<string, IConnectionMultiplexer>();
        foreach (string region in CountryRegions.Regions)
        {
            string connectionString = ReadConnectionString(
                $"DB_{region}",
                DefaultShardConnections[region]);

            shardConnections[region] = ConnectionMultiplexer.Connect(connectionString);
        }

        return new RedisShardStore(
            ConnectionMultiplexer.Connect(mainConnectionString),
            shardConnections);
    }

    public IConnectionMultiplexer GetShardConnection(string region)
    {
        if (!_shardConnections.TryGetValue(region, out IConnectionMultiplexer? connection))
        {
            throw new ArgumentException($"Unknown region: {region}", nameof(region));
        }

        return connection;
    }

    public IDatabase GetShardDatabase(string region)
    {
        return GetShardConnection(region).GetDatabase();
    }

    public void SaveShardKey(string id, string region)
    {
        MainDatabase.StringSet(KeyBuilder.BuildShardMapKey(id), region);
    }

    public string GetShardKey(string id)
    {
        RedisValue region = MainDatabase.StringGet(KeyBuilder.BuildShardMapKey(id));
        if (region.IsNullOrEmpty)
        {
            throw new Exception($"No shard key found for id: {id}");
        }

        return region.ToString();
    }

    public void Dispose()
    {
        MainConnection.Dispose();

        foreach (IConnectionMultiplexer connection in _shardConnections.Values)
        {
            connection.Dispose();
        }
    }

    private static string ReadConnectionString(string variableName, string fallback)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
