using Microsoft.Data.Sqlite;

static class BriefReport
{
    public static void Run(string[] args)
    {
        bool sheet = args.Length == 5 && args[2] == "--sheet";
        if (args.Length != 2 && args.Length != 3 && !sheet)
            throw new FormatException("Usage: brief moot.db study.tsv");
        using var db = Open(args[1]);
        using var tx = Begin(db);
        Sql(db, "CREATE TABLE IF NOT EXISTS authority(id INTEGER PRIMARY KEY, name TEXT NOT NULL COLLATE NOCASE UNIQUE); CREATE TABLE IF NOT EXISTS note(issue TEXT NOT NULL, authority_id INTEGER NOT NULL REFERENCES authority(id), pinpoint TEXT NOT NULL, side TEXT NOT NULL, proposition TEXT NOT NULL, PRIMARY KEY(issue,authority_id,pinpoint,side,proposition))");
        if (args.Length == 3)
            foreach (var r in Rows(args[2], 5))
            {
                Require(r[3] is "support" or "distinguish", "Side must be support or distinguish.");
                Sql(db, "INSERT INTO authority(name) VALUES($a) ON CONFLICT(name) DO NOTHING", r[1]);
                Sql(db, "INSERT INTO note SELECT $a,id,$c,$d,$e FROM authority WHERE name=$b ON CONFLICT DO NOTHING", r[0], r[1], r[2], r[3], r[4]);
            }

        var notes = Query(db, "SELECT n.issue,a.name,n.pinpoint,n.side,n.proposition FROM note n JOIN authority a ON a.id=n.authority_id ORDER BY n.issue,n.proposition,a.name,n.pinpoint");
        tx.Commit();
        if (sheet)
        {
            var chosen = notes.Where(n => string.Equals(n[0], args[3], StringComparison.OrdinalIgnoreCase)).ToArray();
            Require(chosen.Length > 0, "No notes for issue " + args[3]);
            Require(Path.GetExtension(args[4]).Equals(".html", StringComparison.OrdinalIgnoreCase)
                && !Path.GetFullPath(args[1]).Equals(Path.GetFullPath(args[4]), StringComparison.OrdinalIgnoreCase), "Choose a separate .html output file.");
            static string Html(string text) => System.Net.WebUtility.HtmlEncode(text);
            var page = new System.Text.StringBuilder("<!doctype html><html lang=en><meta charset=utf-8><title>Oral argument sheet</title><style>@page{margin:16mm}body{font:15px/1.4 system-ui;max-width:75ch;margin:1em auto}li{margin:.65em 0;break-inside:avoid}small{display:block}</style>");
            page.Append($"<h1>{Html(args[3])}</h1><ol>");
            foreach (var row in chosen) page.Append($"<li><b>{Html(row[4])}</b><small>{Html(row[1])}, {Html(row[2])} · {Html(row[3])}</small></li>");
            page.Append("</ol></html>");
            File.WriteAllText(args[4], page.ToString());
            Console.WriteLine($"Saved {chosen.Length} cited propositions to {args[4]}");
            return;
        }
        Console.WriteLine("ORAL ARGUMENT SHEET");
        foreach (var issue in notes.GroupBy(n => n[0]))
        {
            Console.WriteLine($"\n{issue.Key} — {issue.Select(n => n[1]).Distinct().Count()} distinct authorities");
            foreach (var n in issue)
                Console.WriteLine($"  {n[4]}  [{n[3]}] {n[1]}, {n[2]}");
        }
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static SqliteTransaction? transaction;
    static SqliteConnection Open(string file)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file, ForeignKeys = true }.ToString());
        connection.Open();
        return connection;
    }

    static SqliteTransaction Begin(SqliteConnection db) => transaction = db.BeginTransaction();
    static SqliteCommand Command(SqliteConnection db, string sql, string[] values)
    {
        var cmd = db.CreateCommand();
        if (transaction?.Connection != null)
            cmd.Transaction = transaction;
        cmd.CommandText = sql;
        for (int i = 0; i < values.Length; i++)
            cmd.Parameters.AddWithValue("$" + (char)(97 + i), values[i]);
        return cmd;
    }

    static void Sql(SqliteConnection db, string sql, params string[] values)
    {
        using var cmd = Command(db, sql, values);
        cmd.ExecuteNonQuery();
    }

    static List<string[]> Query(SqliteConnection db, string sql, params string[] values)
    {
        using var cmd = Command(db, sql, values);
        using var reader = cmd.ExecuteReader();
        var records = new List<string[]>();
        while (reader.Read())
            records.Add(Enumerable.Range(0, reader.FieldCount).Select(i => Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? "").ToArray());
        return records;
    }
}
