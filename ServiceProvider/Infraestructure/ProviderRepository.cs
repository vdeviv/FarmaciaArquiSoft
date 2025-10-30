using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceProvider.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Infraestructure
{
    public class ProviderRepository : IRepository<Provider>
    {
        private readonly DatabaseConnection _db;

        public ProviderRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        #region Helpers

        private static object DbNullIfNull(string? value)
            => value is null ? System.DBNull.Value : value;

        private Provider MapProvider(DbDataReader reader)
        {
            int idIndex = reader.GetOrdinal("id");
            int firstNameIndex = reader.GetOrdinal("first_name");
            int secondNameIndex = reader.GetOrdinal("second_name");
            int lastFirstNameIndex = reader.GetOrdinal("last_first_name");
            int lastSecondNameIndex = reader.GetOrdinal("last_second_name");
            int nitIndex = reader.GetOrdinal("nit");
            int addressIndex = reader.GetOrdinal("address");
            int emailIndex = reader.GetOrdinal("email");
            int phoneIndex = reader.GetOrdinal("phone");
            int isDeletedIndex = reader.GetOrdinal("is_deleted");
            int statusIndex = reader.GetOrdinal("status");
            int createdAtIndex = reader.GetOrdinal("created_at");
            int updatedAtIndex = reader.GetOrdinal("updated_at");
            int createdByIndex = reader.GetOrdinal("created_by");
            int updatedByIndex = reader.GetOrdinal("updated_by");

            return new Provider
            {
                id = reader.GetInt32(idIndex),
                first_name = reader.GetString(firstNameIndex),
                second_name = reader.IsDBNull(secondNameIndex) ? null : reader.GetString(secondNameIndex),
                last_first_name = reader.GetString(lastFirstNameIndex),
                last_second_name = reader.IsDBNull(lastSecondNameIndex) ? null : reader.GetString(lastSecondNameIndex),
                nit = reader.IsDBNull(nitIndex) ? null : reader.GetString(nitIndex),
                address = reader.IsDBNull(addressIndex) ? null : reader.GetString(addressIndex),
                email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                phone = reader.IsDBNull(phoneIndex) ? null : reader.GetString(phoneIndex),
                is_deleted = reader.GetBoolean(isDeletedIndex),
                status = reader.GetBoolean(statusIndex),
                created_at = reader.GetDateTime(createdAtIndex),
                updated_at = reader.IsDBNull(updatedAtIndex) ? (System.DateTime?)null : reader.GetDateTime(updatedAtIndex),
                created_by = reader.IsDBNull(createdByIndex) ? (int?)null : reader.GetInt32(createdByIndex),
                updated_by = reader.IsDBNull(updatedByIndex) ? (int?)null : reader.GetInt32(updatedByIndex),
            };
        }

        #endregion

        public async Task<Provider> Create(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO providers
(first_name, second_name, last_first_name, last_second_name, nit, address, email, phone, status, created_by)
VALUES
(@first_name, @second_name, @last_first_name, @last_second_name, @nit, @address, @email, @phone, @status, @created_by);
";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@second_name", DbNullIfNull(entity.second_name));
            cmd.Parameters.AddWithValue("@last_first_name", entity.last_first_name);
            cmd.Parameters.AddWithValue("@last_second_name", DbNullIfNull(entity.last_second_name));
            cmd.Parameters.AddWithValue("@nit", DbNullIfNull(entity.nit));
            cmd.Parameters.AddWithValue("@address", DbNullIfNull(entity.address));
            cmd.Parameters.AddWithValue("@email", DbNullIfNull(entity.email));
            cmd.Parameters.AddWithValue("@phone", DbNullIfNull(entity.phone));
            cmd.Parameters.AddWithValue("@status", entity.status);
            cmd.Parameters.AddWithValue("@created_by", entity.created_by.HasValue ? entity.created_by.Value : (object)System.DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            entity.id = (int)cmd.LastInsertedId;
            return entity;
        }

        public async Task<Provider?> GetById(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT id, first_name, second_name, last_first_name, last_second_name,
       nit, address, email, phone, status, is_deleted,
       created_at, updated_at, created_by, updated_by
FROM providers
WHERE id = @id AND is_deleted = 0;
";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapProvider(reader);

            return null;
        }

        public async Task<IEnumerable<Provider>> GetAll()
        {
            var list = new List<Provider>();

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT id, first_name, second_name, last_first_name, last_second_name,
       nit, address, email, phone, status, is_deleted,
       created_at, updated_at, created_by, updated_by
FROM providers
WHERE is_deleted = 0
ORDER BY last_first_name ASC, last_second_name ASC, first_name ASC, second_name ASC;
";

            using var cmd = new MySqlCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapProvider(reader));
            }

            return list;
        }

        public async Task Update(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE providers
SET first_name       = @first_name,
    second_name      = @second_name,
    last_first_name  = @last_first_name,
    last_second_name = @last_second_name,
    nit              = @nit,
    address          = @address,
    email            = @email,
    phone            = @phone,
    status           = @status,
    updated_by       = @updated_by,
    updated_at       = CURRENT_TIMESTAMP
WHERE id = @id AND is_deleted = 0;
";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@second_name", DbNullIfNull(entity.second_name));
            cmd.Parameters.AddWithValue("@last_first_name", entity.last_first_name);
            cmd.Parameters.AddWithValue("@last_second_name", DbNullIfNull(entity.last_second_name));
            cmd.Parameters.AddWithValue("@nit", DbNullIfNull(entity.nit));
            cmd.Parameters.AddWithValue("@address", DbNullIfNull(entity.address));
            cmd.Parameters.AddWithValue("@email", DbNullIfNull(entity.email));
            cmd.Parameters.AddWithValue("@phone", DbNullIfNull(entity.phone));
            cmd.Parameters.AddWithValue("@status", entity.status);
            cmd.Parameters.AddWithValue("@updated_by", entity.updated_by.HasValue ? entity.updated_by.Value : (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE providers
SET is_deleted = 1,
    updated_by = @updated_by,
    updated_at = CURRENT_TIMESTAMP
WHERE id = @id AND is_deleted = 0;
";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);
            cmd.Parameters.AddWithValue("@updated_by", entity.updated_by.HasValue ? entity.updated_by.Value : (object)System.DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}