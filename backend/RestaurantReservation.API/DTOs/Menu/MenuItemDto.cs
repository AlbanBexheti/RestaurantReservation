using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RestaurantReservation.API.DTOs.Menu;


//Sent back to client when they request menu items
public class MenuItemDto
{
    
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsAvailable { get; set; }
    public string Category { get; set; } = string.Empty;
}
// Sent BY the client (Admin) when creating a menu item
public class CreateMenuItemDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01,10000)]
    public decimal Price { get; set; }
    
    [Required]
    public string Category { get; set; } = string.Empty;
}

// Sent BY the client (Admin) when updating a menu item
public  class UpdateMenuItemDto
{
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}






