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

        public MenuService(IMenuRepository repo)
        {
            _repo = repo;
        }

        // Admin: get all items including unavailable
        public async Task<IEnumerable<MenuItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(m => MapToDto(m));
        }

        // Customer: get only available items
        public async Task<IEnumerable<MenuItemDto>> GetAvailableAsync()
        {
            var items = await _repo.GetAvailableAsync();
            return items.Select(m => MapToDto(m));
        }

        public async Task<MenuItemDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;
            return MapToDto(item);
        }

        public async Task<IEnumerable<MenuItemDto>> GetByCategoryAsync(string category)
        {
            var items = await _repo.GetByCategoryAsync(category);
            return items.Select(m => MapToDto(m));
        }

        public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
        {
            var item = new MenuItem
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Category    = dto.Category,
                IsAvailable = true  // new items are available by default
            };

            var created = await _repo.CreateAsync(item);
            return MapToDto(created);
        }

        public async Task<MenuItemDto?> UpdateAsync(int id, UpdateMenuItemDto dto)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;

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
            return await _repo.DeleteAsync(id);
        }

        private MenuItemDto MapToDto(MenuItem m) => new MenuItemDto
        {
            Id          = m.Id.ToString(),
            Name        = m.Name,
            Description = m.Description,
            Price       = m.Price,
            Category    = m.Category,
            IsAvailable = m.IsAvailable
        };
    }
}