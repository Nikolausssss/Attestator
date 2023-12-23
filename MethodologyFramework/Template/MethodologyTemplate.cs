using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodologyFramework.Template;

public class MethodologyTemplate
{
    public string Name { get; set; }
    public List<MeasureTemplate> Measures { get; set; } = new();
    public List<ParameterTemplate> Parameters { get; set; } = new();
    public List<CalculationTemplate> Calculations { get; set; } = new();
    public List<TableTemplate> TableSpace { get; set; } = new();
}

