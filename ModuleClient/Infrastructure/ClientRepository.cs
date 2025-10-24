using MySql.Data.MySqlClient;
using ServiceClient.Domain;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Infrastructure
{
    public class ClientRepository : IRepository<Client>
    {
        private readonly DatabaseConnection _db;

        public ClientRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        /// <summary>
        /// Maps a record from the DbDataReader to a Client object.
        /// This method correctly uses the generic DbDataReader, preventing the conversion error.
        /// </summary>
        /// <param name="reader">The DbDataReader instance.</param>
        /// <returns>A new Client object.</returns>
        private Client MapClient(DbDataReader reader)
        {
            // Use reader.GetOrdinal to get column index or just the name (if supported by provider)
            // and use the type-specific getter. This works for any DbDataReader implementation.
            return new Client(
                id: reader.GetInt32("id"),
                first_name: reader.GetString("first_name"),
                last_name: reader.GetString("last_name"),
                // Handle potential NULL for nit before calling GetString
                nit: reader.IsDBNull("nit") ? string.Empty : reader.GetString("nit"),
                email: reader.GetString("email"),
                is_deleted: reader.GetBoolean("is_deleted")
            );
        }

        public async Task<Client> Create(Client entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                INSERT INTO clients (first_name, last_name, nit, email)
                VALUES (@first_name, @last_name, @nit, @email);
            ";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@nit", entity.nit);
            cmd.Parameters.AddWithValue("@email", entity.email);

            await cmd.ExecuteNonQueryAsync();
            entity.id = (int)cmd.LastInsertedId;
            return entity;
        }

        public async Task<Client?> GetById(Client entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = "SELECT id, first_name, last_name, nit, email, is_deleted FROM clients WHERE id = @id AND is_deleted = FALSE;";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);

            // ExecuteReaderAsync returns DbDataReader. No need to cast.
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapClient(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Client>> GetAll()
        {
            var list = new List<Client>();
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = "SELECT id, first_name, last_name, nit, email, is_deleted FROM clients WHERE is_deleted = FALSE ORDER BY last_name ASC, first_name ASC;";

            using var cmd = new MySqlCommand(query, connection);


            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapClient(reader));
            }

            return list;
        }

        public async Task Update(Client entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                UPDATE clients 
                SET first_name = @first_name,
                    last_name  = @last_name,
                    nit        = @nit,
                    email      = @email
                WHERE id = @id;
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@nit", entity.nit);
            cmd.Parameters.AddWithValue("@email", entity.email);
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Client entity)
        {
            // Note: This is a soft delete based on the column name `is_deleted`
            string query = "UPDATE clients SET is_deleted = TRUE WHERE id=@id";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
