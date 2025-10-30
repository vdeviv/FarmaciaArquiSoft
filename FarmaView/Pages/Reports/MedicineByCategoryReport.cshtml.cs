// Este código es el que usted proporcionó y es correcto, ahora que ReportRepository.cs está corregido.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;

// Alias para evitar errores si el DTO de categoría no está en este proyecto.
// 🚀 Funciona una vez que el archivo CategoryDto.cs existe.
using CategoryDto = ServiceReports.Application.DTOs.CategoryDto;

namespace FarmaView.Pages.Reports
{
    public class MedicineByCategoryReportModel : PageModel
    {
        private readonly ReportRepository _reportRepository;
        private readonly IMedicineByCategoryReportService _reportService;
        private readonly IWebHostEnvironment _env;

        public MedicineByCategoryReportModel(
            ReportRepository reportRepository,
            IMedicineByCategoryReportService reportService,
            IWebHostEnvironment env)
        {
            _reportRepository = reportRepository;
            _reportService = reportService;
            _env = env;
        }

        // 🎯 PROPIEDADES CRÍTICAS INICIALIZADAS para evitar NullReferenceException
        [BindProperty]
        public MedicineByCategoryFilter Filter { get; set; } = new MedicineByCategoryFilter();

        // 🚀 CategoryDto ahora es reconocido si se creó el archivo CategoryDto.cs
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

        public IEnumerable<MedicineByCategoryDto> ReportData { get; set; } = new List<MedicineByCategoryDto>();

        // Flag para saber si se ha intentado generar el reporte
        public bool IsGenerated { get; set; } = false;

        // ... (Resto de métodos OnGetAsync, OnPostGenerateAsync, OnPostExportPdfAsync, etc.)
        // Los métodos auxiliares llaman a LoadInitialData y LoadReportData, que a su vez llaman a los métodos corregidos del repositorio.

        // =======================================================
        // Carga Inicial (GET)
        // =======================================================

        public async Task OnGetAsync()
        {
            await LoadInitialData();
        }

        // =======================================================
        // Handler para Generar Reporte (POST Generate)
        // =======================================================

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            await LoadInitialData();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                ReportData = await _reportRepository.GetMedicinesByCategoryAsync(Filter);
                IsGenerated = true;

                if (!ReportData.Any())
                {
                    ModelState.AddModelError(string.Empty, "No se encontraron resultados para los filtros seleccionados.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al generar el reporte: {ex.Message}");
            }

            return Page();
        }

        // ... (Resto de métodos de Exportación)

        // =======================================================
        // Métodos Auxiliares
        // =======================================================
        private async Task LoadInitialData()
        {
            // 🚀 Este método ahora usa el ReportRepository corregido.
            Categories = await _reportRepository.GetAllCategoriesAsync();
            if (Filter.LowStockThreshold == 0) Filter.LowStockThreshold = 10;
        }

        private async Task LoadReportData()
        {
            // 🚀 Este método ahora usa el ReportRepository corregido.
            ReportData = await _reportRepository.GetMedicinesByCategoryAsync(Filter);
            IsGenerated = true;
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            // Asegurarse de que los datos iniciales y el reporte estén cargados
            await LoadInitialData();
            await LoadReportData();

            if (!ReportData.Any())
            {
                // Si no hay datos, muestra un error y quédate en la página
                ModelState.AddModelError(string.Empty, "No hay datos para exportar. Genere el reporte primero.");
                return Page();
            }

            try
            {
                // 1. Obtener la ruta completa del logo (asumiendo que está en wwwroot/images)
                string logoPath = Path.Combine(_env.WebRootPath, "images", "logoOficial.png"); 

                // 2. Llamar al servicio para generar el PDF
                byte[] pdfBytes = await _reportService.GeneratePdfReportAsync(
                    Filter,
                    User.Identity?.Name ?? "Usuario Desconocido", // Nombre del usuario actual
                    logoPath);

                // 3. Devolver el archivo PDF al navegador
                string fileName = $"ReporteInventarioCategoria_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al exportar a PDF: {ex.Message}");
                return Page();
            }
        }

        // =======================================================
        // 🚀 2. Handler para Exportar a Excel
        // =======================================================
        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            // Asegurarse de que los datos iniciales y el reporte estén cargados
            await LoadInitialData();
            await LoadReportData();

            if (!ReportData.Any())
            {
                // Si no hay datos, muestra un error y quédate en la página
                ModelState.AddModelError(string.Empty, "No hay datos para exportar. Genere el reporte primero.");
                return Page();
            }

            try
            {
                // 1. Llamar al servicio para generar el Excel
                byte[] excelBytes = await _reportService.GenerateExcelReportAsync(Filter);

                // 2. Devolver el archivo Excel al navegador
                string fileName = $"ReporteInventarioCategoria_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al exportar a Excel: {ex.Message}");
                return Page();
            }
        }
    }
}