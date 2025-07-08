namespace Zoo.Models
{
    public class QueryDefinition
    {
        public string Name { get; set; }
        public string SqlText { get; set; }

        public QueryDefinition(string name, string sqlText)
        {
            Name = name;
            SqlText = sqlText;
        }
    }
}