using Microsoft.EntityFrameworkCore;
using CongresoUMG.Models;

namespace CongresoUMG.Data
{
    public class CongresoContext : DbContext
    {
        public CongresoContext(DbContextOptions<CongresoContext> options) : base(options) { }

        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

    }
}
