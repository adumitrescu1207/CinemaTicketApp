using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Cinematograf.Models;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizatorController : ControllerBase
    {
        private readonly string _connectionString;

        public UtilizatorController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Lipsește conexiunea 'DefaultConnection' din appsettings.json");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizator>>> GetUtilizatori()
        {
            var utilizatori = new List<Utilizator>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT UtilizatorId, Nume, Prenume, Email, ParolaHash FROM Utilizatori";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        utilizatori.Add(new Utilizator
                        {
                            UtilizatorId = reader.GetInt32(0),
                            Nume = reader.GetString(1),
                            Prenume = reader.GetString(2),
                            Email = reader.GetString(3),
                            ParolaHash = reader.GetString(4)
                        });
                    }
                }
            }

            return Ok(utilizatori);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Utilizator>> GetUtilizator(int id)
        {
            Utilizator? utilizator = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT UtilizatorId, Nume, Prenume, Email, ParolaHash FROM Utilizatori WHERE UtilizatorId = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            utilizator = new Utilizator
                            {
                                UtilizatorId = reader.GetInt32(0),
                                Nume = reader.GetString(1),
                                Prenume = reader.GetString(2),
                                Email = reader.GetString(3),
                                ParolaHash = reader.GetString(4)
                            };
                        }
                    }
                }
            }

            if (utilizator == null)
                return NotFound();

            return Ok(utilizator);
        }

        [HttpPost]
        public async Task<ActionResult<Utilizator>> PostUtilizator(Utilizator utilizator)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    INSERT INTO Utilizatori (Nume, Prenume, Email, ParolaHash)
                    OUTPUT INSERTED.UtilizatorId
                    VALUES (@Nume, @Prenume, @Email, @ParolaHash)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nume", utilizator.Nume);
                    command.Parameters.AddWithValue("@Prenume", utilizator.Prenume);
                    command.Parameters.AddWithValue("@Email", utilizator.Email);
                    command.Parameters.AddWithValue("@ParolaHash", utilizator.ParolaHash);

                    utilizator.UtilizatorId = (int)await command.ExecuteScalarAsync();
                }
            }

            return CreatedAtAction(nameof(GetUtilizator), new { id = utilizator.UtilizatorId }, utilizator);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtilizator(int id, Utilizator utilizator)
        {
            if (id != utilizator.UtilizatorId)
                return BadRequest();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    UPDATE Utilizatori
                    SET Nume = @Nume, Prenume = @Prenume, Email = @Email, ParolaHash = @ParolaHash
                    WHERE UtilizatorId = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Nume", utilizator.Nume);
                    command.Parameters.AddWithValue("@Prenume", utilizator.Prenume);
                    command.Parameters.AddWithValue("@Email", utilizator.Email);
                    command.Parameters.AddWithValue("@ParolaHash", utilizator.ParolaHash);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound();
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtilizator(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Utilizatori WHERE UtilizatorId = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                        return NotFound();
                }
            }

            return NoContent();
        }
    }
}
