# Cashier Exhaust - Rencana Build

> Produk: Cashier Exhaust. POS desktop offline untuk toko dan minimarket.
> Output: .exe untuk Windows, .deb untuk Linux, .dmg untuk macOS.
> Stack final: Avalonia 12.1.3, .NET 8, CommunityToolkit.Mvvm 8.4.2, EF Core SQLite 8.0.11.
> Prinsip jalan: satu subfase selesai penuh, build hijau, baru lanjut.

## 0. Fakta repo hari ini

Ada:

- `Kasir.sln` dengan `src/Kasir.Core`, `src/Kasir.Data`, `src/Kasir.Desktop`
- Entity: `Category`, `Product`, `User`, `Sale`, `SaleItem`, `StockMovement`
- `KasirDbContext` dengan `EnsureCreated`, seed kategori `Umum` dan user `admin`
- `MainViewModel` prototipe: cari produk, tambah dari barcode, keranjang, hitung total, checkout tunai, potong stok
- `CartLine` untuk baris keranjang

Rusak atau kurang:

1. `Views/MainWindow.axaml` masih bind ke `Greeting`. Properti itu sudah tidak ada. UI ini gagal tampil dengan benar.
2. Belum ada shell navigasi. Masih satu window.
3. `CashierId` hardcode 1. Belum ada login dan sesi kasir.
4. Password admin masih plaintext. Wajib hash sebelum rilis internal.
5. Invoice `INV-yyyyMMdd-HHmmss` bisa tabrakan jika dua transaksi dalam satu detik dan tidak urut.
6. Checkout simpan `Sale` dulu lalu cek stok. Pola ini bisa menyisakan data setengah jadi.
7. Belum ada diskon baris, diskon struk, pajak, retur, riwayat detail, laporan.
8. Belum ada migration. Skema belum bisa upgrade aman.
9. Belum ada cetak struk, status printer, cash drawer, shortcut kasir.
10. Judul aplikasi masih `Kasir.Desktop`. DB masih di folder `Kasir`.
11. Belum ada packaging, versioning, backup, dan panduan rilis.

Keputusan yang dikunci:

- Namespace kode tetap `Kasir.*` sampai v1.0 agar tidak buang waktu rename.
- Nama produk di UI, judul window, folder data, nama DB, dan invoice memakai `Cashier Exhaust` dan prefix `CX-`.
- Satu DB SQLite lokal. Semua fungsi jual beli wajib jalan tanpa internet.
- Uang memakai `decimal`. Qty memakai `double` karena ada barang timbang. Stok tidak boleh minus dari penjualan.

## 1. Identitas Cashier Exhaust

Rasa yang ditarget: alat kerja toko yang tenang, cepat, dan terbaca. Bukan aplikasi demo. Bukan landing page.

Ciri visual:

- Kertas hangat, tinta pekat, satu warna kerja.
- Angka harga selalu tabular dan rata kanan.
- Baris data rapat tapi target klik tetap besar.
- Ikon garis tipis satu set. Tidak ada emoji di tombol produksi.
- Bahasa Indonesia pendek: Bayar, Cari, Simpan, Batal, Retur, Tutup Shift.

Contoh nada teks:

- Benar: `Stok Kopi tinggal 2. Tambah 3?`
- Salah: `Ups! Sepertinya ada kendala pada stok kamu.`
- Benar: `Uang kurang Rp 4.500`
- Salah: `Mohon periksa kembali nominal pembayaran Anda.`
- Benar: `Sukses CX-20261001-0007. Kembalian Rp 7.500`
- Salah: `Transaksi berhasil! Terima kasih sudah menggunakan layanan kami!`

## 2. Aturan anti pola generik

Aturan ini berlaku untuk semua fase. Jika dilanggar, subfase dianggap gagal.

Dilarang:

- Gradasi ungu biru, efek kaca tebal, glow neon, background abstrak.
- Ilustrasi stok, foto stok, lorem ipsum, tombol pajangan yang tidak jalan.
- Lebih dari satu warna aksen dalam satu layar.
- Card besar kosong. POS butuh kepadatan, bukan halaman marketing.
- Copy generik seperti canggih, revolusioner, mulus tanpa hambatan, pengalaman luar biasa.
- Dialog sukses dengan confetti atau animasi lama. Kasir butuh 300 ms lalu kerja lagi.
- Tabel tanpa state kosong. Setiap tabel wajib punya teks kosong yang konkret.
- Warna saja sebagai penanda. Status wajib ikon plus teks.

