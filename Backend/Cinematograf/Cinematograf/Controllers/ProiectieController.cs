using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProiectieController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProiectieController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proiectie>>> GetProiectii()
        {
            return await _context.Proiectii
                .Include(p => p.Film)
                .Include(p => p.Sala)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Proiectie>> GetProiectie(int id)
        {
            var proiectie = await _context.Proiectii
                .Include(p => p.Film)
                .Include(p => p.Sala)
                .FirstOrDefaultAsync(p => p.ProiectieId == id);

            if (proiectie == null) return NotFound();
            return proiectie;
        }

        [HttpPost]
        public async Task<ActionResult<Proiectie>> PostProiectie(Proiectie proiectie)
        {
            _context.Proiectii.Add(proiectie);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProiectie), new { id = proiectie.ProiectieId }, proiectie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProiectie(int id, Proiectie proiectie)
        {
            if (id != proiectie.ProiectieId) return BadRequest();

            _context.Entry(proiectie).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProiectie(int id)
        {
            var proiectie = await _context.Proiectii.FindAsync(id);
            if (proiectie == null) return NotFound();

            _context.Proiectii.Remove(proiectie);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
