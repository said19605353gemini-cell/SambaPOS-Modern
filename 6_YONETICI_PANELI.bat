@echo off
title SambaPOS - Restoran Yonetici ve Ayarlar Paneli
chcp 65001 > nul
cd /d "%~dp0"
cls
echo ========================================================
echo        SAMBAPOS YONETICI VE AYARLAR PANELI
echo ========================================================
echo.
echo Adres: http://localhost:5000/admin.html
echo.
echo * Personel Ekle / Duzenle / Sil
echo * Garson, Kasiyer ve Asci PIN Kodlari & Yetkileri
echo * Toplam Salon Masa Sayisi Ayari
echo * Yemek Menusu, Kategoriler, Fiyat ve KDV Yonetimi
echo.
echo Varsayilan Mudur PIN: 0000
echo.
start "" "http://localhost:5000/admin.html"
exit
