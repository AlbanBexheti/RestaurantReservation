using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interfaces;

namespace RestaurantReservation.API.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get ALL reservations with user and table info included
        // .Include() tells EF Core to JOIN the related tables
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.User)    // JOIN Users table
                .Include(r => r.Table)   // JOIN Tables table
                .ToListAsync();
        }

        // Get all reservations belonging to a specific user
        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        // Get a single reservation by ID
        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ── CORE BUSINESS LOGIC ──────────────────────────────────────────
        // Check if a table is already booked at a given date and time
        // A 2-hour window is used - no two reservations within 2 hours
        // of each other on the same table
        public async Task<bool> IsTableBookedAsync(int tableId, DateTime reservationDateTime)
        {
            // Define a 2-hour window around the requested time
            var windowStart = reservationDateTime.AddHours(-2);
            var windowEnd   = reservationDateTime.AddHours(2);

            return await _context.Reservations.AnyAsync(r =>
                r.TableId == tableId &&                        // same table
                r.Status != "Cancelled" &&                     // not cancelled
                r.ReservationDateTime >= windowStart &&        // within window
                r.ReservationDateTime <= windowEnd             // within window
            );
        }

        // Get all tables available for a given date/time and party size
        // This orchestrates multiple checks in one query
        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(
            DateTime reservationDateTime, int partySize)
        {
            var windowStart = reservationDateTime.AddHours(-2);
            var windowEnd   = reservationDateTime.AddHours(2);

            // Get IDs of tables that ARE booked in this time window
            var bookedTableIds = await _context.Reservations
                .Where(r =>
                    r.Status != "Cancelled" &&
                    r.ReservationDateTime >= windowStart &&
                    r.ReservationDateTime <= windowEnd)
                .Select(r => r.TableId)   // only get the TableId column
                .ToListAsync();

            // Return tables that:
            // 1. Are NOT in the booked list
            // 2. Have enough capacity for the party
            // 3. Are marked as available by admin
            return await _context.Tables
                .Where(t =>
                    !bookedTableIds.Contains(t.Id) &&  // not booked
                    t.Capacity >= partySize &&          // big enough
                    t.IsAvailable)                      // not disabled
                .ToListAsync();
        }

        // Create a new reservation
        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        // Update a reservation (e.g. status change)
        public async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        // Delete a reservation by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}