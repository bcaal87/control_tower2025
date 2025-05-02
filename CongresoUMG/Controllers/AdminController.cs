using CongresoUMG.Data;
using CongresoUMG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace CongresoUMG.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly CongresoContext _context;

        public AdminController(CongresoContext context)
        {
            _context = context;
        }

        // GET: /Admin/CheckIn
        public async Task<IActionResult> CheckIn(string search)
        {
            var query = _context.Participantes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Carne.Contains(search) ||
                    p.NombreCompleto.Contains(search) ||
                    p.Dpi.Contains(search));
            }

            var participantes = await query.ToListAsync();
            return View(participantes);
        }

        // POST: /Admin/CheckInConfirm/5
        [HttpPost]
        public async Task<IActionResult> CheckInConfirm(int id)
        {
            var participante = await _context.Participantes.FindAsync(id);
            if (participante != null && !participante.CheckIn)
            {
                participante.CheckIn = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CheckIn));
        }

        // GET: /Admin/Importar
        [HttpGet]
        public IActionResult Importar()
        {
            return View();
        }

        // POST: /Admin/Importar
        [HttpPost]
        public async Task<IActionResult> Importar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                ViewBag.Error = "Debe seleccionar un archivo válido.";
                return View();
            }

            var participantes = new List<Participante>();

            using (var stream = new MemoryStream())
            {
                await archivo.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var hoja = workbook.Worksheet(1);
                    var filas = hoja.RangeUsed().RowsUsed().Skip(1); // Saltar encabezado

                    foreach (var row in filas)
                    {
                        try
                        {
                            var p = new Participante
                            {
                                Carne = row.Cell(1).GetValue<string>().Trim(),
                                NombreCompleto = row.Cell(2).GetValue<string>().Trim(),
                                Dpi = row.Cell(3).GetValue<string>().Trim(),
                                Telefono = row.Cell(4).GetValue<string>().Trim(),
                                CorreoElectronico = row.Cell(5).GetValue<string>().Trim(),
                                CicloOSemestre = row.Cell(6).GetValue<string>().Trim(),
                                CheckIn = false
                            };

                            if (!string.IsNullOrWhiteSpace(p.Carne) && !string.IsNullOrWhiteSpace(p.NombreCompleto))
                                participantes.Add(p);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            if (participantes.Any())
            {
                _context.AddRange(participantes);
                await _context.SaveChangesAsync();
                ViewBag.Mensaje = $"{participantes.Count} participantes importados correctamente.";
            }
            else
            {
                ViewBag.Error = "No se importó ningún registro válido.";
            }

            return View();
        }
    }
}

