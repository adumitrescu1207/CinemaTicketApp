using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cinematograf.Models;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Lipsește conexiunea 'DefaultConnection' din appsettings.json");
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(Utilizator dto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Utilizatori WHERE Email = @Email", connection);
                checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                int exists = (int)await checkCmd.ExecuteScalarAsync();

                if (exists > 0)
                    return BadRequest("Emailul este deja folosit.");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.ParolaHash);

                var insertCmd = new SqlCommand(@"
                    INSERT INTO Utilizatori (Nume, Prenume, Email, ParolaHash, Telefon)
                    VALUES (@Nume, @Prenume, @Email, @ParolaHash, @Telefon)", connection);

                insertCmd.Parameters.AddWithValue("@Nume", dto.Nume ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Prenume", dto.Prenume ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Email", dto.Email);
                insertCmd.Parameters.AddWithValue("@ParolaHash", passwordHash);
                insertCmd.Parameters.AddWithValue("@Telefon", string.IsNullOrWhiteSpace(dto.Telefon) ? DBNull.Value : dto.Telefon);

                await insertCmd.ExecuteNonQueryAsync();
            }

            return Ok("Cont creat cu succes!");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            Utilizator? utilizator = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var cmd = new SqlCommand("SELECT UtilizatorId, Nume, Prenume, Email, ParolaHash FROM Utilizatori WHERE Email = @Email", connection);
                cmd.Parameters.AddWithValue("@Email", dto.Email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        utilizator = new Utilizator
                        {
                            UtilizatorId = reader.GetInt32(0),
                            Nume = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Prenume = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Email = reader.GetString(3),
                            ParolaHash = reader.GetString(4)
                        };
                    }
                }
            }

            if (utilizator == null)
                return BadRequest("Utilizator inexistent.");

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

                Utilizator? utilizator = null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var cmd = new SqlCommand("SELECT UtilizatorId, Nume, Prenume, Email FROM Utilizatori WHERE UtilizatorId = @Id", connection);
                    cmd.Parameters.AddWithValue("@Id", utilizatorId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            utilizator = new Utilizator
                            {
                                UtilizatorId = reader.GetInt32(0),
                                Nume = reader.GetString(1),
                                Prenume = reader.GetString(2),
                                Email = reader.GetString(3)
                            };
                        }
                    }
                }

                if (utilizator == null)
                    return Unauthorized();

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
                new Claim(ClaimTypes.Name, utilizator.Nume ?? "")
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
