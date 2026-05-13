using RestaurantReservation.API.DTOs.Table;

namespace RestaurantReservation.API.Services.Interfaces
{
    // Defines what table operations are available
    // Notice: returns DTOs not Models - service handles the mapping
    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllAsync();
        Task<TableDto?> GetByIdAsync(int id);
        Task<TableDto> CreateAsync(TableDto.CreateTableDto dto);
        Task<TableDto?> UpdateAsync(int id, TableDto.UpdateTableDto dto);
        Task<bool> DeleteAsync(int id);
    }
}