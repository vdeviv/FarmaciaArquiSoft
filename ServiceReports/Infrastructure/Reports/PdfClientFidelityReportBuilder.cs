using PdfSharpCore.Drawing; // Solo usamos PdfSharpCore
using PdfSharpCore.Pdf;     // Solo usamos PdfSharpCore
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic; 

namespace ServiceReports.Infrastructure.Reports
{
    public class PdfClientFidelityReportBuilder : IClientFidelityReportBuilder
    {
        private string _title = "";
        private string? _logoPath;
        private string _generatedBy = "";
        private ClientFidelityFilter? _filters;
        private List<ClientFidelityDto> _data = new();

        public IClientFidelityReportBuilder SetTitle(string title) { _title = title; return this; }
        public IClientFidelityReportBuilder SetLogoPath(string path) { _logoPath = path; return this; }
        public IClientFidelityReportBuilder SetGeneratedBy(string user) { _generatedBy = user; return this; }
        public IClientFidelityReportBuilder SetFilters(ClientFidelityFilter filters) { _filters = filters; return this; }
        public IClientFidelityReportBuilder SetData(IEnumerable<ClientFidelityDto> data) { _data = data.ToList(); return this; }

        public byte[] Build()
        {
            using var ms = new MemoryStream();
            var doc = new PdfDocument();
            doc.Info.Title = _title;

            var page = doc.AddPage();
            // 1. CORREGIDO: Usamos PdfSharpCore.PageSize.A4 (o simplemente PageSize.A4 si incluimos using PdfSharpCore.Pdf;)
            page.Size = PdfSharpCore.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            // 2. CORREGIDO: XFontStyle.Regular no existe, se usa XFontStyle.Regular
            var fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
            var fontNormal = new XFont("Arial", 10, XFontStyle.Bold);
            var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

            double margin = 40;
            double y = margin;

            // Logo
            if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
            {
                using var img = XImage.FromFile(_logoPath);
                gfx.DrawImage(img, margin, y, XUnit.FromPoint(60), XUnit.FromPoint(60));
            }

            // Título
            // 3. CORREGIDO: Usamos XBrushes.Black y XRect directamente
            gfx.DrawString(_title, fontTitle, XBrushes.Black, new XRect(margin + 80, y, page.Width - margin * 2, XUnit.FromPoint(20)), XStringFormats.TopLeft);
            y += 70;

            // Filtros y cabecera
            if (_filters != null)
            {
                gfx.DrawString($"Periodo: {_filters.StartDate:yyyy-MM-dd} → {_filters.EndDate:yyyy-MM-dd}", fontSmall, XBrushes.Gray, new XRect(margin, y, 300, XUnit.FromPoint(12)), XStringFormats.TopLeft);
                gfx.DrawString($"Min Total: {_filters.MinTotal ?? 0:F2}", fontSmall, XBrushes.Gray, new XRect(page.Width - margin - 150, y, 150, XUnit.FromPoint(12)), XStringFormats.TopLeft);
                y += 20;
            }

            // Información General
            gfx.DrawString($"Generado por: {_generatedBy}", fontSmall, XBrushes.Gray, new XRect(margin, y, 300, XUnit.FromPoint(12)), XStringFormats.TopLeft);
            gfx.DrawString($"Fecha: {DateTime.Now.ToString("g", CultureInfo.InvariantCulture)}", fontSmall, XBrushes.Gray, new XRect(page.Width - margin - 150, y, 150, XUnit.FromPoint(12)), XStringFormats.TopLeft);
            y += 20;

            // 4. CORREGIDO: Usamos XPens.Black directamente
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 10;

            // Encabezado tabla
            string[] headers = { "#", "Cliente", "Ventas", "Total", "Promedio", "Última venta" };
            double[] widths = { 30, 180, 60, 70, 70, 100 };
            double x = margin;

            for (int i = 0; i < headers.Length; i++)
            {
                // 5. CORREGIDO: Usamos XBrushes.Black y XRect/XUnit directamente
                gfx.DrawString(headers[i], fontNormal, XBrushes.Black, new XRect(x, y, widths[i], XUnit.FromPoint(16)), XStringFormats.TopLeft);
                x += widths[i];
            }
            y += 18;

            // Datos (Loop)
            int idx = 1;
            foreach (var c in _data)
            {
                x = margin;
                gfx.DrawString(idx.ToString(), fontSmall, XBrushes.Black, new XRect(x, y, widths[0], XUnit.FromPoint(12)), XStringFormats.TopLeft); x += widths[0];
                gfx.DrawString(c.FullName, fontSmall, XBrushes.Black, new XRect(x, y, widths[1], XUnit.FromPoint(12)), XStringFormats.TopLeft); x += widths[1];
                gfx.DrawString(c.SalesCount.ToString(), fontSmall, XBrushes.Black, new XRect(x, y, widths[2], XUnit.FromPoint(12)), XStringFormats.TopLeft); x += widths[2];
                gfx.DrawString(c.TotalSpent.ToString("F2"), fontSmall, XBrushes.Black, new XRect(x, y, widths[3], XUnit.FromPoint(12)), XStringFormats.TopLeft); x += widths[3];
                // 6. CORREGIDO: Se usa XUnit.FromPoint(12) directamente
                gfx.DrawString(c.AvgTicket.ToString("F2"), fontSmall, XBrushes.Black, new XRect(x, y, widths[4], XUnit.FromPoint(12)), XStringFormats.TopLeft); x += widths[4];
                gfx.DrawString(c.LastSale?.ToString("yyyy-MM-dd") ?? "-", fontSmall, XBrushes.Black, new XRect(x, y, widths[5], XUnit.FromPoint(12)), XStringFormats.TopLeft);
                y += 16;
                idx++;
            }

            // Totales
            y += 10;
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 8;
            gfx.DrawString($"Total clientes: {_data.Count}", fontSmall, XBrushes.Black, new XRect(margin, y, 200, XUnit.FromPoint(12)), XStringFormats.TopLeft);
            gfx.DrawString($"Monto total: {_data.Sum(d => d.TotalSpent):F2}", fontSmall, XBrushes.Black, new XRect(page.Width - margin - 200, y, 200, XUnit.FromPoint(12)), XStringFormats.TopLeft);

            doc.Save(ms, false);
            return ms.ToArray();
        }
    }
}
