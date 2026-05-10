namespace RestaurantReservation.API.Models
{
    public class Table
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public ICollection<Reservation> Reservations { get; set; } 
            = new List<Reservation>();
    }
}