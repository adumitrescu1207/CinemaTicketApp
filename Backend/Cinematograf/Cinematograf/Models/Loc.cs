namespace Cinematograf.Models
{
    public class Loc
    {
        public int LocId { get; set; }
        public int SalaId { get; set; }
        public int NumarRand { get; set; }
        public int NumarLoc { get; set; }

        public Sala? Sala { get; set; }
        public ICollection<Bilet>? Bilete { get; set; }
    }
}
