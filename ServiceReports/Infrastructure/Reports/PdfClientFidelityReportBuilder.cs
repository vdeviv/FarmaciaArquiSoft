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
        private string _title = "Reporte de Fidelidad de Clientes";
        private string? _logoPath;
        private string _generatedBy = "Sistema";
        private ClientFidelityFilter? _filters;
        private List<ClientFidelityDto> _data = new();

        public IClientFidelityReportBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public IClientFidelityReportBuilder SetLogoPath(string path)
        {
            _logoPath = path;
            return this;
        }

        public IClientFidelityReportBuilder SetGeneratedBy(string user)
        {
            _generatedBy = user;
            return this;
        }

        public IClientFidelityReportBuilder SetFilters(ClientFidelityFilter filters)
        {
            _filters = filters;
            return this;
        }

        public IClientFidelityReportBuilder SetData(IEnumerable<ClientFidelityDto> data)
        {
            _data = data.ToList();
            return this;
        }

        public byte[] Build()
        {
            using var ms = new MemoryStream();
            var doc = new PdfDocument();
            doc.Info.Title = _title;
            doc.Info.Author = _generatedBy;
            doc.Info.Creator = "FarmaView - Sistema de Reportes";
            doc.Info.CreationDate = DateTime.Now;

            // Fuentes
            var fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
            var fontNormal = new XFont("Arial", 10, XFontStyle.Bold);
            var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);
            var fontSmallBold = new XFont("Arial", 9, XFontStyle.Bold);
            var fontFooter = new XFont("Arial", 8, XFontStyle.Italic);

            double margin = 40;
            double footerHeight = 30; // Espacio reservado para el pie de página
            int currentPageNumber = 1;

            // =======================================================
            // CREAR PRIMERA PÁGINA
            // =======================================================
            var page = doc.AddPage();
            page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            double y = margin;

            // =======================================================
            // 1. ENCABEZADO: LOGO Y TÍTULO
            // =======================================================
            y = DrawHeader(gfx, page, margin, y, fontTitle, fontSmall);

            // =======================================================
            // 2. LÍNEA DE SEPARACIÓN
            // =======================================================
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 10;

            // =======================================================
            // 3. TÍTULO DE LA TABLA
            // =======================================================
            gfx.DrawString("Detalle de Clientes y Gasto", fontNormal, XBrushes.Black,
                new XRect(margin, y, page.Width - margin * 2, 12), XStringFormats.TopLeft);
            y += 20;

            // =======================================================
            // 4. TABLA DE DATOS
            // =======================================================
            string[] headers = { "Cliente", "NIT", "Compras", "Total Gastado", "Ticket Promedio", "Última Compra" };
            double[] widths = { 150, 60, 50, 90, 95, 95 };
            double tableWidth = widths.Sum();

            // Ajustar anchos si la tabla es muy ancha
            if (tableWidth > page.Width - (margin * 2))
            {
                double scale = (page.Width - (margin * 2)) / tableWidth;
                for (int i = 0; i < widths.Length; i++)
                {
                    widths[i] *= scale;
                }
            }

            // Dibujar encabezados de tabla
            double x = margin;
            foreach (var h in headers)
            {
                int idx = Array.IndexOf(headers, h);
                gfx.DrawString(h, fontSmallBold, XBrushes.White,
                    new XRect(x, y, widths[idx], 16), XStringFormats.TopLeft);

                // Fondo oscuro para encabezados
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(45, 55, 72)), x, y - 2, widths[idx], 18);
                gfx.DrawString(h, fontSmallBold, XBrushes.White,
                    new XRect(x, y, widths[idx], 16), XStringFormats.TopLeft);

                x += widths[idx];
            }
            y += 20;

            // =======================================================
            // 5. DATOS DE CLIENTES
            // =======================================================
            bool isAlternateRow = false;

            foreach (var client in _data)
            {
                // Verificar si necesitamos nueva página (espacio para pie de página)
                if (y > page.Height - margin - footerHeight - 30)
                {
                    // 🚀 DIBUJAR PIE DE PÁGINA ANTES DE CAMBIAR
                    DrawFooter(gfx, page, currentPageNumber, margin, fontFooter);
                    currentPageNumber++;

                    // Nueva página
                    page = doc.AddPage();
                    page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;

                    // Redibujar encabezado de tabla
                    x = margin;
                    foreach (var h in headers)
                    {
                        int idx = Array.IndexOf(headers, h);
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(45, 55, 72)), x, y - 2, widths[idx], 18);
                        gfx.DrawString(h, fontSmallBold, XBrushes.White,
                            new XRect(x, y, widths[idx], 16), XStringFormats.TopLeft);
                        x += widths[idx];
                    }
                    y += 20;
                    isAlternateRow = false;
                }

                // Fondo alternado para filas
                if (isAlternateRow)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)),
                        margin, y - 2, widths.Sum(), 16);
                }

                // Dibujar fila de datos
                x = margin;

                // Cliente
                gfx.DrawString(client.FullName, fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[0], 12), XStringFormats.TopLeft);
                x += widths[0];

                // NIT
                gfx.DrawString(client.ClientNit ?? "S/N", fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[1], 12), XStringFormats.TopLeft);
                x += widths[1];

                // Compras
                gfx.DrawString(client.SalesCount.ToString(), fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[2], 12), XStringFormats.Center);
                x += widths[2];

                // Total Gastado
                gfx.DrawString("Bs. " + client.TotalSpent.ToString("N2", CultureInfo.InvariantCulture),
                    fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[3], 12), XStringFormats.TopLeft);
                x += widths[3];

                // Ticket Promedio
                gfx.DrawString("Bs. " + client.AvgTicket.ToString("N2", CultureInfo.InvariantCulture),
                    fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[4], 12), XStringFormats.TopLeft);
                x += widths[4];

                // Última Compra
                gfx.DrawString(client.LastSale?.ToString("dd/MM/yyyy") ?? "Nunca",
                    fontSmall, XBrushes.Black,
                    new XRect(x, y, widths[5], 12), XStringFormats.Center);

                y += 16;
                isAlternateRow = !isAlternateRow;
            }

            // =======================================================
            // 6. TOTALES AL FINAL
            // =======================================================
            y += 10;
            gfx.DrawLine(new XPen(XColors.Black, 2), margin, y, margin + widths.Sum(), y);
            y += 10;

            // Fondo para totales
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 251, 235)),
                margin, y - 5, widths.Sum(), 20);

            gfx.DrawString($"Total de clientes: {_data.Count}",
                fontSmallBold, XBrushes.Black,
                new XRect(margin + 5, y, 250, 12), XStringFormats.TopLeft);

            gfx.DrawString($"Monto total gastado: Bs. {_data.Sum(d => d.TotalSpent):N2}",
                fontSmallBold, XBrushes.Black,
                new XRect(page.Width - margin - 270, y, 270, 12), XStringFormats.TopRight);

            // =======================================================
            // 7. 🚀 DIBUJAR PIE DE PÁGINA EN LA ÚLTIMA PÁGINA
            // =======================================================
            DrawFooter(gfx, page, currentPageNumber, margin, fontFooter);

            // =======================================================
            // 8. GUARDAR DOCUMENTO
            // =======================================================
            doc.Save(ms, false);
            return ms.ToArray();
        }

        /// <summary>
        /// Dibuja el encabezado con logo, título e información
        /// </summary>
        private double DrawHeader(XGraphics gfx, PdfPage page, double margin, double y, XFont fontTitle, XFont fontSmall)
        {
            double logoWidth = 60;
            double logoHeight = 60;
            bool logoDrawn = false;

            // =======================================================
            // LOGO
            // =======================================================
            if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(_logoPath);

                    if (fileInfo.Length > 0 && fileInfo.Length < 5_000_000)
                    {
                        byte[] imageBytes = File.ReadAllBytes(_logoPath);
                        Func<Stream> streamProvider = () => new MemoryStream(imageBytes);

                        using (var img = XImage.FromStream(streamProvider))
                        {
                            gfx.DrawImage(img, margin, y, logoWidth, logoHeight);
                            logoDrawn = true;
                        }
                    }
                }
                catch
                {
                    // Si falla, dibujar placeholder
                    gfx.DrawRectangle(XPens.LightGray, XBrushes.White, margin, y, logoWidth, logoHeight);
                    gfx.DrawString("LOGO", new XFont("Arial", 10), XBrushes.LightGray,
                        new XRect(margin, y, logoWidth, logoHeight), XStringFormats.Center);
                }
            }

            if (!logoDrawn)
            {
                gfx.DrawRectangle(XPens.LightGray, XBrushes.White, margin, y, logoWidth, logoHeight);
                gfx.DrawString("LOGO", new XFont("Arial", 10), XBrushes.LightGray,
                    new XRect(margin, y, logoWidth, logoHeight), XStringFormats.Center);
            }

            // =======================================================
            // TÍTULO Y DATOS
            // =======================================================
            double textX = margin + logoWidth + 15;
            double textWidth = page.Width - textX - margin;

            // Título
            gfx.DrawString(_title, fontTitle, XBrushes.DarkBlue,
                new XRect(textX, y, textWidth, 30), XStringFormats.TopLeft);
            y += 25;

            // Generado por
            gfx.DrawString($"Generado por: {_generatedBy}", fontSmall, XBrushes.Black,
                new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
            y += 15;

            // Período
            if (_filters != null)
            {
                string fechaInicio = _filters.StartDate.ToString("dd/MM/yyyy");
                string fechaFin = _filters.EndDate.ToString("dd/MM/yyyy");

                gfx.DrawString($"Período: {fechaInicio} - {fechaFin}", fontSmall, XBrushes.Black,
                    new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
                y += 15;

                // Filtro mínimo (si aplica)
                if (_filters.MinTotal.HasValue && _filters.MinTotal.Value > 0)
                {
                    gfx.DrawString($"Gasto Mínimo: Bs. {_filters.MinTotal.Value:N2}",
                        fontSmall, XBrushes.Black,
                        new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
                    y += 15;
                }
            }

            // Fecha de generación
            gfx.DrawString($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                fontSmall, XBrushes.Gray,
                new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
            y += 15;

            // 🏆 Indicador de Top N (si aplica)
            if (_filters != null && _filters.IsTopNFilter)
            {
                // Fondo amarillo para destacar
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 243, 205)),
                    textX - 5, y - 2, textWidth + 5, 18);

                gfx.DrawString($"🏆 Mostrando Top {_filters.TopN} clientes con mayor gasto",
                    new XFont("Arial", 9, XFontStyle.Bold), XBrushes.DarkOrange,
                    new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
                y += 18;
            }

            y += 5;

            return y;
        }

        /// <summary>
        /// 🚀 Dibuja el pie de página con número de página y información adicional
        /// </summary>
        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber, double margin, XFont fontFooter)
        {
            double footerY = page.Height - margin + 5;

            // Fondo del pie de página
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(250, 250, 250)),
                0, footerY - 5, page.Width, 30);

            // Línea superior del pie
            gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1),
                margin, footerY, page.Width - margin, footerY);
            footerY += 8;

            // Texto izquierdo: Sistema
            gfx.DrawString("FarmaView - Sistema de Gestión Farmacéutica",
                fontFooter, XBrushes.Gray,
                new XRect(margin, footerY, page.Width / 3, 20),
                XStringFormats.TopLeft);

            // Texto centro: Fecha
            gfx.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                fontFooter, XBrushes.Gray,
                new XRect(0, footerY, page.Width, 20),
                XStringFormats.TopCenter);

            // Texto derecho: Número de página
            gfx.DrawString($"Página {pageNumber}",
                fontFooter, new XSolidBrush(XColor.FromArgb(60, 60, 60)),
                new XRect(page.Width * 2 / 3, footerY, page.Width / 3 - margin, 20),
                XStringFormats.TopRight);
        }
    }
}