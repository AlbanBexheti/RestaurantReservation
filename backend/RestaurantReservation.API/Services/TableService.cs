using RestaurantReservation.API.DTOs.Table;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _repo;
        private readonly ILogger<TableService> _logger;

        public TableService(ITableRepository repo, ILogger<TableService> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<TableDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all tables");
            var tables = await _repo.GetAllAsync();
            return tables.Select(t => MapToDto(t));
        }

        public async Task<TableDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching table with ID {Id}", id);
            var table = await _repo.GetByIdAsync(id);
            if (table == null)
            {
                _logger.LogWarning("Table with ID {Id} not found", id);
                return null;
            }
            return MapToDto(table);
        }

        public async Task<TableDto> CreateAsync(TableDto.CreateTableDto dto)
        {
            _logger.LogInformation("Creating table number {TableNumber}", dto.TableNumber);
            var table = new Table
            {
                TableNumber = dto.TableNumber,
                Capacity    = dto.Capacity,
                Location    = dto.Location,
                IsAvailable = true
            };
            var created = await _repo.CreateAsync(table);
            _logger.LogInformation("Table {TableNumber} created with ID {Id}", created.TableNumber, created.Id);
            return MapToDto(created);
        }

        public async Task<TableDto?> UpdateAsync(int id, TableDto.UpdateTableDto dto)
        {
            _logger.LogInformation("Updating table with ID {Id}", id);
            var table = await _repo.GetByIdAsync(id);
            if (table == null)
            {
                _logger.LogWarning("Table with ID {Id} not found for update", id);
                return null;
            }
            table.Capacity    = dto.Capacity;
            table.Location    = dto.Location;
            table.IsAvailable = dto.IsAvailable;
            var updated = await _repo.UpdateAsync(table);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting table with ID {Id}", id);
            var result = await _repo.DeleteAsync(id);
            if (!result)
                _logger.LogWarning("Table with ID {Id} not found for deletion", id);
            return result;
        }

        private TableDto MapToDto(Table t) => new TableDto
        {
            Id          = t.Id,
            TableNumber = t.TableNumber,
            Capacity    = t.Capacity,
            Location    = t.Location,
            IsAvailable = t.IsAvailable
        };
    }
}