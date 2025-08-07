using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Services;
using FirmaDasboardDemo.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Ortam yapılandırmaları (appsettings.json, environment.json, user-secrets)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables(); // Azure vs. için uygundur

// 📧 MailHelper yapılandırması
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
    if (!context.Request.Path.StartsWithSegments("/Error"))
    {
        context.Session.SetString("SonURL", context.Request.Path);
    }
    await next();
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