Wajib:

- Setiap aksi penting punya jalan keyboard.
- Setiap angka uang punya format sama: `Rp 12.500`, rata kanan, tanpa desimal sen di layar kasir.
- Setiap error menyebut objek, angka, dan aksi berikutnya.
- Setiap layar punya satu aksi utama yang jelas. Di POS, aksi utama adalah `Bayar [F5]`.
- Kontras teks normal minimal 4.5:1. Target layar toko murah yang silau.

## 3. Sistem desain konkret

### 3.1 Warna

Tema terang sebagai default karena toko biasanya terang.

- Paper: `#F6F4EE`
- Surface: `#FFFFFF`
- Surface 2 untuk zebra tabel dan panel samping: `#EFECE3`
- Border: `#E2DCCD`
- Border kuat untuk input fokus: `#0F766E`
- Ink: `#191817`
- Ink sekunder: `#6C6759`
- Ink redup untuk placeholder: `#A39D8D`
- Aksen kerja: `#0E6F67`, hover `#0B5D56`, tekan `#084A45`
- Tinta di atas aksen: `#FFFFFF`
- Sukses: `#166534`, background sukses: `#DCFCE7`
- Peringatan: `#92400E`, background peringatan: `#FEF3C7`
- Bahaya: `#991B1B`, background bahaya: `#FEE2E2`
- Info: `#1E40AF`, background info: `#DBEAFE`
- Fokus keyboard: outline 2 px `#0E6F67` dengan offset 2 px.

Tema gelap disiapkan di Fase 1 sebagai token, diterapkan penuh di Fase 8. Jangan buat dua tema setengah jadi sejak awal.

### 3.2 Tipografi

Font: Inter. Angka memakai fitur tabular agar titik ribuan tidak goyang saat qty berubah.

Skala:

- Display kasir untuk total: 32 bold, tracking -0.5
- Kembalian besar: 28 bold
- Judul panel: 14 semibold
- Isi: 14 regular
- Tabel: 13.5 regular, header 12 semibold uppercase dengan letter spacing 0.4
- Bantuan kecil: 12 regular untuk hint shortcut
- Struk preview: monospace 12, lebar 32 kolom untuk 58 mm dan 42 kolom untuk 80 mm

Aturan:

- Harga tidak pernah lebih kecil dari nama barang di baris yang sama.
- Jangan pakai italic untuk informasi penting.
- Shortcut ditulis konsisten: `[F5]`, `[Enter]`, `[Esc]`, `[Ctrl+K]`.

### 3.3 Spasi, radius, elevasi

- Basis 4 px. Padding panel 12 atau 16. Jarak antar grup 12. Jarak antar section 20.
- Radius input dan tombol: 8. Radius panel: 10. Radius dialog: 12.
- Baris tabel 40 px. Baris keranjang 56 px agar stepper mudah disentuh.
- Target sentuh minimal 40 x 40.
- Elevasi hanya tiga level: datar, dialog dengan shadow lembut, menu popup. Tidak ada shadow di tiap card kecil.

### 3.4 Gerak

- Durasi 80 sampai 120 ms. Tanpa bounce.
- Tambah barang: flash latar baris 120 ms, bukan animasi geser.
- Dialog: fade plus naik 6 px, 100 ms.
- Tidak ada autoplay, tidak ada carousel, tidak ada skeleton berkedip lebih dari 600 ms.

### 3.5 Komponen yang harus konsisten

Buat sekali di `src/Kasir.Desktop/Controls`, pakai di semua layar:

