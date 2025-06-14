﻿using CongresoUMG.Data;
using CongresoUMG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using CongresoUMG.Services; // Asegúrate de tener esta línea para EmailService

namespace CongresoUMG.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly CongresoContext _context;
        private readonly EmailService _emailService;

        public AdminController(CongresoContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> CheckIn(string search, int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;

            if (string.IsNullOrEmpty(search) && TempData["Search"] != null)
                search = TempData["Search"].ToString();

            var query = _context.Participantes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Carne.Contains(search) ||
                    p.NombreCompleto.Contains(search) ||
                    p.Dpi.Contains(search));
            }

            var participantes = await PaginatedList<Participante>.CreateAsync(query.OrderBy(p => p.NombreCompleto), page, pageSize);

            ViewBag.Search = search;
            return View(participantes);
        }

        [HttpPost]
        public async Task<IActionResult> CheckInConfirm(int id)
        {
            var participante = await _context.Participantes.FindAsync(id);
            if (participante != null && !participante.CheckIn)
            {
                participante.CheckIn = true;
                await _context.SaveChangesAsync();

                // Enviar correo de bienvenida
                if (!string.IsNullOrWhiteSpace(participante.CorreoElectronico))
                {
                    var asunto = "¡Bienvenido al XIII Congreso de Ingeniería!";
                    var mensaje = $@"
                        <p>Estimado(a) <strong>{participante.NombreCompleto}</strong>,</p>
                        <p>Le damos una cordial bienvenida al <strong>XIII Congreso de Ingeniería en Sistemas</strong> de la Universidad Mariano Gálvez, sede Cobán.</p>
                        <p>📅 <strong>Fecha:</strong> 16 y 17 de mayo de 2025<br/>
                        📍 <strong>Lugar:</strong> Hotel Alcázar de Doña Victoria, Cobán Alta Verapaz</p>
                        <p>¡Gracias por ser parte de esta experiencia de innovación y tecnología!</p>
                        <p>Puedes ver nuestra agenda del evento https://sistemasumgcoban.com.gt/</p>
                        <hr />
                        <small>UMG - Campus Cobán | © PROLINK GT </small>";

                    await _emailService.EnviarCorreoAsync(participante.CorreoElectronico, asunto, mensaje);
                }
            }

            if (Request.HasFormContentType && Request.Form.ContainsKey("search"))
                TempData["Search"] = Request.Form["search"];

            return RedirectToAction(nameof(CheckIn));
        }

        [HttpGet]
        public IActionResult Importar()
        {
            return View();
        }

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
                    var filas = hoja.RangeUsed().RowsUsed().Skip(1);

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
                        catch { continue; }
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

        [HttpGet]
        public async Task<IActionResult> ExportarCheckInPorCiclo()
        {
            var participantes = await _context.Participantes
                .Where(p => p.CheckIn)
                .OrderBy(p => p.CicloOSemestre)
                .ThenBy(p => p.NombreCompleto)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("CheckIn por Ciclo");

            worksheet.Cell(1, 1).Value = "Carné";
            worksheet.Cell(1, 2).Value = "Nombre Completo";
            worksheet.Cell(1, 3).Value = "DPI";
            worksheet.Cell(1, 4).Value = "Teléfono";
            worksheet.Cell(1, 5).Value = "Correo";
            worksheet.Cell(1, 6).Value = "Ciclo o Semestre";

            int fila = 2;
            foreach (var p in participantes)
            {
                worksheet.Cell(fila, 1).Value = p.Carne;
                worksheet.Cell(fila, 2).Value = p.NombreCompleto;
                worksheet.Cell(fila, 3).Value = p.Dpi;
                worksheet.Cell(fila, 4).Value = p.Telefono;
                worksheet.Cell(fila, 5).Value = p.CorreoElectronico;
                worksheet.Cell(fila, 6).Value = p.CicloOSemestre;
                fila++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "CheckIn_Por_Ciclo.xlsx");
        }
    }
}
