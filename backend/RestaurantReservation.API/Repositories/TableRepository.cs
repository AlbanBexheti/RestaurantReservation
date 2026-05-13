using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.Models;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;

namespace RestaurantReservation.API.Repositories
{
    // TableRepository implements ITableRepository
    // This is the ONLY class that talks to the database for tables
    public class TableRepository : ITableRepository
    {
        // AppDbContext is injected via Dependency Injection
        // We never create it manually with "new AppDbContext()"
        private readonly AppDbContext _context;

        public TableRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all tables from the Tables table
        public async Task<IEnumerable<Table>> GetAllAsync()
        {
            // ToListAsync() runs SELECT * FROM Tables
            return await _context.Tables.ToListAsync();
        }

        // Get a single table by its ID
        public async Task<Table?> GetByIdAsync(int id)
        {
            // FindAsync() runs SELECT * FROM Tables WHERE Id = @id
            // Returns null if not found
            return await _context.Tables.FindAsync(id);
        }

        // Add a new table to the database
        public async Task<Table> CreateAsync(Table table)
        {
            // AddAsync() stages the new table (doesn't save yet)
            await _context.Tables.AddAsync(table);

            // SaveChangesAsync() actually runs the INSERT INTO SQL
            await _context.SaveChangesAsync();

            // Return the table with its new auto-generated Id
            return table;
        }

        // Update an existing table
        public async Task<Table> UpdateAsync(Table table)
        {
            // Update() marks all properties as modified
            _context.Tables.Update(table);

            // SaveChangesAsync() runs the UPDATE SQL
            await _context.SaveChangesAsync();
            return table;
        }

        // Deletes a table by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            // If table is not existing returns false
            if (table == null) return false;
            
            _context.Tables.Remove(table);

            // SaveChangesAsync() runs the DELETE SQL
            await _context.SaveChangesAsync();
            return true;
        }
    }
}