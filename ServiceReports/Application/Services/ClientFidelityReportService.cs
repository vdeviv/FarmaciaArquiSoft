using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using ServiceReports.Infrastructure.Reports;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceReports.Application.Services
{
    public class ClientFidelityReportService : IClientFidelityReportService
    {
        private readonly ReportRepository _reportRepository;
        private readonly IClientFidelityReportBuilder _pdfReportBuilder;
        private readonly IClientFidelityReportBuilder _excelReportBuilder;

        public ClientFidelityReportService(
            ReportRepository reportRepository,
            IEnumerable<IClientFidelityReportBuilder> reportBuilders)
        {
            _reportRepository = reportRepository;
            _pdfReportBuilder = reportBuilders.OfType<PdfClientFidelityReportBuilder>().First();
            _excelReportBuilder = reportBuilders.OfType<ExcelClientFidelityReportBuilder>().First();
        }

        public async Task<byte[]> GeneratePdfReportAsync(
            ClientFidelityFilter filter,
            string generatedBy,
            string logoPath)
        {
            // Obtener datos del repositorio
            IEnumerable<ClientFidelityDto> data = await _reportRepository.GetClientFidelityAsync(filter);

            // Construir PDF con TODOS los datos necesarios
            byte[] pdfBytes = _pdfReportBuilder
                .SetTitle("Reporte de Fidelidad de Clientes")
                .SetLogoPath(logoPath)
                .SetGeneratedBy(generatedBy) // 🚀 IMPORTANTE: Pasar el usuario
                .SetFilters(filter)           // 🚀 IMPORTANTE: Pasar los filtros
                .SetData(data)
                .Build();

            return pdfBytes;
        }

        public async Task<byte[]> GenerateExcelReportAsync(ClientFidelityFilter filter)
        {
            // Obtener datos del repositorio
            IEnumerable<ClientFidelityDto> data = await _reportRepository.GetClientFidelityAsync(filter);

            // Construir Excel
            byte[] excelBytes = _excelReportBuilder
                .SetTitle("Reporte de Fidelidad de Clientes")
                .SetFilters(filter)
                .SetData(data)
                .Build();

            return excelBytes;
        }
    }
}