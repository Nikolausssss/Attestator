using MethodologyFramework.Template;

using System.Collections.Generic;

namespace MethodologyFramework.Data;

public class Measure : Table
{
    public bool CanAddRows { get; }

    public Measure(MeasureTemplate template, Action<Table> updator) : base(template, updator)
    {
        CanAddRows = template.CanAddRows;
    }
}
