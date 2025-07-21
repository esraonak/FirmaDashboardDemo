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

            // Rolleri ekle
            var rolCalisan = new Rol { Ad = "Calisan" };
            var rolBayi = new Rol { Ad = "Bayi" };
            context.Roller.AddRange(rolCalisan, rolBayi);
            context.SaveChanges();

            // Firma ekle
            var firma = new Firma
            {
                Ad = "Firmamıd1",
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
            context.SaveChanges();

            // Firma Çalışanı ekle
            var calisan = new FirmaCalisani
            {
                AdSoyad = "calısanıd1",
                Email = "calısan@ornekfirma.com",
                Sifre = "1234",
                Telefon = "02126667788",
                FirmaId = firma.Id,
                AktifMi = true,
                RolId = rolCalisan.Id
            };
            context.FirmaCalisanlari.Add(calisan);
            context.SaveChanges();

            // Bayi ekle
            var bayi = new Bayi
            {
                Ad = "Avrupa Bayiıd1",
                Adres = "istanbul, Türkiye",
                Il = "istanbul",
                Ilce = "cekmekoy",
                Email = "bayi@avrupa.com",
                Telefon = "03124445566",
                Sifre = "1234",
                AktifMi = true,
                RolId = rolBayi.Id
            };
            context.Bayiler.Add(bayi);
            context.SaveChanges();

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
