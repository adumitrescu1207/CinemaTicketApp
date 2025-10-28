using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sala>>> GetSali()
        {
            return await _context.Sali.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sala>> GetSala(int id)
        {
            var sala = await _context.Sali.FindAsync(id);
            if (sala == null) return NotFound();
            return sala;
        }

        [HttpPost]
        public async Task<ActionResult<Sala>> PostSala(Sala sala)
        {
            _context.Sali.Add(sala);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSala), new { id = sala.SalaId }, sala);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSala(int id, Sala sala)
        {
            if (id != sala.SalaId) return BadRequest();

            _context.Entry(sala).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSala(int id)
        {
            var sala = await _context.Sali.FindAsync(id);
            if (sala == null) return NotFound();

            _context.Sali.Remove(sala);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

