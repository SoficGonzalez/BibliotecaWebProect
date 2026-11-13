using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BibliotecaPrjt.Data;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaPrjt.Controllers
{
    [Authorize]
    public class AutoresController : Controller
    {
        

        /* public readonly IAutorService _repositorio;

         public AutoresController(IAutorService repositorio) 
         { 
             _repositorio = repositorio;
         }


         private List<Autor> ObtenerListaInterna()
         {

             return _repositorio.ObtenerAutores() as List<Autor> ?? new List<Autor>();

         }*/

        private readonly BibliotecaContext _context;

        private readonly ILogger<AutoresController> _logger;

        public AutoresController(BibliotecaContext context , ILogger<AutoresController>logger) 
        {
            _context = context;
            _logger = logger;
        }


       
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de autores");

            try
            {
                var autores = await _context.Autores.ToListAsync();
                return View(autores);
            }
            catch (Exception ex) 
                {
                    _logger.LogError(ex, "Se produjo un error al consultar el listado de autores");
                    throw;
                }

       }
            
        public  async Task<IActionResult> Report()
        {
            var autores = await _context.Autores
                .AsNoTracking()
                .OrderBy(autor => autor.Nombre) 
                .ToListAsync();
            return File(GenerarReporte(autores), "application/pdf");
        }

        public async Task<IActionResult> ReportDowload()
        {
            var autores = await _context.Autores
                .AsNoTracking()
                .OrderBy(autor => autor.Nombre)
                .ToListAsync();
            return File(GenerarReporte(autores), "application/pdf", "reporte-autores.pdf");
        }


        private static byte[] GenerarReporte(IReadOnlyCollection<Autor> autores)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(42);
                page.PageColor(Colors.Grey.Lighten5);

                page.Header().Column(column =>
                {
                    column.Item().Text("Biblioteca Universitaria")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Reporte de autores")
                    .FontSize(26)
                    .Bold()
                    .FontColor (Colors.Grey.Darken3);
                    column.Item().Text($"Listado actualizado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10)
                    .Bold()
                    .FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(14).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingVertical(22).Column(column =>
                {
                    column.Item().Background(Colors.Blue.Darken2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("Autores Registrados")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.White);
                        row.ConstantItem(45).AlignRight().Text(autores.Count.ToString())
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.White);
                    });
                    column.Item().PaddingTop(18).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.3f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID");
                            header.Cell().Element(HeaderCellStyle).Text("Nombre");
                            header.Cell().Element(HeaderCellStyle).Text("Nacionalidad");
                            header.Cell().Element(HeaderCellStyle).Text("Nacimiento");
                        });

                        foreach(var autor in autores)
                        {
                            table.Cell().Element(BodyCellStyle).Text(autor.ID.ToString());
                            table.Cell().Element(BodyCellStyle).Text(autor.Nombre);
                            table.Cell().Element(BodyCellStyle).Text(autor.Nacionalidad ?? "No especificada");
                            table.Cell().Element(BodyCellStyle).Text(autor.FechaDeNacimiento.ToString("dd/MM/yyyy"));
                        }
                    });
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Biblioteca Universitaria - Pagina");
                    text.CurrentPageNumber();
                    text.Span("de");
                    text.TotalPages();
                });
            })).GeneratePdf();

            static IContainer HeaderCellStyle(IContainer container) => container
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .DefaultTextStyle(text => text.FontSize(9).Bold().FontColor(Colors.White));

            static IContainer BodyCellStyle(IContainer container) => container
                .Background(Colors.Blue.Darken2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .DefaultTextStyle(text => text.FontSize(9).Bold().FontColor(Colors.Grey.Darken3));
        }
        

        public async Task<IActionResult> Details(int id) 
        {
            _logger.LogInformation("Consultando los detalles del autor con identificador {AutorId}", id);
            
            try
            {
                var autor = await _context.Autores.FindAsync(id);
                if (autor == null)
                {
                    _logger.LogWarning("No se encontro el autor solicitado con identificador {Autorid}", id);
                    return NotFound();

                }
                return View(autor);
            }catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del autor con identificador {AutorId}", id);
                throw;
            }
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Create() 
        {
            _logger.LogInformation("Iniciando la operacion de creacion de un autor");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Create(Autor autor)
        {

            _logger.LogInformation("Iniciando la operacion de creacion de un autor");
            if (!ModelState.IsValid)
            {
                _logger.LogError("El autor enviado no cumple con los requisitos");
                return View(autor);
            }

            try
            {
                _context.Autores.Add(autor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear el autor");
                throw;
            }
            
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id) 
        {
            _logger.LogInformation("Iniciando la operacion de edicion del autor con identificador {AutorId}", id);
            try
            {
                var autor = await _context.Autores.FindAsync(id);

                if (autor == null)
                {
                    return NotFound();
                }
                return View(autor);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del  autor para su edicion con identificador {Autorid}",id);
                return View();
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id, Autor autor)
        {
            _logger.LogInformation("Iniciando la edicion del autor con identificador {AutorId}", id);
            if (id != autor.ID)
            {
                return BadRequest();
            }

            if(!ModelState.IsValid)
            {
                return View(autor);
            }

            try
            {

                var exists = await _context.Autores.AnyAsync(a => a.ID == id);
                if (!exists)
                {
                    _logger.LogWarning("No se encontro el autor solicitado con identificador {AutorId}", id);
                    return NotFound();
                }

                _context.Update(autor);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al editar el autor con identificador {Autorid}", id);
                throw;
            }
        } 

        /*
         public IActionResult Delete(int id)
         {
             var autor = _autores.FirstOrDefault(x => x.ID == id);
             if(autor == null)
             {
                 return NotFound();
             }
             return View(autor);
         }*/

         [HttpPost, ActionName("Delete")]
         [ValidateAntiForgeryToken]
         [Authorize(Roles = "Administrador, Bibliotecario")]
        public  async Task<IActionResult> DeleteConfirmed(int id)
         {
            _logger.LogInformation("Iniciando la eliminacion del autor con identificador {AutorId}", id);

            try
            {
                var autor = await _context.Autores.FindAsync(id);
                if (autor == null)
                {
                    _logger.LogWarning("No se encontro el autor solicitado con identificador {AutorId}",id);
                    return NotFound();
                }

                //_context.Remove(autor);
                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar el autor con identificador {AutorId}", id);
                throw;
            }

         } 


    }
}