1. `SearchBox`: ikon cari kiri, tombol clear kanan, hint shortcut. State kosong, fokus, ada teks, dan loading.
2. `BarcodeField`: input dengan indikator mode scan, bunyi visual hijau saat cocok dan merah saat tidak ada. Wajib tahan tempel cepat dari scanner.
3. `QtyStepper`: tombol minus, angka, tombol plus. Nonaktif saat stok 0 dengan alasan tertulis.
4. `MoneyInput`: hanya digit, format otomatis `12.500`, tolak huruf, tempel teks kotor tetap aman.
5. `PayPad`: tombol `Uang Pas`, `10rb`, `20rb`, `50rb`, `100rb`, dan `Hapus`.
6. `StatusBanner`: varian info, sukses, peringatan, bahaya. Selalu ada ikon plus teks plus aksi jika perlu.
7. `EmptyState`: judul konkret, satu kalimat sebab, satu tombol aksi. Contoh: `Belum ada produk. Tambah produk pertama untuk mulai jualan. [Tambah Produk]`.
8. `ConfirmDialog`: judul, dampak, tombol utama dan sekunder. Tombol bahaya selalu di kanan dengan label jelas seperti `Hapus Produk`.
9. `DataGrid` toko: header sticky, kolom angka rata kanan, baris hover netral, baris terpilih dengan border kiri aksen 3 px.
10. `ReceiptPreview`: pratinjau struk apa adanya sebelum cetak. Tidak boleh beda dengan hasil cetak.

## 4. Struktur layar

### 4.1 Shell aplikasi

Lebar navigasi kiri 76 px, mode ikon plus label dua baris. Isi: POS, Produk, Riwayat, Stok, Laporan, Setelan.

Top bar 52 px berisi:

- Kiri: nama `Cashier Exhaust`, chip cabang `TOKO UTAMA`, chip offline `Lokal`.
- Tengah: search universal `Ctrl+K` untuk cari produk atau invoice.
- Kanan: chip shift `Shift Pagi`, nama kasir, jam live, titik printer hijau atau merah.

Status bar bawah 28 px berisi:

- Pesan status terakhir.
- Lokasi DB singkat.
- Versi aplikasi.
- Jumlah item parkir jika ada.

Aturan navigasi:

- Pindah halaman terasa instan. Target di bawah 100 ms untuk yang sudah dimuat.
- Halaman yang belum jadi di fase berjalan tidak boleh jadi tombol mati. Tampilkan halaman rapi dengan status `Segera di Fase X` plus tombol kembali ke POS.
- Fokus keyboard pindah ke judul halaman setiap ganti halaman agar pengguna screen reader dan kasir tidak tersesat.

### 4.2 POS sebagai layar utama

Layout tiga zona:

- Kiri 340 px: `BarcodeField` di paling atas, lalu `SearchBox`, lalu grid produk 2 kolom dengan kartu kecil berisi nama, harga, stok. Stok 0 tampil redup plus label `Habis`.
- Tengah fleksibel: keranjang. Header berisi jumlah item dan tombol `Bersihkan`. Baris berisi nama, harga satuan, stepper, subtotal, hapus. Footer keranjang selalu nempel berisi subtotal, diskon, pajak, dan total besar.
- Kanan 300 px: panel bayar sticky. Isi berurutan: total, metode `Tunai`, `MoneyInput`, `PayPad`, kembalian besar, tombol `Bayar [F5]`, tombol sekunder `Parkir` dan `Bersihkan`.

Alur cepat:

1. Fokus awal di barcode.
2. Scan atau ketik lalu `Enter`.
3. Qty default 1. Scan sama menambah qty.
4. Kasir ketik tunai atau klik nominal.
5. `F5` bayar. Dialog sukses muncul dengan kembalian besar.
6. `Enter` mulai transaksi baru. Fokus kembali ke barcode otomatis.

State yang wajib diuji:

- Produk tidak ketemu: banner merah `Barcode 8999999 tidak ada. Cek fisik barang atau tambah produk.`
- Stok kurang: banner kuning di baris itu plus cegah checkout dengan pesan `Stok Indomie tinggal 2, keranjang minta 3. Kurangi qty atau tambah stok.`
- Uang kurang: tombol bayar aktif tapi hasilkan pesan `Uang kurang Rp 4.500`.
- DB gagal tulis: pesan `Gagal simpan. Transaksi dibatalkan, tidak ada stok yang berubah. Coba lagi.`

### 4.3 Produk

- Tabel dengan kolom: barcode, nama, kategori, harga beli, harga jual, margin, stok, status.
- Margin dihitung otomatis dan berwarna netral. Margin negatif diberi label `Rugi` agar jelas.
- Filter: teks, kategori, status `Semua, Aktif, Nonaktif, Stok menipis, Habis`.
- Form tambah punya validasi inline. Barcode duplikat ditolak dengan tombol `Buka produk lama`.
- Opsi density `Rapat` dan `Nyaman`.

