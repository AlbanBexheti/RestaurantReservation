using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.DTOs.Auth;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        // IConfiguration lets us read from appsettings.json
        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config  = config;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            // Check if email is already taken
            var exists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);
            if (exists) return null;

            // Hash the password - NEVER store plain text
            var user = new User
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "Customer"  // new users are always customers
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate and return JWT token
            return new AuthResponseDto
            {
                Token    = GenerateToken(user),
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Return null if user not found OR password is wrong
            // BCrypt.Verify compares plain password against the hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new AuthResponseDto
            {
                Token    = GenerateToken(user),
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            };
        }

        // Generate a JWT token for a user
        private string GenerateToken(User user)
        {
            // Claims are pieces of info stored INSIDE the token
            // The API reads these on every request - no database needed
            var claims = new[]
            {
                // NameIdentifier stores the user's ID
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Email stores the user's email
                new Claim(ClaimTypes.Email, user.Email),
                // Role stores "Admin" or "Customer"
                // This is what [Authorize(Roles = "Admin")] checks
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Read the secret key from appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));

            // Sign the token with the key using HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the token
            var token = new JwtSecurityToken(
                issuer:   _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims:   claims,
                expires:  DateTime.UtcNow.AddMinutes(
                    double.Parse(_config["JwtSettings:ExpiryInMinutes"]!)),
                signingCredentials: creds
            );

            // Serialize token to string e.g. "eyJhbGci..."
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}