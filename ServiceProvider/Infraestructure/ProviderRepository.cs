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

        private static object? DbNullIfNull<T>(T? value) where T : class
            => value is null ? System.DBNull.Value : (object)value;

        private Provider MapProvider(DbDataReader reader)
        {
            int idIndex = reader.GetOrdinal("id");
            int firstNameIndex = reader.GetOrdinal("first_name");
            int lastNameIndex = reader.GetOrdinal("last_name");
            int nitIndex = reader.GetOrdinal("nit");
            int addressIndex = reader.GetOrdinal("address");
            int emailIndex = reader.GetOrdinal("email");
            int phoneIndex = reader.GetOrdinal("phone");
            int isDeletedIndex = reader.GetOrdinal("is_deleted");
            int statusIndex = reader.GetOrdinal("status");

            return new Provider(
                id: reader.GetInt32(idIndex),
                first_name: reader.GetString(firstNameIndex),
                last_name: reader.GetString(lastNameIndex),
                nit: reader.IsDBNull(nitIndex) ? null : reader.GetString(nitIndex),
                address: reader.IsDBNull(addressIndex) ? null : reader.GetString(addressIndex),
                email: reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                phone: reader.IsDBNull(phoneIndex) ? null : reader.GetString(phoneIndex),
                is_deleted: reader.GetBoolean(isDeletedIndex),
                status: reader.IsDBNull(statusIndex) ? true : reader.GetBoolean(statusIndex)
            );
        }


        public async Task<Provider> Create(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                INSERT INTO providers (first_name, last_name, nit, address, email, phone, status)
                VALUES (@first_name, @last_name, @nit, @address, @email, @phone, @status);
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@nit", entity.nit ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@address", entity.address ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@email", entity.email ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", entity.phone ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@status", entity.status);

            await cmd.ExecuteNonQueryAsync();
            entity.id = (int)cmd.LastInsertedId;
            return entity;
        }

        public async Task<Provider?> GetById(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                SELECT id, first_name, last_name, nit, address, email, phone, status, is_deleted
                FROM providers
                WHERE id = @id AND is_deleted = FALSE;
            ";

            using var cmd = new MySqlCommand(query, connection);
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

            const string query = @"
                SELECT id, first_name, last_name, nit, address, email, phone, status, is_deleted
                FROM providers
                WHERE is_deleted = FALSE
                ORDER BY last_name ASC, first_name ASC;
            ";

            using var cmd = new MySqlCommand(query, connection);
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

            const string query = @"
                UPDATE providers
                SET first_name = @first_name,
                    last_name  = @last_name,
                    nit        = @nit,
                    address    = @address,
                    email      = @email,
                    phone      = @phone,
                    status     = @status,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @id;
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@nit", entity.nit ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@address", entity.address ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@email", entity.email ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", entity.phone ?? (object)System.DBNull.Value);
            cmd.Parameters.AddWithValue("@status", entity.status);
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"UPDATE providers SET is_deleted = TRUE, updated_at = CURRENT_TIMESTAMP WHERE id = @id;";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
