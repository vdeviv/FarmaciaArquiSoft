// Ruta: FarmaView/Pages/Reports/Index.cshtml.cs

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FarmaView.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ReportRepository _reportRepository;
        private readonly IClientFidelityReportService _reportService;
        private readonly IWebHostEnvironment _env;

        // Constructor para inyección de dependencias
        public IndexModel(ReportRepository reportRepository, IClientFidelityReportService reportService, IWebHostEnvironment env)
        {
            _reportRepository = reportRepository;
            _reportService = reportService;
            _env = env;
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
            ReportData = await _reportRepository.GetClientFidelityAsync(Filter);
            return Page();
        }

        // Handler para la exportación a PDF
        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            // 🚀 CAMBIO CLAVE: Obtención de la ruta ABSOLUTA del logo
            // Usa el nombre del archivo de logo que tengas optimizado en wwwroot/images/
            string logoFileName = "LogoOficial.png"; // O "LogoReportes.jpeg" si prefieres el otro archivo
            string logoPath = Path.Combine(_env.WebRootPath, "images", logoFileName);

            string generatedBy = HttpContext.User.Identity?.Name ?? "Sistema";

            // Llamada al servicio con el parámetro logoPath
            byte[] pdfBytes = await _reportService.GeneratePdfReportAsync(Filter, generatedBy, logoPath);

            string fileName = $"ReporteFidelidadClientes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // Devolver el archivo al navegador
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}