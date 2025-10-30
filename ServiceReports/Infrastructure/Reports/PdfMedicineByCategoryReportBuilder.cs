// Ruta: ServiceReports.Infrastructure.Reports/PdfMedicineByCategoryReportBuilder.cs

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
    public class PdfMedicineByCategoryReportBuilder : IMedicineByCategoryReportBuilder
    {
        private string _title = "Reporte de Medicinas por Categoría";
        private string? _logoPath;
        private string _generatedBy = "Sistema";
        private MedicineByCategoryFilter? _filters;
        private List<MedicineByCategoryDto> _data = new();

        public IMedicineByCategoryReportBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public IMedicineByCategoryReportBuilder SetLogoPath(string path)
        {
            _logoPath = path;
            return this;
        }

        public IMedicineByCategoryReportBuilder SetGeneratedBy(string user)
        {
            _generatedBy = user;
            return this;
        }

        public IMedicineByCategoryReportBuilder SetFilters(MedicineByCategoryFilter filters)
        {
            _filters = filters;
            return this;
        }

        public IMedicineByCategoryReportBuilder SetData(IEnumerable<MedicineByCategoryDto> data)
        {
            _data = (data ?? Enumerable.Empty<MedicineByCategoryDto>()).ToList();
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
            var fontCategoryHeader = new XFont("Arial", 11, XFontStyle.Bold);
            var fontNormal = new XFont("Arial", 10, XFontStyle.Bold);
            var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);
            var fontSmallBold = new XFont("Arial", 9, XFontStyle.Bold);
            var fontFooter = new XFont("Arial", 8, XFontStyle.Italic);

            double margin = 40;
            double footerHeight = 30;
            int currentPageNumber = 1;

            // =======================================================
            // CREAR PRIMERA PÁGINA
            // =======================================================
            var page = doc.AddPage();
            page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            double y = margin;

            // =======================================================
            // 1. ENCABEZADO
            // =======================================================
            y = DrawHeader(gfx, page, margin, y, fontTitle, fontSmall);

            // Línea separadora
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 10;

            // =======================================================
            // 2. AGRUPAR DATOS POR CATEGORÍA
            // =======================================================
            var groupedData = _data
                .GroupBy(m => new { m.CategoryId, m.CategoryName })
                .OrderBy(g => g.Key.CategoryName);

            foreach (var categoryGroup in groupedData)
            {
                // 🔹 VERIFICAR ESPACIO PARA ENCABEZADO DE CATEGORÍA
                if (y > page.Height - margin - footerHeight - 80)
                {
                    DrawFooter(gfx, page, currentPageNumber, margin, fontFooter);
                    currentPageNumber++;

                    page = doc.AddPage();
                    page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                // 🔹 ENCABEZADO DE CATEGORÍA
                var categoryMedicines = categoryGroup.ToList();
                decimal categoryTotalValue = categoryMedicines.Sum(m => m.TotalValue);

                // Fondo de categoría
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(70, 130, 180)),
                    margin, y - 2, page.Width - margin * 2, 20);

                gfx.DrawString($"▼ {categoryGroup.Key.CategoryName}",
                    fontCategoryHeader, XBrushes.White,
                    new XRect(margin + 5, y, 300, 16), XStringFormats.TopLeft);

                gfx.DrawString($"{categoryMedicines.Count} medicinas | Valor Total: Bs. {categoryTotalValue:N2}",
                    fontSmallBold, XBrushes.White,
                    new XRect(page.Width - margin - 250, y, 240, 16), XStringFormats.TopRight);

                y += 22;

                // 🔹 ENCABEZADOS DE COLUMNAS DE MEDICINAS
                string[] headers = { "Medicina", "Presentación", "Stock", "Estado", "P.Unit (Bs.)", "Total (Bs.)" };
                double[] widths = { 180, 90, 50, 50, 70, 80 };

                double x = margin + 10; // Indent para medicinas
                foreach (var h in headers)
                {
                    int idx = Array.IndexOf(headers, h);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(45, 55, 72)),
                        x, y - 2, widths[idx], 16);
                    gfx.DrawString(h, fontSmallBold, XBrushes.White,
                        new XRect(x, y, widths[idx], 14), XStringFormats.TopLeft);
                    x += widths[idx];
                }
                y += 18;

                // 🔹 MEDICINAS DE LA CATEGORÍA
                bool isAlternate = false;

                foreach (var medicine in categoryMedicines)
                {
                    // Verificar espacio
                    if (y > page.Height - margin - footerHeight - 30)
                    {
                        DrawFooter(gfx, page, currentPageNumber, margin, fontFooter);
                        currentPageNumber++;

                        page = doc.AddPage();
                        page.Size = (PdfSharpCore.PageSize)PdfSharp.PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;

                        // Redibujar encabezado de columnas
                        x = margin + 10;
                        foreach (var h in headers)
                        {
                            int idx = Array.IndexOf(headers, h);
                            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(45, 55, 72)),
                                x, y - 2, widths[idx], 16);
                            gfx.DrawString(h, fontSmallBold, XBrushes.White,
                                new XRect(x, y, widths[idx], 14), XStringFormats.TopLeft);
                            x += widths[idx];
                        }
                        y += 18;
                        isAlternate = false;
                    }

                    // Fondo alternado
                    if (isAlternate)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(245, 245, 245)),
                            margin + 10, y - 2, widths.Sum(), 14);
                    }

                    x = margin + 10;

                    // Medicina
                    gfx.DrawString(medicine.MedicineName, fontSmall, XBrushes.Black,
                        new XRect(x, y, widths[0] - 5, 12), XStringFormats.TopLeft);
                    x += widths[0];

                    // Presentación
                    gfx.DrawString(medicine.PresentationName ?? "N/A", fontSmall, XBrushes.Black,
                        new XRect(x, y, widths[1], 12), XStringFormats.TopLeft);
                    x += widths[1];

                    // Stock (con color según nivel)
                    XBrush stockBrush = medicine.StockTotal <= 10 ? XBrushes.Red :
                                       medicine.StockTotal <= 50 ? XBrushes.Orange : XBrushes.Black;

                    gfx.DrawString(medicine.StockTotal.ToString(), fontSmallBold, stockBrush,
                        new XRect(x, y, widths[2], 12), XStringFormats.Center);
                    x += widths[2];

                    // Estado de Stock
                    gfx.DrawString(medicine.StockStatusText, fontSmall, stockBrush,
                        new XRect(x, y, widths[3], 12), XStringFormats.Center);
                    x += widths[3];

                    // Precio Unitario
                    gfx.DrawString(medicine.UnitPrice.ToString("N2", CultureInfo.InvariantCulture),
                        fontSmall, XBrushes.Black,
                        new XRect(x, y, widths[4], 12), XStringFormats.TopRight);
                    x += widths[4];

                    // Valor Total
                    gfx.DrawString(medicine.TotalValue.ToString("N2", CultureInfo.InvariantCulture),
                        fontSmallBold, XBrushes.Black,
                        new XRect(x, y, widths[5], 12), XStringFormats.TopRight);

                    y += 14;
                    isAlternate = !isAlternate;
                }

                // Línea separadora entre categorías
                y += 5;
                gfx.DrawLine(new XPen(XColors.Gray, 1), margin, y, page.Width - margin, y);
                y += 10;
            }

            // =======================================================
            // 3. TOTALES GENERALES
            // =======================================================
            y += 5;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 251, 235)),
                margin, y - 5, page.Width - margin * 2, 25);

            gfx.DrawString($"TOTALES GENERALES: {_data.Count} medicinas",
                fontNormal, XBrushes.DarkBlue,
                new XRect(margin + 5, y, 300, 12), XStringFormats.TopLeft);

            gfx.DrawString($"VALOR TOTAL INVENTARIO: Bs. {_data.Sum(m => m.TotalValue):N2}",
                fontNormal, XBrushes.DarkBlue,
                new XRect(page.Width - margin - 320, y, 310, 12), XStringFormats.TopRight);

            // =======================================================
            // 4. PIE DE PÁGINA FINAL
            // =======================================================
            DrawFooter(gfx, page, currentPageNumber, margin, fontFooter);

            // =======================================================
            // 5. GUARDAR DOCUMENTO
            // =======================================================
            doc.Save(ms, false);
            return ms.ToArray();
        }

        private double DrawHeader(XGraphics gfx, PdfPage page, double margin, double y, XFont fontTitle, XFont fontSmall)
        {
            double logoWidth = 60;
            double logoHeight = 60;
            bool logoDrawn = false;

            // Logo
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
                catch { }
            }

            if (!logoDrawn)
            {
                gfx.DrawRectangle(XPens.LightGray, XBrushes.White, margin, y, logoWidth, logoHeight);
                gfx.DrawString("LOGO", new XFont("Arial", 10), XBrushes.LightGray,
                    new XRect(margin, y, logoWidth, logoHeight), XStringFormats.Center);
            }

            // Título
            double textX = margin + logoWidth + 15;
            double textWidth = page.Width - textX - margin;

            gfx.DrawString(_title, fontTitle, XBrushes.DarkBlue,
                new XRect(textX, y, textWidth, 30), XStringFormats.TopLeft);
            y += 25;

            gfx.DrawString($"Generado por: {_generatedBy}", fontSmall, XBrushes.Black,
                new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
            y += 15;

            gfx.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontSmall, XBrushes.Gray,
                new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
            y += 15;

            // Filtros aplicados
            if (_filters != null)
            {
                string filtros = "";
                if (_filters.OnlyLowStock)
                    filtros += $"Stock Bajo (<={_filters.LowStockThreshold}); ";
                if (_filters.MinPrice.HasValue)
                    filtros += $"Precio Min: Bs.{_filters.MinPrice:N2}; ";

                if (!string.IsNullOrEmpty(filtros))
                {
                    gfx.DrawString($"Filtros: {filtros}", fontSmall, XBrushes.Gray,
                        new XRect(textX, y, textWidth, 15), XStringFormats.TopLeft);
                    y += 15;
                }
            }

            y += 5;
            return y;
        }

        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber, double margin, XFont fontFooter)
        {
            double footerY = page.Height - margin + 5;

            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(250, 250, 250)),
                0, footerY - 5, page.Width, 30);

            gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1),
                margin, footerY, page.Width - margin, footerY);
            footerY += 8;

            gfx.DrawString("FarmaView - Sistema de Gestión Farmacéutica",
                fontFooter, XBrushes.Gray,
                new XRect(margin, footerY, page.Width / 3, 20), XStringFormats.TopLeft);

            gfx.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                fontFooter, XBrushes.Gray,
                new XRect(0, footerY, page.Width, 20), XStringFormats.TopCenter);

            gfx.DrawString($"Página {pageNumber}",
                fontFooter, new XSolidBrush(XColor.FromArgb(60, 60, 60)),
                new XRect(page.Width * 2 / 3, footerY, page.Width / 3 - margin, 20), XStringFormats.TopRight);
        }
    }
}