using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReports.Application.DTOs;
using ServiceReports.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace FarmaView.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ReportRepository _reportRepository;

        public IndexModel(ReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public IEnumerable<ClientFidelityDto>? ReportData { get; set; }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var filter = new ClientFidelityFilter
            {
                StartDate = StartDate,
                EndDate = EndDate,
                MinTotal = 0
            };

            ReportData = await _reportRepository.GetClientFidelityAsync(filter);
            return Page();
        }
    }
}
