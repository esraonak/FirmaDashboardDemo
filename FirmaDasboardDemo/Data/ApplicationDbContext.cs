using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Models;

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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bayi-Firma çoktan çoğa ilişki
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
            // 🔧 FORMULTABLOSU için ekle
            modelBuilder.Entity<FormulTablosu>()
                .HasOne(ft => ft.Urun)
                .WithMany(u => u.FormulTablolari)
                .HasForeignKey(ft => ft.UrunId)
                .OnDelete(DeleteBehavior.Restrict); // ❗ burası hatayı çözer

            modelBuilder.Entity<FormulTablosu>()
                .HasOne(ft => ft.Calisan)
                .WithMany()
                .HasForeignKey(ft => ft.CalisanId)
                .OnDelete(DeleteBehavior.Restrict); // ❗
        }
    }
}
