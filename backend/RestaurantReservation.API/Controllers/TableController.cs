using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.API.DTOs.Table;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        // GET /api/table
        // Anyone can see the tables - no auth required
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _tableService.GetAllAsync();
            return Ok(tables);
        }

        // GET /api/table/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null) return NotFound();
            return Ok(table);
        }

        // POST /api/table
        // Only Admin can create tables
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] TableDto.CreateTableDto dto)
        {
            var created = await _tableService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/table/{id}
        // Only Admin can update tables
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] TableDto.UpdateTableDto dto)
        {
            var updated = await _tableService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/table/{id}
        // Only Admin can delete tables
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _tableService.DeleteAsync(id);
            if (!deleted) return NotFound();

            // 204 No Content - success but nothing to return
            return NoContent();
        }
    }
}