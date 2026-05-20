namespace Utils;

public static class KeyBuilder
{
    private const string TextKey = "TEXT-";
    private const string RankKey = "RANK-";
    private const string SimilarityKey = "SIMILARITY-";
    private const string TextOwnerKey = "OWNER-";
    private const string UserKey = "USER-";
    private const string UserLoginKey = "USER-LOGIN-";

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

    public static string BuildTextOwnerKey(string id)
    {
        return TextOwnerKey + id;
    }

    public static string BuildUserKey(string id)
    {
        return UserKey + id;
    }

    public static string BuildUserLoginKey(string login)
    {
        return UserLoginKey + login.Trim().ToUpperInvariant();
    }
}
