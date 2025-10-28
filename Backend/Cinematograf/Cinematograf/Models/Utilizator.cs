namespace Cinematograf.Models
{
    public class Utilizator
    {
        public int UtilizatorId { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string ParolaHash { get; set; }
        public string? Telefon { get; set; }

        public ICollection<Bilet>? Bilete { get; set; }
    }
}
