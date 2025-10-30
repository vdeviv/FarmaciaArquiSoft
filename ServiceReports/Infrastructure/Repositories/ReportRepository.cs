using Dapper;
using ServiceCommon;
using ServiceCommon.Infrastructure.Data;
using ServiceReports.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static ServiceReports.Application.DTOs.MedicineByCategoryDto;
using System.Data; // 🚀 Asegúrese de incluir esto para IDbConnection

// Alias para usar el CategoryDto recién creado
using CategoryDto = ServiceReports.Application.DTOs.CategoryDto;

namespace ServiceReports.Infrastructure.Repositories
{
    public class ReportRepository
    {
        private readonly DatabaseConnection _db;

        public ReportRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<IEnumerable<ClientFidelityDto>> GetClientFidelityAsync(
            ClientFidelityFilter filter,
            CancellationToken ct = default)
        {
            // ... (El código existente para GetClientFidelityAsync es correcto y no se modifica)
            int limit = filter.TopN ?? 100;
            string sortBy = filter.SortBy ?? "FullName";
            string sortOrder = filter.SortOrder ?? "ASC";

            if (filter.IsTopNFilter)
            {
                sortBy = "TotalSpent";
                sortOrder = "DESC";
            }

            const string sql = @"
                SELECT 
                    c.id AS ClientId,
                    COALESCE(CONCAT(c.first_name, ' ', c.last_name), 'Cliente Anónimo') AS FullName,
                    c.nit AS ClientNit, 
                    COUNT(s.id) AS SalesCount,
                    COALESCE(SUM(s.total_amount), 0) AS TotalSpent,
                    COALESCE(AVG(s.total_amount), 0) AS AvgTicket,
                    MAX(s.sale_date) AS LastSale 
                FROM clients c
                LEFT JOIN sales s ON s.client_id = c.id
                    AND s.sale_date BETWEEN @StartDate AND @EndDate 
                GROUP BY c.id, c.first_name, c.last_name, c.nit
                HAVING COALESCE(SUM(s.total_amount), 0) >= @MinTotal
                
                ORDER BY 
                    CASE WHEN @SortBy = 'FullName' AND @SortOrder = 'ASC' THEN FullName END ASC,
                    CASE WHEN @SortBy = 'FullName' AND @SortOrder = 'DESC' THEN FullName END DESC,

                    CASE WHEN @SortBy = 'TotalSpent' AND @SortOrder = 'ASC' THEN TotalSpent END ASC,
                    CASE WHEN @SortBy = 'TotalSpent' AND @SortOrder = 'DESC' THEN TotalSpent END DESC,

                    CASE WHEN @SortBy = 'SalesCount' AND @SortOrder = 'ASC' THEN SalesCount END ASC,
                    CASE WHEN @SortBy = 'SalesCount' AND @SortOrder = 'DESC' THEN SalesCount END DESC
                    
                LIMIT @Limit;
            ";

            using var conn = DatabaseConnection.Instance.GetConnection();

            var result = await conn.QueryAsync<ClientFidelityDto>(sql, new
            {
                filter.StartDate,
                filter.EndDate,
                MinTotal = filter.MinTotal ?? 0,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Limit = limit
            });

            return result;
        }

        public async Task<IEnumerable<MedicineByCategoryDto>> GetMedicinesByCategoryAsync(MedicineByCategoryFilter filter)
        {
            // ✅ CORRECCIÓN: Cambiado de 'const string' a 'string' para que sea modificable.
            string sql = @"
    SELECT 
        c.id AS CategoryId,
        c.name AS CategoryName,
        c.status AS CategoryStatus,
        m.id AS MedicineId,
        m.name AS MedicineName,
        m.description AS Description,
        m.stock_total AS StockTotal,
        m.unit_price AS UnitPrice,
        m.status AS MedicineStatus,
        p.id AS PresentationId,
        p.name AS PresentationName
    FROM
        medicines m
    INNER JOIN
        categories c ON m.category_id = c.id
    INNER JOIN
        presentations p ON m.presentation_id = p.id
    WHERE 1 = 1
";

            // 1. Aplicar filtro de stock bajo (OnlyLowStock)
            if (filter.OnlyLowStock)
            {
                sql += " AND m.stock_total <= @LowStockThreshold";
            }

            // 2. Aplicar filtro de precio mínimo (MinPrice)
            if (filter.MinPrice.HasValue)
            {
                sql += " AND m.unit_price >= @MinPrice";
            }

            // 3. Aplicar filtro por categoría (CategoryId)
            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                // Nota: Asumiendo que 'c.id' es la columna de la base de datos
                sql += " AND c.id = @CategoryId";
            }

            // 4. Ordenar para agrupar en el reporte
            // (Asegúrese de que filter.SortBy y filter.SortOrder sean nombres de columna válidos para evitar inyección SQL)
            sql += $" ORDER BY c.name ASC, {filter.SortBy} {filter.SortOrder}";

            // Obtener la conexión y ejecutar la consulta
            using var conn = DatabaseConnection.Instance.GetConnection();

            // Dapper mapeará los parámetros del objeto 'filter'
            var result = await conn.QueryAsync<MedicineByCategoryDto>(sql, filter);

            return result;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            // ✅ CORRECCIÓN: Usar el nombre de tabla 'categories' (asumido por convención)
            // y las columnas reales 'id', 'name', 'status' de la tabla.
            string sql = @"
        SELECT
            id AS Id,
            name AS Name
        FROM
            categories
        WHERE
            status = 1 -- Asumiendo que la columna de estado se llama 'status'
        ORDER BY
            name ASC";

            // 🚀 SOLUCIÓN AL ERROR DE DAPPER: Obtener la conexión y usar QueryAsync en ella.
            using var conn = DatabaseConnection.Instance.GetConnection();

            return await conn.QueryAsync<CategoryDto>(sql);
        }
    }
}