using RestaurantReservation.API.DTOs.Reservation;
using RestaurantReservation.API.DTOs.Table;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Services
{
    public class ReservationService : IReservationService
    {
        // We need both repositories - reservations AND tables
        private readonly IReservationRepository _reservationRepo;
        private readonly ITableRepository _tableRepo;

        public ReservationService(
            IReservationRepository reservationRepo,
            ITableRepository tableRepo)
        {
            _reservationRepo = reservationRepo;
            _tableRepo       = tableRepo;
        }

        // Admin: get all reservations
        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            var reservations = await _reservationRepo.GetAllAsync();
            return reservations.Select(r => MapToDto(r));
        }

        // Customer: get only their own reservations
        public async Task<IEnumerable<ReservationDto>> GetByUserIdAsync(int userId)
        {
            var reservations = await _reservationRepo.GetByUserIdAsync(userId);
            return reservations.Select(r => MapToDto(r));
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var reservation = await _reservationRepo.GetByIdAsync(id);
            if (reservation == null) return null;
            return MapToDto(reservation);
        }

        // Get available tables for a date/time and party size
        public async Task<IEnumerable<TableDto>> GetAvailableTablesAsync(
            AvailabilityRequestDto dto)
        {
            var tables = await _reservationRepo.GetAvailableTablesAsync(
                dto.ReservationDateTime, dto.PartySize);

            // Map Table models to TableDtos
            return tables.Select(t => new TableDto
            {
                Id          = t.Id,
                TableNumber = t.TableNumber,
                Capacity    = t.Capacity,
                Location    = t.Location,
                IsAvailable = t.IsAvailable
            });
        }

        // ── CORE BUSINESS LOGIC ──────────────────────────────────────────
        // Create a reservation with full validation
        public async Task<ReservationDto?> CreateAsync(int userId, CreateReservationDto dto)
        {
            // RULE 1: Check the table exists
            var table = await _tableRepo.GetByIdAsync(dto.TableId);
            if (table == null) return null;

            // RULE 2: Check the table is not disabled by admin
            if (!table.IsAvailable) return null;

            // RULE 3: Check party size fits the table
            if (dto.PartySize > table.Capacity) return null;

            // RULE 4: Check the reservation is not in the past
            if (dto.ReservationDateTime < DateTime.UtcNow) return null;

            // RULE 5: Check the table is not already booked (double-booking prevention)
            var isBooked = await _reservationRepo.IsTableBookedAsync(
                dto.TableId, dto.ReservationDateTime);
            if (isBooked) return null;

            // All rules passed - create the reservation
            var reservation = new Reservation
            {
                UserId              = userId,
                TableId             = dto.TableId,
                ReservationDateTime = dto.ReservationDateTime,
                PartySize           = dto.PartySize,
                Notes               = dto.Notes,
                Status              = "Pending",   // always starts as Pending
                CreatedAt           = DateTime.UtcNow  // set here, not in model
            };

            var created = await _reservationRepo.CreateAsync(reservation);

            // Load the full reservation with User and Table included
            var full = await _reservationRepo.GetByIdAsync(created.Id);
            return MapToDto(full!);
        }

        // Admin: update reservation status (Confirm or Cancel)
        public async Task<ReservationDto?> UpdateStatusAsync(
            int id, UpdateReservationStatusDto dto)
        {
            var reservation = await _reservationRepo.GetByIdAsync(id);
            if (reservation == null) return null;

            // Only allow valid status values
            var validStatuses = new[] { "Confirmed", "Cancelled" };
            if (!validStatuses.Contains(dto.Status)) return null;

            reservation.Status = dto.Status;
            await _reservationRepo.UpdateAsync(reservation);

            return MapToDto(reservation);
        }

        // Delete a reservation
        public async Task<bool> DeleteAsync(int id)
        {
            return await _reservationRepo.DeleteAsync(id);
        }

        // Map Reservation model → ReservationDto
        private ReservationDto MapToDto(Reservation r) => new ReservationDto
        {
            Id                  = r.Id,
            // Use null-safe operator ?. in case Include() wasn't called
            TableNumber         = r.Table?.TableNumber ?? 0,
            CustomerName        = r.User?.FullName ?? "Unknown",
            ReservationDateTime = r.ReservationDateTime,
            PartySize           = r.PartySize,
            Status              = r.Status,
            Notes               = r.Notes,
            CreatedAt           = r.CreatedAt
        };
    }
}