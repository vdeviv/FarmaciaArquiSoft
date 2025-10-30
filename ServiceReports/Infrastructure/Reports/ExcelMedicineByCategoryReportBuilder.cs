using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;

namespace ServiceReports.Infrastructure.Reports
{
    public class ExcelMedicineByCategoryReportBuilder : IMedicineByCategoryReportBuilder
    {
        private string _title = "Reporte de Medicinas por Categoría";
        private MedicineByCategoryFilter? _filters;
        private List<MedicineByCategoryDto> _data = new();

        public IMedicineByCategoryReportBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public IMedicineByCategoryReportBuilder SetLogoPath(string path)
        {
            // No se usa en Excel
            return this;
        }

        public IMedicineByCategoryReportBuilder SetGeneratedBy(string user)
        {
            // No se usa en Excel
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
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Medicinas por Categoría");

            // =======================================================
            // 1. ENCABEZADO (Filas 1-4)
            // =======================================================
            var titleCell = worksheet.Cell("A1");
            titleCell.Value = _title;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            titleCell.Style.Font.FontColor = XLColor.DarkBlue;
            worksheet.Range("A1:H1").Merge();
            worksheet.Range("A1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("A3").Value = "Fecha de Generación:";
            worksheet.Cell("A3").Style.Font.Bold = true;
            worksheet.Cell("B3").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Filtros aplicados
            if (_filters != null)
            {
                worksheet.Cell("A4").Value = "Filtros Aplicados:";
                worksheet.Cell("A4").Style.Font.Bold = true;

                string filtrosTexto = "";
                if (_filters.OnlyLowStock)
                    filtrosTexto += $"Stock Bajo (<={_filters.LowStockThreshold}); ";
                if (_filters.MinPrice.HasValue)
                    filtrosTexto += $"Precio Min: Bs.{_filters.MinPrice:N2}; ";
                if (_filters.MaxPrice.HasValue)
                    filtrosTexto += $"Precio Max: Bs.{_filters.MaxPrice:N2}; ";

                worksheet.Cell("B4").Value = string.IsNullOrEmpty(filtrosTexto) ? "Ninguno" : filtrosTexto;
            }

            // =======================================================
            // 2. ENCABEZADOS DE TABLA (Fila 6)
            // =======================================================
            var headerRow = 6;
            var headers = new string[] {
                "Categoría",
                "Medicina",
                "Presentación",
                "Stock",
                "Estado Stock",
                "Precio Unitario (Bs.)",
                "Valor Total (Bs.)",
                "Estado"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = worksheet.Cell(headerRow, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // =======================================================
            // 3. DATOS AGRUPADOS POR CATEGORÍA (Formato Desplegable)
            // =======================================================
            int row = headerRow + 1;

            // Agrupar por categoría
            var groupedData = _data
                .GroupBy(m => new { m.CategoryId, m.CategoryName })
                .OrderBy(g => g.Key.CategoryName);

            foreach (var categoryGroup in groupedData)
            {
                // 🔹 FILA DE CATEGORÍA (destacada)
                var categoryRow = worksheet.Range(row, 1, row, 8);
                categoryRow.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                categoryRow.Style.Font.Bold = true;
                categoryRow.Style.Font.FontSize = 11;

                worksheet.Cell(row, 1).Value = $"▼ {categoryGroup.Key.CategoryName}";
                worksheet.Range(row, 1, row, 3).Merge();

                var categoryMedicines = categoryGroup.ToList();
                int medicineCount = categoryMedicines.Count;
                decimal totalValueCategory = categoryMedicines.Sum(m => m.TotalValue);

                worksheet.Cell(row, 4).Value = $"{medicineCount} medicinas";
                worksheet.Range(row, 4, row, 6).Merge();

                worksheet.Cell(row, 7).Value = totalValueCategory;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(row, 8).Value = "TOTAL CATEGORÍA";

                categoryRow.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                row++;

                // 🔹 MEDICINAS DE LA CATEGORÍA
                foreach (var medicine in categoryMedicines)
                {
                    // Columna 1: Vacía (indent visual)
                    worksheet.Cell(row, 1).Value = "";

                    // Columna 2: Nombre de medicina
                    worksheet.Cell(row, 2).Value = medicine.MedicineName;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    // Columna 3: Presentación
                    worksheet.Cell(row, 3).Value = medicine.PresentationName ?? "N/A";
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Columna 4: Stock
                    worksheet.Cell(row, 4).Value = medicine.StockTotal;
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Color según nivel de stock
                    if (medicine.StockTotal <= 10)
                        worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                    else if (medicine.StockTotal <= 50)
                        worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.Orange;

                    // Columna 5: Estado de Stock
                    worksheet.Cell(row, 5).Value = medicine.StockStatusText;
                    worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;

                    if (medicine.StockStatusText == "BAJO")
                        worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.LightPink;
                    else if (medicine.StockStatusText == "MEDIO")
                        worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    // Columna 6: Precio Unitario
                    worksheet.Cell(row, 6).Value = medicine.UnitPrice;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Columna 7: Valor Total
                    worksheet.Cell(row, 7).Value = medicine.TotalValue;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, 7).Style.Font.Bold = true;

                    // Columna 8: Estado
                    worksheet.Cell(row, 8).Value = medicine.StatusText;
                    worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (medicine.MedicineStatus == 0)
                        worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.Red;

                    // Bordes
                    worksheet.Range(row, 1, row, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Alternancia de color
                    if ((row - headerRow) % 2 == 0)
                        worksheet.Range(row, 2, row, 8).Style.Fill.BackgroundColor = XLColor.LightGray;

                    row++;
                }

                // Línea separadora entre categorías
                row++;
            }

            // =======================================================
            // 4. FILA DE TOTALES GENERALES
            // =======================================================
            var totalRow = row;
            worksheet.Range(totalRow, 1, totalRow, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
            worksheet.Range(totalRow, 1, totalRow, 8).Style.Font.Bold = true;
            worksheet.Range(totalRow, 1, totalRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            worksheet.Cell(totalRow, 1).Value = $"TOTALES GENERALES:";
            worksheet.Range(totalRow, 1, totalRow, 3).Merge();

            worksheet.Cell(totalRow, 4).Value = _data.Count;
            worksheet.Cell(totalRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(totalRow, 5).Value = "medicinas";
            worksheet.Range(totalRow, 5, totalRow, 6).Merge();

            worksheet.Cell(totalRow, 7).Value = _data.Sum(m => m.TotalValue);
            worksheet.Cell(totalRow, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(totalRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(totalRow, 7).Style.Font.FontColor = XLColor.DarkBlue;

            // =======================================================
            // 5. AJUSTES FINALES
            // =======================================================
            worksheet.Columns().AdjustToContents();

            worksheet.Column(1).Width = 20; // Categoría
            worksheet.Column(2).Width = 35; // Medicina
            worksheet.Column(3).Width = 15; // Presentación
            worksheet.Column(4).Width = 10; // Stock
            worksheet.Column(5).Width = 12; // Estado Stock
            worksheet.Column(6).Width = 18; // Precio
            worksheet.Column(7).Width = 18; // Valor Total
            worksheet.Column(8).Width = 12; // Estado

            worksheet.SheetView.FreezeRows(headerRow);

            // =======================================================
            // 6. GUARDAR
            // =======================================================
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}