using System.Globalization;
using System.Text;

namespace Kasir.Core;

public static class ReceiptText
{
    public const int Width58mm = 32;
    public const int Width80mm = 42;

    private static readonly CultureInfo Id = new("id-ID");

    public static string Rup(decimal v) => "Rp " + v.ToString("N0", Id);

    public static string QtyStr(double q)
    {
        if (Math.Abs(q - Math.Round(q)) < 0.0005) return ((long)Math.Round(q)).ToString(Id);
        return q.ToString("0.###", Id);
    }

    public static List<string> Build(ReceiptModel m, int width)
    {
        if (width != Width58mm && width != Width80mm)
            throw new ArgumentOutOfRangeException(nameof(width), "Lebar harus 32 atau 42.");
        var out_ = new List<string>();
        var h = m.Header;

        if (m.IsCopy) out_.Add(Center("*** SALINAN ***", width));
        out_.Add(Center(h.StoreName.ToUpperInvariant(), width));
        foreach (var a in Wrap(h.Address, width)) out_.Add(Center(a, width));
        if (!string.IsNullOrWhiteSpace(h.Phone)) out_.Add(Center(h.Phone.Trim(), width));
        out_.Add(Dashes(width));
        out_.Add(Fit("No: " + h.InvoiceNo, h.PaymentMethod, width));
        var local = h.CreatedAt.ToLocalTime();
        out_.Add(Fit(local.ToString("dd/MM/yyyy HH:mm"), "Kasir: " + h.CashierName, width));
        out_.Add(Dashes(width));

        foreach (var l in m.Lines)
        {
            foreach (var n in Wrap(l.Name, width)) out_.Add(Clip(n, width));
            var left = $"{QtyStr(l.Qty)} x {Rup(l.UnitPrice)}";
            if (l.LineDiscount != 0) left += $" -{Rup(l.LineDiscount)}";
            out_.Add(Fit(left, Rup(l.Subtotal), width));
        }
        out_.Add(Dashes(width));

        var t = m.Totals;
        out_.Add(Fit("Subtotal", Rup(t.Subtotal), width));
        if (t.Discount != 0) out_.Add(Fit("Diskon", "-" + Rup(t.Discount), width));
        if (t.Tax != 0) out_.Add(Fit("Pajak", Rup(t.Tax), width));
        out_.Add(Fit("TOTAL", Rup(t.Total), width));
        out_.Add(Fit("Bayar", Rup(t.Paid), width));
        out_.Add(Fit("Kembalian", Rup(t.Change), width));
        out_.Add(Dashes(width));
        foreach (var f in Wrap(m.Footer, width)) out_.Add(Center(f, width));
        return out_;
    }

    private static string Dashes(int w) => new('-', w);

    private static string Clip(string s, int w)
    {
        s ??= string.Empty;
        return s.Length <= w ? s : s.Substring(0, w);
    }

    private static string Center(string s, int w)
    {
        s = (s ?? string.Empty).Trim();
        if (s.Length >= w) return Clip(s, w);
        var pad = w - s.Length;
        return new string(' ', pad / 2) + s + new string(' ', pad - pad / 2);
    }

    private static string Fit(string left, string right, int w)
    {
        left ??= string.Empty;
        right ??= string.Empty;
        if (left.Length + right.Length >= w)
            left = Clip(left, w - right.Length - 1);
        return (left + new string(' ', w - left.Length - right.Length) + right);
    }

    private static List<string> Wrap(string s, int w)
    {
        var res = new List<string>();
        s = (s ?? string.Empty).Trim();
        if (s.Length == 0) return res;
        var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cur = new StringBuilder();
        foreach (var word in words)
        {
            var w2 = word.Length > w ? word.Substring(0, w) : word;
            if (cur.Length == 0) cur.Append(w2);
            else if (cur.Length + 1 + w2.Length <= w) cur.Append(' ').Append(w2);
            else { res.Add(cur.ToString()); cur.Clear(); cur.Append(w2); }
        }
        if (cur.Length > 0) res.Add(cur.ToString());
        return res;
    }
}
