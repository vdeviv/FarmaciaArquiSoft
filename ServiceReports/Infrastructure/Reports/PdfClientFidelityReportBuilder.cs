// Ruta: ServiceReports.Infrastructure.Reports/PdfClientFidelityReportBuilder.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;

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
            page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            // Declaración de fuentes (CORREGIDA)
            var fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
            // var fontHeader = new XFont("Arial", 14, XFontStyle.Bold); // No usada, eliminada para limpieza
            XFont fontNormal = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmall = new XFont("Arial", 9, XFontStyle.Regular);
            XFont fontSmallBold = new XFont("Arial", 9, XFontStyle.Bold);

            double margin = 40;
            double y = margin;
            double headerHeight = 70;

            // =======================================================
            // 1. DIBUJO DEL LOGO (Manejo de OutOfMemoryError)
            // =======================================================
            if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(_logoPath);

                    Func<Stream> streamProvider = () => new MemoryStream(imageBytes);

                    using (var img = XImage.FromStream(streamProvider))
                    {
                        gfx.DrawImage(img, margin, y, 60, 60);
                    }
                }
                catch (Exception ex)
                {
                    string error = ex is OutOfMemoryException ? "OOM: Reduzca el tamaño del logo." : ex.Message;
                    gfx.DrawString($"Logo no disponible (Error: {error.Substring(0, Math.Min(error.Length, 40))}...)", fontSmall, XBrushes.Red, new XRect(margin, y, 100, 60), XStringFormats.TopLeft);
                }
            }

            // DIBUJO DEL TÍTULO Y FILTROS
            gfx.DrawString(_title, fontTitle, XBrushes.Black, new XRect(margin + 70, y, page.Width - margin - 70, 30), XStringFormats.TopLeft);
            y += 30;
            gfx.DrawString($"Generado por: {_generatedBy}", fontSmall, XBrushes.Black, new XRect(margin + 70, y, page.Width - margin - 70, 15), XStringFormats.TopLeft);
            y += 15;
            gfx.DrawString($"Período: {_filters?.StartDate:d} - {_filters?.EndDate:d}", fontSmall, XBrushes.Black, new XRect(margin + 70, y, page.Width - margin - 70, 15), XStringFormats.TopLeft);
            y += 15;

            // LÍNEA DE SEPARACIÓN DESPUÉS DEL ENCABEZADO
            y += headerHeight;
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 10;

            // ==============================================================
            // 2. DIBUJO DEL GRÁFICO CIRCULAR (PIE CHART) - SECCIÓN ELIMINADA ❌
            // La variable 'y' ahora continúa directamente para dibujar la tabla.
            // ==============================================================

            // TÍTULO DE LA TABLA
            gfx.DrawString("Detalle de Clientes y Gasto", fontNormal, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 12), XStringFormats.TopLeft);
            y += 15;

            // ==============================================================
            // 3. TABLA DE DATOS
            // ==============================================================
            string[] headers = { "Cliente", "NIT", "Compras", "Total Gastado", "Ticket Promedio", "Última Compra" };
            double[] widths = { 150, 70, 70, 90, 70, 90 };
            double x = margin;

            // Dibujar encabezados de tabla
            foreach (var h in headers)
            {
                gfx.DrawString(h, fontSmallBold, XBrushes.Black, new XRect(x, y, widths[Array.IndexOf(headers, h)], 16), XStringFormats.TopLeft);
                x += widths[Array.IndexOf(headers, h)];
            }
            y += 18;

            int idx = 0;
            foreach (var client in _data)
            {
                x = margin;
                gfx.DrawString(client.FullName, fontSmall, XBrushes.Black, new XRect(x, y, widths[0], 12), XStringFormats.TopLeft);
                x += widths[0];
                gfx.DrawString(client.ClientNit ?? "-", fontSmall, XBrushes.Black, new XRect(x, y, widths[1], 12), XStringFormats.TopLeft);
                x += widths[1];
                gfx.DrawString(client.SalesCount.ToString(), fontSmall, XBrushes.Black, new XRect(x, y, widths[2], 12), XStringFormats.TopLeft);
                x += widths[2];
                gfx.DrawString(client.TotalSpent.ToString("N2", CultureInfo.InvariantCulture), fontSmall, XBrushes.Black, new XRect(x, y, widths[3], 12), XStringFormats.TopLeft);
                x += widths[3];
                gfx.DrawString(client.AvgTicket.ToString("N2", CultureInfo.InvariantCulture), fontSmall, XBrushes.Black, new XRect(x, y, widths[4], 12), XStringFormats.TopLeft);
                x += widths[4];
                gfx.DrawString(client.LastSale?.ToString("d") ?? "-", fontSmall, XBrushes.Black, new XRect(x, y, widths[5], 12), XStringFormats.TopLeft);

                y += 16;
                idx++;

                // Manejo de salto de página
                if (y > page.Height - margin)
                {
                    page = doc.AddPage();
                    page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;

                    // Redibujar encabezado de tabla en la nueva página
                    x = margin;
                    foreach (var h in headers)
                    {
                        gfx.DrawString(h, fontSmallBold, XBrushes.Black, new XRect(x, y, widths[Array.IndexOf(headers, h)], 16), XStringFormats.TopLeft);
                        x += widths[Array.IndexOf(headers, h)];
                    }
                    y += 18;
                }
            }

            // Totales al final
            y += 10;
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 8;
            gfx.DrawString($"Total clientes listados: {_data.Count}", fontSmall, XBrushes.Black, new XRect(margin, y, 200, 12), XStringFormats.TopLeft);
            gfx.DrawString($"Monto total gastado: {_data.Sum(d => d.TotalSpent):N2}", fontSmall, XBrushes.Black, new XRect(page.Width - margin - 200, y, 200, 12), XStringFormats.TopRight);

            doc.Save(ms, false);
            return ms.ToArray();
        }
    }
}