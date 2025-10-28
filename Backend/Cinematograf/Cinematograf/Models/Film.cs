namespace Cinematograf.Models
{
    public class Film
    {
        public int FilmId { get; set; }
        public string Titlu { get; set; }
        public string? Descriere { get; set; }
        public int Durata { get; set; }
        public DateTime? DataLansare { get; set; }
        public string? Gen { get; set; }

        public ICollection<Proiectie> Proiectii { get; set; }
    }
}
