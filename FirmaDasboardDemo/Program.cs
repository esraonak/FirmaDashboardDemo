using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Services;
using FirmaDasboardDemo.Helpers;
using FirmaDasboardDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Config zinciri (son eklenen, öncekileri ezer)
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    // Plesk’te env ayarlayamasan da bunu özellikle yükle:
    .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    // Local’de secrets prod değerlerini ezsin
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// İstersen dursun; prod’da etkisi yok çünkü env set edemiyorsun
builder.Configuration.AddEnvironmentVariables();

// (Varsa) senin mevcut çağrın:
MailHelper.Init(builder.Configuration);

// 🔗 Veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔄 MVC + Session
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TenteCRM.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🧠 Zamanlayıcı servis (Lisans kontrolü)
builder.Services.AddHostedService<LisansKontrolServisi>();

var app = builder.Build();


// 🌐 Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
    // app.UseDeveloperExceptionPage(); // Açmak istersen
}

// 🔧 Middleware zinciri
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// 🌐 Son ziyaret edilen URL'yi Session’a kaydet
app.Use(async (context, next) =>
{
    // Son ziyaret edilen URL'yi (query string ile birlikte) tut
    if (!context.Request.Path.StartsWithSegments("/Error"))
    {
        var lastUrl = context.Request.Path + context.Request.QueryString;
        context.Session.SetString("SonURL", lastUrl);
    }

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Hata olduğunda SuperAdminHataKayitlari'na yaz
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
            {
                KullaniciRol = context.Session.GetString("UserRole") ?? "Bilinmiyor",
                KullaniciAdi = context.Session.GetString("UserAd") ?? "Anonim",
                FirmaSeo = context.Session.GetString("FirmaSeoUrl") ?? "belirsiz",
                Url = context.Session.GetString("SonURL") ?? context.Request.Path,
                Tarih = DateTime.Now,
                HataMesaji = ex.Message,
                StackTrace = ex.ToString()
            });
            db.SaveChanges();
        }
        catch
        {
            // DB'ye yazılamazsa uygulamayı düşürmeyelim
        }

        // Mevcut hata yönetimi (UseExceptionHandler) çalışsın
        throw;
    }
});

app.UseAuthentication();
app.UseAuthorization();


// 🔁 Route Tanımları
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SuperAdmin}/{action=Login}/{id?}");

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

app.MapControllerRoute(
    name: "firmaAdmin",
    pattern: "{firmaSeoUrl}/Admin/{action=Login}",
    defaults: new { controller = "Calisan" });

app.MapControllerRoute(
    name: "firmaCalisan",
    pattern: "{firmaSeoUrl}/Calisan/{action=Calisanlar}/{id?}",
    defaults: new { controller = "Calisan" });

app.MapControllerRoute(
    name: "firmaTablo",
    pattern: "{firmaSeoUrl}/Tablo/{action=TabloOlustur}/{id?}",
    defaults: new { controller = "Tablo" });

app.MapControllerRoute(
    name: "firmaBayiLogin",
    pattern: "{firmaSeoUrl:regex(^((?!SuperAdmin|Bayi|Calisan).)*$)}",
    defaults: new { controller = "BayiSayfasi", action = "Login" });

app.MapControllerRoute(
    name: "firmaBayiDashboard",
    pattern: "{firmaSeoUrl}/Dashboard",
    defaults: new { controller = "BayiSayfasi", action = "Dashboard" });

app.MapControllerRoute(
    name: "firmaBayiRoutes",
    pattern: "{firmaSeoUrl}/Bayi/{action=Login}/{id?}",
    defaults: new { controller = "Bayi" });

app.MapControllerRoute(
    name: "firmaBayiSayfasi",
    pattern: "{firmaSeoUrl}/Bayi/{action}/{id?}",
    defaults: new { controller = "BayiSayfasi" });

app.MapControllerRoute(
    name: "site",
    pattern: "{controller=Site}/{action=Index}/{id?}");

// ✅ Uygulamayı çalıştır
app.Run();
