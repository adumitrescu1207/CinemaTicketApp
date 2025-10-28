namespace Cinematograf.Models
{
    public class Sala
    {
        public int SalaId { get; set; }
        public string? Nume { get; set; }
        public int Randuri { get; set; }
        public int LocuriPeRand { get; set; }

        public ICollection<Proiectie>? Proiectii { get; set; }
        public ICollection<Loc>? Locuri { get; set; }
    }
}

