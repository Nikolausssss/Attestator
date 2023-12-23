using MethodologyFramework.Template;

using ReactiveUI;

namespace Attestator.ViewModels;

public class MeasureTemplateViewModel : TableTemplateViewModel
{
    private bool _canAddRows;

    public MeasureTemplateViewModel(MeasureTemplate measureTemplate) : base(measureTemplate)
    {
        CanAddRows = measureTemplate.CanAddRows;
    }

    public override MeasureTemplate SaveChanges()
    {
        var tableTemplate = base.SaveChanges();
        return new MeasureTemplate()
        {
            Title = tableTemplate.Title,
            Name = tableTemplate.Name,
            Columns = tableTemplate.Columns,
            Rows = tableTemplate.Rows,
            CanAddRows = _canAddRows
        };
    }

    public MeasureTemplate MeasureTemplate => (MeasureTemplate)base.TableTemplate;

    public bool CanAddRows
    {
        get => _canAddRows;
        set
        {
            _canAddRows = value;
            IsEdited = true;
            this.RaisePropertyChanged();
        }
    }
}
