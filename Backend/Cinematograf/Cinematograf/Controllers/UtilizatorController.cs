using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizatorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilizatorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizator>>> GetUtilizatori()
        {
            return await _context.Utilizatori.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Utilizator>> GetUtilizator(int id)
        {
            var utilizator = await _context.Utilizatori.FindAsync(id);
            if (utilizator == null) return NotFound();
            return utilizator;
        }

        [HttpPost]
        public async Task<ActionResult<Utilizator>> PostUtilizator(Utilizator utilizator)
        {
            _context.Utilizatori.Add(utilizator);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUtilizator), new { id = utilizator.UtilizatorId }, utilizator);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtilizator(int id, Utilizator utilizator)
        {
            if (id != utilizator.UtilizatorId) return BadRequest();

            _context.Entry(utilizator).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtilizator(int id)
        {
            var utilizator = await _context.Utilizatori.FindAsync(id);
            if (utilizator == null) return NotFound();

            _context.Utilizatori.Remove(utilizator);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
