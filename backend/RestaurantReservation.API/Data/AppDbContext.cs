using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Models;

namespace RestaurantReservation.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── USER ────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(150);
                entity.Property(u => u.Role).HasMaxLength(20);
            });

            // ── TABLE ───────────────────────────────────────────────────
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasIndex(t => t.TableNumber).IsUnique();
                entity.Property(t => t.Location).HasMaxLength(50);
            });

            // ── MENU ITEM ───────────────────────────────────────────────
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.Property(m => m.Name).HasMaxLength(100);
                entity.Property(m => m.Category).HasMaxLength(50);
            });

            // ── RESERVATION ─────────────────────────────────────────────
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.Property(r => r.Status).HasMaxLength(20);
                entity.Property(r => r.Notes).HasMaxLength(500);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Table)
                      .WithMany(t => t.Reservations)
                      .HasForeignKey(r => r.TableId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}