using System.Collections.Generic;

using MethodologyFramework.Template;

namespace Attestator.Services;

public class Research
{
    public string Name { get; set; }

    public MethodologyTemplate Template { get; set; }

    public List<Experiment> Experiments { get; set; } = new();
}

