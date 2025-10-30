
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceReports.Application.Services
{
    public class ClientFidelityReportService : IClientFidelityReportService
    {
        private readonly ReportRepository _reportRepository;
        private readonly IClientFidelityReportBuilder _reportBuilder;

        public ClientFidelityReportService(ReportRepository reportRepository, IClientFidelityReportBuilder reportBuilder)
        {
            _reportRepository = reportRepository;
            _reportBuilder = reportBuilder;
        }

        public async Task<byte[]> GeneratePdfReportAsync(ClientFidelityFilter filter, string generatedBy, string logoPath)
        {
            // 1. Obtener los datos del repositorio
            IEnumerable<ClientFidelityDto> data = await _reportRepository.GetClientFidelityAsync(filter);

            // 2. Configurar el Builder y generar el reporte
            byte[] pdfBytes = _reportBuilder
                .SetTitle("Reporte de Fidelidad de Clientes")
                .SetLogoPath(logoPath)
                .SetGeneratedBy(generatedBy)
                .SetFilters(filter)
                .SetData(data)
                .Build();

            return pdfBytes;
        }
    }
}