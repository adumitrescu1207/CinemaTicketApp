using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiletController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BiletController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bilet>>> GetBilete()
        {
            return await _context.Bilete
                .Include(b => b.Proiectie)
                .Include(b => b.Utilizator)
                .Include(b => b.Loc)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bilet>> GetBilet(int id)
        {
            var bilet = await _context.Bilete
                .Include(b => b.Proiectie)
                .Include(b => b.Utilizator)
                .Include(b => b.Loc)
                .FirstOrDefaultAsync(b => b.BiletId == id);

            if (bilet == null) return NotFound();
            return bilet;
        }

        [HttpPost]
        public async Task<ActionResult<Bilet>> PostBilet(Bilet bilet)
        {
            _context.Bilete.Add(bilet);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBilet), new { id = bilet.BiletId }, bilet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBilet(int id, Bilet bilet)
        {
            if (id != bilet.BiletId) return BadRequest();

            _context.Entry(bilet).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilet(int id)
        {
            var bilet = await _context.Bilete.FindAsync(id);
            if (bilet == null) return NotFound();

            _context.Bilete.Remove(bilet);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
