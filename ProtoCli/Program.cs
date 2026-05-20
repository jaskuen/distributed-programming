namespace ProtoCli;

internal static class Program
{
    private const string ProtoKeyHost = "http://127.0.0.1:7777";

    private static async Task Main()
    {
        using HttpClient httpClient = new();
        ProtoKeyClient protoKeyClient = new(httpClient, ProtoKeyHost);

        PrintUsage();

        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();

            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (IsExitCommand(line))
            {
                break;
            }

            IReadOnlyList<string> args = CommandLineParser.Parse(line);

            if (args.Count == 0)
            {
                continue;
            }

            await ExecuteAsync(protoKeyClient, args);
        }
    }

    private static async Task ExecuteAsync(ProtoKeyClient protoKeyClient, IReadOnlyList<string> args)
    {
        try
        {
            _ = args[0].ToLowerInvariant() switch
            {
                "set" => await SetAsync(protoKeyClient, args),
                "get" => await GetAsync(protoKeyClient, args),
                "keys" => await KeysAsync(protoKeyClient, args),
                _ => UnknownCommand(args[0])
            };
        }
        catch (HttpRequestException exception)
        {
            Console.Error.WriteLine($"Request failed: {exception.Message}");
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine("Request timed out.");
        }
    }

    private static async Task<int> SetAsync(ProtoKeyClient protoKeyClient, IReadOnlyList<string> args)
    {
        if (args.Count != 3 || !int.TryParse(args[2], out int value))
        {
            Console.Error.WriteLine("Usage: set <key> <value>");
            return 1;
        }

        CommandResult result = await protoKeyClient.SetAsync(args[1], value);
        return PrintResult(result, "OK");
    }

    private static async Task<int> GetAsync(ProtoKeyClient protoKeyClient, IReadOnlyList<string> args)
    {
        if (args.Count != 2)
        {
            Console.Error.WriteLine("Usage: get <key>");
            return 1;
        }

        CommandResult result = await protoKeyClient.GetAsync(args[1]);
        return PrintResult(result);
    }

    private static async Task<int> KeysAsync(ProtoKeyClient protoKeyClient, IReadOnlyList<string> args)
    {
        if (args.Count != 2)
        {
            Console.Error.WriteLine("Usage: keys <prefix>");
            return 1;
        }

        CommandResult result = await protoKeyClient.KeysAsync(args[1]);

        if (!result.Success)
        {
            Console.Error.WriteLine(result.Error);
            return 2;
        }

        foreach (string key in result.Keys)
        {
            Console.WriteLine(key);
        }

        return 0;
    }

    private static int PrintResult(CommandResult result, string? successMessage = null)
    {
        if (!result.Success)
        {
            Console.Error.WriteLine(result.Error);
            return 2;
        }

        Console.WriteLine(successMessage ?? result.Output);
        return 0;
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  set <key> <value>");
        Console.WriteLine("  get <key>");
        Console.WriteLine("  keys <prefix>");
        Console.WriteLine("  exit");
    }

    private static bool IsExitCommand(string line)
    {
        string trimmed = line.Trim();
        return string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "quit", StringComparison.OrdinalIgnoreCase);
    }
}