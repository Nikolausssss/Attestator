using Newtonsoft.Json;
using MethodologyFramework;

namespace Attestator.Services;

public class Experiment
{
    public string Name { get; set; }
    public string FilePath { get; set; }
    public string SourceProviderName { get; set; }

    [JsonIgnore]
    public MethodologyContext Context { get; internal set; }
}

