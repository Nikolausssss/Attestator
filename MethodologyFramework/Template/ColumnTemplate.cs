using MethodologyFramework.Data;

namespace MethodologyFramework.Template;

public class ColumnTemplate
{
    public string Name { get; set; }
    public string Title { get; set; }
    public DataType DataType { get; set; }

    public object GetDefaultCellValue()
    {
        return DataType switch
        {
            DataType.Number => 0,
            DataType.Float => 0,
            DataType.String => string.Empty,
            DataType.Check => false,
            _ => new object()
        };
    }
}

