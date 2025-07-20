using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FirmaDasboardDemo.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // ❌ Eğer test için sürekli eklemek istiyorsan bu satırı yoruma al
           

            // Firma
            var firma = new Firma
            {
                Ad = "Örnek3 Firma",
                Adres = "İstanbul, Türkiye",
                Il = "İstanbul",
                Ilce = "Kadıköy",
                Email = "info@ornekfirma.com",
                Telefon = "02125556677",
                MaxCalisanSayisi = 10,
                MaxBayiSayisi = 3,
                LisansBitisTarihi = DateTime.Now.AddYears(1),
                AktifMi = true
            };
            context.Firmalar.Add(firma);
            context.SaveChanges(); // firma.Id oluşur

            // Firma Çalışanı
            var calisan = new FirmaCalisani
            {
                AdSoyad = "Ayşe Yılmaz",
                Email = "ayse@ornekfirma.com",
                Sifre = "1234",
                FirmaId = firma.Id,
                AktifMi = true,
                 Telefon = "02126667788"
            };
            context.FirmaCalisanlari.Add(calisan);
            context.SaveChanges(); // calisan.Id oluşur

            // Bayi
            var bayi = new Bayi
            {
                Ad = "Anadolu Bayi",
                Adres = "Ankara, Türkiye", // ✅ NULL olmaması için adres ver
                Il = "Ankara",
                Ilce = "Çankaya",
                Email = "bayi@anadolu.com",
                Telefon = "03124445566",   // ✅ Telefonu da unutma
                Sifre = "1234",
                AktifMi = true
            };
            context.Bayiler.Add(bayi);
            context.SaveChanges(); // bayi.Id oluşur

            // Bayi-Firma ilişkilendirmesi
            var bayiFirma = new BayiFirma
            {
                BayiId = bayi.Id,
                FirmaId = firma.Id
            };
            context.BayiFirmalari.Add(bayiFirma);

            context.SaveChanges();
        }
    }
}
