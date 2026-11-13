using BibliotecaPrjt.Data;
using BibliotecaPrjt.Models;
using BibliotecaPrjt.Repositories;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.IO;

namespace BibliotecaPrjt.Controllers
{
    [Authorize]
    public class LibroController : Controller
    {
        /* private readonly IRepositorioLibro _repositorio;

         public LibroController(IRepositorioLibro repositorio)
         {
             _repositorio = repositorio;
         }

         private List<Libro> ObtenerListaInterna()
         {
             return _repositorio.ObtenerTodos() as List<Libro> ?? new List<Libro>();
         }*/

        private readonly BibliotecaContext _context;

        private readonly ILogger<LibroController> _logger;
        public LibroController(BibliotecaContext context, ILogger<LibroController>logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de libros");
            
            try
            {
                var libros = await _context.Libros.ToListAsync();
                return View(libros);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar el listado de libros");
                throw;
            }
            
        }

        private async Task<List<Libro>> ObtenerLibrosFiltrados(string? autor)
        {
            var query = _context.Libros.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(autor))
                query = query.Where(l => l.Autor == autor);

            return await query.OrderBy(l => l.Titulo).ToListAsync();
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Report(string? autor)
        {
            var libros = await ObtenerLibrosFiltrados(autor);
            return File(GenerarReportePdf(libros, autor), "application/pdf");
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> ReportDownload(string? autor)
        {
            var libros = await ObtenerLibrosFiltrados(autor);
            return File(GenerarReportePdf(libros, autor), "application/pdf", "ReporteLibros.pdf");
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> ExportarExcel(string? autor)
        {
            var libros = await ObtenerLibrosFiltrados(autor);
            var bytes = GenerarReporteExcel(libros, autor);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ReporteLibros.xlsx");
        }

        private static byte[] GenerarReportePdf(IReadOnlyCollection<Libro> libros, string? autor)
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
                        .FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Reporte de libros")
                        .FontSize(26).Bold().FontColor(Colors.Grey.Darken3);
                    column.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}"
                            + (string.IsNullOrWhiteSpace(autor) ? "" : $" · Autor: {autor}"))
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(14).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                });

                page.Content().PaddingVertical(22).Column(column =>
                {
                    // Resumen
                    column.Item().Background(Colors.Blue.Darken2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("Total de libros incluidos")
                            .FontSize(11).Bold().FontColor(Colors.White);
                        row.ConstantItem(45).AlignRight().Text(libros.Count.ToString())
                            .FontSize(18).Bold().FontColor(Colors.White);
                    });

                    column.Item().PaddingTop(18).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.3f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID");
                            header.Cell().Element(HeaderCellStyle).Text("Título");
                            header.Cell().Element(HeaderCellStyle).Text("Autor");
                            header.Cell().Element(HeaderCellStyle).Text("Precio");
                            header.Cell().Element(HeaderCellStyle).Text("Estado");
                        });

                        foreach (var libro in libros)
                        {
                            table.Cell().Element(BodyCellStyle).Text(libro.ID.ToString());
                            table.Cell().Element(BodyCellStyle).Text(libro.Titulo);
                            table.Cell().Element(BodyCellStyle).Text(libro.Autor ?? "No especificado");
                            table.Cell().Element(BodyCellStyle).Text($"${libro.Precio:N2}");
                            table.Cell().Element(BodyCellStyle).Text(libro.Disponible ? "Disponible" : "Prestado");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Biblioteca Universitaria - Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            })).GeneratePdf();

            static IContainer HeaderCellStyle(IContainer container) => container
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(9).PaddingHorizontal(8)
                .DefaultTextStyle(t => t.FontSize(9).Bold().FontColor(Colors.White));

            static IContainer BodyCellStyle(IContainer container) => container
                .Background(Colors.White)
                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(9).PaddingHorizontal(8)
                .DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Darken3));
        }

        private static byte[] GenerarReporteExcel(IReadOnlyCollection<Libro> libros, string? autor)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte de Libros");

            ws.Cell("A1").Value = "Biblioteca Universitaria - Reporte de libros";
            ws.Range("A1:E1").Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A2").Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}"
                + (string.IsNullOrWhiteSpace(autor) ? "" : $" · Autor: {autor}");
            ws.Range("A2:E2").Merge();

            ws.Cell("A3").Value = $"Total de libros: {libros.Count}";
            ws.Range("A3:E3").Merge().Style.Font.SetBold();

            var headers = new[] { "ID", "Título", "Autor", "Precio", "Estado" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(5, i + 1).Value = headers[i];

            var headerRange = ws.Range(5, 1, 5, headers.Length);
            headerRange.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#1E40AF"));
            headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int fila = 6;
            foreach (var libro in libros)
            {
                ws.Cell(fila, 1).Value = libro.ID;
                ws.Cell(fila, 2).Value = libro.Titulo;
                ws.Cell(fila, 3).Value = libro.Autor ?? "No especificado";
                ws.Cell(fila, 4).Value = libro.Precio;
                ws.Cell(fila, 4).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(fila, 5).Value = libro.Disponible ? "Disponible" : "Prestado";
                fila++;
            }

            var tabla = ws.Range(5, 1, Math.Max(fila - 1, 5), headers.Length);
            tabla.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            tabla.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Consultando los detalles del libro con identificador {LibroId}", id);
            
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    _logger.LogWarning("No se encontro el libro solicitado con identificador {LibroId}", id);
                    return NotFound();

                }
                return View(libro);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del libro con identificador {LibroId}", id);
                throw;
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operacion de creacion de un libro");
            return View();
        }

       
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Libro libro, IFormFile imagenPortada)
        {
            _logger.LogInformation("Iniciando la operacion de creacion de un libro");
            if (!ModelState.IsValid)
            {
                _logger.LogError("El libro enviado no cumple con los requisitos");
                return View(libro);
            }

            try
            {
                if (imagenPortada != null && imagenPortada.Length > 0)
                {
                    var extension = Path.GetExtension(imagenPortada.FileName);
                    var nombreArchivoUnico = Guid.NewGuid().ToString() + extension;

                    var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "libros");

                    if (!Directory.Exists(rutaCarpeta))
                    {
                        Directory.CreateDirectory(rutaCarpeta);
                    }

                    var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivoUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        imagenPortada.CopyTo(stream);
                    }


                    libro.ImageUrl = "/images/libros/" + nombreArchivoUnico;
                }
                else
                {

                    libro.ImageUrl = "/images/libros/default-book.png";
                }

                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al crear el libro");
                throw;
            }
        }



        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Iniciando la operacion de edicion del libro con identificador {LibroId}", id);
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }
                return View(libro);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del libro para su edicion con identificador {Libroid}", id);
                return View();
            }
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            _logger.LogInformation("Iniciando la edicion del libro con identificador {LibroId}", id);
            if (id != libro.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            try
            {
                var existente = await _context.Libros.AnyAsync(a => a.ID == id);
                if (!existente)
                {
                    return NotFound();
                }

                _context.Update(libro);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Se produjo un error al editar el libro con identificador {Libroid}", id);
                throw;
            }
        }

        
        /*public IActionResult Delete(int id)
        {
            var libro = _repositorio.ObtenerTodos().FirstOrDefault(x => x.ID == id);
            if (libro == null)
            {
                return NotFound();
            }
            return View(libro);
        }*/

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Iniciando la eliminacion del libro con identificador {LibroId}", id);

            try
            {
                var listaLibros = await _context.Libros.FindAsync(id);
                if (listaLibros == null)
                {
                    return NotFound();
                }

                _context.Libros.Remove(listaLibros);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar el libro con identificador {LirboId}", id);
                throw;
            }

        }
    }



}
