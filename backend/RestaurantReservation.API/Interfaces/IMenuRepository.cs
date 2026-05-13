using RestaurantReservation.API.Models;

namespace RestaurantReservation.API.Interfaces
{
    // This interface defines WHAT operations exist for MenuItems
    public interface IMenuRepository
    {
        // Get all menu items
        Task<IEnumerable<MenuItem>> GetAllAsync();

        // Get only available menu items (IsAvailable = true)
        // This is what customers see - hidden items are excluded
        Task<IEnumerable<MenuItem>> GetAvailableAsync();

        // Get a single menu item by ID
        Task<MenuItem?> GetByIdAsync(int id); // nullable to match implementation

        // Get all items belonging to a specific category
        // e.g. GetByCategoryAsync("Starters")
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(string category);

        // Add a new menu item
        Task<MenuItem> CreateAsync(MenuItem menuItem);

        // Update an existing menu item
        Task<MenuItem> UpdateAsync(MenuItem menuItem);

        // Delete a menu item by ID
        Task<bool> DeleteAsync(int id);
    }
}