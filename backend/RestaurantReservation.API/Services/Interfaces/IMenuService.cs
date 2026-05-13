using RestaurantReservation.API.DTOs.Menu;

namespace RestaurantReservation.API.Services.Interfaces
{
    // Defines what menu operations are available
    public interface IMenuService
    {
        // Get all items - Admin sees everything including unavailable
        Task<IEnumerable<MenuItemDto>> GetAllAsync();

        // Get only available items - what customers see
        Task<IEnumerable<MenuItemDto>> GetAvailableAsync();

        Task<MenuItemDto?> GetByIdAsync(int id);

        // Get items filtered by category e.g. "Starters"
        Task<IEnumerable<MenuItemDto>> GetByCategoryAsync(string category);

        Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto);
        Task<MenuItemDto?> UpdateAsync(int id, UpdateMenuItemDto dto);
        Task<bool> DeleteAsync(int id);
    }
}