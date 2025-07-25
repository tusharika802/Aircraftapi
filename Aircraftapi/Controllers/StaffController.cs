using Aircraftapi.Data;
using Aircraftapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aircraftapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class StaffController : ControllerBase
    {
       private readonly ApplicationDbContext _context;
        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _context.Staffs.ToListAsync();
            return Ok(staff);
        }

        [HttpGet("active-count")]
        public async Task<IActionResult> GetActiveStaffCount()
        {
            var count = await _context.Staffs.CountAsync(s => s.IsActive);
            return Ok(count);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddStaff([FromBody] Staff staff)
        {
            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();
            return Ok(staff);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditStaff(int id, [FromBody] Staff staff)
        {
            var existing = await _context.Staffs.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = staff.Name;
            existing.IsActive = staff.IsActive;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return NotFound();

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}

