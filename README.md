# SambaPOS Modern (.NET 11)

> **Not:** Bu proje **SambaPOS V3** altyapısı (`Samba.Domain`, `Samba.Infrastructure`) temel alınarak **Google Antigravity (Gemini)** ile modernize edilip geliştirilmiştir.

Türkiye restoran ve kafe işletmeleri için modernize edilmiş, çoklu cihaz destekli, çevrimdışı (offline-first) çalışabilen yeni nesil POS ve Adisyon yönetim sistemi.

## 🚀 Özellikler

- **Gerçek Zamanlı Senkronizasyon:** ASP.NET Core (.NET 11) Minimal API + SignalR ile Kasa, Mutfak, Garson ve Masa tabletleri anlık haberleşir.
- **Türkiye POS & Maliye Uyumluluğu:** %1 / %10 / %20 KDV matrah ayrımı, E-Adisyon uyumlu fiş yapısı, Nakit / Kredi Kartı / Yemek Kartı (Multinet, Sodexo) / FAST QR tahsilatları, Alman usulü hesap bölme, masa taşıma ve birleştirme.
- **Çoklu Ekran Desteği:**
  - **Kasa Terminali (`pos.html`):** Masa yönetimi, hızlı ödeme, ikram, indirim, KDV özeti.
  - **Mutfak Ekranı / KDS (`index.html`):** Sesli bildirimler, geciken sipariş uyarısı, hazırlık durumu takibi.
  - **Garson El Terminali (`garson.html`):** Kişisel PIN ile giriş, masa siparişi alma, garson ciro/performans raporu.
  - **Müşteri Masa Tableti (`table.html`):** Masaya özel QR/URL ile self-servis sipariş ve "Self-Servis Alacağım" indirimi.
  - **Yönetici Paneli (`admin.html`):** Personel & PIN yönetimi, dinamik salon/masa sayısı, menü/kategori/fiyat/KDV düzenleme, ürün seçenekleri (porsiyon/sos), stok takibi, yazıcılar, paket servis komisyonları ve Z-Raporu.

## 🔐 Varsayılan PIN Kodları

- **Yönetici Paneli:** `0000`
- **Kasa Ekranı:** `1111` (veya `0000`)
- **Mutfak Ekranı (KDS):** `2222`, `3333`
- **Garson Terminali:** `1234`, `5678`, `9999`

## ▶️ Çalıştırma

1. **`1_SUNUCUYU_BASLAT.bat`** dosyasını çalıştırarak yerel sunucuyu (`http://localhost:5000`) başlatın.
2. Kullanmak istediğiniz ekranın `.bat` dosyasını açın:
   - `2_KASA_EKRANI.bat`
   - `3_MUTFAK_EKRANI.bat`
   - `4_GARSON_EKRANI.bat`
   - `5_MUSTERI_MASA_TABLETI.bat`
   - `6_YONETICI_PANELI.bat`

## 🛠️ Derleme (Build)

GitHub Actions (`.github/workflows/build.yml`) üzerinden `.NET 11` ile `win-x64` self-contained tek dosya (`Samba.ModernServer.exe`) otomatik olarak derlenir.
