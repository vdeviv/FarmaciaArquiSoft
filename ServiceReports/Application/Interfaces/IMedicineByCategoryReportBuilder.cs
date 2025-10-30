using ServiceReports.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceReports.Application.DTOs;

namespace ServiceReports.Application.Interfaces
{
    public interface IMedicineByCategoryReportBuilder
    {
        IMedicineByCategoryReportBuilder SetTitle(string title);
        IMedicineByCategoryReportBuilder SetLogoPath(string path);
        IMedicineByCategoryReportBuilder SetGeneratedBy(string user);
        IMedicineByCategoryReportBuilder SetFilters(MedicineByCategoryFilter filters);
        IMedicineByCategoryReportBuilder SetData(IEnumerable<MedicineByCategoryDto> data);
        byte[] Build();
    }
}
