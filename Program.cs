using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    var notes = lines.Select(s => Fields(s, 5)).ToArray();
    foreach (var f in notes)
        Check(f[3] is "support" or "distinguish", "Side must be support or distinguish.");
    foreach (var issue in notes.GroupBy(f => f[0]).OrderBy(g => g.Key))
    {
        int support = issue.Count(f => f[3] == "support");
        Console.WriteLine($"{issue.Key} ({support} supporting, {issue.Count() - support} distinguishing)");
        foreach (var f in issue.OrderBy(f => f[1]).ThenBy(f => f[2]))
            Console.WriteLine($"  [{f[3]}] {f[1]}, {f[2]} — {f[4]}");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}