### 4.4 Riwayat

- Filter tanggal cepat: Hari ini, Kemarin, 7 hari, pilih rentang.
- Pencarian invoice `CX-...` dengan hasil di bawah 1 detik untuk 10 ribu transaksi.
- Klik baris membuka detail kanan: items, kasir, shift, bayar, kembalian, tombol cetak ulang dan retur.
- Void hanya untuk hari berjalan dan butuh peran admin plus alasan.

### 4.5 Stok

- Dua aksi utama: `Stok Masuk` dan `Opname`.
- Setiap perubahan mencatat `StockMovement` dengan RefNo. Tidak ada jalan pintas ubah stok langsung.
- Opname menampilkan selisih `Sistem 20, Fisik 18, Selisih -2` sebelum simpan.

### 4.6 Laporan

- Kartu ringkas di atas: omzet, item terjual, rata-rata struk, estimasi margin.
- Grafik hanya dua jenis: garis omzet harian dan batang 10 produk laris. Tanpa 3D, tanpa banyak warna.
- Tabel bisa export CSV dengan nama file `cashier-exhaust-laporan-YYYYMMDD.csv`.

### 4.7 Setelan

- Profil toko, pajak default, pembulatan, printer, backup, info versi.
- Tombol `Test Print` wajib ada dan memberi hasil pass atau fail yang jelas.
- Pengaturan berbahaya seperti hapus data contoh dipisah di zona merah dengan konfirmasi ketik nama toko.

### 4.8 Login dan shift

- Login sederhana: username, password, tombol masuk. Tidak ada link sosial, tidak ada hiasan.
- Setelah login, jika tidak ada shift terbuka, tampilkan dialog `Buka Shift` dengan modal awal. Tanpa ini, tombol bayar terkunci.
- Tutup shift menampilkan hitung tunai vs sistem dan selisih. Selisih wajib disimpan, bukan disembunyikan.

## 5. Arsitektur logic

### 5.1 Uang

- Hitung dengan `decimal`.
- Rumus resmi:
  `Baris = Qty x Harga - DiskonBaris`
  `Subtotal = jumlah Baris`
  `Total = Subtotal - DiskonStruk + Pajak`
- Simpan persen dan nominal untuk diskon dan pajak agar bisa diaudit. Total saja tidak cukup.
- Pembulatan default mati di awal. Opsi `0, 100, 500` ditambah di Fase 7. Selisih bulat dicatat sebagai baris tersendiri di struk jika dipakai.

### 5.2 Stok

- Perubahan stok selalu pasangan: update `Product.Stock` plus satu baris `StockMovement`.
- Penjualan normal tidak boleh membuat stok minus.
- Barang timbang memakai 3 desimal dan satuan `kg` atau `g`. Barang pcs hanya integer.
- Stok menipis berarti `Stock <= MinStock`. Stok habis berarti `Stock <= 0`.

### 5.3 Invoice dan transaksi

- Format: `CX-YYYYMMDD-NNNN`. Nomor urut reset tiap hari.
- Sumber nomor dari tabel `DailySequence(Tanggal, LastNumber)` dengan update atomik. Jangan hitung dari `Count()` karena balapan.
- Checkout memakai satu transaksi DB:
  1. Kunci dan ambil nomor invoice.
  2. Validasi semua stok.
  3. Insert `Sale`.
  4. Insert `SaleItem`.
  5. Update stok plus insert movement.
  6. Commit. Gagal di langkah mana pun berarti rollback penuh.
- SQLite satu penulis. Tambahkan retry singkat untuk error `busy`, lalu tampilkan pesan yang jujur jika masih gagal.

### 5.4 Pengguna dan keamanan

- Hash password dengan PBKDF2 atau BCrypt. Tidak ada plaintext di DB dan log.
- Peran: `Admin` dan `Kasir`. Kasir tidak bisa hapus produk, void, ubah pajak, atau restore backup.
- Kunci login 1 menit setelah 5 gagal. Catat percobaan di log lokal.
- Akun bawaan wajib ganti password saat login pertama.

### 5.5 Data dan migrasi

