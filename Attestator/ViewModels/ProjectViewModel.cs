using Attestator.Services;

using Avalonia.Input.TextInput;
using Avalonia.Platform.Storage;

using ReactiveUI;

using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class ProjectViewModel : IFileViewModel
{
    private readonly MethodologyService _methodologyService;
    private ReactiveCommand<Unit, Task> _addResearchCommand;
    private Project _originalProject;
    private ReactiveCommand<ResearchViewModel, Unit> _deleteResearchCommand;

    public ProjectViewModel(Project project, IStorageFile projectStorage, MethodologyService methodologyService)
    {
        _originalProject = project;
        _methodologyService = methodologyService;

        ProjectStorage = projectStorage;
        ProjectFolderPath = Path.GetDirectoryName(projectStorage.Path.AbsolutePath);
        Name = project.Name;
        foreach (var space in _originalProject.Researches)
        {
            Researches.Add(new ResearchViewModel(space, ProjectFolderPath));
        }
    }

    public string Name { get; }
    public IStorageFile ProjectStorage { get; }
    public string? ProjectFolderPath { get; }
    public ObservableCollection<ResearchViewModel> Researches { get; } = new();
    public Interaction<FileDialogOptions, IStorageFile?> ShowOpenMethodologyTemplateDialog { get; } = new();

    public ICommand AddResearchCommand => _addResearchCommand ??= ReactiveCommand.Create(async () =>
    {
        IStorageFile fileStorage = await ShowOpenMethodologyTemplateDialog.Handle(new FileDialogOptions("", ".myt"));
        if (fileStorage == null) return;

        var template = _methodologyService.OpenMethodologyTymplate((FileStream)await fileStorage.OpenReadAsync());

        if (Researches.Any(s => s.Name.Equals(template.Name))) return;

        Researches.Add(new ResearchViewModel(new Research()
        {
            Name = template.Name,
            Template = template,
        }, ProjectFolderPath));
    });

    public ICommand DeleteResearchCommand => _deleteResearchCommand ??= ReactiveCommand.Create<ResearchViewModel>(research =>
    {
        Researches.Remove(research);
        research.Dispose();
    });

    public Project SaveChanges()
    {
        return new Project()
        {
            Name = Name,
            Researches = Researches.Select(ms => ms.SaveChanges()).ToList()
        };
    }

    public void Dispose()
    {
        foreach (var item in Researches)
        {
            item.Dispose();
        }
        Researches.Clear();
    }
}
