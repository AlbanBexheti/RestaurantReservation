using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantReservation.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedComprehensiveMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Category", "Description", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { "menu-calamari", "Appetizer", "Lightly battered calamari rings served with a tangy lemon-garlic aioli.", true, "Crispy Calamari", 13.00m },
                    { "menu-cheesecake", "Dessert", "Classic dense and creamy cheesecake with a sweet strawberry compote drizzle.", true, "New York Cheesecake", 7.99m },
                    { "menu-lava-cake", "Dessert", "Rich chocolate cake with a warm flowing center, served with vanilla bean gelato.", true, "Molten Lava Cake", 8.50m },
                    { "menu-margherita", "Main", "Classic neapolitan style pizza with fresh mozzarella, san marzano tomatoes, and fresh basil.", true, "Margherita Pizza", 14.99m },
                    { "menu-mojito", "Drink", "Refreshing blend of white rum, fresh lime juice, muddled mint leaves, and club soda.", true, "Classic Mojito", 11.00m },
                    { "menu-ribeye", "Main", "12oz choice ribeye grilled to perfection, served with garlic mashed potatoes and rosemary butter.", true, "Ribeye Steak", 32.00m },
                    { "menu-salmon", "Main", "Atlantic salmon fillet served over a bed of garlic herb quinoa and grilled asparagus.", true, "Pan-Seared Salmon", 24.50m },
                    { "menu-truffle-fries", "Appetizer", "Crispy golden fries tossed in white truffle oil, grated parmesan cheese, and fresh parsley.", true, "Truffle Parmesan Fries", 9.50m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-calamari");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-cheesecake");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-lava-cake");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-margherita");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-mojito");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-ribeye");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-salmon");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: "menu-truffle-fries");
        }
    }
}
