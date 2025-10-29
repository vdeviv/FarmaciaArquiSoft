using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceReports.Application.DTOs;

namespace ServiceReports.Application.Interfaces
{
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