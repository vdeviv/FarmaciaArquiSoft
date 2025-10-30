using MySql.Data.MySqlClient;
using ServiceLot.Domain;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLot.Infrastructure
{
    public class LotRepository : IRepository<Lot>
    {
        private readonly DatabaseConnection _db;

        public LotRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<Lot> Create(Lot entity)
        {
            string query = @"INSERT INTO lots 
                            (medicine_id, batch_number, expiration_date, quantity, unit_cost, created_at, created_by)
                            VALUES (@medicine_id, @batch_number, @expiration_date, @quantity, @unit_cost, @created_at, @created_by)";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);

            // ✅ convertir a DBNull si no hay medicine_id (se mantiene tu lógica de parámetros)
            if (entity.MedicineId <= 0)
                cmd.Parameters.AddWithValue("@medicine_id", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@medicine_id", entity.MedicineId);

            cmd.Parameters.AddWithValue("@batch_number", entity.BatchNumber);
            cmd.Parameters.AddWithValue("@expiration_date", entity.ExpirationDate);
            cmd.Parameters.AddWithValue("@quantity", entity.Quantity);
            cmd.Parameters.AddWithValue("@unit_cost", entity.UnitCost);
            cmd.Parameters.AddWithValue("@created_at", entity.CreatedAt);
            cmd.Parameters.AddWithValue("@created_by", entity.CreatedBy);

            await cmd.ExecuteNonQueryAsync();
            entity.Id = (int)cmd.LastInsertedId;
            return entity;
        }

        public async Task<Lot?> GetById(Lot entity)
        {
            string query = "SELECT * FROM lots WHERE id=@id AND is_deleted = FALSE";
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.Id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // ✅ manejar nulos en medicine_id
                int? medId = reader["medicine_id"] == DBNull.Value ? (int?)null : reader.GetInt32("medicine_id");

                return new Lot(
                    reader.GetInt32("id"),
                    // 🎯 CORRECCIÓN: Usar '?? 0' para convertir 'int?' (medId) a 'int' para el constructor.
                    medId ?? 0,
                    reader.GetString("batch_number"),
                    reader.GetDateTime("expiration_date"),
                    reader.GetInt32("quantity"),
                    reader.GetDecimal("unit_cost"),
                    reader.GetBoolean("is_deleted")
                )
                {
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader["updated_at"] as DateTime?,
                    CreatedBy = reader["created_by"] as int?,
                    UpdatedBy = reader["updated_by"] as int?
                };
            }
            return null;
        }

        public async Task<IEnumerable<Lot>> GetAll()
        {
            var list = new List<Lot>();
            string query = "SELECT * FROM lots WHERE is_deleted = FALSE ORDER BY batch_number ASC;";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int? medId = reader["medicine_id"] == DBNull.Value ? (int?)null : reader.GetInt32("medicine_id");

                list.Add(new Lot(
                    reader.GetInt32("id"),
                    // 🎯 CORRECCIÓN: Usar '?? 0' para convertir 'int?' (medId) a 'int' para el constructor.
                    medId ?? 0,
                    reader.GetString("batch_number"),
                    reader.GetDateTime("expiration_date"),
                    reader.GetInt32("quantity"),
                    reader.GetDecimal("unit_cost"),
                    reader.GetBoolean("is_deleted")
                )
                {
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader["updated_at"] as DateTime?,
                    CreatedBy = reader["created_by"] as int?,
                    UpdatedBy = reader["updated_by"] as int?
                });
            }
            return list;
        }

        public async Task Update(Lot entity)
        {
            string query = @"UPDATE lots 
                            SET medicine_id=@medicine_id,
                                batch_number=@batch_number,
                                expiration_date=@expiration_date,
                                quantity=@quantity,
                                unit_cost=@unit_cost,
                                updated_at=@updated_at,
                                updated_by=@updated_by
                            WHERE id=@id";
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);

            if (entity.MedicineId <= 0)
                cmd.Parameters.AddWithValue("@medicine_id", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@medicine_id", entity.MedicineId);

            cmd.Parameters.AddWithValue("@batch_number", entity.BatchNumber);
            cmd.Parameters.AddWithValue("@expiration_date", entity.ExpirationDate);
            cmd.Parameters.AddWithValue("@quantity", entity.Quantity);
            cmd.Parameters.AddWithValue("@unit_cost", entity.UnitCost);
            cmd.Parameters.AddWithValue("@updated_at", entity.UpdatedAt);
            cmd.Parameters.AddWithValue("@updated_by", entity.UpdatedBy);
            cmd.Parameters.AddWithValue("@id", entity.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Lot entity)
        {
            string query = @"UPDATE lots 
                            SET is_deleted = TRUE,
                                updated_at = @updated_at,
                                updated_by = @updated_by
                            WHERE id=@id";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@updated_at", entity.UpdatedAt);
            cmd.Parameters.AddWithValue("@updated_by", entity.UpdatedBy);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}