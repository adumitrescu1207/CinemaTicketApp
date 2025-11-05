using Cinematograf.Data;
using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiletController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public BiletController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

        [HttpGet("utilizator/{utilizatorId}")]
        public async Task<ActionResult<IEnumerable<Bilet>>> GetBileteByUtilizator(int utilizatorId)
        {
            var bilete = await _context.Bilete
                .Include(b => b.Proiectie)
                    .ThenInclude(p => p.Film)
                .Include(b => b.Proiectie)
                    .ThenInclude(p => p.Sala)
                .Include(b => b.Loc)
                .Include(b => b.Utilizator)
                .Where(b => b.UtilizatorId == utilizatorId)
                .ToListAsync();

            if (!bilete.Any()) return NotFound("Acest utilizator nu are bilete.");

            return Ok(bilete);
        }

        [HttpGet("ocupate/{proiectieId}")]
        public async Task<ActionResult<IEnumerable<int>>> GetLocuriOcupate(int proiectieId)
        {
            var locuri = await _context.Bilete
                .Where(b => b.ProiectieId == proiectieId)
                .Select(b => b.LocId)
                .ToListAsync();

            return Ok(locuri);
        }


        [HttpPost]
        public async Task<ActionResult<Bilet>> PostBilet([FromBody] Bilet bilet)
        {
            try
            {
                var jwtHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(jwtHeader))
                    return Unauthorized(new { message = "JWT lipsă" });

                var jwt = jwtHeader.Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var utilizatorId = int.Parse(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                bilet.UtilizatorId = utilizatorId;
                bilet.DataRezervare = DateTime.UtcNow;
                if (string.IsNullOrEmpty(bilet.Status))
                    bilet.Status = "In asteptare";

                var proiectie = await _context.Proiectii.FindAsync(bilet.ProiectieId);
                if (proiectie == null) return BadRequest("Proiecție invalidă.");
                bilet.SalaId = proiectie.SalaId;

                _context.Bilete.Add(bilet);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBilet), new { id = bilet.BiletId }, bilet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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
