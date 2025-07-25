using Aircraftapi.Data;
using Aircraftapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aircraftapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ServiceCentreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServiceCentreController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllServiceCentres()
        {
            var centres = await _context.ServiceCentres.ToListAsync();
            return Ok(centres);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetServiceCentreCount()
        {
            var count = await _context.ServiceCentres.CountAsync();
            return Ok(count);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddServiceCentre([FromBody] ServiceCentre centre)
        {
            _context.ServiceCentres.Add(centre);
            await _context.SaveChangesAsync();
            return Ok(centre);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditServiceCentre(int id, [FromBody] ServiceCentre centre)
        {
            var existing = await _context.ServiceCentres.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Location = centre.Location;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteServiceCentre(int id)
        {
            var centre = await _context.ServiceCentres.FindAsync(id);
            if (centre == null)
                return NotFound();

            _context.ServiceCentres.Remove(centre);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
