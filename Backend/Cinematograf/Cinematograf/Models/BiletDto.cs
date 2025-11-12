namespace Cinematograf.Models
{
    public class BiletDto
    {
        public int BiletId { get; set; }
        public DateTime DataRezervare { get; set; }
        public string Status { get; set; }
        public ProiectieDto Proiectie { get; set; }
        public LocDto Loc { get; set; }
    }

    public class LocDto
    {
        public int LocId { get; set; }
        public int NumarRand { get; set; }
        public int NumarLoc { get; set; }
    }
}
