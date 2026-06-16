
using Moq;
using Microsoft.Extensions.Logging;
using RestaurantReservation.API.DTOs.Reservation;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services;

namespace RestaurantReservation.Tests.Services
{
    public class ReservationServiceTests
    {
        // Mock objects - fake implementations of the repositories
        // We don't need a real database to test business logic
        private readonly Mock<IReservationRepository> _mockReservationRepo;
        private readonly Mock<ITableRepository> _mockTableRepo;
        private readonly ReservationService _service;

        public ReservationServiceTests()
        {
            _mockReservationRepo = new Mock<IReservationRepository>();
            _mockTableRepo       = new Mock<ITableRepository>();

            // Inject mocks into the service - same as DI in Program.cs
            // Logger is mocked too - we don't care about log output in tests
            var mockLogger = new Mock<ILogger<ReservationService>>();
            _service = new ReservationService(
                _mockReservationRepo.Object,
                _mockTableRepo.Object,
                mockLogger.Object);
        }

        // ── TEST 1 ────────────────────────────────────────────────────────
        // A valid reservation should be created successfully
        [Fact]
        public async Task CreateAsync_ValidReservation_ReturnsReservationDto()
        {
            // ARRANGE - set up the fake data and mock behavior
            var userId = 1;
            var dto = new CreateReservationDto
            {
                TableId             = 1,
                ReservationDateTime = DateTime.UtcNow.AddDays(1), // tomorrow
                PartySize           = 2,
                Notes               = "Window seat please"
            };

            var fakeTable = new Table
            {
                Id          = 1,
                TableNumber = 1,
                Capacity    = 4,
                Location    = "Indoor",
                IsAvailable = true
            };

            var fakeReservation = new Reservation
            {
                Id                  = 1,
                UserId              = userId,
                TableId             = dto.TableId,
                ReservationDateTime = dto.ReservationDateTime,
                PartySize           = dto.PartySize,
                Status              = "Pending",
                CreatedAt           = DateTime.UtcNow,
                User                = new User { FullName = "Test User", Email = "test@test.com" },
                Table               = fakeTable
            };

            // Tell the mock what to return when these methods are called
            _mockTableRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fakeTable);

            _mockReservationRepo
                .Setup(r => r.IsTableBookedAsync(1, dto.ReservationDateTime))
                .ReturnsAsync(false); // table is NOT booked

            _mockReservationRepo
                .Setup(r => r.CreateAsync(It.IsAny<Reservation>()))
                .ReturnsAsync(fakeReservation);

            _mockReservationRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fakeReservation);

            // ACT - call the method we're testing
            var result = await _service.CreateAsync(userId, dto);

            // ASSERT - verify the result is what we expect
            Assert.NotNull(result);
            Assert.Equal("Pending", result.Status);
            Assert.Equal(2, result.PartySize);
        }

        // ── TEST 2 ────────────────────────────────────────────────────────
        // Should return null when table is already booked (double-booking prevention)
        [Fact]
        public async Task CreateAsync_TableAlreadyBooked_ReturnsNull()
        {
            // ARRANGE
            var dto = new CreateReservationDto
            {
                TableId             = 1,
                ReservationDateTime = DateTime.UtcNow.AddDays(1),
                PartySize           = 2
            };

            var fakeTable = new Table
            {
                Id          = 1,
                Capacity    = 4,
                IsAvailable = true
            };

            _mockTableRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fakeTable);

            // Table IS already booked
            _mockReservationRepo
                .Setup(r => r.IsTableBookedAsync(1, dto.ReservationDateTime))
                .ReturnsAsync(true);

            // ACT
            var result = await _service.CreateAsync(1, dto);

            // ASSERT - must return null when table is booked
            Assert.Null(result);
        }

        // ── TEST 3 ────────────────────────────────────────────────────────
        // Should return null when party size exceeds table capacity
        [Fact]
        public async Task CreateAsync_PartySizeExceedsCapacity_ReturnsNull()
        {
            // ARRANGE
            var dto = new CreateReservationDto
            {
                TableId             = 1,
                ReservationDateTime = DateTime.UtcNow.AddDays(1),
                PartySize           = 10  // too many people for this table
            };

            var fakeTable = new Table
            {
                Id          = 1,
                Capacity    = 4,  // only fits 4
                IsAvailable = true
            };

            _mockTableRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fakeTable);

            // ACT
            var result = await _service.CreateAsync(1, dto);

            // ASSERT - must return null when party is too large
            Assert.Null(result);
        }

        // ── TEST 4 ────────────────────────────────────────────────────────
        // Should return null when table does not exist
        [Fact]
        public async Task CreateAsync_TableNotFound_ReturnsNull()
        {
            // ARRANGE
            var dto = new CreateReservationDto
            {
                TableId             = 999, // non-existent table
                ReservationDateTime = DateTime.UtcNow.AddDays(1),
                PartySize           = 2
            };

            // Mock returns null - table not found
            _mockTableRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Table?)null);

            // ACT
            var result = await _service.CreateAsync(1, dto);

            // ASSERT
            Assert.Null(result);
        }

        // ── TEST 5 ────────────────────────────────────────────────────────
        // Should return null when reservation date is in the past
        [Fact]
        public async Task CreateAsync_PastDate_ReturnsNull()
        {
            // ARRANGE
            var dto = new CreateReservationDto
            {
                TableId             = 1,
                ReservationDateTime = DateTime.UtcNow.AddDays(-1), // yesterday
                PartySize           = 2
            };

            var fakeTable = new Table
            {
                Id          = 1,
                Capacity    = 4,
                IsAvailable = true
            };

            _mockTableRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fakeTable);

            // ACT
            var result = await _service.CreateAsync(1, dto);

            // ASSERT - past dates must be rejected
            Assert.Null(result);
        }
    }
}