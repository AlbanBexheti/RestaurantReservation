using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.Interfaces;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;

namespace RestaurantReservation.API.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get ALL menu items including unavailable ones (Admin use)
        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems.ToListAsync();
        }

        public Task<IEnumerable<MenuItem>> GetAvailableAsync(int id)
        {
            throw new NotImplementedException();
        }

        // Get only available items (Customer use)
        // WHERE IsAvailable = 1
        public async Task<IEnumerable<MenuItem>> GetAvailableAsync()
        {
            return await _context.MenuItems
                .Where(m => m.IsAvailable)
                .ToListAsync();
        }

        // Get a single menu item by ID
        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            return await _context.MenuItems.FindAsync(id);
        }

        // Get all items in a specific category
        // e.g. WHERE Category = 'Starters'
        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(string category)
        {
            return await _context.MenuItems
                .Where(m => m.Category == category && m.IsAvailable)
                .ToListAsync();
        }

        // Add a new menu item
        public async Task<MenuItem> CreateAsync(MenuItem menuItem)
        {
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();
            return menuItem;
        }

        // Update an existing menu item
        public async Task<MenuItem> UpdateAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
            await _context.SaveChangesAsync();
            return menuItem;
        }

        // Delete a menu item by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return false;

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}