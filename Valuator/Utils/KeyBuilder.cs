namespace Valuator.Utils;

public static class KeyBuilder
{
    private const string TextKey = "TEXT-";
    private const string RankKey = "RANK-";
    private const string SimilarityKey = "SIMILARITY-";

    public static string BuildTextKey(string id)
    {
        return TextKey + id;
    }

    public static string BuildRankKey(string id)
    {
        return RankKey + id;
    }

    public static string BuildSimilarityKey(string id)
    {
        return SimilarityKey + id;
    }
}