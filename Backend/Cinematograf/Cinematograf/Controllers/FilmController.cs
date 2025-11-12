using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Cinematograf.Models;

namespace Cinematograf.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public FilmController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        // GET: api/film
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var filme = new List<Film>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Filme";
                SqlCommand cmd = new SqlCommand(query, conn);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    filme.Add(new Film
                    {
                        FilmId = (int)reader["FilmId"],
                        Titlu = reader["Titlu"].ToString(),
                        Descriere = reader["Descriere"]?.ToString(),
                        Gen = reader["Gen"]?.ToString(),
                        Durata = (int)reader["Durata"],
                        DataLansare = Convert.ToDateTime(reader["DataLansare"])
                    });
                }
            }

            return Ok(filme);
        }

        // GET: api/film/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Film film = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Filme WHERE FilmId = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    film = new Film
                    {
                        FilmId = (int)reader["FilmId"],
                        Titlu = reader["Titlu"].ToString(),
                        Descriere = reader["Descriere"]?.ToString(),
                        Gen = reader["Gen"]?.ToString(),
                        Durata = (int)reader["Durata"],
                        DataLansare = Convert.ToDateTime(reader["DataLansare"])
                    };
                }
            }

            if (film == null)
                return NotFound();

            return Ok(film);
        }

        // POST: api/film
        [HttpPost]
        public async Task<IActionResult> Create(Film film)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Filme (Titlu, Descriere, Gen, Durata, DataLansare)
                                 VALUES (@Titlu, @Descriere, @Gen, @Durata, @DataLansare);
                                 SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Titlu", film.Titlu);
                cmd.Parameters.AddWithValue("@Descriere", (object?)film.Descriere ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gen", (object?)film.Gen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Durata", film.Durata);
                cmd.Parameters.AddWithValue("@DataLansare", film.DataLansare);

                await conn.OpenAsync();
                film.FilmId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            return CreatedAtAction(nameof(GetById), new { id = film.FilmId }, film);
        }

        // PUT: api/film/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Film film)
        {
            if (id != film.FilmId)
                return BadRequest();

            int rowsAffected;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Filme 
                                 SET Titlu = @Titlu, Descriere = @Descriere, Gen = @Gen, Durata = @Durata, DataLansare = @DataLansare
                                 WHERE FilmId = @FilmId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Titlu", film.Titlu);
                cmd.Parameters.AddWithValue("@Descriere", (object?)film.Descriere ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gen", (object?)film.Gen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Durata", film.Durata);
                cmd.Parameters.AddWithValue("@DataLansare", film.DataLansare);
                cmd.Parameters.AddWithValue("@FilmId", film.FilmId);

                await conn.OpenAsync();
                rowsAffected = await cmd.ExecuteNonQueryAsync();
            }

            if (rowsAffected == 0)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/film/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int rowsAffected;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Filme WHERE FilmId = @id";
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
