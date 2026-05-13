using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantReservation.API.Controllers;
using RestaurantReservation.API.DTOs.Reservation;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.Tests.Controllers
{
    public class ReservationControllerTests
    {
        private readonly Mock<IReservationService> _mockService;
        private readonly ReservationController _controller;

        public ReservationControllerTests()
        {
            _mockService = new Mock<IReservationService>();
            _controller  = new ReservationController(_mockService.Object);

            // Simulate a logged-in user with userId = 1
            // This is what the JWT token provides in real requests
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Customer")
            };
            var identity  = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ── TEST 1 ────────────────────────────────────────────────────────
        // GetMine should return 200 OK with the user's reservations
        [Fact]
        public async Task GetMine_ReturnsOk_WithReservations()
        {
            // ARRANGE
            var fakeReservations = new List<ReservationDto>
            {
                new ReservationDto { Id = 1, Status = "Pending",   PartySize = 2 },
                new ReservationDto { Id = 2, Status = "Confirmed", PartySize = 4 }
            };

            _mockService
                .Setup(s => s.GetByUserIdAsync(1))
                .ReturnsAsync(fakeReservations);

            // ACT
            var result = await _controller.GetMine();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data     = Assert.IsAssignableFrom<IEnumerable<ReservationDto>>(okResult.Value);
            Assert.Equal(2, data.Count());
        }

        // ── TEST 2 ────────────────────────────────────────────────────────
        // Create should return 409 Conflict when table is unavailable
        [Fact]
        public async Task Create_TableUnavailable_ReturnsConflict()
        {
            // ARRANGE
            var dto = new CreateReservationDto
            {
                TableId             = 1,
                ReservationDateTime = DateTime.UtcNow.AddDays(1),
                PartySize           = 2
            };

            // Service returns null = table unavailable
            _mockService
                .Setup(s => s.CreateAsync(1, dto))
                .ReturnsAsync((ReservationDto?)null);

            // ACT
            var result = await _controller.Create(dto);

            // ASSERT - must return 409 Conflict
            Assert.IsType<ConflictObjectResult>(result);
        }

        // ── TEST 3 ────────────────────────────────────────────────────────
        // GetById should return 404 when reservation not found
        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // ARRANGE
            _mockService
                .Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((ReservationDto?)null);

            // ACT
            var result = await _controller.GetById(999);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }
    }
}