- Ganti `EnsureCreated` dengan EF Core Migrations maksimal di Fase 8, idealnya mulai Fase 3.
- Index wajib: `Product.Barcode` unik, `Sale.InvoiceNo` unik, `Sale.CreatedAt`, `SaleItem.SaleId`, `StockMovement.ProductId`.
- Backup adalah salin file SQLite saat aplikasi idle plus verifikasi buka DB hasil salinan. Simpan 7 terakhir.

### 5.6 Cetak

- Template teks ESC/POS, bukan screenshot UI.
- Lebar 32 kolom untuk 58 mm, 42 kolom untuk 80 mm.
- Selalu ada mode pratinjau. Tombol cetak nonaktif dengan label `Pilih printer dulu` jika belum ada printer.
- Potong teks dengan aturan jelas, bukan hilang diam-diam. Nama panjang dipotong 2 baris lalu `...`.

## 6. Rencana fase detail

Setiap subfase punya pola sama: `Masuk, Kerja, Bukti selesai`.

### FASE 0 - Rebrand dan perbaikan build

Target: aplikasi bernama Cashier Exhaust dan bisa dibuka tanpa error bind.

0.1 Identitas dasar

- Masuk: judul masih `Kasir.Desktop`, DB masih folder `Kasir`.
- Kerja:
  - Ubah judul window, top bar, dan about menjadi `Cashier Exhaust`.
  - Tambah `AppInfo.cs` berisi nama produk, versi `0.1.0`, prefix invoice `CX-`.
  - Pindah DB ke `CashierExhaust/cashier-exhaust.db`.
  - Siapkan migrasi data lama jika file lama ada: salin otomatis sekali lalu catat di log.
- Bukti: judul benar, DB baru terbuat, aplikasi jalan.

0.2 Perbaiki MainWindow

- Masuk: bind `Greeting` tidak valid.
- Kerja:
  - Ganti isi window dengan layout POS sementara yang hanya bind ke properti yang ada.
  - Hapus model `Models` kosong yang tidak dipakai atau isi dengan TODO yang jelas.
  - Pastikan tidak ada warning binding saat debug.
- Bukti: `dotnet build Kasir.sln` hijau, window terbuka, tidak ada error binding.

0.3 Baseline manual

- Kerja: tulis `docs/run.md` 1 halaman berisi cara run, lokasi DB, akun awal, dan cara reset DB contoh.
- Bukti: orang baru bisa run dalam 5 menit mengikuti dokumen itu.

### FASE 1 - Fondasi visual dan shell

Target: semua layar berikutnya tinggal pakai komponen yang sama.

1.1 Token dan style

- Kerja:
  - Buat `Styles/Tokens.axaml`, `Styles/Text.axaml`, `Styles/Buttons.axaml`, `Styles/Inputs.axaml`, `Styles/Tables.axaml`, `Styles/Dialogs.axaml`.
  - Terapkan warna section 3.1 dan skala section 3.2.
  - Hapus warna hardcode dari Views.
- Bukti: grep warna hex di folder `Views` kosong kecuali contoh yang disengaja.

1.2 Shell navigasi

- Kerja:
  - Buat `ShellWindow`, `ShellViewModel`, dan router sederhana berbasis index halaman.
  - Top bar, nav rail 76 px, content, status bar.
  - Halaman placeholder yang rapi untuk Produk, Riwayat, Stok, Laporan, Setelan.
- Bukti: pindah halaman cepat, judul dan fokus konsisten, tidak ada tombol mati yang membingungkan.

1.3 Komponen ulang

- Kerja: buat 10 komponen section 3.5 dengan state normal, fokus, kosong, error, dan nonaktif.
- Bukti: halaman demo internal `Styleguide` bisa dibuka dari Setelan dan menampilkan semua state.

### FASE 2 - POS inti tunai

Target: toko bisa jualan satu shift penuh hanya dengan fase ini.

2.1 Katalog cepat dan scan

- Kerja:
  - `BarcodeField` dengan debounce 50 ms dan penanganan tempel cepat.
  - Daftar produk virtualized maksimal 100 hasil per pencarian.
  - Pencarian toleran spasi dan kapital.
- Bukti: scan 10 barcode beruntun tanpa mouse, semua masuk, tidak ada yang hilang.

2.2 Keranjang

