namespace RestaurantReservation.API.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public DateTime ReservationDateTime { get; set; }
        public int PartySize { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}