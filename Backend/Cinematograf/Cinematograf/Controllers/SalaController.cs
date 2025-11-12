using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Cinematograf.Models;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaController : ControllerBase
    {
        private readonly string _connectionString;

        public SalaController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Lipsește conexiunea 'DefaultConnection' din appsettings.json");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sala>>> GetSali()
        {
            var sali = new List<Sala>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var cmd = new SqlCommand("SELECT SalaId, Nume, Randuri, LocuriPeRand FROM Sali", connection);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sali.Add(new Sala
                        {
                            SalaId = reader.GetInt32(0),
                            Nume = reader.GetString(1),
                            Randuri = reader.GetInt32(2),
                            LocuriPeRand = reader.GetInt32(3)
                        });
                    }
                }
            }

            return Ok(sali);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sala>> GetSala(int id)
        {
            Sala? sala = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var cmd = new SqlCommand("SELECT SalaId, Nume, Randuri, LocuriPeRand FROM Sali WHERE SalaId = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        sala = new Sala
                        {
                            SalaId = reader.GetInt32(0),
                            Nume = reader.GetString(1),
                            Randuri = reader.GetInt32(2),
                            LocuriPeRand = reader.GetInt32(3)
                        };
                    }
                }
            }

            if (sala == null)
                return NotFound();

            return Ok(sala);
        }

        [HttpPost]
        public async Task<ActionResult<Sala>> PostSala(Sala sala)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var cmd = new SqlCommand(@"
                    INSERT INTO Sali (Nume, Randuri, LocuriPeRand)
                    VALUES (@Nume, @Randuri, @LocuriPeRand);
                    SELECT SCOPE_IDENTITY();", connection);

                cmd.Parameters.AddWithValue("@Nume", sala.Nume);
                cmd.Parameters.AddWithValue("@Randuri", sala.Randuri);
                cmd.Parameters.AddWithValue("@LocuriPeRand", sala.LocuriPeRand);

                var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                sala.SalaId = insertedId;
            }

            return CreatedAtAction(nameof(GetSala), new { id = sala.SalaId }, sala);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSala(int id, Sala sala)
        {
            if (id != sala.SalaId)
                return BadRequest();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var cmd = new SqlCommand(@"
                    UPDATE Sali 
                    SET Nume = @Nume, Randuri = @Randuri, LocuriPeRand = @LocuriPeRand
                    WHERE SalaId = @Id", connection);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nume", sala.Nume);
                cmd.Parameters.AddWithValue("@Randuri", sala.Randuri);
                cmd.Parameters.AddWithValue("@LocuriPeRand", sala.LocuriPeRand);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSala(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var cmd = new SqlCommand("DELETE FROM Sali WHERE SalaId = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", id);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
