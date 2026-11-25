using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// CORS yapılandırması (Tablet, Mobil, Web KDS ve POS istemcileri için)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<OrderStateService>();

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Ana sayfa doğrudan Mutfak Ekranına (KDS) yönlendirilsin
app.MapGet("/", () => Results.Redirect("/index.html"));

// Her masaya özel URL rotası: /table/1 -> table.html?table=Masa 1
app.MapGet("/table/{tableNo}", (string tableNo) => Results.Redirect($"/table.html?table=Masa+{tableNo}"));

// SignalR Hub yönlendirmesi
app.MapHub<PosHub>("/hubs/pos");

// ---------------------- ENDPOINTS ----------------------

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    runtime = ".NET 11",
    version = "Modern SambaPOS 3.0",
    compliance = "Türkiye GİB & Yeni Nesil ÖKC Standartları",
    defaultVatRates = new[] { 1, 10, 20 }
}));

// Tüm aktif siparişleri/adisyonları getir
app.MapGet("/api/orders", (OrderStateService service) => Results.Ok(service.GetActiveOrders()));

// Yeni sipariş/adisyon oluştur veya güncelle
app.MapPost("/api/orders", async (OrderModel order, OrderStateService service, IHubContext<PosHub> hub) =>
{
    var created = service.CreateOrUpdateOrder(order);
    await hub.Clients.All.SendAsync("OrderUpdated", created);
    return Results.Created($"/api/orders/{created.Id}", created);
});

// Mutfak Ekranı (KDS): Ürün/Sipariş durumunu güncelle
app.MapPut("/api/orders/{id}/items/{itemId}/status", async (string id, string itemId, UpdateItemStatusRequest req, OrderStateService service, IHubContext<PosHub> hub) =>
{
    var updated = service.UpdateItemStatus(id, itemId, req.Status);
    if (updated == null) return Results.NotFound();

    var item = updated.Items.FirstOrDefault(i => i.Id == itemId);
    var itemName = item?.MenuItemName ?? "Ürün";

    // Hem tüm adisyonu güncelle, hem de özel durum bildirimi yayınla
    await hub.Clients.All.SendAsync("OrderUpdated", updated);
    await hub.Clients.All.SendAsync("ItemStatusChanged", new
    {
        OrderId = updated.Id,
        TableNumber = updated.TableNumber,
        WaiterName = updated.WaiterName,
        ItemName = itemName,
        Status = req.Status, // Hazırlanıyor (Pişiriliyor), Hazır (Pişti/Getiriliyor), TeslimEdildi
        Time = DateTime.Now.ToString("HH:mm:ss")
    });

    return Results.Ok(updated);
});

// Adisyon Hesabı / Ödeme Alma (Nakit, Kredi Kartı, Yemek Çeki / Ticket / Multinet, FAST / Karekod)
app.MapPost("/api/orders/{id}/pay", async (string id, PaymentRequest req, OrderStateService service, IHubContext<PosHub> hub) =>
{
    var result = service.AddPayment(id, req);
    if (result == null) return Results.NotFound();

    await hub.Clients.All.SendAsync("OrderUpdated", result);
    if (result.Status == "Completed")
    {
        await hub.Clients.All.SendAsync("OrderCompleted", id);
    }
    return Results.Ok(result);
});

// Garson PIN Doğrulama & Ciro / Satış Performans Raporu
app.MapGet("/api/waiters/report", (OrderStateService service) =>
{
    var allOrders = service.GetAllOrders();
    var waiters = new[]
    {
        new { Pin = "1234", Name = "Ali Yılmaz", Role = "Kıdemli Garson" },
        new { Pin = "5678", Name = "Mehmet Can", Role = "Garson" },
        new { Pin = "9999", Name = "Fatma Demir", Role = "Garson / Barista" },
        new { Pin = "1111", Name = "Kasa / Kasiyer", Role = "Kasa Personeli" }
    };

    var report = waiters.Select(w =>
    {
        var myOrders = allOrders.Where(o => o.WaiterName.Contains(w.Name) || o.WaiterName.Contains(w.Pin)).ToList();
        var totalSales = myOrders.Sum(o => o.TotalAmount);
        var orderCount = myOrders.Count;
        var itemsSold = myOrders.SelectMany(o => o.Items).Sum(i => i.Quantity);

        return new
        {
            w.Pin,
            w.Name,
            w.Role,
            OrderCount = orderCount,
            ItemsSold = itemsSold,
            TotalRevenue = totalSales,
            FormattedRevenue = totalSales.ToString("N2") + " ₺",
            RecentOrders = myOrders.OrderByDescending(o => o.CreatedAt).Take(5).Select(o => new
            {
                o.TableNumber,
                o.TotalAmount,
                Date = o.CreatedAt.ToString("HH:mm")
            })
        };
    });

    return Results.Ok(report);
});

