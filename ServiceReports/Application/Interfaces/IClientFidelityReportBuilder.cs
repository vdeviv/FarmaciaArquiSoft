using ServiceReports.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceReports.Application.DTOs;

namespace ServiceReports.Application.Interfaces
{
    public interface IClientFidelityReportService
    {
        Task<byte[]> GeneratePdfReportAsync(ClientFidelityFilter filter, string generatedBy, CancellationToken ct = default);
    }
}
