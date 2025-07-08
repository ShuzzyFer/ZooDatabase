using Npgsql;
using System.Collections.Generic;
using System.Data;
using Zoo.Infrastructure;

namespace Zoo.DataAccess
{
    public class PostgresRepository : IRepository
    {
        private readonly string _connectionString = ConfigManager.ConnectionString;

        public NpgsqlConnection GetNewConnection() => new NpgsqlConnection(_connectionString);

        public List<string> GetTableNames()
        {
            var tables = new List<string>();
            using (var conn = GetNewConnection())
            {
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader["table_name"].ToString());
                    }
                }
            }
            return tables;
        }

        public DataTable GetTableData(string tableName)
        {
            var dataTable = new DataTable();
            using (var conn = GetNewConnection())
            {
                conn.Open();
                string query = $"SELECT * FROM public.\"{tableName}\"";
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }

        public void UpdateTable(string tableName, DataTable dataTable)
        {
             using (var conn = GetNewConnection())
            {
                conn.Open();
                string query = $"SELECT * FROM public.\"{tableName}\"";
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    using (var builder = new NpgsqlCommandBuilder(adapter))
                    {
                        adapter.Update(dataTable);
                    }
                }
            }
        }

        public DataTable ExecuteQuery(string sql)
        {
            var dataTable = new DataTable();
            using (var conn = GetNewConnection())
            {
                conn.Open();
                using (var adapter = new NpgsqlDataAdapter(sql, conn))
                {
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }
        
        public void ExecuteNonQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetNewConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetColumnNames(string tableName)
        {
            var columns = new List<string>();
            var sql = "SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = @tableName;";
            using (var conn = GetNewConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("tableName", tableName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return columns;
        }
        
        public string GetPrimaryKeyColumnName(string tableName)
        {
            string pkName = null;
            string sql = @"
                SELECT kcu.column_name
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
                WHERE tc.table_schema = 'public' AND tc.table_name = @tableName AND tc.constraint_type = 'PRIMARY KEY';";
            
            using (var conn = GetNewConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("tableName", tableName);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        pkName = result.ToString();
                    }
                }
            }
            return pkName;
        }

        public bool TableExists(string tableName)
        {
            var sql = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @tableName);";
             using (var conn = GetNewConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("tableName", tableName);
                    return (bool)cmd.ExecuteScalar();
                }
            }
        }
    }
}