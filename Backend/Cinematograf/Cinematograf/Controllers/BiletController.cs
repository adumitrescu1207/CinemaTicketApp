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
    public class BiletController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public BiletController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        // GET: api/bilet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bilet>>> GetBilete()
        {
            var bilete = new List<Bilet>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT b.*, 
                                        p.FilmId, p.SalaId, 
                                        u.UtilizatorId, u.Nume AS NumeUtilizator, 
                                        l.Numar AS NumarLoc
                                 FROM Bilete b
                                 LEFT JOIN Proiectii p ON b.ProiectieId = p.ProiectieId
                                 LEFT JOIN Utilizatori u ON b.UtilizatorId = u.UtilizatorId
                                 LEFT JOIN Locuri l ON b.LocId = l.LocId";

                SqlCommand cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    bilete.Add(new Bilet
                    {
                        BiletId = (int)reader["BiletId"],
                        ProiectieId = (int)reader["ProiectieId"],
                        SalaId = (int)reader["SalaId"],
                        LocId = (int)reader["LocId"],
                        UtilizatorId = (int)reader["UtilizatorId"],
                        DataRezervare = Convert.ToDateTime(reader["DataRezervare"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return Ok(bilete);
        }

        // GET: api/bilet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Bilet>> GetBilet(int id)
        {
            Bilet bilet = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM Bilete WHERE BiletId = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    bilet = new Bilet
                    {
                        BiletId = (int)reader["BiletId"],
                        ProiectieId = (int)reader["ProiectieId"],
                        SalaId = (int)reader["SalaId"],
                        LocId = (int)reader["LocId"],
                        UtilizatorId = (int)reader["UtilizatorId"],
                        DataRezervare = Convert.ToDateTime(reader["DataRezervare"]),
                        Status = reader["Status"].ToString()
                    };
                }
            }

            if (bilet == null)
                return NotFound();

            return Ok(bilet);
        }

        [HttpGet("utilizator/{utilizatorId}")]
        public async Task<IActionResult> GetBileteByUtilizator(int utilizatorId)
        {
            var bilete = new List<BiletDto>();

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
            SELECT b.BiletId, b.DataRezervare, b.Status,
                   p.ProiectieId, p.DataOraStart, f.Titlu AS TitluFilm,
                   s.SalaId, s.Nume AS NumeSala,
                   l.LocId, l.NumarRand, l.NumarLoc
            FROM Bilete b
            INNER JOIN Proiectii p ON b.ProiectieId = p.ProiectieId
            INNER JOIN Filme f ON p.FilmId = f.FilmId
            INNER JOIN Sali s ON p.SalaId = s.SalaId
            LEFT JOIN Locuri l ON b.LocId = l.LocId
            WHERE b.UtilizatorId = @userId", conn);

                cmd.Parameters.AddWithValue("@userId", utilizatorId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bilete.Add(new BiletDto
                        {
                            BiletId = reader.GetInt32(reader.GetOrdinal("BiletId")),
                            DataRezervare = reader.GetDateTime(reader.GetOrdinal("DataRezervare")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Proiectie = new ProiectieDto
                            {
                                ProiectieId = reader.GetInt32(reader.GetOrdinal("ProiectieId")),
                                DataOraStart = reader.GetDateTime(reader.GetOrdinal("DataOraStart")),
                                TitluFilm = reader.GetString(reader.GetOrdinal("TitluFilm")),
                                NumeSala = reader.GetString(reader.GetOrdinal("NumeSala"))
                            },
                            Loc = reader.IsDBNull(reader.GetOrdinal("LocId")) ? null : new LocDto
                            {
                                LocId = reader.GetInt32(reader.GetOrdinal("LocId")),
                                NumarRand = reader.GetInt32(reader.GetOrdinal("NumarRand")),
                                NumarLoc = reader.GetInt32(reader.GetOrdinal("NumarLoc"))
                            }
                        });
                    }
                }
            }

            return Ok(bilete);
        }


        // GET: api/bilet/ocupate/{proiectieId}
        [HttpGet("ocupate/{proiectieId}")]
        public async Task<ActionResult<IEnumerable<int>>> GetLocuriOcupate(int proiectieId)
        {
            var locuri = new List<int>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT LocId FROM Bilete WHERE ProiectieId = @pid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pid", proiectieId);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    locuri.Add((int)reader["LocId"]);
            }

            return Ok(locuri);
        }

        // POST: api/bilet
        [HttpPost]
        public async Task<ActionResult<Bilet>> PostBilet([FromBody] Bilet bilet)
        {
            try
            {
                // === JWT VALIDATION ===
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

                // === Complete data ===
                bilet.UtilizatorId = utilizatorId;
                bilet.DataRezervare = DateTime.UtcNow;
                if (string.IsNullOrEmpty(bilet.Status))
                    bilet.Status = "In asteptare";

                // Get SalaId based on ProiectieId
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string getSalaQuery = "SELECT SalaId FROM Proiectii WHERE ProiectieId = @pid";
                    SqlCommand salaCmd = new SqlCommand(getSalaQuery, conn);
                    salaCmd.Parameters.AddWithValue("@pid", bilet.ProiectieId);

                    await conn.OpenAsync();
                    var salaResult = await salaCmd.ExecuteScalarAsync();
                    if (salaResult == null)
                        return BadRequest("Proiecție invalidă.");
                    bilet.SalaId = Convert.ToInt32(salaResult);

                    // === INSERT BILET ===
                    string insertQuery = @"INSERT INTO Bilete (ProiectieId, SalaId, LocId, UtilizatorId, DataRezervare, Status)
                                           VALUES (@ProiectieId, @SalaId, @LocId, @UtilizatorId, @DataRezervare, @Status);
                                           SELECT SCOPE_IDENTITY();";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@ProiectieId", bilet.ProiectieId);
                    insertCmd.Parameters.AddWithValue("@SalaId", bilet.SalaId);
                    insertCmd.Parameters.AddWithValue("@LocId", bilet.LocId);
                    insertCmd.Parameters.AddWithValue("@UtilizatorId", bilet.UtilizatorId);
                    insertCmd.Parameters.AddWithValue("@DataRezervare", bilet.DataRezervare);
                    insertCmd.Parameters.AddWithValue("@Status", bilet.Status);

                    bilet.BiletId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
                }

                return CreatedAtAction(nameof(GetBilet), new { id = bilet.BiletId }, bilet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/bilet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilet(int id)
        {
            int rowsAffected;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Bilete WHERE BiletId = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                rowsAffected = await cmd.ExecuteNonQueryAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }
    }
}
