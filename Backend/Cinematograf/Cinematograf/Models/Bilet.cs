namespace Cinematograf.Models
{
    public class Bilet
    {
        public int BiletId { get; set; }
        public int ProiectieId { get; set; }
        public int UtilizatorId { get; set; }
        public int? LocId { get; set; }
        public DateTime DataRezervare { get; set; }
        public string Status { get; set; } = "In asteptare";

        public Proiectie? Proiectie { get; set; }
        public Utilizator? Utilizator { get; set; }
        public Loc? Loc { get; set; }
    }
}
