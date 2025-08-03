using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔗 Veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC ve session ayarları
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

// 🔐 Session çakışmasını engellemek için session cookie adını tanımla
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TenteCRM.Session"; // Aynı tarayıcıda karışmaması için özelleştirilmiş ad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHostedService<LisansKontrolServisi>();

var app = builder.Build();

// 🔧 Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session, authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 🧠 Route yapılandırmaları

// 1️⃣ Ana yönlendirme → SuperAdmin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SuperAdmin}/{action=Login}/{id?}");

// 2️⃣ Sabit yollar
app.MapControllerRoute(
    name: "superadmin",
    pattern: "SuperAdmin/{action=Login}/{id?}",
    defaults: new { controller = "SuperAdmin" });

app.MapControllerRoute(
    name: "bayi",
    pattern: "Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });

app.MapControllerRoute(
    name: "calisan",
    pattern: "Calisan/{action=Login}/{id?}",
    defaults: new { controller = "Calisan" });

// 3️⃣ SEO URL ile çalışan paneli
app.MapControllerRoute(
    name: "firmaAdmin",
    pattern: "{firmaSeoUrl}/Admin/{action=Login}",
    defaults: new { controller = "Calisan" });

// 4️⃣ Bayi SEO URL → Dashboard
app.MapControllerRoute(
    name: "firmaBayiDashboard",
    pattern: "{firmaSeoUrl}/Dashboard",
    defaults: new { controller = "BayiSayfasi", action = "Dashboard" });

// 5️⃣ Bayi SEO URL login
app.MapControllerRoute(
    name: "firmaBayiRoutes",
    pattern: "{firmaSeoUrl}/Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });

// 6️⃣ Firma Çalışan işlemleri
app.MapControllerRoute(
    name: "firmaCalisan",
    pattern: "{firmaSeoUrl}/Calisan/{action=Calisanlar}/{id?}",
    defaults: new { controller = "Calisan" });

// 7️⃣ Formül işlemleri
app.MapControllerRoute(
    name: "firmaTablo",
    pattern: "{firmaSeoUrl}/Tablo/{action=TabloOlustur}/{id?}",
    defaults: new { controller = "Tablo" });

// 8️⃣ En son: SEO URL'li bayi login fallback
app.MapControllerRoute(
    name: "firmaBayiLogin",
    pattern: "{firmaSeoUrl:regex(^((?!SuperAdmin|Bayi|Calisan).)*$)}",
    defaults: new { controller = "BayiSayfasi", action = "Login" });

// 9️⃣ Ana sayfa (public)
app.MapControllerRoute(
    name: "site",
    pattern: "{controller=Site}/{action=Index}/{id?}");

app.Run();
