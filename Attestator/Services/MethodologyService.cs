using Newtonsoft.Json;

using System;
using System.IO;
using MethodologyFramework.Template;
using MethodologyFramework;

namespace Attestator.Services;

public class MethodologyService
{
    public MethodologyTemplate OpenMethodologyTymplate(FileStream fileStream)
    {
        MethodologyTemplate? template;
        var serializer = new JsonSerializer();

        using (var sw = new StreamReader(fileStream))
        using (var jsonTextReader = new JsonTextReader(sw))
        {
            template = serializer.Deserialize<MethodologyTemplate>(jsonTextReader);
        }
        return template ?? throw new InvalidOperationException("Не удалось открыть файл методики");
    }

    public void SaveMethodologyTemplate(MethodologyTemplate methodology, FileStream fileStream)
    {
        var serializer = new JsonSerializer();

        using (var sw = new StreamWriter(fileStream))
        using (var jsonTextWriter = new JsonTextWriter(sw))
        {
            serializer.Serialize(jsonTextWriter, methodology);
        }
    }

    public MethodologyContext CreateMethodologyContext(MethodologyTemplate template, ISourceProvider sourceProvider)
    {
        return new MethodologyContext(template, sourceProvider);
    }
}
