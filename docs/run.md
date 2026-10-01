# Run Cashier Exhaust

## Syarat

- .NET SDK 8.0.425 atau lebih baru di jalur 8.0.
- Windows 10 atau lebih baru untuk dev saat ini. Linux dan macOS menyusul di Fase 9.

## Jalankan dev

```bash
dotnet build Kasir.sln
dotnet run --project src/Kasir.Desktop
```

## Data lokal (tanpa database)

- Folder: `%LocalAppData%/CashierExhaust/data`
- File: `products.csv`, `sales.csv`, `sale_items.csv`, `stock_movements.csv`, `daily_sequences.csv`, `users.csv`, `categories.csv`
- File `.db` lama tidak dipakai lagi dan boleh dihapus.
- Semua tulis file atomik via temp + move dalam satu kunci proses.

## Akun awal

- Username: `admin`
- Password: masih plaintext di fase 0. Wajib diganti dengan hash di Fase 5. Jangan pakai untuk data asli.

## Reset data contoh

1. Tutup aplikasi.
2. Hapus folder `%LocalAppData%/CashierExhaust/data`.
3. Jalankan lagi. Seed 5 produk contoh akan dibuat ulang.

## Uji cepat Fase 0

1. Judul window `Cashier Exhaust`.
2. Ketik barcode `8991001` lalu klik Tambah. Item masuk keranjang.
3. Klik Uang Pas lalu Bayar. Status menampilkan invoice dan kembalian Rp 0.
4. Tutup dan buka lagi. Stok Indomie berkurang 1.
