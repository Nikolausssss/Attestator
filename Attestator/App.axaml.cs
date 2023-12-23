using Attestator.Services;
using Attestator.Services.SourceProviders;
using Attestator.ViewModels;
using Attestator.Views;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using System.Data.SQLite;
using System.IO;

namespace Attestator;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ProjectService.RegisterSourceProvider(nameof(SQLiteSourceProvider), path =>
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
            return new SQLiteSourceProvider($"Data Source={path}; Pooling=False");
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new ProjectService(), new MethodologyService()),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(new ProjectService(), new MethodologyService()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
