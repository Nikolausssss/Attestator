using Avalonia.Platform.Storage;

using MethodologyFramework.Template;

using ReactiveUI;

using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class MethodologyTemplateViewModel : ViewModelBase, IFileViewModel
{
    private ReactiveCommand<Unit, Unit> _addMeasuresCommand;
    private ReactiveCommand<Unit, Unit> _addTableCommand;
    private ReactiveCommand<Unit, Unit> _addParameterCommand;
    private ReactiveCommand<Unit, Unit> _addCalculationCommand;
    private ReactiveCommand<ParameterTemplateViewModel, Unit> _deleteParameterCommand;
    private ReactiveCommand<MeasureTemplateViewModel, Unit> _deleteMeasuresCommand;
    private ReactiveCommand<CalculationTemplateViewModel, Unit> _deleteCalculationCommand;
    private ReactiveCommand<TableTemplateViewModel, Unit> _deleteTableCommand;
    private string _name;


    public MethodologyTemplateViewModel(MethodologyTemplate methodologyTemplate, IStorageFile? templateStorage)
    {
        MethodologyTemplate = methodologyTemplate;
        TemplateStorage = templateStorage;

        Name = methodologyTemplate.Name;

        foreach (var mes in methodologyTemplate.Measures)
        {
            Measures.Add(new MeasureTemplateViewModel(mes));
        }

        foreach (var param in methodologyTemplate.Parameters)
        {
            Parameters.Add(new ParameterTemplateViewModel(param));
        }

        foreach (var calc in methodologyTemplate.Calculations)
        {
            Calculations.Add(new CalculationTemplateViewModel(calc));
        }

        foreach (var table in methodologyTemplate.TableSpace)
        {
            TableSpace.Add(new TableTemplateViewModel(table));
        }
    }


    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public MethodologyTemplate MethodologyTemplate { get; }
    public IStorageFile? TemplateStorage { get; }

    public ObservableCollection<MeasureTemplateViewModel> Measures { get; } = new();
    public ObservableCollection<ParameterTemplateViewModel> Parameters { get; } = new();
    public ObservableCollection<CalculationTemplateViewModel> Calculations { get; } = new();
    public ObservableCollection<TableTemplateViewModel> TableSpace { get; } = new();

    public ICommand AddMeasuresCommand => _addMeasuresCommand ??= ReactiveCommand.Create(() =>
    {
        Measures.Add(new MeasureTemplateViewModel(new MeasureTemplate()
        {
            Name = NameGenerator.Generate("Measure", Measures.Select(m => m.Name)),
            Title = NameGenerator.Generate("Измерение", Measures.Select(m => m.Name)),
            CanAddRows = true,
        }));
    });

    public ICommand AddParameterCommand => _addParameterCommand ??= ReactiveCommand.Create(() =>
    {
        Parameters.Add(new ParameterTemplateViewModel(new ParameterTemplate()
        {
            Name = NameGenerator.Generate("Param", Measures.Select(m => m.Name)),
            Title = NameGenerator.Generate("Параметр", Measures.Select(m => m.Name)),
            Type = MethodologyFramework.Data.DataType.String,
        }));
    });

    public ICommand AddCalculationCommand => _addCalculationCommand ??= ReactiveCommand.Create(() =>
    {
        Calculations.Add(new CalculationTemplateViewModel(new CalculationTemplate()
        {
            Name = NameGenerator.Generate("Calc", Measures.Select(m => m.Name)),
            Title = NameGenerator.Generate("Расчет", Measures.Select(m => m.Name)),
        }));
    });

    public ICommand AddTableCommand => _addTableCommand ??= ReactiveCommand.Create(() =>
    {
        TableSpace.Add(new TableTemplateViewModel(new TableTemplate() 
        {
            Name = NameGenerator.Generate("Table", Measures.Select(m => m.Name)),
            Title = NameGenerator.Generate("Таблица", Measures.Select(m => m.Name))
        }));
    });

    public ICommand DeleteMeasuresCommand => _deleteMeasuresCommand ??= ReactiveCommand.Create<MeasureTemplateViewModel>(p => Measures.Remove(p));

    public ICommand DeleteParameterCommand => _deleteParameterCommand ??= ReactiveCommand.Create<ParameterTemplateViewModel>(p => Parameters.Remove(p));

    public ICommand DeleteCalculationCommand => _deleteCalculationCommand ??= ReactiveCommand.Create<CalculationTemplateViewModel>(p => Calculations.Remove(p));

    public ICommand DeleteTableCommand => _deleteTableCommand ??= ReactiveCommand.Create<TableTemplateViewModel>(p => TableSpace.Remove(p));

    public void Dispose()
    {
    }

    public MethodologyTemplate SaveChanges()
    {
        return new MethodologyTemplate()
        {
            Name = Name,
            Measures = Measures.Select(m => m.SaveChanges()).ToList(),
            Parameters = Parameters.Select(p => p.SaveChanges()).ToList(),
            Calculations = Calculations.Select(c => c.SaveChanges()).ToList(),
            TableSpace = TableSpace.Select(t => t.SaveChanges()).ToList(),
        };
    }
}
