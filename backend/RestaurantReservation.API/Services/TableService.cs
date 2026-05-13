using RestaurantReservation.API.DTOs.Table;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Services
{
    public class TableService : ITableService
    {
        // Repository is injected - service never touches DbContext directly
        private readonly ITableRepository _repo;

        public TableService(ITableRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<TableDto>> GetAllAsync()
        {
            var tables = await _repo.GetAllAsync();

            // Map each Table model to a TableDto
            // We never return raw models from the service layer
            return tables.Select(t => MapToDto(t));
        }

        public async Task<TableDto?> GetByIdAsync(int id)
        {
            var table = await _repo.GetByIdAsync(id);

            // If not found return null - controller will handle 404
            if (table == null) return null;

            return MapToDto(table);
        }
        
        public async Task<TableDto> CreateAsync(TableDto.CreateTableDto dto)
        {
            // Map DTO → Model before saving to database
            var table = new Table
            {
                TableNumber = dto.TableNumber,
                Capacity    = dto.Capacity,
                Location    = dto.Location,
                IsAvailable = true  // new tables are available by default
            };

            var created = await _repo.CreateAsync(table);
            return MapToDto(created);
        }

        public async Task<TableDto?> UpdateAsync(int id, TableDto.UpdateTableDto dto)
        {
            var table = await _repo.GetByIdAsync(id);
            if (table == null) return null;

            // Only update the fields that were sent
            table.Capacity    = dto.Capacity;
            table.Location    = dto.Location;
            table.IsAvailable = dto.IsAvailable;

            var updated = await _repo.UpdateAsync(table);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        // Private helper - converts a Table model to a TableDto
        // Keeps mapping logic in one place - easy to update
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