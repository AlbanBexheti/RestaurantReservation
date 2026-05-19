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
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config  = config;
            _logger  = logger;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Register attempt for email {Email}", dto.Email);

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
            {
                _logger.LogWarning("Registration failed - email {Email} already taken", dto.Email);
                return null;
            }

            var user = new User
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "Customer"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Email} registered successfully with ID {Id}", user.Email, user.Id);

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
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email {Email} - invalid credentials", dto.Email);
                return null;
            }

            _logger.LogInformation("User {Email} logged in successfully", dto.Email);

            return new AuthResponseDto
            {
                Token    = GenerateToken(user),
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            };
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _config["JwtSettings:Issuer"],
                audience:           _config["JwtSettings:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpiryInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}