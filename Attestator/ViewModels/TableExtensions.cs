using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

using MethodologyFramework.Data;
using MethodologyFramework.Template;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Reactive.Linq;

namespace Attestator.ViewModels;

public static class TableExtensions
{
    public static IDataTemplate CreateCellTemplate(DataColumn col, Func<string?, object?> converter)
    {
        return new FuncDataTemplate(typeof(DataRow), (data, scope) =>
        {
            var row = (DataRow)data;
            var tb = new TextBox();
            if (row.Table.Columns.Contains(col.ColumnName))
            {
                tb.Text = row[col.ColumnName].ToString();
            }

            tb.TextChanged += (s, e) =>
            {
                var val = converter(tb.Text);
                if (val != null && !row[col.ColumnName].Equals(val))
                {
                    row.SetField(col, val);
                }
            };
            return tb;
        });
    }

    public static IDataTemplate CreateCellTemplate(TableTemplate table, ColumnTemplate col)
    {
        return new FuncDataTemplate(typeof(object[]), (data, scope) =>
        {
            var row = data as object[];

            var tb = new TextBox()
            {
                Text = row[table.Columns.IndexOf(col)].ToString()
            };

            tb.TextChanged += (s, e) =>
            {
                object? val = col.DataType switch
                {
                    DataType.Number => int.TryParse(tb.Text, out var intRes) ? intRes : null,
                    DataType.Float => double.TryParse(tb.Text, out var floatRes) ? floatRes : null,
                    DataType.String => tb.Text,
                    _ => null
                };

                if (val != null)
                {
                    row[table.Columns.IndexOf(col)] = val;
                }
            };
            return tb;
        });
    }

    public static FlatTreeDataGridSource<DataRow> ToDataGridSource(this DataTable table)
    {
        ObservableCollection<DataRow> itemsSource = new(table.AsEnumerable());
        FlatTreeDataGridSource<DataRow> source = new(itemsSource);
        
        table.RowDeleted += (s, e) =>
        itemsSource.Remove(e.Row);
       
        table.RowChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case DataRowAction.Delete:
                    itemsSource.Remove(e.Row);
                    break;
                case DataRowAction.Add:
                    itemsSource.Add(e.Row);
                    break;
                case DataRowAction.Commit:
                case DataRowAction.Nothing:
                case DataRowAction.Change:
                case DataRowAction.Rollback:
                case DataRowAction.ChangeOriginal:
                case DataRowAction.ChangeCurrentAndOriginal:
                default:
                    break;
            }
        };

        table.Columns.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    CreateColumn(e.Element as DataColumn, source);
                    break;
                case CollectionChangeAction.Remove:
                    var deletedColumn = source.Columns.OfType<IColumn<DataRow>>().FirstOrDefault(c => c.Header.Equals(e.Element as DataColumn));
                    if (deletedColumn != null)
                    {
                        source.Columns.Remove(deletedColumn);
                    }
                    break;
                case CollectionChangeAction.Refresh:
                    break;
                default:
                    break;
            }
        };

        foreach (DataColumn col in table.Columns.OfType<DataColumn>())
        {
            CreateColumn(col, source);
        }
        return source;
    }

    private static void CreateColumn(DataColumn col, FlatTreeDataGridSource<DataRow> source)
    {
        if (col.DataType == typeof(bool))
        {
            source.Columns.Add(new CheckBoxColumn<DataRow>(col, row => row.Field<bool>(col), (row, v) => row.SetField(col, v)));
        }
        else if (col.DataType == typeof(int) || col.DataType == typeof(long))
            source.Columns.Add(new TemplateColumn<DataRow>(col, CreateCellTemplate(col, t => int.TryParse(t, out var v) ? v : null)));
        //source.Columns.Add(CreateTextColumn<int>(col));
        else if (col.DataType == typeof(double))
            source.Columns.Add(new TemplateColumn<DataRow>(col, CreateCellTemplate(col, t => double.TryParse(t, out var v) ? v : null)));
        //source.Columns.Add(CreateTextColumn<double>(col));
        else if (col.DataType == typeof(float))
            source.Columns.Add(new TemplateColumn<DataRow>(col, CreateCellTemplate(col, t => float.TryParse(t, out var v) ? v : null)));
        //source.Columns.Add(CreateTextColumn<float>(col));
        else if (col.DataType == typeof(object))
            source.Columns.Add(CreateTextColumn<object>(col));
        else
            source.Columns.Add(CreateTextColumn<string>(col));
    }

    private static TextColumn<DataRow, T> CreateTextColumn<T>(DataColumn col)
    {
        var tcol = new TextColumn<DataRow, T>(col, row => row.Field<T>(col), (row, v) => row.SetField(col, v));
        tcol.Options.BeginEditGestures = BeginEditGestures.Tap;
        return tcol;
    }

    private static TextColumn<object[], T> CreateTextColumn<T>(ColumnTemplate col, TableTemplate table)
    {
        return new TextColumn<object[], T>(col, row => (T)row[table.Columns.IndexOf(col)], (row, v) => row[table.Columns.IndexOf(col)] = v);
    }


    public static FlatTreeDataGridSource<object[]> ToDataGridSource(this TableTemplate table)
    {
        ObservableCollection<object[]> itemsSource = new(table.Rows);
        FlatTreeDataGridSource<object[]> source = new(itemsSource);
        
        foreach (var col in table.Columns)
        {
            if (col.DataType == DataType.Check)
            {
                source.Columns.Add(new CheckBoxColumn<object[]>(col, row => (bool)row[table.Columns.IndexOf(col)], (row, v) => row[table.Columns.IndexOf(col)] = v));
            }
            else
                source.Columns.Add(new TemplateColumn<object[]>(col, CreateCellTemplate(table, col)));
            //else if (col.DataType == DataType.Number)
            //    source.Columns.Add(new TemplateColumn<object[]>(col, CreateCellTemplate(table, col)));
            ////source.Columns.Add(CreateTextColumn<int>(col, table));
            //else if (col.DataType == DataType.Float)
            //    source.Columns.Add(CreateTextColumn<double>(col, table));
            //else if (col.DataType == DataType.String)
            //    source.Columns.Add(CreateTextColumn<string>(col, table));
        }
        return source;
    }
}
