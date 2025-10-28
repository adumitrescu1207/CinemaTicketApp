using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loc>>> GetLocuri()
        {
            return await _context.Locuri.Include(l => l.Sala).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Loc>> GetLoc(int id)
        {
            var loc = await _context.Locuri.FindAsync(id);
            if (loc == null) return NotFound();
            return loc;
        }

        [HttpPost]
        public async Task<ActionResult<Loc>> PostLoc(Loc loc)
        {
            _context.Locuri.Add(loc);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLoc), new { id = loc.LocId }, loc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoc(int id, Loc loc)
        {
            if (id != loc.LocId) return BadRequest();

            _context.Entry(loc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoc(int id)
        {
            var loc = await _context.Locuri.FindAsync(id);
            if (loc == null) return NotFound();

            _context.Locuri.Remove(loc);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
