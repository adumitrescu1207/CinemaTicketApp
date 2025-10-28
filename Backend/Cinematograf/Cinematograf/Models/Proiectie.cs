using Cinematograf.Models;

namespace Cinematograf.Models
{
    public class Proiectie
    {
        public int ProiectieId { get; set; }
        public int FilmId { get; set; }
        public int SalaId { get; set; }
        public DateTime DataOraStart { get; set; }
        public DateTime? DataOraSfarsit { get; set; }
        public decimal Pret { get; set; }

        public Film Film { get; set; }
        public Sala Sala { get; set; }
        public ICollection<Bilet> Bilete { get; set; }
    }
}
