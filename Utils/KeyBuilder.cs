namespace Utils;

public static class KeyBuilder
{
    private const string TextKey = "TEXT-";
    private const string CountryKey = "COUNTRY-";
    private const string RankKey = "RANK-";
    private const string SimilarityKey = "SIMILARITY-";
    private const string ShardMapKey = "SHARD-";

    public static string BuildTextKey(string id)
    {
        return TextKey + id;
    }

    public static string BuildCountryKey(string id)
    {
        return CountryKey + id;
    }

    public static string BuildRankKey(string id)
    {
        return RankKey + id;
    }

    public static string BuildSimilarityKey(string id)
    {
        return SimilarityKey + id;
    }

    public static string BuildShardMapKey(string id)
    {
        return ShardMapKey + id;
    }
}
