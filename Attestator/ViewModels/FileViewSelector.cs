using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Attestator.ViewModels;

public class FileViewSelector : IDataTemplate
{
    public FileViewSelector()
    {
    }

    public IDataTemplate EmptyFileDataTemplate { get; set; }
    public IDataTemplate ProjectDataTemplate { get; set; }
    public IDataTemplate MethodologyTemplateDataTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is ProjectViewModel)
        {
            return ProjectDataTemplate.Build(param);
        }
        if (param is MethodologyTemplateViewModel)
        {
            return MethodologyTemplateDataTemplate.Build(param);
        }

        return EmptyFileDataTemplate.Build(param);
    }

    public bool Match(object? data)
    {
        return data is IFileViewModel;
    }
}
