using Avalonia.Controls;

using MethodologyFramework;

using ReactiveUI;

using System.Data;
using System.Reactive;
using System.Windows.Input;

namespace Attestator.ViewModels;

public class DataTableViewModel : ViewModelBase
{
    private ReactiveCommand<Unit, Unit> _addRowCommand;
    private ReactiveCommand<DataRow, Unit> _deleteCommand;


    public DataTableViewModel(MethodologyContext context, DataTable table, bool canAddRow = true)
    {
        Context = context;
        Table = table;
        CanAddRow = canAddRow;
        GridSource = Table.ToDataGridSource();
    }


    public MethodologyContext Context { get; }

    public DataTable Table { get; }
    public bool CanAddRow { get; }
    public FlatTreeDataGridSource<DataRow> GridSource { get; }

    public ICommand AddRowCommand => _addRowCommand ??= ReactiveCommand.Create(() =>
    {
        Table.Rows.Add();
        //Table.AcceptChanges();
    }, this.WhenAnyValue(vm => vm.CanAddRow));

    public ICommand DeleteRowCommand => _deleteCommand ??= ReactiveCommand.Create<DataRow>(row =>
    {
        Table.Rows.Remove(row);
        //Table.AcceptChanges();
    }, this.WhenAnyValue(vm => vm.CanAddRow));
}
