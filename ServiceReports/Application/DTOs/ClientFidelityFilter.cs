using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReports.Application.DTOs
{
    public class ClientFidelityFilter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MinTotal { get; set; } = 0;
        public string SortBy { get; set; } = "FullName";
        public string SortOrder { get; set; } = "ASC";

        /// <summary>
        /// Número de registros a retornar (Top N)
        /// Si es null, retorna todos los registros (hasta 100)
        /// </summary>
        public int? TopN { get; set; } = null;

        /// <summary>
        /// Indica si el filtro está configurado para Top N
        /// </summary>
        public bool IsTopNFilter => TopN.HasValue && TopN.Value > 0;
    }
}