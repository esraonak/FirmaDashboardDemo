using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Models;
using FirmaDasboardDemo.Enums;

namespace FirmaDasboardDemo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanı tabloları
        public DbSet<Firma> Firmalar { get; set; }
        public DbSet<FirmaCalisani> FirmaCalisanlari { get; set; }
        public DbSet<Bayi> Bayiler { get; set; }
        public DbSet<BayiFirma> BayiFirmalari { get; set; }
        public DbSet<Rol> Roller { get; set; }
        public DbSet<Urun> Urun { get; set; }
        public DbSet<FormulTablosu> FormulTablosu { get; set; }
        public DbSet<Hucre> Hucre { get; set; }
        public DbSet<SuperAdmin> SuperAdminler { get; set; }
        public DbSet<FirmaDegisken> FirmaDegiskenler { get; set; }
        public DbSet<HataKaydi> HataKayitlari { get; set; }
        public DbSet<BayiMesaj> BayiMesajlar { get; set; }
        public BayiMesajTuru MesajTuru { get; set; }
        public DbSet<BayiHesaplamaKaydi> BayiHesaplamaKayitlari { get; set; }
        public DbSet<MesajSatiri> MesajSatirlari { get; set; }
        public DbSet<SuperAdminHataKaydi> SuperAdminHataKayitlari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔗 Bayi-Firma çoktan çoğa ilişki
            modelBuilder.Entity<BayiFirma>()
                .HasKey(bf => new { bf.BayiId, bf.FirmaId });

            modelBuilder.Entity<BayiFirma>()
                .HasOne(bf => bf.Bayi)
                .WithMany(b => b.BayiFirmalari)
                .HasForeignKey(bf => bf.BayiId);

            modelBuilder.Entity<BayiFirma>()
                .HasOne(bf => bf.Firma)
                .WithMany(f => f.BayiFirmalari)
                .HasForeignKey(bf => bf.FirmaId);

            // 🔧 FormülTablosu ilişkileri
            modelBuilder.Entity<FormulTablosu>()
                .HasOne(ft => ft.Urun)
                .WithMany(u => u.FormulTablolari)
                .HasForeignKey(ft => ft.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormulTablosu>()
                .HasOne(ft => ft.Calisan)
                .WithMany()
                .HasForeignKey(ft => ft.CalisanId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔧 BayiMesaj ilişkileri
            modelBuilder.Entity<BayiMesaj>()
                .HasOne(m => m.Bayi)
                .WithMany()
                .HasForeignKey(m => m.BayiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BayiMesaj>()
                .HasOne(m => m.Urun)
                .WithMany()
                .HasForeignKey(m => m.UrunId)
                .IsRequired(false);

            modelBuilder.Entity<BayiMesaj>()
                .HasOne(m => m.Firma)
                .WithMany()
                .HasForeignKey(m => m.FirmaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔧 MesajSatiri ilişkileri
            modelBuilder.Entity<MesajSatiri>()
                .HasOne(ms => ms.BayiMesaj)
                .WithMany(bm => bm.Mesajlar)
                .HasForeignKey(ms => ms.BayiMesajId)
                .OnDelete(DeleteBehavior.Cascade); // BayiMesaj silinince tüm mesajlar silinsin

            modelBuilder.Entity<MesajSatiri>()
                .HasOne(ms => ms.Bayi)
                .WithMany()
                .HasForeignKey(ms => ms.BayiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MesajSatiri>()
                .HasOne(ms => ms.Firma)
                .WithMany()
                .HasForeignKey(ms => ms.FirmaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MesajSatiri>()
                .HasOne(ms => ms.Urun)
                .WithMany()
                .HasForeignKey(ms => ms.UrunId)
                .OnDelete(DeleteBehavior.SetNull); // Ürün silinirse null bırak
        }

    }
}
