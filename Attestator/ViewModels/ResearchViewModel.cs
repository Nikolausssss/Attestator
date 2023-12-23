using Attestator.Services;

using ReactiveUI;

using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class ResearchViewModel : ViewModelBase
{
    private readonly Research _originalResearch;
    private ReactiveCommand<Unit, Unit> _addExperimentCommand;

    public ResearchViewModel(Research research, string projectFolder)
    {
        _originalResearch = research;
        ProjectFolder = projectFolder;

        foreach (var context in _originalResearch.Experiments)
        {
            Experiments.Add(new ExperimentViewModel(context, projectFolder));
        }
    }

    public string Name => _originalResearch.Name;
    public string ProjectFolder { get; }

    public ObservableCollection<ExperimentViewModel> Experiments { get; } = new();

    public ICommand AddExperimentCommand => _addExperimentCommand ??= ReactiveCommand.Create(() =>
    {
        Experiments.Add(new ExperimentViewModel(_originalResearch.Template, ProjectFolder)
        {
            Name = NameGenerator.Generate("Испытание", Experiments.Select(ci => ci.Name)),
        });
    });


    public Research SaveChanges()
    {
        return new Research()
        {
            Name = Name,
            Template = _originalResearch.Template,
            Experiments = Experiments.Select(ci => ci.SaveChanges()).ToList(),
        };
    }

    public void Dispose()
    {
        foreach (var item in Experiments)
        {
            item.Dispose();
        }
        Experiments.Clear();
    }
}
