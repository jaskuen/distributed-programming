namespace Utils;

public static class CountryRegions
{
    public const string Russia = "Russia";
    public const string France = "France";
    public const string Germany = "Germany";
    public const string Uae = "UAE";
    public const string India = "India";

    public static readonly IReadOnlyList<CountryOption> Countries =
    [
        new(Russia, "RU"),
        new(France, "EU"),
        new(Germany, "EU"),
        new(Uae, "ASIA"),
        new(India, "ASIA")
    ];

    public static readonly IReadOnlyList<string> Regions =
    [
        "RU",
        "EU",
        "ASIA"
    ];

    public static string GetRegionForCountry(string country)
    {
        CountryOption? option = Countries.FirstOrDefault(x =>
            string.Equals(x.Country, country, StringComparison.OrdinalIgnoreCase));

        if (option is null)
        {
            throw new ArgumentException($"Unknown country: {country}", nameof(country));
        }

        return option.Region;
    }
}

public sealed record CountryOption(string Country, string Region);
