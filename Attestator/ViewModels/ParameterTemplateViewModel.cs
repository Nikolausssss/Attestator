using MethodologyFramework.Data;
using MethodologyFramework.Template;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class ParameterTemplateViewModel : ViewModelBase
{
    private DataType _type;
    private object _defaultValue;
    private ReactiveCommand<Unit, Unit> _addChoiseCommand;

    public ParameterTemplateViewModel(ParameterTemplate parameterTemplate)
    {
        ParameterTemplate = parameterTemplate;
        Title = parameterTemplate.Title;
        Name = parameterTemplate.Name;
        _type = parameterTemplate.Type;
        DefaultValue = parameterTemplate.DefaultValue;
        ChoiseItems = new(parameterTemplate.ChoiseItems ?? []);
    }


    public bool IsEdited => ParameterTemplate.Title != Title
        || ParameterTemplate.Name != Name
        || ParameterTemplate.Type != Type
        || ParameterTemplate.DefaultValue != DefaultValue
        || ChoiseItems.FirstOrDefault(v => !ParameterTemplate.ChoiseItems?.Contains(v) ?? false) != null;

    public ParameterTemplate ParameterTemplate { get; }

    public string Title { get; set; }
    public string Name { get; set; }
    public object DefaultValue 
    {
        get => _defaultValue;
        set => this.RaiseAndSetIfChanged(ref _defaultValue, value);
    }

    public DataType Type
    {
        get => _type;
        set
        {
            _type = value;
            ChoiseItems.Clear();
            this.RaisePropertyChanged();
            DefaultValue = GetDefaultValue(_type);
        }
    }

    private object? GetDefaultValue(DataType type)
    {
        return type switch
        {
            DataType.Number => 0,
            DataType.Float => 0,
            DataType.String => string.Empty,
            DataType.Check => false,
            _ => null
        };
    }

    public Array Types { get; } = Enum.GetValues(typeof(DataType));
    public ObservableCollection<object> ChoiseItems { get; set; }

    public ICommand AddChoiseCommand => _addChoiseCommand ??= ReactiveCommand.Create(() =>
    {
        ChoiseItems.Add(GetDefaultValue(_type));
    }, this.WhenAnyValue(vm => vm.Type, t => t != DataType.Check));

    public ParameterTemplate SaveChanges()
    {
        return new ParameterTemplate()
        {
            Title = Title,
            Name = Name,
            DefaultValue = DefaultValue,
            Type = Type,
            ChoiseItems = ChoiseItems.Count() > 0 ? ChoiseItems.ToList() : null
        };
    }
}
