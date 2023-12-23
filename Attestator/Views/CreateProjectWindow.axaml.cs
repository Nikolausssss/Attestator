using Attestator.ViewModels;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using System.Linq;

namespace Attestator.Views;

public partial class CreateProjectWindow : Window
{
    public CreateProjectWindow()
    {
        InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (DataContext is CreateProjectWindowViewModel vm)
            {
                vm.ShowOpenFolderDialog.RegisterHandler(async context =>
                {
                    var files = await this.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                    {
                        AllowMultiple = false,
                    });
                    if (files.Any())
                    {
                        context.SetOutput(files[0]);
                        return;
                    }
                    context.SetOutput(null);
                });

            }
        };

        CreateBtn.Click += async (s, e) =>
        {
            Close(await (this.DataContext as CreateProjectWindowViewModel)?.GetProjectFileAsync());
        };

        CloseBtn.Click += (s, e) => { Close(null); };
    }

}
