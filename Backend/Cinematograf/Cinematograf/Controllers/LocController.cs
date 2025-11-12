using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public LocController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loc>>> GetLocuri()
        {
            var locuri = new List<Loc>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT l.*, s.Nume AS NumeSala 
                                 FROM Locuri l 
                                 INNER JOIN Sali s ON l.SalaId = s.SalaId";

                SqlCommand cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    locuri.Add(new Loc
                    {
                        LocId = (int)reader["LocId"],
                        SalaId = (int)reader["SalaId"],
                        NumarRand = (int)reader["NumarRand"],
                        NumarLoc = (int)reader["NumarLoc"],
                        Sala = new Sala
                        {
                            SalaId = (int)reader["SalaId"],
                            Nume = reader["NumeSala"].ToString()
                        }
                    });
                }
            }

            return Ok(locuri);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Loc>> GetLoc(int id)
        {
            Loc loc = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT l.*, s.Nume AS NumeSala 
                                 FROM Locuri l 
                                 INNER JOIN Sali s ON l.SalaId = s.SalaId
                                 WHERE l.LocId = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    loc = new Loc
                    {
                        LocId = (int)reader["LocId"],
                        SalaId = (int)reader["SalaId"],
                        NumarRand = (int)reader["NumarRand"],
                        NumarLoc = (int)reader["NumarLoc"],
                        Sala = new Sala
                        {
                            SalaId = (int)reader["SalaId"],
                            Nume = reader["NumeSala"].ToString()
                        }
                    };
                }
            }

            if (loc == null)
                return NotFound();

            return Ok(loc);
        }

        [HttpGet("bysala/{salaId}")]
        public async Task<ActionResult<IEnumerable<Loc>>> GetLocuriBySala(int salaId)
        {
            var locuri = new List<Loc>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT l.*, s.Nume AS NumeSala 
                                 FROM Locuri l
                                 INNER JOIN Sali s ON l.SalaId = s.SalaId
                                 WHERE l.SalaId = @salaId
                                 ORDER BY l.NumarRand, l.NumarLoc";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@salaId", salaId);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    locuri.Add(new Loc
                    {
                        LocId = (int)reader["LocId"],
                        SalaId = (int)reader["SalaId"],
                        NumarRand = (int)reader["NumarRand"],
                        NumarLoc = (int)reader["NumarLoc"],
                        Sala = new Sala
                        {
                            SalaId = (int)reader["SalaId"],
                            Nume = reader["NumeSala"].ToString()
                        }
                    });
                }
            }

            if (!locuri.Any())
                return NotFound($"Nu există locuri pentru sala cu ID {salaId}.");

            return Ok(locuri);
        }

        [HttpPost]
        public async Task<ActionResult<Loc>> PostLoc(Loc loc)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Locuri (SalaId, NumarRand, NumarLoc)
                                 VALUES (@SalaId, @NumarRand, @NumarLoc);
                                 SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SalaId", loc.SalaId);
                cmd.Parameters.AddWithValue("@NumarRand", loc.NumarRand);
                cmd.Parameters.AddWithValue("@NumarLoc", loc.NumarLoc);

                await conn.OpenAsync();
                loc.LocId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            return CreatedAtAction(nameof(GetLoc), new { id = loc.LocId }, loc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoc(int id, Loc loc)
        {
            if (id != loc.LocId)
                return BadRequest();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Locuri 
                                 SET SalaId = @SalaId, NumarRand = @NumarRand, NumarLoc = @NumarLoc 
                                 WHERE LocId = @LocId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SalaId", loc.SalaId);
                cmd.Parameters.AddWithValue("@NumarRand", loc.NumarRand);
                cmd.Parameters.AddWithValue("@NumarLoc", loc.NumarLoc);
                cmd.Parameters.AddWithValue("@LocId", loc.LocId);

                await conn.OpenAsync();
                int affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoc(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Locuri WHERE LocId = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
