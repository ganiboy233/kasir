namespace Kasir.Data;

public sealed class CsvStore<T>
{
    private readonly object _gate = new();
    private readonly string _path;
    private readonly string[] _header;
    private readonly Func<T, string[]> _toRow;
    private readonly Func<string[], T?> _fromRow;
    private List<T> _items = new();

    public CsvStore(string path, string[] header, Func<T, string[]> toRow, Func<string[], T?> fromRow)
    {
        _path = path;
        _header = header;
        _toRow = toRow;
        _fromRow = fromRow;
    }

    public void Load()
    {
        lock (_gate)
        {
            var list = new List<T>();
            var first = true;
            foreach (var r in Csv.ReadAll(_path))
            {
                if (first && IsHeader(r)) { first = false; continue; }
                first = false;
                var item = _fromRow(r);
                if (item is not null) list.Add(item);
            }
            _items = list;
        }
    }

    public List<T> All()
    {
        lock (_gate) return _items.ToList();
    }

    public void ReplaceAll(List<T> items)
    {
        lock (_gate)
        {
            _items = items.ToList();
            Csv.WriteAll(_path, _header, _items.Select(_toRow));
        }
    }

    public void Add(T item)
    {
        lock (_gate)
        {
            _items.Add(item);
            Csv.WriteAll(_path, _header, _items.Select(_toRow));
        }
    }

    private bool IsHeader(string[] row)
    {
        if (row.Length != _header.Length) return false;
        for (var i = 0; i < row.Length; i++)
            if (row[i] != _header[i]) return false;
        return true;
    }
}
