using System;

namespace ServiceReports.Application.DTOs
{
    public class MedicineByCategoryDto
    {
       
        // Datos de Categoría
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryStatus { get; set; }

        // Datos de Medicina
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string Description { get; set; }
        public int StockTotal { get; set; }
        public decimal UnitPrice { get; set; }
        public int MedicineStatus { get; set; }

        // Datos de Presentación
        public int PresentationId { get; set; }
        public string PresentationName { get; set; }

        // Campos calculados (útiles para el reporte)
        public decimal TotalValue => StockTotal * UnitPrice; // Valor total del stock
        public string StatusText => MedicineStatus == 1 ? "Activo" : "Inactivo";
        public string StockStatusText => StockTotal <= 10 ? "BAJO" : StockTotal <= 50 ? "MEDIO" : "ALTO";
    }
    public class MedicineByCategoryFilter
    {
        public int? CategoryId { get; set; }
        public bool OnlyLowStock { get; set; } = false;
        public int LowStockThreshold { get; set; } = 10; // Valor por defecto
        public decimal? MinPrice { get; set; } = null;
        public string SortBy { get; set; } = "CategoryName";
        public string SortOrder { get; set; } = "ASC";
        public decimal? MaxPrice { get; set; } = null;
    }
}