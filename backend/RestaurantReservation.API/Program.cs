using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.API.Data;
using RestaurantReservation.API.Interfaces;
using RestaurantReservation.API.Repositories;
using RestaurantReservation.API.Repositories.Interface;
using RestaurantReservation.API.Repositories.Interfaces;
using RestaurantReservation.API.Services;
using RestaurantReservation.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ─────────────────────────────────────────────────────────────────
// Register AppDbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── DEPENDENCY INJECTION ─────────────────────────────────────────────────────
// Every time someone asks for ITableRepository, give them TableRepository
// AddScoped means one instance per HTTP request
builder.Services.AddScoped<ITableRepository,       TableRepository>();
builder.Services.AddScoped<IMenuRepository,        MenuRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ITableService,          TableService>();
builder.Services.AddScoped<IMenuService,           MenuService>();
builder.Services.AddScoped<IReservationService,    ReservationService>();
builder.Services.AddScoped<IAuthService,           AuthService>();

// ── JWT AUTHENTICATION ────────────────────────────────────────────────────────
// Read JWT settings from appsettings.json
var jwtSecret   = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer   = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the server that created the token
        ValidateIssuer   = true,
        ValidIssuer      = jwtIssuer,

        // Validate the recipient of the token
        ValidateAudience = true,
        ValidAudience    = jwtAudience,

        // Validate the token hasn't expired
        ValidateLifetime = true,

        // Validate the signature - prevents token tampering
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
// Allow the React frontend to call the API
// Without this, the browser blocks cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // React dev server (Vite)
                "http://localhost:3000")  // React dev server (CRA)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── CONTROLLERS & SWAGGER ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── BUILD APP ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── SEED DATA ─────────────────────────────────────────────────────────────────
// Run once on startup - adds default admin + tables + menu items if DB is empty
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Only seed if there are no users yet
    if (!context.Users.Any())
    {
        context.Users.Add(new RestaurantReservation.API.Models.User
        {
            FullName     = "Admin User",
            Email        = "admin@restaurant.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role         = "Admin"
        });

        context.Tables.AddRange(
            new RestaurantReservation.API.Models.Table { TableNumber = 1, Capacity = 2, Location = "Indoor",  IsAvailable = true },
            new RestaurantReservation.API.Models.Table { TableNumber = 2, Capacity = 4, Location = "Indoor",  IsAvailable = true },
            new RestaurantReservation.API.Models.Table { TableNumber = 3, Capacity = 4, Location = "Outdoor", IsAvailable = true },
            new RestaurantReservation.API.Models.Table { TableNumber = 4, Capacity = 6, Location = "Indoor",  IsAvailable = true },
            new RestaurantReservation.API.Models.Table { TableNumber = 5, Capacity = 8, Location = "Outdoor", IsAvailable = true }
        );

        context.MenuItems.AddRange(
            new RestaurantReservation.API.Models.MenuItem { Name = "Bruschetta",       Description = "Toasted bread with tomatoes",   Price = 5.99m,  Category = "Starters", IsAvailable = true },
            new RestaurantReservation.API.Models.MenuItem { Name = "Margherita Pizza", Description = "Classic tomato and mozzarella", Price = 12.99m, Category = "Main",     IsAvailable = true },
            new RestaurantReservation.API.Models.MenuItem { Name = "Grilled Salmon",   Description = "With lemon butter sauce",       Price = 18.99m, Category = "Main",     IsAvailable = true },
            new RestaurantReservation.API.Models.MenuItem { Name = "Tiramisu",         Description = "Classic Italian dessert",       Price = 6.99m,  Category = "Desserts", IsAvailable = true },
            new RestaurantReservation.API.Models.MenuItem { Name = "Sparkling Water",  Description = "500ml bottle",                  Price = 2.99m,  Category = "Drinks",   IsAvailable = true }
        );

        context.SaveChanges();
    }
}

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Order matters! Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();