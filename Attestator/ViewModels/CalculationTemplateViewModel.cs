using MethodologyFramework.Template;

namespace Attestator.ViewModels;

public class CalculationTemplateViewModel : ViewModelBase
{
    private CalculationTemplate _oldCalculationTemplate;


    public CalculationTemplateViewModel(CalculationTemplate calculationTemplate)
    {
        _oldCalculationTemplate = calculationTemplate;
        Title = calculationTemplate.Title;
        Name = calculationTemplate.Name;
        Query = calculationTemplate.Query;
    }


    public CalculationTemplate CalculationTemplate => IsEdited ? SaveChanges() : _oldCalculationTemplate;
    public bool IsEdited => _oldCalculationTemplate.Title != Title
        || _oldCalculationTemplate.Name != Name
        || _oldCalculationTemplate.Query != Query;

    public string Title { get; set; }
    public string Name { get; set; }
    public string Query { get; set; }


    public CalculationTemplate SaveChanges()
    {
        return new CalculationTemplate()
        {
            Name = Name,
            Title = Title,
            Query = Query,
        };
    }
}
