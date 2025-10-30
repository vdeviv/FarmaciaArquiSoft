using ServiceReports.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ServiceReports.Application.DTOs;

namespace ServiceReports.Application.Interfaces
{
    public interface IMedicineByCategoryReportService
    {
        Task<byte[]> GeneratePdfReportAsync(MedicineByCategoryFilter filter, string generatedBy, string logoPath);
        Task<byte[]> GenerateExcelReportAsync(MedicineByCategoryFilter filter);
    }
}
