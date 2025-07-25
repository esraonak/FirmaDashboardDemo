using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data; // DbContext sınıfının namespace’i

var builder = WebApplication.CreateBuilder(args);

// 🔗 Veritabanı bağlantısı — appsettings.json içindeki DefaultConnection alınır
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // Session için gerekli
builder.Services.AddSession();
var app = builder.Build();

// Hata yönetimi ve HTTPS yönlendirme
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Giriş kontrolü varsa
app.UseAuthorization();

// Başlangıç route

// 1️⃣ Sabit (öncelikli) yollar
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Login}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "bayi",
    pattern: "Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });

app.MapControllerRoute(
    name: "calisan",
    pattern: "Calisan/{action=Login}/{id?}",
    defaults: new { controller = "Calisan" });


// 2️⃣ FirmaSeoUrl ile ÇALIŞAN paneli için
app.MapControllerRoute(
    name: "firmaAdmin",
    pattern: "{firmaSeoUrl}/Admin/{action=Login}",
    defaults: new { controller = "Calisan" });


// 3️⃣ FirmaSeoUrl ile BAYİ login + dashboard için
app.MapControllerRoute(
    name: "firmaBayiDashboard",
    pattern: "{firmaSeoUrl}/Dashboard",
    defaults: new { controller = "BayiSayfasi", action = "Dashboard" });

app.MapControllerRoute(
    name: "firmaBayiLogin",
    pattern: "{firmaSeoUrl}",
    defaults: new { controller = "BayiSayfasi", action = "Login" });


// 4️⃣ Fallback (gerekirse)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "firmaBayiRoutes",
    pattern: "{firmaSeoUrl}/Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });
app.MapControllerRoute(
    name: "firmaCalisan",
    pattern: "{firmaSeoUrl}/Calisan/{action=Calisanlar}/{id?}",
    defaults: new { controller = "Calisan" });
app.MapControllerRoute(
    name: "firmaTablo",
    pattern: "{firmaSeoUrl}/Tablo/{action=TabloOlustur}/{id?}",
    defaults: new { controller = "Tablo" });

// ✅ Seed verilerini ekle

/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}*/

app.Run();
