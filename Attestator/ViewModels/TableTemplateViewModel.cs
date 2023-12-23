using Attestator.Services.SourceProviders;

using Avalonia.Controls;

using MethodologyFramework.Data;
using MethodologyFramework.Template;

using ReactiveUI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Attestator.ViewModels;

public static class NameGenerator
{
    public static string Generate(string prefix, IEnumerable<string> names, int startIndex = 1)
    {
        int index = startIndex;
        string newName = prefix.Trim();
        while (names.Contains(newName))
        {
            newName = $"{prefix}{index++}";
        }
        return newName;
    }
}

public class TableTemplateViewModel : ViewModelBase
{
    private ReactiveCommand<Unit, Unit> _addColumnCommand;
    private ReactiveCommand<DataColumn, Unit> _deleteColumnCommand;
    private ReactiveCommand<Unit, Unit> _addRowCommand;
    private ReactiveCommand<DataRow, Unit> _deleteRowCommand;
    

    public TableTemplateViewModel(TableTemplate tableTemplate)
    {
        TableTemplate = tableTemplate;
        Table = new Table(TableTemplate, t => { });   
        GridSource = Table.ToDataGridSource();
    }
    

    public bool IsEdited { get; protected set; }

    public string Name
    {
        get => Table.TableName;
        set
        {
            Table.TableName = value;
            this.RaisePropertyChanged();
            IsEdited = true;
        }
    }

    public string Title
    {
        get => Table.Title;
        set
        {
            Table.Title = value;
            this.RaisePropertyChanged();
            IsEdited = true;
        }
    }

    public Table Table { get; }
    public TableTemplate TableTemplate { get; }

    public virtual TableTemplate SaveChanges()
    {
        var tableTemplate = new TableTemplate()
        {
            Title = Table.Title,
            Name = Table.TableName,
        };

        foreach (var col in Table.Columns.OfType<Column>())
        {
            tableTemplate.Columns.Add(new ColumnTemplate()
            {
                Name = col.ColumnName,
                Title = col.Title,
                DataType = col.Type
            });
        }

        tableTemplate.Rows = Table.AsEnumerable().Select(row => row.ItemArray!).ToList();

        return tableTemplate;
    }

    public FlatTreeDataGridSource<DataRow> GridSource { get; }

    public DataType NewColumnType { get; set; }

    public DataType[] ColumnTypes { get; } = new DataType[]
    {
        DataType.Number,
        DataType.Float,
        DataType.String,
        DataType.Check,
    };

    public ICommand AddColumnCommand => _addColumnCommand ??= ReactiveCommand.Create(() =>
    {
        Table.Columns.Add(new Column(new ColumnTemplate()
        {
            Title = NameGenerator.Generate("Ст", Table.Columns.OfType<Column>().Select(col => col.Title)),
            Name = NameGenerator.Generate("Col", Table.Columns.OfType<Column>().Select(col => col.ColumnName)),
            DataType = NewColumnType
        }));

        this.RaisePropertyChanged(nameof(GridSource));
        IsEdited = true;
    });

    public ICommand DeleteColumnCommand => _deleteColumnCommand ??= ReactiveCommand.Create<DataColumn>(col =>
    {
        if (col.ColumnName == "Number") return;

        Table.Columns.Remove(col);
        this.RaisePropertyChanged(nameof(GridSource));
        IsEdited = true;
    });

    public ICommand AddRowCommand => _addRowCommand ??= ReactiveCommand.Create(() =>
    {
        Table.Rows.Add(Table.Rows.Count > 0 ? Table.AsEnumerable().Max(row => row.Field<int>(0)) + 1 : 1);
    });

    public ICommand DeleteRowCommand => _deleteRowCommand ??= ReactiveCommand.Create<DataRow>(row =>
    {
        Table.Rows.Remove(row);
    });
}
