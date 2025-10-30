using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using ServiceReports.Infrastructure.Reports;

namespace ServiceReports.Application.Services
{
    public class MedicineByCategoryReportService : IMedicineByCategoryReportService
    {
        private readonly ReportRepository _reportRepository;
        private readonly IMedicineByCategoryReportBuilder _pdfReportBuilder;
        private readonly IMedicineByCategoryReportBuilder _excelReportBuilder;

        public MedicineByCategoryReportService(
            ReportRepository reportRepository,
            IEnumerable<IMedicineByCategoryReportBuilder> reportBuilders)
        {
            _reportRepository = reportRepository;
            _pdfReportBuilder = reportBuilders.OfType<PdfMedicineByCategoryReportBuilder>().First();
            _excelReportBuilder = reportBuilders.OfType<ExcelMedicineByCategoryReportBuilder>().First();
        }

        public async Task<byte[]> GeneratePdfReportAsync(
            MedicineByCategoryFilter filter,
            string generatedBy,
            string logoPath)
        {
            // Obtener datos del repositorio
            IEnumerable<MedicineByCategoryDto> data = await _reportRepository.GetMedicinesByCategoryAsync(filter);

            // Construir PDF
            byte[] pdfBytes = _pdfReportBuilder
                .SetTitle("Reporte de Medicinas por Categoría")
                .SetLogoPath(logoPath)
                .SetGeneratedBy(generatedBy)
                .SetFilters(filter)
                .SetData(data)
                .Build();

            return pdfBytes;
        }

        public async Task<byte[]> GenerateExcelReportAsync(MedicineByCategoryFilter filter)
        {
            // Obtener datos del repositorio
            IEnumerable<MedicineByCategoryDto> data = await _reportRepository.GetMedicinesByCategoryAsync(filter);

            // Construir Excel
            byte[] excelBytes = _excelReportBuilder
                .SetTitle("Reporte de Medicinas por Categoría")
                .SetFilters(filter)
                .SetData(data)
                .Build();

            return excelBytes;
        }
    }
}