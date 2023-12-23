using System.Windows.Input;

namespace Attestator.ViewModels;

public class EmptyFileViewModel : IFileViewModel
{
    public EmptyFileViewModel(ICommand openProjectCommand, ICommand createProjectCommand, ICommand openMethodologyTemplateCommand, ICommand createMethodologyTemplateCommand)
    {
        OpenProjectCommand = openProjectCommand;
        CreateProjectCommand = createProjectCommand;
        OpenMethodologyTemplateCommand = openMethodologyTemplateCommand;
        CreateMethodologyTemplateCommand = createMethodologyTemplateCommand;
    }

    public string Name { get; } = string.Empty;
    public ICommand OpenProjectCommand { get; }
    public ICommand CreateProjectCommand { get; }
    public ICommand OpenMethodologyTemplateCommand { get; }
    public ICommand CreateMethodologyTemplateCommand { get; }

    public void Dispose()
    {
    }
}