- Kerja:
  - Tambah, stepper, hapus baris, bersihkan.
  - Peringatan stok live, bukan blokir awal yang memperlambat kasir.
  - Total dan kembalian dihitung live.
- Bukti: 50 baris tetap responsif, tidak ada lag ketik.

2.3 Bayar dan simpan atomik

- Kerja:
  - `MoneyInput`, `PayPad`, validasi bayar.
  - Tabel `DailySequence`, generator invoice `CX-YYYYMMDD-NNNN`.
  - Transaksi DB satu commit dengan rollback penuh.
- Bukti: simulasi stok 0 saat checkout tidak menyisakan `Sale` yatim.

2.4 Dialog sukses dan parkir sederhana

- Kerja:
  - Dialog sukses berisi invoice, total, bayar, kembalian, tombol `Transaksi Baru [Enter]`.
  - Parkir 3 struk aktif dengan label waktu dan total.
- Bukti: 1 transaksi tanpa mouse di bawah 20 detik.

### FASE 3 - Produk dan stok masuk

3.1 Produk CRUD

- Field: barcode unik, nama min 3 huruf, kategori, harga beli, harga jual, stok awal, satuan, min stok, aktif.
- Aturan: barcode unik, harga jual boleh di bawah beli hanya dengan centang `Saya sadar margin negatif` plus catatan.
- Bukti: tambah 100 produk manual tanpa duplikat, semua validasi muncul inline.

3.2 Kategori dan supplier

- CRUD kategori. Supplier minimal nama dan kontak. Relasi produk ke supplier nullable.
- Index dan paging untuk 5 ribu produk.
- Bukti: filter kategori terasa instan.

3.3 Stok masuk dan opname

- Form stok masuk menambah movement `In`.
- Opname membandingkan sistem vs fisik lalu menyimpan selisih sebagai `Adjustment`.
- Bukti: riwayat stok menampilkan RefNo untuk tiap perubahan.

### FASE 4 - Riwayat dan retur

4.1 Riwayat

- Filter tanggal, kasir, teks invoice. Detail kanan berisi items dan pembayaran.
- Bukti: cari invoice di 10 ribu transaksi di bawah 1 detik.

4.2 Retur dan void

- Void hari berjalan butuh admin dan alasan.
- Retur parsial mengembalikan stok dan membuat movement `Return`.
- Data asli tidak dihapus. Koreksi sebagai data baru.
- Bukti: stok kembali tepat setelah retur 2 dari 5 item.

4.3 Cetak ulang

- Buka struk lama lalu pratinjau ulang dengan data yang sama persis.
- Bukti: cetak ulang sama dengan cetak awal kecuali label `SALINAN`.

### FASE 5 - Login, peran, shift

5.1 Auth

- Hash password, wajib ganti password bawaan, kunci setelah 5 gagal.
- Peran `Admin` dan `Kasir` dengan batas aksi yang diuji.
- Bukti: kasir tidak bisa membuka void dan restore.

5.2 Shift

- Dialog buka shift dengan modal awal. Penjualan terkunci jika shift tutup.
- Tutup shift dengan rekap tunai sistem vs fisik plus selisih.
- Bukti: total shift sama dengan jumlah `Sale` pada rentang shift.

5.3 Parkir penuh

- Parkir 5 struk, panggil, hapus kedaluwarsa setelah shift tutup.
- Bukti: parkir lalu panggil menghasilkan total dan item sama.

### FASE 6 - Printer dan laci

6.1 Template struk

- Header toko, invoice, tanggal, kasir, shift, items ringkas, subtotal, diskon, pajak, total, bayar, kembalian, footer.
- Pratinjau 58 dan 80 mm.
- Bukti: 10 item tidak terpotong di 80 mm.

6.2 Koneksi perangkat

- Pilih printer, tombol `Test Print`, status online atau offline.
- Buka laci via perintah printer.
- Bukti: status printer selalu terlihat di top bar.

6.3 Label rak opsional

- Cetak label nama, harga, barcode untuk 1 produk.
- Bukti: hasil scan label kembali ke POS dengan benar.

### FASE 7 - Diskon, pajak, laporan

7.1 Harga lanjutan

- Diskon baris rupiah atau persen. Diskon struk rupiah atau persen. Pajak persen default dari Setelan.
- Simpan sumber persen dan hasil nominal.
- Bukti: struk menampilkan baris diskon dengan jelas, total bisa direkonsiliasi manual.

