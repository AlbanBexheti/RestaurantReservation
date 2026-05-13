using RestaurantReservation.API.DTOs.Auth;

namespace RestaurantReservation.API.Services.Interfaces
{
    // Defines what authentication operations are available
    public interface IAuthService
    {
        // Register a new user - returns a JWT token on success
        // Returns null if email is already taken
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

        // Login an existing user - returns a JWT token on success
        // Returns null if email/password is wrong
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}