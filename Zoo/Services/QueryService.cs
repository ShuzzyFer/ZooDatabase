using System;
using System.Collections.Generic;
using System.Linq;
using Zoo.Infrastructure;
using Zoo.Models;

namespace Zoo.Services
{
    public class QueryService : IQueryService
    {
        private readonly FileStorage _fileStorage;
        private List<QueryDefinition> _queries;

        public QueryService(FileStorage fileStorage)
        {
            _fileStorage = fileStorage;
            _queries = new List<QueryDefinition>();
        }

        public List<QueryDefinition> GetQueries()
        {
            if (_queries.Count == 0)
            {
                try
                {
                    _queries = _fileStorage.LoadQueries();
                }
                catch (Exception)
                {
                    // Log error if needed
                    _queries = new List<QueryDefinition>();
                }
                AddPredefinedQueries();
            }
            return _queries;
        }

        public void SaveQuery(QueryDefinition query)
        {
            if (_queries.Exists(q => q.Name.Equals(query.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Запрос с таким именем уже существует.");
            }
            _queries.Add(query);
            _fileStorage.SaveQueries(_queries);
        }

        public void DeleteQuery(string queryName)
        {
            var queryToRemove = _queries.FirstOrDefault(q => q.Name.Equals(queryName, StringComparison.OrdinalIgnoreCase));
            if (queryToRemove != null)
            {
                _queries.Remove(queryToRemove);
                _fileStorage.SaveQueries(_queries);
            }
        }

        private void AddPredefinedQueries()
        {
            var predefined = new List<QueryDefinition>
            {
                new QueryDefinition("Животные старше 5 лет", "SELECT \"имя\", \"вид\", \"возраст\" FROM public.\"Животное\" WHERE \"возраст\" > 5;"),
                new QueryDefinition("Смотрители со стажем > 3", "SELECT \"имя\", \"должность\", \"стаж\" FROM public.\"Сотрудник\" WHERE \"должность\" = 'Смотритель' AND \"стаж\" > 3;"),
                // ... добавьте остальные ваши предопределенные запросы
            };

            foreach (var pq in predefined)
            {
                if (!_queries.Exists(q => q.Name.Equals(pq.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _queries.Add(pq);
                }
            }
        }
    }
}