using MethodologyFramework.Template;

using System.Data;

using DataType = MethodologyFramework.Data.DataType;

namespace MethodologyFramework;

public interface ISourceProvider : IDisposable
{
    string Name { get; }
    void CreateTable(DataTable tableTemplate, string? schema = null);
    void CreateView(string name, string query, string? schema = null);

    DataTable GetTable(string name, string? schema = null);
    void FillTable(DataTable table, string? schema = null);
    void FillOrCreateTable(DataTable dataTable, string? schema = null);
    void FillOrCreateView(DataTable dataTable, string query, string? schema = null);

    void SaveChanges(DataTable table, string? schema = null);
    void FillView(DataTable view, string? schema = null);
}