7.2 Laporan

- Omzet harian, 10 produk laris, margin, stok menipis.
- Export CSV.
- Bukti: laporan 30 hari untuk 20 ribu item di bawah 3 detik di PC kelas Celeron 4 GB.

### FASE 8 - Setelan, backup, migrasi

8.1 Profil toko dan preferensi

- Nama, alamat, telepon, footer struk, pajak default, pembulatan, density, tema terang.
- Bukti: ubah nama toko langsung tampil di pratinjau struk.

8.2 Backup

- Tombol backup dan restore plus auto backup harian 7 file.
- Verifikasi buka DB hasil backup sebelum klaim sukses.
- Bukti: restore file kemarin membuka data kemarin.

8.3 Migrasi resmi

- Buat migration awal, hapus `EnsureCreated` dari jalur produksi.
- Uji upgrade dari DB Fase 2 tanpa hilang data.
- Bukti: DB lama terbuka di build baru tanpa reset.

### FASE 9 - Packaging

9.1 Build rilis

- Windows x64 self contained sebagai `.exe` plus installer.
- Linux x64 sebagai `.deb` yang bisa dipasang normal.
- macOS x64 dan arm64 sebagai `.dmg`.
- Bukti: install bersih di tiap OS lalu jualan offline 3 transaksi.

9.2 Versi dan update

- Nomor versi terlihat di status bar dan halaman Setelan.
- Update manual dulu lewat file. Tanpa update diam-diam yang berisiko saat toko ramai.
- Bukti: user bisa tahu versi dan unduh versi baru dari satu tempat.

9.3 Aset final

- Ikon Cashier Exhaust, hapus logo Avalonia, splash cepat maksimal 800 ms.
- Bukti: tidak ada aset demo tersisa.

### FASE 10 - QA toko dan rilis 1.0

10.1 Beban dan mati paksa

- 5 ribu produk, 20 ribu sale, matikan paksa saat checkout, buka lagi dan pastikan tidak ada transaksi setengah jadi.
- Bukti: integritas DB lolos cek.

10.2 Uji shift nyata

- Satu shift penuh dengan scanner dan printer asli. Catat waktu per transaksi dan keluhan kasir.
- Bukti: minimal 50 transaksi nyata tanpa error data.

10.3 Paket rilis

- Changelog, panduan kasir 1 halaman, panduan backup admin 1 halaman.
- Bukti: orang non teknis bisa install, jualan, backup, dan tutup shift.

## 7. Aturan main tiap subfase

1. Baca dulu file yang mau diubah. Jangan menebak API Avalonia atau EF.
2. Ubah maksimal 3 file besar per langkah. Build tiap langkah.
3. Setiap selesai tampilkan: file diubah, perilaku baru, dan 3 sampai 5 langkah uji manual.
4. Jika ada masalah data seperti transaksi tidak atomik atau stok bisa minus, berhenti dan perbaiki dulu.
5. Dependency baru wajib ada alasan tertulis satu paragraf. Jangan tambah library UI hanya untuk satu tombol.
6. Tidak ada teks demo di UI produksi. Kalau belum jadi, tulis status jujur plus fasenya.

Perintah standar:

```bash
dotnet build Kasir.sln
dotnet run --project src/Kasir.Desktop
dotnet publish src/Kasir.Desktop -c Release -r win-x64 --self-contained
```

## 8. Definisi selesai v1.0

v1.0 berarti:

- Install bersih jalan offline di Windows, buka di Linux dan macOS tanpa error fatal.
- Kasir login, buka shift, jual 50 item campuran, terima tunai, cetak struk, tutup shift.
- Admin tambah produk, opname, lihat laporan, backup, restore.
- Tidak ada password plaintext, tidak ada invoice ganda, tidak ada stok minus dari penjualan.
- Panduan 2 halaman tersedia dan sesuai dengan aplikasi.

## 9. Perintah jalan berikutnya

Balas salah satu:

- `lanjut 0.1` untuk rebrand dan identitas dasar.
- `lanjut 0.2` untuk perbaiki MainWindow rusak.
- `lanjut 0.3` untuk dokumen run 1 halaman.

Satu balasan berarti satu subfase sampai build hijau.
