using Microsoft.EntityFrameworkCore;
using Sportify.Models;

namespace Sportify.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Takim> Takimlar { get; set; }
        public DbSet<Oyuncu> Oyuncular { get; set; }
        public DbSet<PuanDurumu> PuanDurumlari { get; set; }
        public DbSet<Lig> Ligler { get; set; }
        public DbSet<Mac> Maclar { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<EpostaOnay> EpostaOnaylar { get; set; }
        public DbSet<KullaniciTakim> KullaniciTakimlar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure complex types/owned types
            modelBuilder.Entity<Takim>().OwnsOne(t => t.Istatistikler);
            modelBuilder.Entity<Oyuncu>().OwnsOne(p => p.Istatistikler);
            modelBuilder.Entity<PuanDurumu>().HasKey(s => new { s.TakimId });

            // Kullanici: Email benzersiz olmalı
            modelBuilder.Entity<Kullanici>()
                .HasIndex(k => k.Email)
                .IsUnique();

            // Kullanici - Takim (Favoriler) Many-to-Many Configuration
            modelBuilder.Entity<KullaniciTakim>()
                .HasKey(kt => new { kt.KullaniciId, kt.TakimId });

            modelBuilder.Entity<KullaniciTakim>()
                .HasOne(kt => kt.Kullanici)
                .WithMany()
                .HasForeignKey(kt => kt.KullaniciId);

            modelBuilder.Entity<KullaniciTakim>()
                .HasOne(kt => kt.Takim)
                .WithMany()
                .HasForeignKey(kt => kt.TakimId);
        }
    }
}
