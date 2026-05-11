using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.API.DTOs.Auth;



public class RegisterDto
{
    [Required] // Means that it is required and field cannot be empty
    //If empty it the API returns 400 Bad Request
    public string FullName { get; set; } =  string.Empty;
    
    [Required] //Validates that the format is coorect
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)] //Ensures that pass is at least 8 characterse
    public string Password { get; set; } = string.Empty;
    
}