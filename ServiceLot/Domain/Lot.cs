using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ServiceLot.Domain
{
    public class Lot
    {
        public int Id { get; set; }

        // 🔸 Cambiado a nullable
        public int? MedicineId { get; set; }

        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // 🔧 Constructor principal (nullable en medicineId)
        public Lot(int id, int? medicineId, string batchNumber, DateTime expirationDate,
                   int quantity, decimal unitCost, bool isDeleted)
        {
            Id = id;
            MedicineId = medicineId;
            BatchNumber = batchNumber;
            ExpirationDate = expirationDate;
            Quantity = quantity;
            UnitCost = unitCost;
            IsDeleted = isDeleted;
        }

        // 🔧 Constructor vacío para compatibilidad
        public Lot() { }
    }
}
