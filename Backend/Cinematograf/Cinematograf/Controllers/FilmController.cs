using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinematograf.Data;
using Cinematograf.Models;

namespace Cinematograf.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilmController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var filme = await _context.Filme.ToListAsync();
            return Ok(filme);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var film = await _context.Filme.FindAsync(id);
            if (film == null) return NotFound();
            return Ok(film);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Film film)
        {
            _context.Filme.Add(film);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = film.FilmId }, film);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Film film)
        {
            if (id != film.FilmId) return BadRequest();

            _context.Entry(film).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilmExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var film = await _context.Filme.FindAsync(id);
            if (film == null) return NotFound();

            _context.Filme.Remove(film);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FilmExists(int id)
        {
            return _context.Filme.Any(f => f.FilmId == id);
        }
    }
}
