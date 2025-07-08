using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;
using Zoo.DataAccess;
using Zoo.Models;

namespace Zoo.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IRepository _repository;

        public DatabaseService(IRepository repository)
        {
            _repository = repository;
        }

        private void ValidateIdentifier(string name, string entityType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Имя {entityType} не может быть пустым.");
            if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я0-9_]+$"))
                throw new ArgumentException($"Имя {entityType} '{name}' содержит недопустимые символы.");
            if (Encoding.UTF8.GetByteCount(name) > 63)
                throw new ArgumentException($"Имя {entityType} '{name}' слишком длинное (макс. 63 байта).");
        }

        public List<string> GetTables() => _repository.GetTableNames();
        public DataTable LoadTableData(string tableName) => _repository.GetTableData(tableName);

        public void CreateTable(string tableName, List<ColumnDefinition> columns)
        {
            ValidateIdentifier(tableName, "таблицы");
            if (_repository.TableExists(tableName))
                throw new InvalidOperationException($"Таблица '{tableName}' уже существует.");
            if (columns == null || columns.Count == 0)
                throw new ArgumentException("Таблица должна содержать хотя бы один столбец.");

            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var col in columns)
            {
                ValidateIdentifier(col.Name, "столбца");
                if (col.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Столбец 'id' создается автоматически.");
                if (!columnNames.Add(col.Name))
                    throw new ArgumentException("Имена столбцов должны быть уникальными.");
            }

            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE public.\"{tableName}\" (");
            sb.Append("\"id\" SERIAL PRIMARY KEY");
            foreach (var col in columns)
            {
                sb.Append($", \"{col.Name}\" {col.DataType}");
            }
            sb.Append(");");

            _repository.ExecuteNonQuery(sb.ToString());
        }

        public void DeleteTable(string tableName)
        {
            ValidateIdentifier(tableName, "таблицы");
            _repository.ExecuteNonQuery($"DROP TABLE public.\"{tableName}\" CASCADE");
        }
        
        public void AddColumn(string tableName, ColumnDefinition column)
        {
            ValidateIdentifier(tableName, "таблицы");
            ValidateIdentifier(column.Name, "столбца");

            var existingColumns = _repository.GetColumnNames(tableName);
            if (existingColumns.Contains(column.Name, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Столбец '{column.Name}' уже существует в таблице '{tableName}'.");

            string sql = $"ALTER TABLE public.\"{tableName}\" ADD COLUMN \"{column.Name}\" {column.DataType};";
            _repository.ExecuteNonQuery(sql);
        }

        public void DeleteColumn(string tableName, string columnName)
        {
            ValidateIdentifier(tableName, "таблицы");
            ValidateIdentifier(columnName, "столбца");
            if (columnName.Equals("id", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Нельзя удалить столбец 'id'.");

            string sql = $"ALTER TABLE public.\"{tableName}\" DROP COLUMN \"{columnName}\";";
            _repository.ExecuteNonQuery(sql);
        }
        
        public List<string> GetColumnNamesForDelete(string tableName)
        {
            return _repository.GetColumnNames(tableName)
                .Where(c => !c.Equals("id", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void RenameColumn(string tableName, string oldName, string newName)
        {
            ValidateIdentifier(tableName, "таблицы");
            ValidateIdentifier(oldName, "старого имени столбца");
            ValidateIdentifier(newName, "нового имени столбца");
            if (oldName.Equals("id", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Нельзя переименовать столбец 'id'.");

            var existingColumns = _repository.GetColumnNames(tableName);
            if (existingColumns.Contains(newName, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Столбец с именем '{newName}' уже существует.");

            string sql = $"ALTER TABLE public.\"{tableName}\" RENAME COLUMN \"{oldName}\" TO \"{newName}\";";
            _repository.ExecuteNonQuery(sql);
        }
        
        public void AddRow(string tableName, Dictionary<string, object> values)
        {
            var columns = values.Keys.Select(c => $"\"{c}\"").ToList();
            var valuePlaceholders = values.Keys.Select((c, i) => $"@p{i}").ToList();

            var sql = $"INSERT INTO public.\"{tableName}\" ({string.Join(", ", columns)}) VALUES ({string.Join(", ", valuePlaceholders)});";
            
            var parameters = new NpgsqlParameter[values.Count];
            int i = 0;
            foreach(var val in values.Values)
            {
                parameters[i] = new NpgsqlParameter($"p{i}", val ?? DBNull.Value);
                i++;
            }
            
            _repository.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteRow(string tableName, DataRow row)
        {
            string pkColumn = _repository.GetPrimaryKeyColumnName(tableName) ?? "id";

            if (row.Table.Columns.Contains(pkColumn) && row[pkColumn] != DBNull.Value)
            {
                var sql = $"DELETE FROM public.\"{tableName}\" WHERE \"{pkColumn}\" = @pkValue";
                var parameters = new[] { new NpgsqlParameter("pkValue", row[pkColumn]) };
                _repository.ExecuteNonQuery(sql, parameters);
            }
            else
            {
                // Fallback for tables without PK or if PK is null
                var conditions = new List<string>();
                var parameters = new List<NpgsqlParameter>();
                int paramIndex = 0;

                foreach (DataColumn column in row.Table.Columns)
                {
                    string paramName = $"@param{paramIndex++}";
                    conditions.Add($"\"{column.ColumnName}\" = {paramName}");
                    parameters.Add(new NpgsqlParameter(paramName, row[column] ?? DBNull.Value));
                }

                if (conditions.Count == 0) throw new InvalidOperationException("Невозможно удалить пустую строку.");
                
                var sql = $"DELETE FROM public.\"{tableName}\" WHERE {string.Join(" AND ", conditions)}";
                _repository.ExecuteNonQuery(sql, parameters.ToArray());
            }
        }

        public void SaveChanges(string tableName, DataTable dataTable)
        {
            _repository.UpdateTable(tableName, dataTable);
        }
        
        public DataTable ExecuteCustomQuery(string sql) => _repository.ExecuteQuery(sql);
    }
}