using BibliotecaPrjt.Controllers;
using BibliotecaPrjt.Data;
using BibliotecaPrjt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BibliotecaPrjt.Tests;

public class LibroControllerTest
{

    [Fact]
    // Con libros registrados, Index devuelve la lista completa.
    public async Task Index_ConLibrosRegistrados_DevuelveLaListaCompleta()
    {
        await using var context = CreateContext();
        context.Libros.AddRange(NuevoLibro("La vegetariana"), NuevoLibro("Cien años de soledad"));
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model);
        Assert.Equal(2, libros.Count());
    }

    [Fact]
    // Sin libros, Index devuelve una vista con la lista vacia (no null ni error).
    public async Task Index_SinLibros_DevuelveListaVacia()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model);
        Assert.Empty(libros);
    }

    [Fact]
    // Index registra la consulta en el log y no registra errores.
    public async Task Index_RegistraLaConsultaSinErrores()
    {
        await using var context = CreateContext();
        var logger = new Mock<ILogger<LibroController>>();
        var controller = CreateController(context, logger);

        await controller.Index();

        VerifyLog(logger, LogLevel.Information, Times.Once());
        VerifyLog(logger, LogLevel.Error, Times.Never());
    }

    [Fact]
    // Si la base de datos falla, Index registra el error y relanza la excepcion.
    public async Task Index_CuandoFallaLaBaseDeDatos_RegistraErrorYRelanza()
    {
        var context = CreateContext();
        var logger = new Mock<ILogger<LibroController>>();
        var controller = CreateController(context, logger);
        // Un contexto liberado provoca una excepcion al consultar.
        await context.DisposeAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => controller.Index());

        VerifyLog(logger, LogLevel.Error, Times.Once());
    }

    // ===================== Details =====================

    [Fact]
    // Details devuelve el libro cuando existe.
    public async Task Details_ConLibroExistente_DevuelveElLibro()
    {
        await using var context = CreateContext();
        var libro = NuevoLibro("Rayuela");
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.Details(libro.ID);

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.Model);
        Assert.Same(libro, view.Model);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    // Details devuelve NotFound para cualquier identificador que no existe.
    public async Task Details_ConIdentificadorInexistente_DevuelveNotFound(int id)
    {
        await using var context = CreateContext();
        context.Libros.Add(NuevoLibro()); 
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.Details(id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Al no encontrar el libro, Details registra una advertencia.
    public async Task Details_ConLibroInexistente_RegistraUnaAdvertencia()
    {
        await using var context = CreateContext();
        var logger = new Mock<ILogger<LibroController>>();
        var controller = CreateController(context, logger);

        await controller.Details(999);

        VerifyLog(logger, LogLevel.Warning, Times.Once());
    }

    // ===================== Create =====================

    [Fact]
    // El GET de Create devuelve el formulario.
    public void Create_Get_DevuelveLaVista()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    // Sin portada, Create guarda el libro con la imagen por defecto y redirige.
    public async Task Create_ConLibroValidoSinPortada_GuardaConImagenPorDefectoYRedirige()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.Create(NuevoLibro("Libro nuevo"), null!);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibroController.Index), redirect.ActionName);
        var guardado = await context.Libros.SingleAsync();
        Assert.Equal("Libro nuevo", guardado.Titulo);
        Assert.Equal("/images/libros/default-book.png", guardado.ImageUrl);
        Assert.True(guardado.ID > 0);
    }

    [Fact]
    // Con una portada vacia (Length = 0) no se copia ningun archivo y se usa la imagen por defecto.
    public async Task Create_ConPortadaVacia_NoCopiaArchivoYUsaImagenPorDefecto()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);
        var portada = new Mock<IFormFile>();
        portada.Setup(f => f.Length).Returns(0);

        var result = await controller.Create(NuevoLibro(), portada.Object);

        Assert.IsType<RedirectToActionResult>(result);
        var guardado = await context.Libros.SingleAsync();
        Assert.Equal("/images/libros/default-book.png", guardado.ImageUrl);
        portada.Verify(f => f.Length, Times.AtLeastOnce());
        portada.Verify(f => f.CopyTo(It.IsAny<Stream>()), Times.Never());
    }

    [Theory]
    [InlineData("Titulo")]
    [InlineData("Autor")]
    [InlineData("Categoria")]
    [InlineData("Precio")]
    // Con cualquier error de validacion, Create no guarda y devuelve el formulario con el mismo modelo.
    public async Task Create_ConModeloInvalido_NoGuardaYDevuelveLaVista(string campoInvalido)
    {
        await using var context = CreateContext();
        var logger = new Mock<ILogger<LibroController>>();
        var controller = CreateController(context, logger);
        controller.ModelState.AddModelError(campoInvalido, "Campo invalido.");
        var libro = NuevoLibro();

        var result = await controller.Create(libro, null!);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(libro, view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(context.Libros);
        VerifyLog(logger, LogLevel.Error, Times.Once());
    }

    // ===================== Edit (GET) =====================

    [Fact]
    // El GET de Edit devuelve la vista con el libro existente.
    public async Task Edit_Get_ConLibroExistente_DevuelveLaVistaConElLibro()
    {
        await using var context = CreateContext();
        var libro = NuevoLibro();
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.Edit(libro.ID);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(libro, view.Model);
    }

    [Fact]
    // El GET de Edit devuelve NotFound si el libro no existe.
    public async Task Edit_Get_ConLibroInexistente_DevuelveNotFound()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ===================== Edit (POST) =====================

    [Fact]
    // Edit actualiza los datos de un libro existente y redirige al listado.
    public async Task Edit_ConLibroValido_ActualizaElLibroYRedirige()
    {
        var databaseName = Guid.NewGuid().ToString();
        int libroId;
        // Contexto separado para preparar los datos (simula otra peticion).
        await using (var seedContext = CreateContext(databaseName))
        {
            var libro = NuevoLibro("Titulo original");
            seedContext.Libros.Add(libro);
            await seedContext.SaveChangesAsync();
            libroId = libro.ID;
        }

        await using var context = CreateContext(databaseName);
        var controller = CreateController(context);
        var libroEditado = new Libro
        {
            ID = libroId,
            Titulo = "Titulo actualizado",
            Autor = "Han Kang",
            Categoria = "Ficcion",
            Disponible = false
        };

        var result = await controller.Edit(libroId, libroEditado);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibroController.Index), redirect.ActionName);
        var guardado = await context.Libros.AsNoTracking().SingleAsync();
        Assert.Equal("Titulo actualizado", guardado.Titulo);
        Assert.False(guardado.Disponible);
    }

    [Fact]
    // Si el id de la ruta no coincide con el del modelo, Edit devuelve NotFound (comportamiento actual del controller).
    public async Task Edit_ConIdentificadorDiferente_DevuelveNotFound()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);
        var libro = NuevoLibro();
        libro.ID = 1;

        var result = await controller.Edit(2, libro);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Edit devuelve NotFound si el libro no existe en la base de datos.
    public async Task Edit_ConLibroInexistente_DevuelveNotFound()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);
        var libro = NuevoLibro();
        libro.ID = 999;

        var result = await controller.Edit(999, libro);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Con un modelo invalido, Edit devuelve la vista y NO modifica el libro guardado.
    public async Task Edit_ConModeloInvalido_NoActualizaYDevuelveLaVista()
    {
        var databaseName = Guid.NewGuid().ToString();
        int libroId;
        await using (var seedContext = CreateContext(databaseName))
        {
            var libro = NuevoLibro("Titulo original");
            seedContext.Libros.Add(libro);
            await seedContext.SaveChangesAsync();
            libroId = libro.ID;
        }

        await using var context = CreateContext(databaseName);
        var controller = CreateController(context);
        controller.ModelState.AddModelError("Titulo", "El titulo es obligatorio.");
        var libroEditado = NuevoLibro("");
        libroEditado.ID = libroId;

        var result = await controller.Edit(libroId, libroEditado);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(libroEditado, view.Model);
        var guardado = await context.Libros.AsNoTracking().SingleAsync();
        Assert.Equal("Titulo original", guardado.Titulo);
    }

    // ===================== Delete =====================

    [Fact]
    // DeleteConfirmed elimina un libro existente y redirige al listado.
    public async Task DeleteConfirmed_ConLibroExistente_EliminaElLibroYRedirige()
    {
        await using var context = CreateContext();
        var libro = NuevoLibro("Libro para eliminar");
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.DeleteConfirmed(libro.ID);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibroController.Index), redirect.ActionName);
        Assert.Empty(context.Libros);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(999)]
    // Con un id inexistente devuelve NotFound y no elimina ningun otro libro.
    public async Task DeleteConfirmed_ConIdentificadorInexistente_DevuelveNotFoundYConservaLosLibros(int id)
    {
        await using var context = CreateContext();
        context.Libros.Add(NuevoLibro());
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.DeleteConfirmed(id);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(1, await context.Libros.CountAsync());
    }

    // ===================== Autorizacion =====================

    [Theory]
    [InlineData("Create")]
    [InlineData("DeleteConfirmed")]
    // Llamar al metodo directamente no pasa por [Authorize]; este test verifica que el atributo siga presente.
    public void AccionesDeEscritura_RequierenElRolAdministrador(string accion)
    {
        var metodos = typeof(LibroController).GetMethods().Where(m => m.Name == accion).ToList();

        Assert.NotEmpty(metodos);
        Assert.All(metodos, metodo =>
        {
            var atributo = metodo.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                                 .Cast<AuthorizeAttribute>()
                                 .SingleOrDefault();
            Assert.NotNull(atributo);
            Assert.Equal("Administrador", atributo!.Roles);
        });
    }

    // ===================== Utilidades =====================

    // Crea el controller con un logger simulado (el propio o uno nuevo).
    private static LibroController CreateController(
        BibliotecaContext context, Mock<ILogger<LibroController>>? logger = null)
    {
        logger ??= new Mock<ILogger<LibroController>>();
        return new LibroController(context, logger.Object);
    }

    // Contexto EF Core en memoria; el nombre permite compartir datos entre dos contextos.
    private static BibliotecaContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<BibliotecaContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
        return new BibliotecaContext(options);
    }

    private static Libro NuevoLibro(string titulo = "La vegetariana") => new()
    {
        Titulo = titulo,
        Autor = "Han Kang",
        Categoria = "Ficcion",
        Disponible = true
    };

    // Verifica cuantas veces se registro un mensaje de cierto nivel en el logger simulado.
    private static void VerifyLog(Mock<ILogger<LibroController>> logger, LogLevel nivel, Times veces)
    {
        logger.Verify(
            l => l.Log(
                nivel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            veces);
    }
}