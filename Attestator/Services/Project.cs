using System.Collections.Generic;

namespace Attestator.Services;

public class Project
{
    public string Name { get; set; }
    public List<Research> Researches { get; set; } = new();
}

