using Attestator.ViewModels;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using System.Collections.Generic;
using System.Linq;

namespace Attestator.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.InitializeComponent();
        DataContextChanged += MainView_DataContextChanged;
    }

    private void MainView_DataContextChanged(object? sender, System.EventArgs e)
    {
        if (this.DataContext is not MainViewModel vm) return;


        vm.ShowCreateProjectDialog.RegisterHandler(async context =>
        {
            CreateProjectWindow dialog = new()
            {
                DataContext = context.Input
            };

            context.SetOutput(await dialog.ShowDialog<IStorageFile?>(this.Parent as Window));
        });

        vm.ShowOpenFileDialog.RegisterHandler(async context =>
        {
            var files = await (this.Parent as Window).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                        new FilePickerFileType("Attestation Project")
                        {
                            Patterns = new[] { $"*{context.Input}" }
                        }
                }
            });

            if (files.Any())
            {
                context.SetOutput(files[0]);
                return;
            }
            context.SetOutput(null);
        });

        vm.ShowSaveFileDialog.RegisterHandler(async context =>
        {
            var file = await (this.Parent as Window).StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = context.Input.DefaultFileExtension,
                SuggestedFileName = context.Input.DefaultFileName
            });

            context.SetOutput(file);
        });

        this.Unloaded += (s, e) => vm.Close();

    }
}
