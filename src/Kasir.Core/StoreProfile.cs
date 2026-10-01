namespace Kasir.Core;

public sealed record StoreProfile(string Name, string Address, string Phone, string Footer)
{
    public static StoreProfile Default => new(
        "TOKO UTAMA",
        string.Empty,
        string.Empty,
        "Terima kasih sudah berbelanja");
}
