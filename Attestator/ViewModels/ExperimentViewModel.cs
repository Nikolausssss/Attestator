using Attestator.Services;

using MethodologyFramework;
using MethodologyFramework.Data;
using MethodologyFramework.Template;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Attestator.ViewModels;

public class ExperimentViewModel
{
    private Experiment _originalExperiment;
    private string _tempPrefix = "temp_";
    private bool _flag = false;


    public ExperimentViewModel(Experiment context, string projectFolderPath)
    {
        _originalExperiment = context;
        ProjectFolderPath = projectFolderPath;

        Name = context.Name;

        TempFilePath = $"{projectFolderPath}\\{Path.GetDirectoryName(context.FilePath)}\\{_tempPrefix}{Path.GetFileName(context.FilePath)}";
        OriginalFilePath = $"{projectFolderPath}\\{context.FilePath}";

        File.Copy(OriginalFilePath, TempFilePath, true);

        TempContext = new MethodologyContext(context.Context.Template, ProjectService.ResolveSourceProvider(context.SourceProviderName, TempFilePath));

        Measures = TempContext.Measures.Select(m =>
        {
            m.RowChanged += DataTable_RowChanged;
            return new DataTableViewModel(context.Context, m, m.CanAddRows);
        }).ToList();

        Tables = TempContext.Tables.Select(t =>
        {
            t.RowChanged += DataTable_RowChanged;
            return new DataTableViewModel(context.Context, t);
        }).ToList();

        Calculations = TempContext.Calculations.Select(t => new DataTableViewModel(context.Context, t, false)).ToList();
    }

    public ExperimentViewModel(MethodologyTemplate template, string projectFolderPath)
    {
        ProjectFolderPath = projectFolderPath;

        var fileName = $"{Guid.NewGuid()}.db";
        var filePath = $"{template.Name}\\{fileName}";
        OriginalFilePath = $"{ProjectFolderPath}\\{filePath}";
        TempFilePath = $"{ProjectFolderPath}\\{template.Name}\\{_tempPrefix}{fileName}";
        var sourceProvider = ProjectService.GetDefaultProvider(TempFilePath);
        TempContext = new MethodologyContext(template, sourceProvider);


        Measures = TempContext.Measures.Select(m =>
        {
            m.RowChanged += DataTable_RowChanged;
            return new DataTableViewModel(TempContext, m, m.CanAddRows);
        }).ToList();

        Tables = TempContext.Tables.Select(t =>
        {
            t.RowChanged += DataTable_RowChanged;
            return new DataTableViewModel(TempContext, t);
        }).ToList();

        Calculations = TempContext.Calculations.Select(t => new DataTableViewModel(TempContext, t, false)).ToList();

        _originalExperiment = new Experiment()
        {
            FilePath = filePath,
            SourceProviderName = sourceProvider.Name,
        };
    }

    private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (_flag || e.Action == DataRowAction.Commit) return;

        _flag = true;
        TempContext.SaveChanges();
        foreach (var item in Calculations)
        {
            var calc = (item.Table as Calculation);
            calc.Update();
        }
        _flag = false;
    }

    public string Name { get; set; }
    public string OriginalFilePath { get; }
    public string TempFilePath { get; }
    public MethodologyContext TempContext { get; }
    public string ProjectFolderPath { get; }

    public IEnumerable<DataTableViewModel> Measures { get; }
    public IEnumerable<Parameter> Parameters => TempContext.Parameters;
    public IEnumerable<DataTableViewModel> Calculations { get; }
    public IEnumerable<DataTableViewModel> Tables { get; }

    public Experiment SaveChanges()
    {
        File.Copy(TempFilePath, OriginalFilePath, true);
        return new Experiment()
        {
            Name = Name,
            Context = _originalExperiment.Context ?? new MethodologyContext(TempContext.Template, ProjectService.ResolveSourceProvider(_originalExperiment.SourceProviderName, OriginalFilePath)),
            SourceProviderName = _originalExperiment.SourceProviderName,
            FilePath = _originalExperiment.FilePath,
        };
    }

    public void Dispose()
    {
        TempContext.Dispose();
        _originalExperiment.Context?.Dispose();

        try
        {
            File.Delete(TempFilePath);
        }
        catch (Exception e)
        {

        }
    }
}
