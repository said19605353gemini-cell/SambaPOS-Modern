@echo off
title SambaPOS Ana Sunucu (.NET 11)
chcp 65001 > nul
cd /d "%~dp0"
cls
echo ========================================================
echo        SAMBAPOS ANA SUNUCUSU (.NET 11)
echo ========================================================
echo.
echo Bu pencere sunucudur. 
echo Sistem calisirken bu pencereyi KAPATMAYINIZ!
echo.

:: Port 5000 mesgulse temizle
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do taskkill /F /PID %%a > nul 2>&1

echo Sunucu dinleniyor: http://localhost:5000
echo.
Samba.ModernServer.exe --urls "http://0.0.0.0:5000"
pause
