using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
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
                string query = @"
                SELECT *
                FROM Bilete
                WHERE ProiectieId IN (
                    SELECT ProiectieId FROM Proiectii
                )
                AND UtilizatorId IN (
                    SELECT UtilizatorId FROM Utilizatori
                )";

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
                string query = @"
                SELECT *
                FROM Bilete
                WHERE BiletId IN (
                    SELECT BiletId
                    FROM Bilete
                    WHERE BiletId = @id
                )";

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

        // GET: api/bilet/utilizator/{utilizatorId}
        [HttpGet("utilizator/{utilizatorId}")]
        public async Task<IActionResult> GetBileteByUtilizator(int utilizatorId)
        {
            var bilete = new List<BiletDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
            SELECT 
                b.BiletId,
                b.DataRezervare,
                b.Status,
                (SELECT p.ProiectieId FROM Proiectii p WHERE p.ProiectieId = b.ProiectieId) AS ProiectieId,
                (SELECT p.DataOraStart FROM Proiectii p WHERE p.ProiectieId = b.ProiectieId) AS DataOraStart,
                (SELECT f.Titlu FROM Filme f 
                 WHERE f.FilmId = (SELECT p2.FilmId FROM Proiectii p2 WHERE p2.ProiectieId = b.ProiectieId)
                ) AS TitluFilm,
                (SELECT s.SalaId FROM Sali s 
                 WHERE s.SalaId = (SELECT p3.SalaId FROM Proiectii p3 WHERE p3.ProiectieId = b.ProiectieId)
                ) AS SalaId,
                (SELECT s.Nume FROM Sali s 
                 WHERE s.SalaId = (SELECT p4.SalaId FROM Proiectii p4 WHERE p4.ProiectieId = b.ProiectieId)
                ) AS NumeSala,
                (SELECT l.LocId FROM Locuri l WHERE l.LocId = b.LocId) AS LocId,
                (SELECT l.NumarRand FROM Locuri l WHERE l.LocId = b.LocId) AS NumarRand,
                (SELECT l.NumarLoc FROM Locuri l WHERE l.LocId = b.LocId) AS NumarLoc
            FROM Bilete b
            WHERE b.UtilizatorId = @userId
        ", conn);

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
                string query = @"
                SELECT LocId
                FROM Locuri
                WHERE LocId IN (
                    SELECT LocId
                    FROM Bilete
                    WHERE ProiectieId = @pid
                )";

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
                var jwtHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(jwtHeader))
                    return Unauthorized();

                var jwt = jwtHeader.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                handler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                var token = (JwtSecurityToken)validatedToken;
                bilet.UtilizatorId = int.Parse(token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                bilet.DataRezervare = DateTime.UtcNow;
                bilet.Status ??= "In asteptare";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string insertQuery = @"
                    INSERT INTO Bilete (ProiectieId, SalaId, LocId, UtilizatorId, DataRezervare, Status)
                    SELECT 
                        @ProiectieId,
                        SalaId,
                        @LocId,
                        @UtilizatorId,
                        @DataRezervare,
                        @Status
                    FROM Proiectii
                    WHERE ProiectieId IN (
                        SELECT ProiectieId
                        FROM Proiectii
                        WHERE ProiectieId = @ProiectieId
                    );

                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@ProiectieId", bilet.ProiectieId);
                    cmd.Parameters.AddWithValue("@LocId", bilet.LocId);
                    cmd.Parameters.AddWithValue("@UtilizatorId", bilet.UtilizatorId);
                    cmd.Parameters.AddWithValue("@DataRezervare", bilet.DataRezervare);
                    cmd.Parameters.AddWithValue("@Status", bilet.Status);

                    await conn.OpenAsync();
                    bilet.BiletId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                return Ok(bilet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/bilet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilet(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                DELETE FROM Bilete
                WHERE BiletId IN (
                    SELECT BiletId
                    FROM Bilete
                    WHERE BiletId = @id
                )";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return NotFound();

                return NoContent();
            }
        }

        // PUT: api/bilet/confirmare/{id}
        [HttpPut("confirmare/{id}")]
        public async Task<IActionResult> ConfirmBilet(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                UPDATE Bilete
                SET Status = 'Confirmat'
                WHERE BiletId IN (
                    SELECT BiletId
                    FROM Bilete
                    WHERE BiletId = @id
                )";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return NotFound();

                return Ok();
            }
        }

        // GET: api/bilet/pret/{biletId}
        [HttpGet("pret/{biletId}")]
        public async Task<IActionResult> GetPretBilet(int biletId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT Pret
                FROM Proiectii
                WHERE ProiectieId IN (
                    SELECT ProiectieId
                    FROM Bilete
                    WHERE BiletId = @id
                )";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", biletId);

                await conn.OpenAsync();
                var pret = await cmd.ExecuteScalarAsync();

                if (pret == null)
                    return NotFound();

                return Ok(pret);
            }
        }
    }
}
