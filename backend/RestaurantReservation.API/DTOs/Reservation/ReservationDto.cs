using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.API.DTOs.Reservation
{
    // Sent BACK to the client when they request reservation info
    public class ReservationDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public int PartySize { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Sent BY the client (Customer) when creating a reservation
    public class CreateReservationDto
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationDateTime { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Party size must be between 1 and 20")]
        public int PartySize { get; set; }

        // Optional note that customer can give
        public string? Notes { get; set; }
    }

    // Sent BY the client (Admin) when updating reservation status
    public class UpdateReservationStatusDto
    {
        [Required] // Only "Confirmed" | "Cancelled" are valid values
        public string Status { get; set; } = string.Empty;
    }

    // Sent BY the client when checking availability
    public class AvailabilityRequestDto
    {
        [Required]
        public DateTime ReservationDateTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int PartySize { get; set; }
    }
}