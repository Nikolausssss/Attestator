using System.Collections.Generic;

namespace MethodologyFramework.Template;

public class TableTemplate
{
    public string Title { get; set; }
    public string Name { get; set; }
    public List<ColumnTemplate> Columns { get; set; } = new();
    public List<object[]> Rows { get; set; } = new();
}

