using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReports.Application.DTOs;
using ServiceReports.Infrastructure.Repositories;
using System.Threading.Tasks;
using ServiceReports.Application.Interfaces;

namespace FarmaView.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ReportRepository _reportRepository;
        private readonly IClientFidelityReportService _reportService;

        public IndexModel(ReportRepository reportRepository, IClientFidelityReportService reportService)
        {
            _reportRepository = reportRepository;
            _reportService = reportService;
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
            // Obtener el nombre del usuario logeado (si existe)
            string generatedBy = HttpContext.User.Identity?.Name ?? "Sistema";

            byte[] pdfBytes = await _reportService.GeneratePdfReportAsync(Filter, generatedBy);

            string fileName = $"ReporteFidelidadClientes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}