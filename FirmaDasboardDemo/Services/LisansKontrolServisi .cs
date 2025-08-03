using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirmaDasboardDemo.Data;

namespace FirmaDasboardDemo.Services
{
    public class LisansKontrolServisi : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LisansKontrolServisi(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var bugun = DateTime.Today;

                    // Lisansı bitmiş ve hâlâ aktif görünen firmaları al
                    var bitenFirmalar = db.Firmalar
                        .Where(f => f.AktifMi && f.LisansBitisTarihi < bugun)
                        .ToList();

                    foreach (var firma in bitenFirmalar)
                    {
                        firma.AktifMi = false;

                        // Firma çalışanlarını pasif yap
                        var calisanlar = db.FirmaCalisanlari
                            .Where(c => c.FirmaId == firma.Id)
                            .ToList();

                        foreach (var calisan in calisanlar)
                        {
                            calisan.AktifMi = false;
                        }

                        // Bayileri pasif yap
                        var bayiler = db.Bayiler
                            .Where(b => b.BayiFirmalari.Any(bf => bf.FirmaId == firma.Id))
                            .ToList();

                        foreach (var bayi in bayiler)
                        {
                            bayi.AktifMi = false;
                        }
                    }

                    if (bitenFirmalar.Any())
                    {
                        await db.SaveChangesAsync();
                    }
                }

                // 6saat sonra tekrar kontrol
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
