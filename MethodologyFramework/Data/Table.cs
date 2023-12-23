using MethodologyFramework.Template;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace MethodologyFramework.Data;

public class Column : DataColumn
{
    public Column(ColumnTemplate template) :this(template.Name, template.Title, template.DataType)
    { }

    internal Column(string name, string? title, DataType type) : base(name, Data.Table.Convert(type))
    {
        Title = title ?? name;
        Type = type;
    }

    public string Title { get; set; }
    public DataType Type { get; }
}

public class Table : DataTable
{
    private readonly Action<Table> _updator;

    public Table(TableTemplate template, Action<Table> updator) : base(template.Name)
    {
        Title = template.Title ?? template.Name;

        Columns.Add(new Column("Number", "#", DataType.Number) { Unique = true, AutoIncrement = true });
        foreach (var col in template.Columns.Where(c => c.Name != "Number"))
        {
            Columns.Add(new Column(col));
        }
        _updator = updator;

        foreach (var row in template.Rows)
        {
            Rows.Add(row);
        }
    }

    public string Title { get; set; }

    public void Update()
    {
        _updator(this);
    }

    public static Type Convert(DataType dataType)
    {
        switch (dataType)
        {
            case DataType.Number:
                return typeof(int);
            case DataType.Float:
                return typeof(double);
            case DataType.String:
                return typeof(string);
            case DataType.Check:
                return typeof(bool);
            default:
                throw new NotImplementedException();
        }
    }
}
