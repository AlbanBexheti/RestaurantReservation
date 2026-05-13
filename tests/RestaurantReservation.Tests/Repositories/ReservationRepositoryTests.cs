using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories;

namespace RestaurantReservation.Tests.Repositories
{
    public class ReservationRepositoryTests
    {
        // Creates a fresh in-memory database for each test
        // This means tests never interfere with each other
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;
            return new AppDbContext(options);
        }

        // ── TEST 1 ────────────────────────────────────────────────────────
        // IsTableBookedAsync should return true when table is booked
        [Fact]
        public async Task IsTableBookedAsync_TableIsBooked_ReturnsTrue()
        {
            // ARRANGE
            using var context = CreateInMemoryContext();
            var repo          = new ReservationRepository(context);
            var dateTime      = DateTime.UtcNow.AddDays(1);

            // Add a user and table first (required by foreign keys)
            var user  = new User  { FullName = "Test", Email = "t@t.com", PasswordHash = "x", Role = "Customer" };
            var table = new Table { TableNumber = 1, Capacity = 4, Location = "Indoor", IsAvailable = true };
            context.Users.Add(user);
            context.Tables.Add(table);
            await context.SaveChangesAsync();

            // Add an existing reservation for this table
            context.Reservations.Add(new Reservation
            {
                UserId              = user.Id,
                TableId             = table.Id,
                ReservationDateTime = dateTime,
                PartySize           = 2,
                Status              = "Pending",
                CreatedAt           = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // ACT - check if the table is booked at the same time
            var isBooked = await repo.IsTableBookedAsync(table.Id, dateTime);

            // ASSERT - should be true since we just added a reservation
            Assert.True(isBooked);
        }

        // ── TEST 2 ────────────────────────────────────────────────────────
        // IsTableBookedAsync should return false for cancelled reservations
        [Fact]
        public async Task IsTableBookedAsync_CancelledReservation_ReturnsFalse()
        {
            // ARRANGE
            using var context = CreateInMemoryContext();
            var repo          = new ReservationRepository(context);
            var dateTime      = DateTime.UtcNow.AddDays(1);

            var user  = new User  { FullName = "Test", Email = "t@t.com", PasswordHash = "x", Role = "Customer" };
            var table = new Table { TableNumber = 1, Capacity = 4, Location = "Indoor", IsAvailable = true };
            context.Users.Add(user);
            context.Tables.Add(table);
            await context.SaveChangesAsync();

            // Add a CANCELLED reservation - should not block new bookings
            context.Reservations.Add(new Reservation
            {
                UserId              = user.Id,
                TableId             = table.Id,
                ReservationDateTime = dateTime,
                PartySize           = 2,
                Status              = "Cancelled",  // cancelled!
                CreatedAt           = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // ACT
            var isBooked = await repo.IsTableBookedAsync(table.Id, dateTime);

            // ASSERT - cancelled reservation should NOT block the table
            Assert.False(isBooked);
        }

        // ── TEST 3 ────────────────────────────────────────────────────────
        // CreateAsync should save and return the reservation with an ID
        [Fact]
        public async Task CreateAsync_ValidReservation_SavesAndReturnsWithId()
        {
            // ARRANGE
            using var context = CreateInMemoryContext();
            var repo          = new ReservationRepository(context);

            var user  = new User  { FullName = "Test", Email = "t@t.com", PasswordHash = "x", Role = "Customer" };
            var table = new Table { TableNumber = 1, Capacity = 4, Location = "Indoor", IsAvailable = true };
            context.Users.Add(user);
            context.Tables.Add(table);
            await context.SaveChangesAsync();

            var reservation = new Reservation
            {
                UserId              = user.Id,
                TableId             = table.Id,
                ReservationDateTime = DateTime.UtcNow.AddDays(1),
                PartySize           = 2,
                Status              = "Pending",
                CreatedAt           = DateTime.UtcNow
            };

            // ACT
            var result = await repo.CreateAsync(reservation);

            // ASSERT - should have an auto-generated Id
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Pending", result.Status);
        }
    }
}