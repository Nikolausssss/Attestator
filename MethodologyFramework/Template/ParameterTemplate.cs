using MethodologyFramework.Data;

using System.Collections.Generic;

namespace MethodologyFramework.Template;

public class ParameterTemplate
{
    public string Title { get; set; }
    public string Name { get; set; }
    public object DefaultValue { get; set; }
    public DataType Type { get; set; }
    public List<object>? ChoiseItems { get; set; }
}

