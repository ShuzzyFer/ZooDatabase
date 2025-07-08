using System.Collections.Generic;
using System.Data;

namespace Zoo.DataAccess
{
    public interface IRepository
    {
        List<string> GetTableNames();
        DataTable GetTableData(string tableName);
        DataTable ExecuteQuery(string sql);
        void ExecuteNonQuery(string sql, Npgsql.NpgsqlParameter[] parameters = null);
        void UpdateTable(string tableName, DataTable dataTable);
        List<string> GetColumnNames(string tableName);
        string GetPrimaryKeyColumnName(string tableName);
        bool TableExists(string tableName);
        Npgsql.NpgsqlConnection GetNewConnection();
    }
}