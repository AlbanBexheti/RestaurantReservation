using RestaurantReservation.API.Models;

namespace RestaurantReservation.API.Repositories.Interfaces
{
    // This interface defines WHAT operations exist for Reservations
    // This is the most important repository - it has the complex logic
    public interface IReservationRepository
    {
        // Get all reservations (Admin only)
        Task<IEnumerable<Reservation>> GetAllAsync();

        // Get all reservations made by a specific user (Customer)
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);

        // Get a single reservation by ID
        Task<Reservation?> GetByIdAsync(int id);

        //checks if a table is free
        //Returns true if the table is already booked at that date/time
        //This is part that prevents double-booking
        Task<bool> IsTableBookedAsync(int tableId, DateTime reservationDateTime);

        // Get all available tables for a given date/time and party size
        // This is the complex business logic - filters by:
        // 1. Party size (table capacity >= partySize)
        // 2. Availability (not already booked at that time)
        // 3. IsAvailable = true (table is not disabled)
        Task<IEnumerable<Table>> GetAvailableTablesAsync(
            DateTime reservationDateTime, int partySize);

        // Create a new reservation
        Task<Reservation> CreateAsync(Reservation reservation);

        // Update reservation status (Pending → Confirmed or Cancelled)
        Task<Reservation> UpdateAsync(Reservation reservation);

        // Delete/cancel a reservation
        Task<bool> DeleteAsync(int id);
    }
}