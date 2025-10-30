// Ruta: ServiceReports.Infrastructure.Reports/ExcelClientFidelityReportBuilder.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ServiceReports.Application.DTOs;
using ServiceReports.Application.Interfaces;

namespace ServiceReports.Infrastructure.Reports
{
    public class ExcelClientFidelityReportBuilder : IClientFidelityReportBuilder
    {
        private string _title = "Reporte de Fidelidad de Clientes";
        private ClientFidelityFilter? _filters;
        private List<ClientFidelityDto> _data = new();

        public IClientFidelityReportBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public IClientFidelityReportBuilder SetLogoPath(string path)
        {
            // No se usa en Excel, pero implementamos la interfaz
            return this;
        }

        public IClientFidelityReportBuilder SetGeneratedBy(string user)
        {
            // No se usa en Excel, pero implementamos la interfaz
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
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Fidelidad Clientes");

            // =======================================================
            // 1. ENCABEZADO Y FILTROS (Filas 1-5)
            // =======================================================
            var titleCell = worksheet.Cell("A1");
            titleCell.Value = _title;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            titleCell.Style.Font.FontColor = XLColor.DarkBlue;
            worksheet.Range("A1:E1").Merge();
            worksheet.Range("A1:E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Línea 2: vacía para separación
            worksheet.Cell("A3").Value = "Período del Reporte:";
            worksheet.Cell("A3").Style.Font.Bold = true;
            worksheet.Cell("B3").Value = $"{_filters?.StartDate:dd/MM/yyyy} - {_filters?.EndDate:dd/MM/yyyy}";

            worksheet.Cell("A4").Value = "Filtro Mínimo de Gasto:";
            worksheet.Cell("A4").Style.Font.Bold = true;
            worksheet.Cell("B4").Value = _filters?.MinTotal ?? 0;
            worksheet.Cell("B4").Style.NumberFormat.Format = "#,##0.00";

            worksheet.Cell("A5").Value = "Fecha de Generación:";
            worksheet.Cell("A5").Style.Font.Bold = true;
            worksheet.Cell("B5").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // =======================================================
            // 2. ENCABEZADOS DE LA TABLA (Fila 7)
            // =======================================================
            var headerRow = 7;
            var headers = new string[] {
                "Cliente (NIT)",
                "Total Compras",
                "Total Gastado (Bs.)",
                "Ticket Promedio (Bs.)",
                "Última Compra"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = worksheet.Cell(headerRow, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // =======================================================
            // 3. DATOS DE CLIENTES (Fila 8 en adelante)
            // =======================================================
            int row = headerRow + 1;

            foreach (var client in _data)
            {
                // Columna 1: Cliente (FullName + NIT)
                worksheet.Cell(row, 1).Value = $"{client.FullName} ({client.ClientNit ?? "S/N"})";
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Columna 2: Total Compras
                worksheet.Cell(row, 2).Value = client.SalesCount;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Columna 3: Total Gastado
                worksheet.Cell(row, 3).Value = client.TotalSpent;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Columna 4: Ticket Promedio
                worksheet.Cell(row, 4).Value = client.AvgTicket;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Columna 5: Última Compra
                worksheet.Cell(row, 5).Value = client.LastSale?.ToString("dd/MM/yyyy") ?? "Nunca";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Aplicar bordes a la fila
                worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Alternar color de fondo para mejor legibilidad
                if ((row - headerRow) % 2 == 0)
                {
                    worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                row++;
            }

            // =======================================================
            // 4. FILA DE TOTALES
            // =======================================================
            row++; // Fila vacía de separación

            var totalRow = row;

            worksheet.Cell(totalRow, 1).Value = $"Total de Clientes: {_data.Count}";
            worksheet.Cell(totalRow, 1).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 1).Style.Font.FontSize = 11;
            worksheet.Range(totalRow, 1, totalRow, 2).Merge();

            worksheet.Cell(totalRow, 3).Value = "TOTAL GASTADO:";
            worksheet.Cell(totalRow, 3).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 3).Style.Font.FontColor = XLColor.DarkBlue;
            worksheet.Cell(totalRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(totalRow, 4).Value = _data.Sum(d => d.TotalSpent);
            worksheet.Cell(totalRow, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(totalRow, 4).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 4).Style.Font.FontColor = XLColor.DarkBlue;
            worksheet.Cell(totalRow, 4).Style.Fill.BackgroundColor = XLColor.LightYellow;
            worksheet.Cell(totalRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Borde al total
            worksheet.Range(totalRow, 1, totalRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // =======================================================
            // 5. AJUSTES FINALES DE FORMATO
            // =======================================================

            // Autoajustar columnas
            worksheet.Columns().AdjustToContents();

            // Establecer anchos mínimos para mejor visualización
            worksheet.Column(1).Width = 35; // Cliente
            worksheet.Column(2).Width = 15; // Total Compras
            worksheet.Column(3).Width = 18; // Total Gastado
            worksheet.Column(4).Width = 20; // Ticket Promedio
            worksheet.Column(5).Width = 15; // Última Compra

            // Congelar la fila de encabezados para scroll
            worksheet.SheetView.FreezeRows(headerRow);

            // =======================================================
            // 6. GUARDAR Y RETORNAR BYTES
            // =======================================================
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}