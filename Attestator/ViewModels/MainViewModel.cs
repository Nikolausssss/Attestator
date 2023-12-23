using Attestator.Services;

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;

using DynamicData;

using MethodologyFramework.Data;
using MethodologyFramework.Template;

using ReactiveUI;

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class TypedDataTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();

    public IDataTemplate DefaultTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param == null) return DefaultTemplate.Build(param);
        string? key = param.GetType().Name;
        return string.IsNullOrEmpty(key) ? null : AvailableTemplates.TryGetValue(key, out var template) ? template.Build(param) : DefaultTemplate?.Build(param);
    }

    public bool Match(object? data)
    {
        if (data is null) return true;
        string? key = data.GetType().Name;
        return !string.IsNullOrEmpty(key) && AvailableTemplates.ContainsKey(key) || DefaultTemplate != null;
    }
}

public class ParameterDataTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();
    public IDataTemplate DefaultTemplate { get; set; }
    public IDataTemplate ChoiseTemplate { get; set; }

    public Control? Build(object? param)
    {
        string? key = (param as ParameterTemplateViewModel)?.Type.ToString()
                      ?? (param as Parameter)?.Type.ToString()
                      ?? (param as ParameterTemplate)?.Type.ToString()
                      ?? param?.ToString();
        int? choiseCount = (param as ParameterTemplateViewModel)?.ChoiseItems?.Count
                           ?? (param as Parameter)?.ChoiseItems?.Length
                           ?? (param as ParameterTemplate)?.ChoiseItems?.Count;

        return choiseCount != null && choiseCount > 0 
               ? ChoiseTemplate.Build(param)
               : string.IsNullOrEmpty(key)
                 ? null 
                 : AvailableTemplates.TryGetValue(key, out var template) 
                   ? template.Build(param) 
                   : DefaultTemplate.Build(param);
    }

    public bool Match(object? param)
    {
        string? key = (param as ParameterTemplateViewModel)?.Type.ToString()
                       ?? (param as Parameter)?.Type.ToString()
                       ?? (param as ParameterTemplate)?.Type.ToString()
                       ?? param?.ToString();
        int? choiseCount = (param as ParameterTemplateViewModel)?.ChoiseItems?.Count
                           ?? (param as Parameter)?.ChoiseItems?.Length
                           ?? (param as ParameterTemplate)?.ChoiseItems?.Count;

        return (choiseCount != null && choiseCount > 0 && ChoiseTemplate != null)
               || (!string.IsNullOrEmpty(key) && AvailableTemplates.ContainsKey(key)) 
               || DefaultTemplate != null;
    }
}


public class FileDialogOptions
{
    public FileDialogOptions(string defaultFileName, string defaultFileExtension)
    {
        DefaultFileName = defaultFileName;
        DefaultFileExtension = defaultFileExtension;
    }

    public string DefaultFileName { get; }
    public string DefaultFileExtension { get; }
}

public class MainViewModel : ViewModelBase
{
    private readonly ProjectService _projectService;
    private readonly MethodologyService _methodologyService;
    private ReactiveCommand<Unit, Task> _saveProjectCommand;
    private ReactiveCommand<Unit, Task> _createProjectCommand;
    private ReactiveCommand<Unit, Unit> _openHelpCommand;
    private ReactiveCommand<Unit, Task> _openProjectCommand;
    
    private IFileViewModel _currentFile = null;
    private EmptyFileViewModel _emptyFileViewModel;
    private ReactiveCommand<Unit, Task> _openMethodologyTemplateCommand;
    private ReactiveCommand<Unit, Task> _createMethodologyTemplateCommand;

    public MainViewModel(ProjectService projectService, MethodologyService methodologyService)
    {
        _projectService = projectService;
        _methodologyService = methodologyService;
        CurrentFile = _emptyFileViewModel = new EmptyFileViewModel(OpenProjectCommand, CreateProjectCommand, OpenMethodologyTemplateCommand, CreateMethodologyTemplateCommand);
    }

    public Interaction<CreateProjectWindowViewModel, IStorageFile?> ShowCreateProjectDialog { get; } = new();
    public Interaction<string, IStorageFile?> ShowOpenFileDialog { get; } = new();
    public Interaction<FileDialogOptions, IStorageFile?> ShowSaveFileDialog { get; } = new();


    public IFileViewModel CurrentFile
    {
        get => _currentFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentFile, value);
        }
    }

    public ICommand SaveFileCommand => _saveProjectCommand ??= ReactiveCommand.Create(async () => 
    {
        if (CurrentFile is ProjectViewModel pvm)
        {
            _projectService.SaveProject(pvm.SaveChanges(), (FileStream)await pvm.ProjectStorage.OpenWriteAsync());
        }
        else if (CurrentFile is MethodologyTemplateViewModel mtvm)
        {

            var fileStorage = mtvm.TemplateStorage ?? (await ShowSaveFileDialog.Handle(new(mtvm.Name, "myt")));

            if (fileStorage == null) return;
            
            _methodologyService.SaveMethodologyTemplate(mtvm.SaveChanges(), (FileStream) await fileStorage.OpenWriteAsync());
        }

    }, this.WhenAnyValue<MainViewModel, bool, IFileViewModel>(vm => vm.CurrentFile, v => v is not EmptyFileViewModel));

    public ICommand OpenProjectCommand => _openProjectCommand ??= ReactiveCommand.Create(async () => 
    {
        IStorageFile projectFileStorage = await ShowOpenFileDialog.Handle(".atproj");

        if (projectFileStorage == null) return;

        CurrentFile.Dispose();
        CurrentFile = new ProjectViewModel(_projectService.OpenProject((FileStream) await projectFileStorage.OpenReadAsync()), projectFileStorage, _methodologyService);
    });

    public ICommand CreateProjectCommand => _createProjectCommand ??= ReactiveCommand.Create(async () => 
    {
        IStorageFile fileStorage = await ShowCreateProjectDialog.Handle(new CreateProjectWindowViewModel());
        if (fileStorage == null) return;

        CurrentFile.Dispose();
        CurrentFile = new ProjectViewModel(_projectService.CreateProject((FileStream) await fileStorage.OpenWriteAsync()), fileStorage, _methodologyService);
    });

    public ICommand OpenMethodologyTemplateCommand => _openMethodologyTemplateCommand ??= ReactiveCommand.Create(async () =>
    {
        IStorageFile templateStorage = await ShowOpenFileDialog.Handle(".myt");

        if (templateStorage == null) return;

        CurrentFile.Dispose();
        CurrentFile = new MethodologyTemplateViewModel(_methodologyService.OpenMethodologyTymplate((FileStream)await templateStorage.OpenReadAsync()), templateStorage);
    });

    public ICommand CreateMethodologyTemplateCommand => _createMethodologyTemplateCommand ??= ReactiveCommand.Create(async () =>
    {
        CurrentFile.Dispose();
        CurrentFile = new MethodologyTemplateViewModel(new MethodologyTemplate() { Name = "Методология 1"}, null);
    });

    public ICommand OpenHelpCommand => _openHelpCommand ??= ReactiveCommand.Create(() => { });

    public void Close()
    {
        CurrentFile.Dispose();
    }
}
