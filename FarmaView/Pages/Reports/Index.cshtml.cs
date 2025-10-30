using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // 🚀 NECESARIO para acceder a wwwroot
using System.IO; // 🚀 NECESARIO para Path.Combine

namespace FarmaView.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ReportRepository _reportRepository;
        private readonly IClientFidelityReportService _reportService;
        private readonly IWebHostEnvironment _env; // 🚀 AGREGADO para obtener la ruta del logo

        // Constructor: Inyección de dependencias
        public IndexModel(
            ReportRepository reportRepository,
            IClientFidelityReportService reportService,
            IWebHostEnvironment env) // 🚀 AGREGADO
        {
            _reportRepository = reportRepository;
            _reportService = reportService;
            _env = env; // 🚀 AGREGADO
        }

        [BindProperty]
        public ClientFidelityFilter Filter { get; set; } = new ClientFidelityFilter
        {
            StartDate = DateTime.Now.AddMonths(-1),
            EndDate = DateTime.Now,
            MinTotal = 0,
            SortBy = "FullName",
            SortOrder = "ASC"
        };

        public IEnumerable<ClientFidelityDto>? ReportData { get; set; }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            if (!ValidateDates())
            {
                return Page();
            }

            try
            {
                ReportData = await _reportRepository.GetClientFidelityAsync(Filter);

                if (ReportData == null || !ReportData.Any())
                {
                    ModelState.AddModelError(string.Empty,
                        "No se encontraron registros para los filtros seleccionados.");
                }

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error al generar el reporte: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!ValidateDates())
            {
                return Page();
            }

            try
            {
                // 🚀 SOLUCIÓN 1: Obtener la ruta del logo desde wwwroot
                string logoPath = Path.Combine(_env.WebRootPath, "images", "LogoOficial.png");

                // 🚀 Si el logo no existe, usar una ruta vacía (el PDF lo manejará)
                if (!System.IO.File.Exists(logoPath))
                {
                    logoPath = string.Empty;
                }

                string generatedBy = HttpContext.User.Identity?.Name ?? "Sistema";

                byte[] pdfBytes = await _reportService.GeneratePdfReportAsync(
                    Filter,
                    generatedBy,
                    logoPath);

                string fileName = $"ReporteFidelidadClientes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error al exportar a PDF: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            if (!ValidateDates())
            {
                return Page();
            }

            try
            {
                byte[] excelBytes = await _reportService.GenerateExcelReportAsync(Filter);

                string fileName = $"ReporteFidelidadClientes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error al exportar a Excel: {ex.Message}");
                return Page();
            }
        }

        private bool ValidateDates()
        {
            if (Filter.StartDate > Filter.EndDate)
            {
                ModelState.AddModelError(string.Empty,
                    "La fecha de inicio no puede ser mayor que la fecha final.");
                return false;
            }

            if (Filter.EndDate > DateTime.Now)
            {
                ModelState.AddModelError(string.Empty,
                    "La fecha final no puede ser mayor a la fecha actual.");
                return false;
            }

            if ((Filter.EndDate - Filter.StartDate).TotalDays > 365)
            {
                ModelState.AddModelError(string.Empty,
                    "El rango de fechas no puede ser mayor a 1 año.");
                return false;
            }

            return true;
        }
    }
}