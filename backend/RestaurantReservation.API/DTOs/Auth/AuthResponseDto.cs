namespace RestaurantReservation.API.DTOs.Auth;

//This is what API needs back aftetr a sucesscul register or login
//No PasswordHash here - NEVER SEND THAT BACK

public class AuthResponseDto
{
    //The JWT token the client must send in every future request
    public string Token { get; set; } = string.Empty;
    
    //Basic user info 
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    //Role, lets frontend show/hide admin features
    public string Role { get; set; } = string.Empty;
}