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
        private readonly IReservationRepository _reservationRepo;
        private readonly ITableRepository _tableRepo;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepo,
            ITableRepository tableRepo,
            ILogger<ReservationService> logger)
        {
            _reservationRepo = reservationRepo;
            _tableRepo       = tableRepo;
            _logger          = logger;
        }

        public ReservationService(IReservationRepository reservationRepo, ITableRepository tableRepo)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all reservations");
            var reservations = await _reservationRepo.GetAllAsync();
            return reservations.Select(r => MapToDto(r));
        }

        public async Task<IEnumerable<ReservationDto>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Fetching reservations for user {UserId}", userId);
            var reservations = await _reservationRepo.GetByUserIdAsync(userId);
            return reservations.Select(r => MapToDto(r));
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching reservation with ID {Id}", id);
            var reservation = await _reservationRepo.GetByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {Id} not found", id);
                return null;
            }
            return MapToDto(reservation);
        }

        public async Task<IEnumerable<TableDto>> GetAvailableTablesAsync(AvailabilityRequestDto dto)
        {
            _logger.LogInformation(
                "Checking available tables for {DateTime} with party size {PartySize}",
                dto.ReservationDateTime, dto.PartySize);

            var tables = await _reservationRepo.GetAvailableTablesAsync(
                dto.ReservationDateTime, dto.PartySize);

            _logger.LogInformation("Found {Count} available tables", tables.Count());

            return tables.Select(t => new TableDto
            {
                Id          = t.Id,
                TableNumber = t.TableNumber,
                Capacity    = t.Capacity,
                Location    = t.Location,
                IsAvailable = t.IsAvailable
            });
        }

        public async Task<ReservationDto?> CreateAsync(int userId, CreateReservationDto dto)
        {
            _logger.LogInformation(
                "User {UserId} attempting to book table {TableId} on {DateTime}",
                userId, dto.TableId, dto.ReservationDateTime);

            var table = await _tableRepo.GetByIdAsync(dto.TableId);
            if (table == null)
            {
                _logger.LogWarning("Table {TableId} not found", dto.TableId);
                return null;
            }

            if (!table.IsAvailable)
            {
                _logger.LogWarning("Table {TableId} is disabled by admin", dto.TableId);
                return null;
            }

            if (dto.PartySize > table.Capacity)
            {
                _logger.LogWarning(
                    "Party size {PartySize} exceeds table {TableId} capacity {Capacity}",
                    dto.PartySize, dto.TableId, table.Capacity);
                return null;
            }

            if (dto.ReservationDateTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Reservation date {DateTime} is in the past", dto.ReservationDateTime);
                return null;
            }

            var isBooked = await _reservationRepo.IsTableBookedAsync(dto.TableId, dto.ReservationDateTime);
            if (isBooked)
            {
                _logger.LogWarning(
                    "Table {TableId} is already booked at {DateTime}",
                    dto.TableId, dto.ReservationDateTime);
                return null;
            }

            var reservation = new Reservation
            {
                UserId              = userId,
                TableId             = dto.TableId,
                ReservationDateTime = dto.ReservationDateTime,
                PartySize           = dto.PartySize,
                Notes               = dto.Notes,
                Status              = "Pending",
                CreatedAt           = DateTime.UtcNow
            };

            var created = await _reservationRepo.CreateAsync(reservation);
            _logger.LogInformation(
                "Reservation {Id} created for user {UserId} at table {TableId}",
                created.Id, userId, dto.TableId);

            var full = await _reservationRepo.GetByIdAsync(created.Id);
            return MapToDto(full!);
        }

        public async Task<ReservationDto?> UpdateStatusAsync(int id, UpdateReservationStatusDto dto)
        {
            _logger.LogInformation("Updating reservation {Id} status to {Status}", id, dto.Status);
            var reservation = await _reservationRepo.GetByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation {Id} not found for status update", id);
                return null;
            }

            var validStatuses = new[] { "Confirmed", "Cancelled" };
            if (!validStatuses.Contains(dto.Status))
            {
                _logger.LogWarning("Invalid status value: {Status}", dto.Status);
                return null;
            }

            reservation.Status = dto.Status;
            await _reservationRepo.UpdateAsync(reservation);
            _logger.LogInformation("Reservation {Id} status updated to {Status}", id, dto.Status);
            return MapToDto(reservation);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting reservation {Id}", id);
            var result = await _reservationRepo.DeleteAsync(id);
            if (!result)
                _logger.LogWarning("Reservation {Id} not found for deletion", id);
            return result;
        }

        private ReservationDto MapToDto(Reservation r) => new ReservationDto
        {
            Id                  = r.Id,
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