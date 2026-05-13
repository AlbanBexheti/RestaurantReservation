using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.API.DTOs.Menu;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET /api/menu
        // Returns only available items - what customers see
        [HttpGet]
        public async Task<IActionResult> GetAvailable()
        {
            var items = await _menuService.GetAvailableAsync();
            return Ok(items);
        }

        // GET /api/menu/all
        // Admin sees everything including unavailable items
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _menuService.GetAllAsync();
            return Ok(items);
        }

        // GET /api/menu/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _menuService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // GET /api/menu/category/{category}
        // e.g. GET /api/menu/category/Starters
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var items = await _menuService.GetByCategoryAsync(category);
            return Ok(items);
        }

        // POST /api/menu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto)
        {
            var created = await _menuService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/menu/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuItemDto dto)
        {
            var updated = await _menuService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/menu/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _menuService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}