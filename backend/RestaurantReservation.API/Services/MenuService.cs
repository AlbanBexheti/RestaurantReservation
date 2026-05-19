using RestaurantReservation.API.DTOs.Menu;
using RestaurantReservation.API.Interfaces;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repo;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IMenuRepository repo, ILogger<MenuService> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all menu items (including unavailable)");
            var items = await _repo.GetAllAsync();
            return items.Select(m => MapToDto(m));
        }

        public async Task<IEnumerable<MenuItemDto>> GetAvailableAsync()
        {
            _logger.LogInformation("Fetching available menu items");
            var items = await _repo.GetAvailableAsync();
            return items.Select(m => MapToDto(m));
        }

        public async Task<MenuItemDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching menu item with ID {Id}", id);
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Menu item with ID {Id} not found", id);
                return null;
            }
            return MapToDto(item);
        }

        public async Task<IEnumerable<MenuItemDto>> GetByCategoryAsync(string category)
        {
            _logger.LogInformation("Fetching menu items for category {Category}", category);
            var items = await _repo.GetByCategoryAsync(category);
            return items.Select(m => MapToDto(m));
        }

        public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
        {
            _logger.LogInformation("Creating menu item {Name}", dto.Name);
            var item = new MenuItem
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Category    = dto.Category,
                IsAvailable = true
            };
            var created = await _repo.CreateAsync(item);
            _logger.LogInformation("Menu item {Name} created with ID {Id}", created.Name, created.Id);
            return MapToDto(created);
        }

        public async Task<MenuItemDto?> UpdateAsync(int id, UpdateMenuItemDto dto)
        {
            _logger.LogInformation("Updating menu item with ID {Id}", id);
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Menu item with ID {Id} not found for update", id);
                return null;
            }
            item.Name        = dto.Name;
            item.Description = dto.Description;
            item.Price       = dto.Price;
            item.Category    = dto.Category;
            item.IsAvailable = dto.IsAvailable;
            var updated = await _repo.UpdateAsync(item);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting menu item with ID {Id}", id);
            var result = await _repo.DeleteAsync(id);
            if (!result)
                _logger.LogWarning("Menu item with ID {Id} not found for deletion", id);
            return result;
        }

        private MenuItemDto MapToDto(MenuItem m) => new MenuItemDto
        {
            Id          = m.Id,
            Name        = m.Name,
            Description = m.Description,
            Price       = m.Price,
            Category    = m.Category,
            IsAvailable = m.IsAvailable
        };
    }
}