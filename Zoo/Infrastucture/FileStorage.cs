using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zoo.Models;

namespace Zoo.Infrastructure
{
    // Этот класс отвечает только за чтение/запись файлов
    public class FileStorage
    {
        private readonly string _filePath = ConfigManager.SavedQueriesFilePath;
        private const string Separator = "|||";

        public List<QueryDefinition> LoadQueries()
        {
            var queries = new List<QueryDefinition>();
            if (!File.Exists(_filePath))
            {
                return queries;
            }

            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { Separator }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    queries.Add(new QueryDefinition(parts[0], parts[1]));
                }
            }
            return queries;
        }

        public void SaveQueries(IEnumerable<QueryDefinition> queries)
        {
            var lines = queries.Select(q => $"{q.Name}{Separator}{q.SqlText}");
            File.WriteAllLines(_filePath, lines);
        }
    }
}