﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ServiceCommon.Infrastructure.Data
{
    public sealed class DatabaseConnection
    {
        private static DatabaseConnection? _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        private DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void Initialize(IConfiguration configuration)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    var connStr = configuration.GetConnectionString("DefaultConnection")!;
                    _instance = new DatabaseConnection(connStr);
                }
            }
        }

        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("DatabaseConnection no fue inicializado. Llama a Initialize() en Program.cs");

                return _instance;
            }
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
