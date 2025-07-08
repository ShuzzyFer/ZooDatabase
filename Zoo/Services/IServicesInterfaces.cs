using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Zoo.Models;

namespace Zoo.Services
{
    public interface IDatabaseService
    {
        List<string> GetTables();
        DataTable LoadTableData(string tableName);
        void CreateTable(string tableName, List<ColumnDefinition> columns);
        void DeleteTable(string tableName);
        void AddColumn(string tableName, ColumnDefinition column);
        void DeleteColumn(string tableName, string columnName);
        void RenameColumn(string tableName, string oldName, string newName);
        void SaveChanges(string tableName, DataTable dataTable);
        DataTable ExecuteCustomQuery(string sql);
        void AddRow(string tableName, Dictionary<string, object> values);
        void DeleteRow(string tableName, DataRow row);
        List<string> GetColumnNamesForDelete(string tableName);
    }
    
    public interface IBackupService
    {
        Task BackupDatabaseAsync(string filePath);
        Task RestoreDatabaseAsync(string filePath);
    }

    public interface IQueryService
    {
        List<QueryDefinition> GetQueries();
        void SaveQuery(QueryDefinition query);
        void DeleteQuery(string queryName);
    }
}