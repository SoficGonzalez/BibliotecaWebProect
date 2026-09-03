using BibliotecaPrjt.Models;
namespace BibliotecaPrjt.Repositories
{
    public interface IAutorService
    {
        IEnumerable<Autor> ObtenerAutores();
    }
}
