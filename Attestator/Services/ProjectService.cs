using Newtonsoft.Json;
using System.IO;
using MethodologyFramework.Data;
using MethodologyFramework;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Concurrent;

namespace Attestator.Services;


public class ProjectService
{
    public static void RegisterSourceProvider(string name, Func<string, ISourceProvider> factory)
    {
        _sourceProviderSelector.AddOrUpdate(name, key => factory, (k, v) => factory);
    }

    public static ISourceProvider? ResolveSourceProvider(string name, string path)
    {
        return _sourceProviderSelector.TryGetValue(name, out var factory) ? factory.Invoke(path) : null;
    }

    public static ISourceProvider GetDefaultProvider(string path)
    {
        return _sourceProviderSelector.Values.First().Invoke(path);
    }

    private static readonly ConcurrentDictionary<string, Func<string, ISourceProvider>> _sourceProviderSelector = new();


    public ProjectService()
    {
    }


    public Project OpenProject(FileStream stream)
    {
        Project? project = null;
        var serializer = new JsonSerializer();

        using (var sw = new StreamReader(stream))
        using (var jsonTextWriter = new JsonTextReader(sw))
        {
            project = serializer.Deserialize<Project>(jsonTextWriter);
        }

        var projectDirectory = Path.GetDirectoryName(stream.Name);
        
        foreach (var item in project.Researches)
        {
            foreach (var contextInfo in item.Experiments)
            {
                contextInfo.Context = new MethodologyContext(item.Template,
                                                             ResolveSourceProvider(contextInfo.SourceProviderName,
                                                                                   $"{projectDirectory}\\{contextInfo.FilePath}"));
            }
        }

        return project;
    }

    public Project CreateProject(FileStream stream)
    {
        Project project = new()
        {
            Name = Path.GetFileNameWithoutExtension(stream.Name),
        };

        JsonSerializer serializer = new();

        using (var sw = new StreamWriter(stream))
        using (var jsonTextWriter = new JsonTextWriter(sw))
        {
            serializer.Serialize(jsonTextWriter, project);
        }

        return project;
    }

    public void SaveProject(Project project, FileStream stream)
    {
        JsonSerializer serializer = new();
        using (var sw = new StreamWriter(stream))
        using (var jsonTextWriter = new JsonTextWriter(sw))
        {
            serializer.Serialize(jsonTextWriter, project);
        }
    }
}

