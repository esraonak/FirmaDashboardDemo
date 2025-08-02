using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data; // DbContext sınıfının namespace’i
using FirmaDasboardDemo.Services;
var builder = WebApplication.CreateBuilder(args);

// 🔗 Veritabanı bağlantısı — appsettings.json içindeki DefaultConnection alınır
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // Session için gerekli
builder.Services.AddSession();
builder.Services.AddHostedService<LisansKontrolServisi>();

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
// ✅ 1. Ana Sayfa yönlendirmesi → SuperAdmin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SuperAdmin}/{action=Login}/{id?}");

// ✅ 2. Sabit yollar (standart admin/çalışan/bayi)
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

// ✅ 3. SEO URL'li yapılar (firma bazlı girişler)

// ÇALIŞAN paneli için SEO url
app.MapControllerRoute(
    name: "firmaAdmin",
    pattern: "{firmaSeoUrl}/Admin/{action=Login}",
    defaults: new { controller = "Calisan" });

// BAYİ dashboard
app.MapControllerRoute(
    name: "firmaBayiDashboard",
    pattern: "{firmaSeoUrl}/Dashboard",
    defaults: new { controller = "BayiSayfasi", action = "Dashboard" });

// BAYİ Login (ana giriş)
app.MapControllerRoute(
    name: "firmaBayiRoutes",
    pattern: "{firmaSeoUrl}/Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });

// CALISAN işlemleri
app.MapControllerRoute(
    name: "firmaCalisan",
    pattern: "{firmaSeoUrl}/Calisan/{action=Calisanlar}/{id?}",
    defaults: new { controller = "Calisan" });

// FORMÜL işlemleri
app.MapControllerRoute(
    name: "firmaTablo",
    pattern: "{firmaSeoUrl}/Tablo/{action=TabloOlustur}/{id?}",
    defaults: new { controller = "Tablo" });

// ✅ 4. En SONA AL: SEO url'li bayi login (boş URL'yi engellemesin)
app.MapControllerRoute(
        name: "firmaBayiLogin",
    pattern: "{firmaSeoUrl:regex(^((?!SuperAdmin|Bayi|Calisan).)*$)}",
    defaults: new { controller = "BayiSayfasi", action = "Login" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Site}/{action=Index}/{id?}");

// ✅ Seed verilerini ekle

/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}*/

app.Run();
