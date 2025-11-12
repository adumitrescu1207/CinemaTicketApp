using Cinematograf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cinematograf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProiectieController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public ProiectieController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proiectie>>> GetProiectii()
        {
            var proiectii = new List<Proiectie>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT p.*, f.Titlu AS TitluFilm, s.Nume AS NumeSala
                                 FROM Proiectii p
                                 INNER JOIN Filme f ON p.FilmId = f.FilmId
                                 INNER JOIN Sali s ON p.SalaId = s.SalaId";

                SqlCommand cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    proiectii.Add(new Proiectie
                    {
                        ProiectieId = (int)reader["ProiectieId"],
                        FilmId = (int)reader["FilmId"],
                        SalaId = (int)reader["SalaId"],
                        DataOraStart = (DateTime)reader["DataOraStart"],
                        DataOraSfarsit = (DateTime)reader["DataOraSfarsit"],
                        Pret = Convert.ToDecimal(reader["Pret"]),
                        Film = new Film { Titlu = reader["TitluFilm"].ToString() },
                        Sala = new Sala { Nume = reader["NumeSala"].ToString() }
                    });
                }
            }

            return Ok(proiectii);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Proiectie>> GetProiectie(int id)
        {
            Proiectie proiectie = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT p.*, f.Titlu AS TitluFilm, s.Nume AS NumeSala
                                 FROM Proiectii p
                                 INNER JOIN Filme f ON p.FilmId = f.FilmId
                                 INNER JOIN Sali s ON p.SalaId = s.SalaId
                                 WHERE p.ProiectieId = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    proiectie = new Proiectie
                    {
                        ProiectieId = (int)reader["ProiectieId"],
                        FilmId = (int)reader["FilmId"],
                        SalaId = (int)reader["SalaId"],
                        DataOraStart = (DateTime)reader["DataOraStart"],
                        DataOraSfarsit = (DateTime)reader["DataOraSfarsit"],
                        Pret = Convert.ToDecimal(reader["Pret"]),
                        Film = new Film { Titlu = reader["TitluFilm"].ToString() },
                        Sala = new Sala { Nume = reader["NumeSala"].ToString() }
                    };
                }
            }

            if (proiectie == null) return NotFound();
            return Ok(proiectie);
        }

        [HttpGet("byfilm/{filmId}")]
        public async Task<ActionResult<IEnumerable<ProiectieDto>>> GetProiectiiByFilm(int filmId)
        {
            var proiectii = new List<ProiectieDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT p.ProiectieId, f.Titlu AS TitluFilm, s.Nume AS NumeSala,
                                        p.DataOraStart, p.DataOraSfarsit, p.Pret
                                 FROM Proiectii p
                                 INNER JOIN Filme f ON p.FilmId = f.FilmId
                                 INNER JOIN Sali s ON p.SalaId = s.SalaId
                                 WHERE p.FilmId = @filmId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@filmId", filmId);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    proiectii.Add(new ProiectieDto
                    {
                        ProiectieId = (int)reader["ProiectieId"],
                        TitluFilm = reader["TitluFilm"].ToString(),
                        NumeSala = reader["NumeSala"].ToString(),
                        DataOraStart = (DateTime)reader["DataOraStart"],
                        DataOraSfarsit = (DateTime)reader["DataOraSfarsit"],
                        Pret = Convert.ToDecimal(reader["Pret"])
                    });
                }
            }

            if (!proiectii.Any()) return NotFound();
            return Ok(proiectii);
        }

        [HttpPost]
        public async Task<ActionResult> PostProiectie(Proiectie proiectie)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Proiectii (FilmId, SalaId, DataOraStart, DataOraSfarsit, Pret)
                                 VALUES (@FilmId, @SalaId, @DataOraStart, @DataOraSfarsit, @Pret);
                                 SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FilmId", proiectie.FilmId);
                cmd.Parameters.AddWithValue("@SalaId", proiectie.SalaId);
                cmd.Parameters.AddWithValue("@DataOraStart", proiectie.DataOraStart);
                cmd.Parameters.AddWithValue("@DataOraSfarsit", proiectie.DataOraSfarsit);
                cmd.Parameters.AddWithValue("@Pret", proiectie.Pret);

                await conn.OpenAsync();
                proiectie.ProiectieId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            return CreatedAtAction(nameof(GetProiectie), new { id = proiectie.ProiectieId }, proiectie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProiectie(int id, Proiectie proiectie)
        {
            if (id != proiectie.ProiectieId) return BadRequest();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Proiectii 
                                 SET FilmId=@FilmId, SalaId=@SalaId, DataOraStart=@DataOraStart, 
                                     DataOraSfarsit=@DataOraSfarsit, Pret=@Pret
                                 WHERE ProiectieId=@ProiectieId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProiectieId", proiectie.ProiectieId);
                cmd.Parameters.AddWithValue("@FilmId", proiectie.FilmId);
                cmd.Parameters.AddWithValue("@SalaId", proiectie.SalaId);
                cmd.Parameters.AddWithValue("@DataOraStart", proiectie.DataOraStart);
                cmd.Parameters.AddWithValue("@DataOraSfarsit", proiectie.DataOraSfarsit);
                cmd.Parameters.AddWithValue("@Pret", proiectie.Pret);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProiectie(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Proiectii WHERE ProiectieId = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0) return NotFound();
            }

            return NoContent();
        }
    }
}
