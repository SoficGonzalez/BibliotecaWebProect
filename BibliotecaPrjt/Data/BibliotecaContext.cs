using BibliotecaPrjt.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaPrjt.Data
{
    public class BibliotecaContext : DbContext
    {
        public BibliotecaContext(DbContextOptions<BibliotecaContext>options) : base(options){}

        public DbSet<Autor> Autores { get; set; }
    }
}