// GİB E-Adisyon / Mali Fiş JSON Çıktısı (ÖKC / Entegratör Uyumlu)
app.MapGet("/api/orders/{id}/fiscal-receipt", (string id, OrderStateService service) =>
{
    var receipt = service.GenerateFiscalReceipt(id);
    if (receipt == null) return Results.NotFound();
    return Results.Ok(receipt);
});

app.Run();

// ---------------------- TÜRKİYE MEVZUATI & POS MODELLERİ ----------------------

public record UpdateItemStatusRequest(string Status);

public record PaymentRequest(
    decimal Amount, 
    string PaymentType, // Nakit, KrediKarti, YemekCeki_Multinet, YemekCeki_Sodexo, Fast_Karekod
    string Currency = "TRY"
);

public class PaymentRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..6];
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = "Nakit"; // Nakit, Kredi Kartı, Multinet, Sodexo, Ticket, FAST
    public DateTime ProcessedAt { get; set; } = DateTime.Now;
}

public class OrderModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public string FiscalTicketNumber { get; set; } = $"AD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
    
    // Sipariş Türü: Salon (Masa), PaketServis (Telefon), GelAl, Yemeksepeti, GetirYemek, TrendyolYemek, MigrosYemek
    public string OrderChannel { get; set; } = "Salon"; 
    public string OnlineOrderCode { get; set; } = ""; // Örn: YS-849201, GTR-39201
    
    public string TableNumber { get; set; } = "Masa";
    public string WaiterName { get; set; } = "Garson";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Note { get; set; } = "";
    
    // Paket Servis & Müşteri / Kurye Bilgileri
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
    public string CourierName { get; set; } = ""; // Atanan Kurye (Ali Kurye, Şirket Kuryesi vb.)
    
    // İkram & İndirim Yönetimi (Türkiye Restoran Standartları)
    public decimal DiscountAmount { get; set; } = 0m; // İndirim Tutarı (TL)
    public string DiscountReason { get; set; } = ""; // Personel indirimi, Sadakat, Müessese ikramı vb.
    public decimal ServiceCharge { get; set; } = 0m; // Kuver / Servis Ücreti

    public string CustomerTaxNumberOrTckn { get; set; } = ""; // TCKN veya VKN (Fatura/E-Adisyon için)
    public string CustomerTitle { get; set; } = ""; // Müşteri / Firma Adı
    public List<OrderItemModel> Items { get; set; } = new();
    public List<PaymentRecord> Payments { get; set; } = new();

    // Mali Toplamlar ve KDV Hesaplamaları (Türkiye 2024+ Mevzuatı: %1, %10, %20)
    public decimal RawTotal => Items != null ? Items.Sum(i => i.IsGift ? 0 : i.Price * i.Quantity) : 0m;
    public decimal TotalAmount => Math.Max(0, RawTotal + ServiceCharge - DiscountAmount);
    public decimal TotalPaid => Payments != null ? Payments.Sum(p => p.Amount) : 0m;
    public decimal RemainingBalance => Math.Max(0, TotalAmount - TotalPaid);

    // KDV Dağılım Matrahları
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, decimal> VatBreakdown
    {
        get
        {
            var dict = new Dictionary<string, decimal>
            {
                { "%1", 0m },
                { "%10", 0m },
                { "%20", 0m }
            };

            foreach (var item in Items.Where(i => !i.IsGift))
            {
                var rateStr = $"%{item.VatRate}";
                var itemTotal = item.Price * item.Quantity;
                var vatAmount = itemTotal - (itemTotal / (1 + (item.VatRate / 100m)));
                if (!dict.ContainsKey(rateStr)) dict[rateStr] = 0;
                dict[rateStr] += Math.Round(vatAmount, 2);
            }
            return dict;
        }
    }

    public decimal TotalVat => VatBreakdown.Values.Sum();
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
}

