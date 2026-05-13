using RestaurantReservation.API.DTOs.Reservation;
using RestaurantReservation.API.DTOs.Table;

namespace RestaurantReservation.API.Services.Interfaces
{
    // Defines what reservation operations are available
    // This is the most complex service - handles all booking logic
    public interface IReservationService
    {
        // Admin: get all reservations
        Task<IEnumerable<ReservationDto>> GetAllAsync();

        // Customer: get only their own reservations
        Task<IEnumerable<ReservationDto>> GetByUserIdAsync(int userId);

        // Get a single reservation by ID
        Task<ReservationDto?> GetByIdAsync(int id);

        // Check available tables for a given date/time and party size
        // Returns list of available tables the customer can choose from
        Task<IEnumerable<TableDto>> GetAvailableTablesAsync(AvailabilityRequestDto dto);

        // Create a new reservation - includes double-booking check
        // Returns null if table is already booked
        Task<ReservationDto?> CreateAsync(int userId, CreateReservationDto dto);

        // Admin: update reservation status (Confirm or Cancel)
        Task<ReservationDto?> UpdateStatusAsync(int id, UpdateReservationStatusDto dto);

        // Customer or Admin: cancel/delete a reservation
        Task<bool> DeleteAsync(int id);
    }
}