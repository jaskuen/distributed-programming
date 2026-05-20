namespace ProtoKey.Persistence;

internal static class PersistencePaths
{
    public static string DataFile(string contentRootPath) => Path.Combine(contentRootPath, "ProtoKey.data");
}