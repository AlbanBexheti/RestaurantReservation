namespace RestaurantReservation.API.DTOs.Table;

//Sent back to client when they request table info 
public class TableDto
{
    
    public int Id { get; set; }
    public int TableNumber { get; set; } 
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool isAvailable { get; set; }
    
    // Sent BY the client (Admin) when creating a new table
    public class CreateTableDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int TableNumber { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required]
        public int Capacity { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required]
        public string Location { get; set; } = string.Empty;
    }
    
    public class UpdateTableDto
    {
        public int Capacity { get; set; }
        
        public string Location { get; set; } = string.Empty;
       
        public bool isAvailable { get; set; } =  true;
        
    }
    
    
}