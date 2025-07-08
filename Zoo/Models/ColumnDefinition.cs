namespace Zoo.Models
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string DataType { get; set; }

        public ColumnDefinition(string name, string dataType)
        {
            Name = name;
            DataType = dataType;
        }
    }
}