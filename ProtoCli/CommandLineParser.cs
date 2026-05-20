using System.Text;

namespace ProtoCli;

internal static class CommandLineParser
{
    public static IReadOnlyList<string> Parse(string line)
    {
        List<string> args = new List<string>();
        StringBuilder current = new StringBuilder();
        bool insideQuotes = false;
        bool argStarted = false;

        foreach (char character in line)
        {
            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                argStarted = true;
                continue;
            }

            if (char.IsWhiteSpace(character) && !insideQuotes)
            {
                AddCurrentArg(args, current, ref argStarted);
                continue;
            }

            argStarted = true;
            current.Append(character);
        }

        AddCurrentArg(args, current, ref argStarted);
        return args;
    }

    private static void AddCurrentArg(List<string> args, StringBuilder current, ref bool argStarted)
    {
        if (!argStarted)
        {
            return;
        }

        args.Add(current.ToString());
        current.Clear();
        argStarted = false;
    }
}