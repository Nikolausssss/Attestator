using MethodologyFramework.Template;

using System;
using System.Data;

namespace MethodologyFramework.Data;

public class Parameter
{
    internal Parameter(ParameterTemplate parameterTemplate, DataTable source)
    {
        Title = parameterTemplate.Title;
        Name = parameterTemplate.Name;
        Type = parameterTemplate.Type;
        DefaultValue = parameterTemplate.DefaultValue;
        ChoiseItems = parameterTemplate.ChoiseItems?.ToArray();
        Source = source;
    }

    public string Title { get; }
    public string Name { get; }
    public object Value
    {
        get => Source.Rows[0][1];
        set => Source.Rows[0][1] = value;
    }

    public object DefaultValue { get; }
    public object[]? ChoiseItems { get; }
    public DataType Type { get; }
    internal DataTable Source { get; }
}
