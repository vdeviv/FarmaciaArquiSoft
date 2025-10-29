using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using ServiceReports.Infrastructure.Repositories;

namespace ServiceReports.Application.Services
{
    public class ClientFidelityReportService : IClientFidelityReportService
    {
        private readonly ReportRepository _repository;
        private readonly IClientFidelityReportBuilder _builder;

        public ClientFidelityReportService(ReportRepository repository, IClientFidelityReportBuilder builder)
        {
            _repository = repository;
            _builder = builder;
        }

        public async Task<byte[]> GeneratePdfReportAsync(ClientFidelityFilter filter, string generatedBy, CancellationToken ct = default)
        {
            var data = (await _repository.GetClientFidelityAsync(filter, ct)).ToList();

            return _builder
                .SetTitle("Reporte de Fidelidad de Clientes")
                .SetLogoPath("wwwroot/images/logo.png")
                .SetGeneratedBy(generatedBy)
                .SetFilters(filter)
                .SetData(data)
                .Build();
        }
    }
}
