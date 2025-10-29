using System.Collections.Generic;
using ServiceReports.Application.DTOs;

namespace ServiceReports.Application.Interfaces
{
    // Interfaz que define los pasos para construir el reporte PDF
    public interface IClientFidelityReportBuilder
    {
        IClientFidelityReportBuilder SetTitle(string title);

        IClientFidelityReportBuilder SetLogoPath(string path);

        IClientFidelityReportBuilder SetGeneratedBy(string user);
        IClientFidelityReportBuilder SetFilters(ClientFidelityFilter filters);
        IClientFidelityReportBuilder SetData(IEnumerable<ClientFidelityDto> data);
        byte[] Build();
    }
}
