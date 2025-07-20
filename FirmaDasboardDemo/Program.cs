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

// En son fallback/default route (isteğe bağlı)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");
// ✅ Seed verilerini ekle

/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}*/

app.Run();
