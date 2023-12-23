using Attestator.Services;

using Avalonia.Platform.Storage;

using ReactiveUI;

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class CreateProjectWindowViewModel : ViewModelBase
{
    private string _projectName = "Attestation";
    private string _projectFolderPath = Environment.CurrentDirectory;
    private ReactiveCommand<Unit, Task> _openFolderPickerCommand;

    public CreateProjectWindowViewModel()
    {
    }

    public Interaction<string?, IStorageFolder?> ShowOpenFolderDialog { get; } = new();

    public string ProjectName
    {
        get => _projectName;
        set
        {
            this.RaiseAndSetIfChanged(ref _projectName, value);
            this.RaisePropertyChanged(nameof(FullProjectPath));
        }
    }

    private IStorageFolder _projectFolder;

    public string ProjectFolderPath
    {
        get => _projectFolderPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _projectFolderPath, value);
            this.RaisePropertyChanged(nameof(FullProjectPath));
        }
    }

    public string FullProjectPath => $"{_projectFolderPath}\\{_projectName}\\{_projectName}.atproj";

    public async Task<IStorageFile?> GetProjectFileAsync()
    {
        var folder = await _projectFolder.CreateFolderAsync(ProjectName);
        if (folder == null) return null;
        return await folder.CreateFileAsync($"{_projectName}.atproj");
    }

    public ICommand OpenFolderPickerCommand => _openFolderPickerCommand ??= ReactiveCommand.Create(async () =>
    {
        var file = await ShowOpenFolderDialog.Handle(ProjectFolderPath);

        if (file == null) return;
        _projectFolder = file;
        ProjectFolderPath = file.Path.AbsolutePath;
    });
}