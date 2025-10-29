﻿using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Data;
using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ServiceUser.Infraestructure.Persistence
{
    public class UserRepository : IRepository<User>
    {
        private readonly DatabaseConnection _db;

        public UserRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<User> Create(User entity)
        {
            // + first_name, last_second_name, last_first_name
            string query = @"
INSERT INTO users 
(first_name, last_first_name, last_second_name,  username, password, mail, phone, ci, role, created_at, created_by, is_deleted) 
VALUES 
(@first_name,@last_first_name, @last_second_name, @username,@password, @mail, @phone, @ci, @role, @created_at, @created_by, @is_deleted)";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var comand = new MySqlCommand(query, connection);
            comand.Parameters.AddWithValue("@first_name", entity.first_name);
            comand.Parameters.AddWithValue("@last_first_name", entity.last_first_name);
            comand.Parameters.AddWithValue("@last_second_name", entity.last_second_name);
            comand.Parameters.AddWithValue("@username", entity.username);
            comand.Parameters.AddWithValue("@password", entity.password);
            comand.Parameters.AddWithValue("@mail", entity.mail);
            comand.Parameters.AddWithValue("@phone", entity.phone);
            comand.Parameters.AddWithValue("@ci", entity.ci);
            comand.Parameters.AddWithValue("@role", entity.role.ToString());
            comand.Parameters.AddWithValue("@created_at", entity.created_at);
            comand.Parameters.AddWithValue("@created_by", entity.created_by);
            comand.Parameters.AddWithValue("@is_deleted", entity.is_deleted);

            await comand.ExecuteNonQueryAsync();
            entity.id = (int)comand.LastInsertedId;
            return entity;
        }

        public async Task<User?> GetById(User entity)
        {
            string query = "SELECT * FROM users WHERE id = @id AND is_deleted = FALSE";
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var comand = new MySqlCommand(query, connection);
            comand.Parameters.AddWithValue("@id", entity.id);

            using var reader = await comand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var u = new User
                {
                    id = reader.GetInt32("id"),

                    first_name = reader.GetString("first_name"),
                    last_first_name = reader.GetString("last_first_name"),
                    last_second_name = reader.GetString("last_second_name"),

                    username = reader.GetString("username"),
                    password = reader.GetString("password"),
                    mail = reader.GetString("mail"),
                    phone = reader.GetInt32("phone"),
                    ci = reader.GetString("ci"),
                    role = Enum.Parse<UserRole>(reader.GetString("role")),

                    is_deleted = reader.GetBoolean("is_deleted"),
                    created_by = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32("created_by"),
                    updated_by = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetInt32("updated_by"),
                    created_at = reader.GetDateTime("created_at"),
                    updated_at = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at")
                };

                return u;
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var lista = new List<User>();
            string query = "SELECT * FROM users WHERE is_deleted = FALSE ORDER BY last_first_name ASC, first_name ASC;";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var comand = new MySqlCommand(query, connection);
            using var reader = await comand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new User
                {
                    id = reader.GetInt32("id"),

                    first_name = reader.GetString("first_name"),
                    last_first_name = reader.GetString("last_first_name"),
                    last_second_name = reader.GetString("last_second_name"),
                    

                    username = reader.GetString("username"),
                    password = reader.GetString("password"),
                    mail = reader.GetString("mail"),
                    phone = reader.GetInt32("phone"),
                    ci = reader.GetString("ci"),
                    role = Enum.Parse<UserRole>(reader.GetString("role")),

                    is_deleted = reader.GetBoolean("is_deleted"),
                    created_by = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32("created_by"),
                    updated_by = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetInt32("updated_by"),
                    created_at = reader.GetDateTime("created_at"),
                    updated_at = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at")
                });
            }
            return lista;
        }

        public async Task Update(User entity)
        {
            string query = @"
UPDATE users 
SET first_name=@first_name, 
last_first_name=@last_first_name,
    last_second_name=@last_second_name,  
    username=@username, 
    password=@password, 
    mail=@mail, 
    phone=@phone, 
    ci=@ci, 
    role=@role, 
    updated_at=@updated_at, 
    updated_by=@updated_by 
WHERE id=@id";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var comand = new MySqlCommand(query, connection);
            comand.Parameters.AddWithValue("@first_name", entity.first_name);
            comand.Parameters.AddWithValue("@last_first_name", entity.last_first_name);
            comand.Parameters.AddWithValue("@last_second_name", entity.last_second_name);
            

            comand.Parameters.AddWithValue("@username", entity.username);
            comand.Parameters.AddWithValue("@password", entity.password);
            comand.Parameters.AddWithValue("@mail", (object?)entity.mail);
            comand.Parameters.AddWithValue("@phone", entity.phone);
            comand.Parameters.AddWithValue("@ci", entity.ci);
            comand.Parameters.AddWithValue("@role", entity.role.ToString());
            comand.Parameters.AddWithValue("@updated_at", DateTime.Now);
            comand.Parameters.AddWithValue("@updated_by", (object?)entity.updated_by ?? DBNull.Value);
            comand.Parameters.AddWithValue("@id", entity.id);

            await comand.ExecuteNonQueryAsync();
        }

        public async Task Delete(User entity)
        {
            const string sql = @"
UPDATE users
SET is_deleted = TRUE,
    updated_at = @updated_at,
    updated_by = @updated_by
WHERE id = @id;";

            using var con = _db.GetConnection();
            await con.OpenAsync();

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", entity.id);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.Now);
            cmd.Parameters.AddWithValue("@updated_by", (object?)entity.updated_by ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }


    }
}
