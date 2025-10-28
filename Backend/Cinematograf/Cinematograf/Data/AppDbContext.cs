using Cinematograf.Models;
using Microsoft.EntityFrameworkCore;
namespace Cinematograf.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Film> Filme { get; set; }
        public DbSet<Sala> Sali { get; set; }
        public DbSet<Proiectie> Proiectii { get; set; }
        public DbSet<Loc> Locuri { get; set; }
        public DbSet<Utilizator> Utilizatori { get; set; }
        public DbSet<Bilet> Bilete { get; set; }
    }
}
