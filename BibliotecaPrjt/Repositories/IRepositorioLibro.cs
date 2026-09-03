using BibliotecaPrjt.Models;

namespace BibliotecaPrjt.Repositories
{
    public interface IRepositorioLibro
    {
        IEnumerable<Libro> ObtenerTodos();
        
    }
}
