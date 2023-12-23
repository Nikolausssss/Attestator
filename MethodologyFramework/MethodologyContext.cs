using MethodologyFramework.Data;
using MethodologyFramework.Template;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MethodologyFramework;

public class MethodologyContext : IDisposable
{
    private readonly ISourceProvider _sourceProvider;
    private Measure[]? _measures;
    private Parameter[]? _parameters;
    private Calculation[]? _calculations;
    private Table[]? _tables;

    public MethodologyContext(MethodologyTemplate template, ISourceProvider sourceProvider)
    {
        Template = template;
        _sourceProvider = sourceProvider;
        BuildDB();
    }


    public MethodologyTemplate Template { get; set; }

    public IEnumerable<Measure> Measures => _measures ??= Template.Measures.Select(table =>
    {
        Measure dataTable = new(table, t => _sourceProvider.FillTable(t));
        _sourceProvider.FillOrCreateTable(dataTable);
        return dataTable;
    }).ToArray();

    public IEnumerable<Table> Tables => _tables ??= Template.TableSpace.Select(table =>
    {
        Table dataTable = new(table, t => _sourceProvider.FillTable(t));
        _sourceProvider.FillOrCreateTable(dataTable);
        return dataTable;
    }).ToArray();

    public IEnumerable<Parameter> Parameters => _parameters ??= Template.Parameters.Select(parameterTemplate =>
    {
        DataTable source = CreateTable(parameterTemplate);
        _sourceProvider.FillOrCreateTable(source);
        return new Parameter(parameterTemplate, source);
    }).ToArray();

    public IEnumerable<Calculation> Calculations => _calculations ??= Template.Calculations.Select(table =>
    {
        Calculation calculation = new(table.Name, t => _sourceProvider.FillView(t), table.Title);
        _sourceProvider.FillOrCreateView(calculation, table.Query);
        return calculation;
    }).ToArray();



    private void BuildDB()
    {
        Template.Measures.Select(table =>
        {
            Measure dataTable = new(table, t => _sourceProvider.FillTable(t));
            _sourceProvider.FillOrCreateTable(dataTable);
            return dataTable;
        }).ToArray();

        Template.TableSpace.Select(table =>
        {
            Table dataTable = new(table, t => _sourceProvider.FillTable(t));
            _sourceProvider.FillOrCreateTable(dataTable);
            return dataTable;
        }).ToArray();

        Template.Parameters.Select(parameterTemplate =>
        {
            DataTable source = CreateTable(parameterTemplate);
            _sourceProvider.FillOrCreateTable(source);
            return new Parameter(parameterTemplate, source);
        }).ToArray();

        Template.Calculations.Select(table =>
        {
            Calculation calculation = new(table.Name, t => _sourceProvider.FillView(t), table.Title);
            _sourceProvider.FillOrCreateView(calculation, table.Query);
            return calculation;
        }).ToArray();
    }

    private DataTable CreateTable(ParameterTemplate parameter)
    {
        DataTable dataTable = new(parameter.Name);
        dataTable.Columns.Add("Id", typeof(int)).Unique = true;
        dataTable.Columns.Add("Value", Table.Convert(parameter.Type));
        dataTable.Rows.Add(1, parameter.DefaultValue);
        return dataTable;
    }

    public void SaveChanges()
    {
        if (_tables != null)
        {
            foreach (var table in _tables)
            {
                //table.AcceptChanges();
                _sourceProvider.SaveChanges(table);
            }
        }

        if (_measures != null)
        {
            foreach (var measure in _measures)
            {
                //measure.AcceptChanges();
                _sourceProvider.SaveChanges(measure);
            }
        }

        if (_parameters != null)
        {
            foreach (var param in Parameters)
            {
                //param.Source.AcceptChanges();
                _sourceProvider.SaveChanges(param.Source);
            }
        }
    }

    public void Dispose()
    {
        _sourceProvider.Dispose();
    }
}

