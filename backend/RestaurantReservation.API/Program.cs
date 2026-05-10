using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ─────────────────────────────────────────────────────────────────
// Register AppDbContext with SQL Server using the connection string
// from appsettings.json ("DefaultConnection")
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── CONTROLLERS ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── SWAGGER ──────────────────────────────────────────────────────────────────
// Swagger gives us a UI to test the API endpoints in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();