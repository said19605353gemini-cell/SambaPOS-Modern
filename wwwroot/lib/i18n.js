// SambaPOS Modern - Offline Multi-Language (i18n) Engine (TR / EN / DE / AR)
const SAMBA_I18N = {
    tr: {
        lang_name: "🇹🇷 TR",
        pos_title: "SAMBAPOS KASA TERMİNALİ",
        kds_title: "MUTFAK EKRANI (KDS)",
        waiter_title: "GARSON EL TERMİNALİ",
        admin_title: "SAMBAPOS RESTORAN YÖNETİM MERKEZİ",
        table_title: "MASA SİPARİŞ EKRANI",
        save_all: "Tüm Ayarları Kaydet",
        lock: "Kilitle",
        logout: "Çıkış",
        pay: "Ödeme Al",
        send_kitchen: "Mutfağa Gönder",
        clear_cart: "Sepeti Temizle",
        total: "TOPLAM",
        subtotal: "Ara Toplam",
        vat: "KDV",
        discount: "İndirim",
        self_service: "Self-Servis Alacağım",
        pin_enter: "PIN Kodunu Giriniz",
        login: "GİRİŞ",
        delete: "SİL",
        ready: "Hazır",
        preparing: "Hazırla",
        waiting: "Bekliyor",
        tab_staff: "Personel & PIN",
        tab_zreport: "Gün Sonu & Z-Raporu",
        tab_tables: "Masalar & Salonlar",
        tab_menu: "Menü & Fiyatlar",
        tab_stock: "Stok & Kritik Seviyeler",
        tab_options: "Ürün Seçenekleri",
        tab_printers: "Yazıcılar",
        tab_online: "Paket Servis",
        tab_gib: "GİB / E-Adisyon"
    },
    en: {
        lang_name: "🇬🇧 EN",
        pos_title: "SAMBAPOS CASHIER POS",
        kds_title: "KITCHEN DISPLAY (KDS)",
        waiter_title: "WAITER TERMINAL",
        admin_title: "SAMBAPOS ADMIN CENTER",
        table_title: "TABLE SELF-ORDERING",
        save_all: "Save All Settings",
        lock: "Lock",
        logout: "Logout",
        pay: "Checkout / Pay",
        send_kitchen: "Send to Kitchen",
        clear_cart: "Clear Cart",
        total: "TOTAL",
        subtotal: "Subtotal",
        vat: "VAT",
        discount: "Discount",
        self_service: "Self-Service Pickup",
        pin_enter: "Enter PIN Code",
        login: "LOGIN",
        delete: "DEL",
        ready: "Ready",
        preparing: "Prepare",
        waiting: "Waiting",
        tab_staff: "Staff & PIN",
        tab_zreport: "End of Day & Z-Report",
        tab_tables: "Tables & Halls",
        tab_menu: "Menu & Prices",
        tab_stock: "Stock & Inventory",
        tab_options: "Product Modifiers",
        tab_printers: "Printers",
        tab_online: "Online Delivery",
        tab_gib: "Tax / E-Receipt"
    },
    de: {
        lang_name: "🇩🇪 DE",
        pos_title: "SAMBAPOS KASSENSYSTEM",
        kds_title: "KÜCHENDISPLAY (KDS)",
        waiter_title: "KELLNER-TERMINAL",
        admin_title: "SAMBAPOS VERWALTUNG",
        table_title: "TISCH-BESTELLUNG",
        save_all: "Alle Speichern",
        lock: "Sperren",
        logout: "Abmelden",
        pay: "Bezahlen",
        send_kitchen: "An Küche senden",
        clear_cart: "Warenkorb leeren",
        total: "GESAMT",
        subtotal: "Zwischensumme",
        vat: "MwSt.",
        discount: "Rabatt",
        self_service: "Selbstbedienung",
        pin_enter: "PIN-Code eingeben",
        login: "ANMELDEN",
        delete: "LÖSCHEN",
        ready: "Fertig",
        preparing: "Zubereiten",
        waiting: "Wartend",
        tab_staff: "Personal & PIN",
        tab_zreport: "Tagesabschluss (Z)",
        tab_tables: "Tische & Bereiche",
        tab_menu: "Speisekarte & Preise",
        tab_stock: "Lagerbestand",
        tab_options: "Produktoptionen",
        tab_printers: "Drucker",
        tab_online: "Lieferdienste",
        tab_gib: "Steuer / Beleg"
    },
    ar: {
        lang_name: "🇸🇦 AR",
        pos_title: "نقطة البيع - الكاشير",
        kds_title: "شاشة المطبخ (KDS)",
        waiter_title: "جهاز النادل",
        admin_title: "لوحة تحكم الإدارة",
        table_title: "طلب الطاولة الذاتي",
        save_all: "حفظ جميع الإعدادات",
        lock: "قفل",
        logout: "خروج",
        pay: "الدفع",
        send_kitchen: "إرسال للمطبخ",
        clear_cart: "إفراغ السلة",
        total: "الإجمالي",
        subtotal: "المجموع الفرعي",
        vat: "الضريبة",
        discount: "الخصم",
        self_service: "خدمة ذاتية",
        pin_enter: "أدخل رمز PIN",
        login: "دخول",
        delete: "مسح",
        ready: "جاهز",
        preparing: "تحضير",
        waiting: "قيد الانتظار",
        tab_staff: "الموظفين و PIN",
        tab_zreport: "تقرير نهاية اليوم",
        tab_tables: "الطاولات والصالات",
        tab_menu: "القائمة والأسعار",
        tab_stock: "المخزون",
        tab_options: "خيارات المنتجات",
        tab_printers: "الطابعات",
        tab_online: "توصيل الطلبات",
        tab_gib: "الضرائب والفواتير"
    }
};

function getSambaLang() {
    return localStorage.getItem("samba_lang") || "tr";
}

function setSambaLang(lang) {
    if (!SAMBA_I18N[lang]) return;
    localStorage.setItem("samba_lang", lang);
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === "ar" ? "rtl" : "ltr";
    applySambaI18n();
    renderLangSwitcher();
}

function t(key) {
    const lang = getSambaLang();
    return (SAMBA_I18N[lang] && SAMBA_I18N[lang][key]) || SAMBA_I18N.tr[key] || key;
}

function applySambaI18n() {
    document.querySelectorAll("[data-i18n]").forEach(el => {
        const key = el.getAttribute("data-i18n");
        if (key) el.textContent = t(key);
    });
}

function renderLangSwitcher() {
    let container = document.getElementById("samba-lang-switcher");
    if (!container) {
        container = document.createElement("div");
        container.id = "samba-lang-switcher";
        container.className = "fixed bottom-3 left-3 z-50 flex items-center gap-1 bg-slate-900/90 backdrop-blur border border-slate-700/80 p-1 rounded-xl shadow-xl";
        document.body.appendChild(container);
    }
    const current = getSambaLang();
    container.innerHTML = Object.keys(SAMBA_I18N).map(code => `
        <button onclick="setSambaLang('${code}')" class="px-2 py-1 rounded-lg text-[10px] font-black transition ${current === code ? 'bg-amber-500 text-slate-950 shadow' : 'text-slate-300 hover:bg-slate-800'}">
            ${SAMBA_I18N[code].lang_name}
        </button>
    `).join('');
}

document.addEventListener("DOMContentLoaded", () => {
    const lang = getSambaLang();
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === "ar" ? "rtl" : "ltr";
    applySambaI18n();
    renderLangSwitcher();
});
