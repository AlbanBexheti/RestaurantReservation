using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.API.DTOs.Reservation;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] means ALL endpoints require a valid JWT token
    // Individual endpoints can override this with [AllowAnonymous]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET /api/reservation
        // Admin only - see all reservations
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _reservationService.GetAllAsync();
            return Ok(reservations);
        }

        // GET /api/reservation/my
        // Customer - see only their own reservations
        // We extract userId from the JWT token - not from the URL
        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var reservations = await _reservationService.GetByUserIdAsync(userId.Value);
            return Ok(reservations);
        }

        // GET /api/reservation/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        // GET /api/reservation/availability
        // Check available tables for a date/time and party size
        // [AllowAnonymous] overrides the class-level [Authorize]
        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTables(
            [FromQuery] AvailabilityRequestDto dto)
        {
            var tables = await _reservationService.GetAvailableTablesAsync(dto);
            return Ok(tables);
        }

        // POST /api/reservation
        // Customer creates a reservation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _reservationService.CreateAsync(userId.Value, dto);

            // null means one of the 5 rules failed
            if (result == null)
                return Conflict("Table is unavailable for the selected date and time.");

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT /api/reservation/{id}/status
        // Admin only - confirm or cancel a reservation
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateReservationStatusDto dto)
        {
            var result = await _reservationService.UpdateStatusAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // DELETE /api/reservation/{id}
        // Customer cancels their own reservation
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _reservationService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // Helper: extract the userId from the JWT token claims
        // The token was generated in AuthService.GenerateToken()
        // ClaimTypes.NameIdentifier holds the userId we stored there
        private int? GetUserIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return null;
            return int.Parse(claim.Value);
        }
    }
}