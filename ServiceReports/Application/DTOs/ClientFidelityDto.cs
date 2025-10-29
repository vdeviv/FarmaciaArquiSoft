using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReports.Application.DTOs
{
    public class ClientFidelityDto // Este DTO es el que recibe los datos de Dapper
    {
        public int ClientId { get; set; }
        public string FullName { get; set; } // Coincide con el alias 'FullName' en SQL
        public string? ClientNit { get; set; }
        public int SalesCount { get; set; } // Coincide con el alias 'SalesCount' en SQL
        public decimal TotalSpent { get; set; } // Coincide con el alias 'TotalSpent' en SQL
        public decimal AvgTicket { get; set; } // Coincide con el alias 'AvgTicket' en SQL
        public DateTime? LastSale { get; set; } // Coincide con el alias 'LastSale' en SQL
    }
}