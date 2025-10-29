using ServiceReports.Application.DTOs;
using ServiceCommon;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceCommon.Infrastructure.Data;

namespace ServiceReports.Infrastructure.Repositories
{
    public class ReportRepository
    {
        private readonly DatabaseConnection _db;

        public ReportRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<IEnumerable<ClientFidelityDto>> GetClientFidelityAsync(ClientFidelityFilter filter, CancellationToken ct = default)
        {
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
                LIMIT 100;
            ";

            using var conn = DatabaseConnection.Instance.GetConnection();
            await conn.OpenAsync(ct);

            var result = await conn.QueryAsync<ClientFidelityDto>(sql, new
            {
                filter.StartDate,
                filter.EndDate,
                MinTotal = filter.MinTotal ?? 0,
                // 🆕 CORRECCIÓN: Ahora que existen en el filtro DTO, se envían
                SortBy = filter.SortBy ?? "FullName",
                SortOrder = filter.SortOrder ?? "ASC"
            });

            return result;
        }
    }
}