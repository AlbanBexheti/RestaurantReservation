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
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(150);
                entity.Property(u => u.Role).HasMaxLength(20);
            });

            // ── TABLE ───────────────────────────────────────────────────
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.TableNumber).IsUnique();
                entity.Property(t => t.Location).HasMaxLength(50);
            });

            // ── MENU ITEM ───────────────────────────────────────────────
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).ValueGeneratedOnAdd();
                entity.Property(m => m.Name).HasMaxLength(100);
                entity.Property(m => m.Category).HasMaxLength(50);

                // SEED DATA: Adding real restaurant items
                entity.HasData(
                    new MenuItem
                    {
                        Id = "menu-truffle-fries",
                        Name = "Truffle Parmesan Fries",
                        Description = "Crispy golden fries tossed in white truffle oil, grated parmesan cheese, and fresh parsley.",
                        Price = 9.50m,
                        Category = "Appetizer",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-calamari",
                        Name = "Crispy Calamari",
                        Description = "Lightly battered calamari rings served with a tangy lemon-garlic aioli.",
                        Price = 13.00m,
                        Category = "Appetizer",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-margherita",
                        Name = "Margherita Pizza",
                        Description = "Classic neapolitan style pizza with fresh mozzarella, san marzano tomatoes, and fresh basil.",
                        Price = 14.99m,
                        Category = "Main",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-salmon",
                        Name = "Pan-Seared Salmon",
                        Description = "Atlantic salmon fillet served over a bed of garlic herb quinoa and grilled asparagus.",
                        Price = 24.50m,
                        Category = "Main",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-ribeye",
                        Name = "Ribeye Steak",
                        Description = "12oz choice ribeye grilled to perfection, served with garlic mashed potatoes and rosemary butter.",
                        Price = 32.00m,
                        Category = "Main",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-lava-cake",
                        Name = "Molten Lava Cake",
                        Description = "Rich chocolate cake with a warm flowing center, served with vanilla bean gelato.",
                        Price = 8.50m,
                        Category = "Dessert",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-cheesecake",
                        Name = "New York Cheesecake",
                        Description = "Classic dense and creamy cheesecake with a sweet strawberry compote drizzle.",
                        Price = 7.99m,
                        Category = "Dessert",
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        Id = "menu-mojito",
                        Name = "Classic Mojito",
                        Description = "Refreshing blend of white rum, fresh lime juice, muddled mint leaves, and club soda.",
                        Price = 11.00m,
                        Category = "Drink",
                        IsAvailable = true
                    }
                );
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