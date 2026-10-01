using System.Text;

namespace Kasir.Data;

public static class Csv
{
    public static string Escape(string? v)
    {
        v ??= string.Empty;
        if (v.Contains('"') || v.Contains(',') || v.Contains('\n') || v.Contains('\r'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    public static List<string[]> ReadAll(string path)
    {
        var rows = new List<string[]>();
        if (!File.Exists(path)) return rows;
        using var r = new StreamReader(path, Encoding.UTF8);
        var field = new StringBuilder();
        var row = new List<string>();
        var inQuotes = false;
        int ch;
        while ((ch = r.Read()) != -1)
        {
            var c = (char)ch;
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (r.Peek() == '"') { r.Read(); field.Append('"'); }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\r') { }
                else if (c == '\n') { row.Add(field.ToString()); field.Clear(); rows.Add(row.ToArray()); row = new List<string>(); }
                else field.Append(c);
            }
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row.ToArray()); }
        return rows;
    }

    public static void WriteAll(string path, string[] header, IEnumerable<string[]> rows)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var tmp = path + ".tmp";
        using (var w = new StreamWriter(tmp, false, new UTF8Encoding(false)))
        {
            w.WriteLine(string.Join(",", header.Select(Escape)));
            foreach (var row in rows)
                w.WriteLine(string.Join(",", row.Select(Escape)));
        }
        File.Move(tmp, path, overwrite: true);
    }
}
