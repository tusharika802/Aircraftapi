using Aircraftapi.Data;
using Aircraftapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AircraftDashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class PartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PartsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAllParts()
        {
            var parts = await _context.Parts.ToListAsync();
            return Ok(parts);
        }

        [HttpGet("in-progress-count")]
        public async Task<IActionResult> GetInProgressPartsCount()
        {
            var count = await _context.Parts.CountAsync(p => p.Status == "In Progress");
            return Ok(count);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddPart([FromBody] Part part)
        {
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            return Ok(part);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditPart(int id, [FromBody] Part part)
        {
            var existing = await _context.Parts.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = part.Name;
            existing.Status = part.Status;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
                return NotFound();

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
    
