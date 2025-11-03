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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(Utilizator dto)
        {
            if (await _context.Utilizatori.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Emailul este deja folosit.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.ParolaHash);

            var utilizator = new Utilizator
            {
                Nume = dto.Nume,
                Prenume = dto.Prenume,
                Email = dto.Email,
                ParolaHash = passwordHash,
                Telefon = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon
            };

            _context.Utilizatori.Add(utilizator);
            await _context.SaveChangesAsync();

            return Ok("Cont creat cu succes!");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            var utilizator = await _context.Utilizatori.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (utilizator == null) return BadRequest("Utilizator inexistent.");

            if (!BCrypt.Net.BCrypt.Verify(dto.ParolaHash, utilizator.ParolaHash))
                return BadRequest("Parolă incorectă.");

            var token = CreateToken(utilizator);

            return Ok(new { message = "Autentificare reușită!", token });
        }

        [HttpGet("me")]
        public async Task<ActionResult> Me()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { message = "Token lipsă sau invalid" });

                var jwt = authHeader.Substring("Bearer ".Length).Trim();

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var utilizatorId = int.Parse(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                var utilizator = await _context.Utilizatori.FindAsync(utilizatorId);
                if (utilizator == null) return Unauthorized();

                return Ok(new { utilizator.UtilizatorId, utilizator.Nume, utilizator.Email });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Delogat cu succes." });
        }

        private string CreateToken(Utilizator utilizator)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizator.UtilizatorId.ToString()),
                new Claim(ClaimTypes.Name, utilizator.Nume)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