public class OrderItemModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..6];
    public string MenuItemName { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public int VatRate { get; set; } = 10;
    public string Portion { get; set; } = "Normal";
    public List<string> Modifiers { get; set; } = new();
    public string OrderNote { get; set; } = "";
    public bool IsGift { get; set; } = false; // İkram Ürün (0 TL)
    public string Status { get; set; } = "Bekliyor"; // Bekliyor, Hazırlanıyor, Hazır, Yolda, TeslimEdildi
    public DateTime AddedAt { get; set; } = DateTime.Now;
}

public class OrderStateService
{
    private readonly ConcurrentDictionary<string, OrderModel> _orders = new();

    public OrderStateService()
    {
        // Türkiye Standartlarında Hazır Örnek Adisyon (KDV %10 ve %20 ayrılmış)
        var sampleOrder = new OrderModel
        {
            TableNumber = "Masa 4",
            WaiterName = "Ahmet",
            CustomerTitle = "Bireysel Müşteri",
            Note = "Acil - Mutfak uyarısı",
            Items = new List<OrderItemModel>
            {
                new() { MenuItemName = "Urfa Kebap Porsiyon", Quantity = 2, Price = 340, VatRate = 10, Modifiers = new() { "Acısız", "Bol Lavaş" } },
                new() { MenuItemName = "Fındık Lahmacun", Quantity = 3, Price = 75, VatRate = 10, Status = "Hazırlanıyor" },
                new() { MenuItemName = "Kutu Meşrubat", Quantity = 2, Price = 50, VatRate = 10, Status = "Hazır" },
                new() { MenuItemName = "Kadeh Kırmızı Şarap (Alkollü)", Quantity = 1, Price = 280, VatRate = 20, Modifiers = new() { "Buzsuz" } }
            }
        };
        _orders[sampleOrder.Id] = sampleOrder;
    }

    public IEnumerable<OrderModel> GetActiveOrders() => _orders.Values.Where(o => o.Status == "Active").OrderBy(o => o.CreatedAt);
    public IEnumerable<OrderModel> GetAllOrders() => _orders.Values;

    public OrderModel CreateOrUpdateOrder(OrderModel order)
    {
        if (string.IsNullOrEmpty(order.Id)) order.Id = Guid.NewGuid().ToString("N")[..8].ToUpper();
        if (string.IsNullOrEmpty(order.FiscalTicketNumber)) order.FiscalTicketNumber = $"AD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        foreach (var item in order.Items.Where(i => string.IsNullOrEmpty(i.Id)))
        {
            item.Id = Guid.NewGuid().ToString("N")[..6];
            // Kategoriye göre KDV varsayılanı (0 girilmişse %10 restoran standardı)
            if (item.VatRate == 0) item.VatRate = 10;
        }
        _orders[order.Id] = order;
        return order;
    }

    public OrderModel UpdateItemStatus(string orderId, string itemId, string status)
    {
        if (!_orders.TryGetValue(orderId, out var order)) return null;
        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            item.Status = status;
        }
        return order;
    }

    public OrderModel AddPayment(string orderId, PaymentRequest req)
    {
        if (!_orders.TryGetValue(orderId, out var order)) return null;

        order.Payments.Add(new PaymentRecord
        {
            Amount = req.Amount,
            PaymentType = req.PaymentType,
            ProcessedAt = DateTime.Now
        });

        if (order.RemainingBalance <= 0)
        {
            order.Status = "Completed";
        }

        return order;
    }

    public object GenerateFiscalReceipt(string orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order)) return null;

        return new
        {
            GibIntegrationType = "E-Adisyon / Yeni Nesil ÖKC",
            DocumentNo = order.FiscalTicketNumber,
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Table = order.TableNumber,
            Waiter = order.WaiterName,
            CustomerInfo = new
            {
                Title = string.IsNullOrWhiteSpace(order.CustomerTitle) ? "Nihai Tüketici" : order.CustomerTitle,
                TaxId = string.IsNullOrWhiteSpace(order.CustomerTaxNumberOrTckn) ? "11111111111" : order.CustomerTaxNumberOrTckn
            },
            Items = order.Items.Select(i => new
            {
                Name = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.Price,
                Total = i.Price * i.Quantity,
                VatRate = $"%{i.VatRate}"
            }),
            SubTotalWithoutVat = order.TotalAmount - order.TotalVat,
            VatBreakdown = order.VatBreakdown,
            TotalVat = order.TotalVat,
            GrandTotal = order.TotalAmount,
            Currency = "TRY (₺)",
            Payments = order.Payments.Select(p => new
            {
                p.PaymentType,
                p.Amount,
                p.ProcessedAt
            })
        };
    }
}

public class PosHub : Hub
{
    public async Task BroadcastMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
