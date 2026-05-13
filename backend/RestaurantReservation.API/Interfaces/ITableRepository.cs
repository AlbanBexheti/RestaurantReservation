using RestaurantReservation.API.Models;

namespace RestaurantReservation.API.Repositories.Interface
{
    public interface ITableRepository
    { 
        //Get all tables from the databse
        Task<IEnumerable<Table>> GetAllAsync();

        //Get table by it's id
        //If the table is not found returns NULL
        Task<Table?> GetByIdAsync(int id);

        //Adding a new table to the database
        Task<Table> CreateAsync(Table table);

        //Update a existing table
        Task<Table> UpdateAsync(Table table);

        //Removing/Deleting a table by ID
        //Returns true if the table is deleted, false if the table is not found
        Task<bool> DeleteAsync(int id);
    }
}