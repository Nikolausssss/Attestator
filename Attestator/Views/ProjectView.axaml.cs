using Attestator.ViewModels;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using System.Linq;

namespace Attestator.Views;

public partial class ProjectView : UserControl
{
    public ProjectView()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is not ProjectViewModel pvm) return;

            Window window = null;
            var parent = Parent;
            while (parent != null && parent is not Window)
            {
                parent = parent.Parent;
            }

            if (parent == null) return;

            window = parent as Window;

            pvm.ShowOpenMethodologyTemplateDialog.RegisterHandler(async context =>
            {
                var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Attestation Project")
                        {
                            Patterns = new[] { $"*{context.Input.DefaultFileExtension}" }
                        }
                    }
                });

                if (!files.Any()) return;
                context.SetOutput(files[0]);
            });
        };
    }
}
