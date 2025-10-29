using ServiceReports.Application.DTOs;
using ServiceCommon;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
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
            // CAMBIO CRÍTICO: Ajustar alias para que coincidan con el DTO (FullName, SalesCount, TotalSpent, AvgTicket)
            // y añadir MAX(s.sale_date) AS LastSale
            const string sql = @"
                SELECT 
                    c.id AS ClientId,
                    COALESCE(CONCAT(c.first_name, ' ', c.last_name), 'Cliente Anónimo') AS FullName,
                    c.nit AS ClientNit,
                    COUNT(s.id) AS SalesCount,
                    COALESCE(SUM(s.total_amount), 0) AS TotalSpent,
                    COALESCE(AVG(s.total_amount), 0) AS AvgTicket,
                    MAX(s.sale_date) AS LastSale -- AÑADIDO: Campo necesario para el reporte
                FROM clients c
                LEFT JOIN sales s ON s.client_id = c.id
                    AND s.sale_date BETWEEN @StartDate AND @EndDate 
                GROUP BY c.id, c.first_name, c.last_name, c.nit
                HAVING COALESCE(SUM(s.total_amount), 0) >= @MinTotal
                ORDER BY 
                    TotalSpent DESC -- SIMPLIFICADO: Se elimina la lógica dinámica y se ordena por TotalSpent
                LIMIT 100; -- Se mantiene el límite para evitar reportes gigantes
            ";

            using var conn = DatabaseConnection.Instance.GetConnection();
            await conn.OpenAsync(ct);

            var result = await conn.QueryAsync<ClientFidelityDto>(sql, new
            {
                filter.StartDate,
                filter.EndDate,
                MinTotal = filter.MinTotal ?? 0
                // IMPORTANTE: Se han eliminado los parámetros @SortBy y @SortOrder
            });

            return result;
        }
    }
}