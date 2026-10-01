using System.Text;

namespace Kasir.Core;

public static class Barcode
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            if (char.IsWhiteSpace(ch) || char.IsControl(ch)) continue;
            sb.Append(ch);
        }
        return sb.ToString().Trim();
    }

    public static bool IsPlausible(string code)
    {
        if (code.Length < 4 || code.Length > 32) return false;
        foreach (var ch in code)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-' || ch == '.') continue;
            return false;
        }
        return true;
    }

    public static string NormalizeFilter(string? raw)
    {
        return (raw ?? string.Empty).Trim().ToLowerInvariant();
    }
}
