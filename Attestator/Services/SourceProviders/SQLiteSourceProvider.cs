using MethodologyFramework.Template;
using MethodologyFramework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodologyFramework.Data;
using System.Data.Entity.Migrations.Model;

namespace Attestator.Services.SourceProviders;

public static class ColumnsExstensions
{
    public static IEnumerable<TResult> Select<TResult>(this DataColumnCollection columnCollection, Func<DataColumn, TResult> selector)
    {
        return columnCollection.OfType<DataColumn>().Select(selector);
    }
}

public class SQLiteSourceProvider : ISourceProvider
{

    private const string s_columnSeparator = ", ";
    private const string s_spaceSeparetor = "_";
    private ConcurrentDictionary<string, SQLiteDataAdapter> _adapterTableCashe = new();
    private ConcurrentDictionary<string, SQLiteDataAdapter> _adapterViewCashe = new();
    private SQLiteConnection _connection;

    public SQLiteSourceProvider(string connectionString)
    {
        ConnectionString = connectionString;
    }


    public string Name { get; } = nameof(SQLiteSourceProvider);
    public string ConnectionString { get; }


    private string GetNames(IEnumerable<string> names, string separator = s_columnSeparator)
    {
        return string.Join(separator, names);
    }

    private string GetNames(DataColumnCollection columns, string separator = s_columnSeparator)
    {
        return GetNames(columns.Select(c => c.ColumnName), separator);
    }

    private string GetTypeName(Type dataType)
    {
        if (dataType == typeof(int) || dataType == typeof(bool))
            return "INTEGER";
        if (dataType == typeof(double) || dataType == typeof(float))
            return "REAL";
        if (dataType == typeof(string) || dataType == typeof(char))
            return "TEXT";

        throw new NotImplementedException();
    }


    private int ExecuteNonQuery(SQLiteConnection conn, string commandText)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = commandText;
        return cmd.ExecuteNonQuery();
    }


    private SQLiteConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SQLiteConnection(ConnectionString);
            _connection.Open();
        }
        return _connection;
    }

    private DataTable GetInfo()
    {
        return GetView("sqlite_schema");
    }

    private bool TableExists(string tableName)
    {
        return GetInfo().AsEnumerable().Where(row => row["type"].Equals("table") && row["name"].Equals(tableName)).Any();
    }

    private bool ViewExists(string viewName)
    {
        return GetInfo().AsEnumerable().Where(row => row["type"].Equals("view") && row["name"].Equals(viewName)).Any();
    }



    public void CreateTable(DataTable table, string? schema = null)
    {
        var conn = GetConnection();
        ExecuteNonQuery(conn,
        $@"CREATE TABLE IF NOT EXISTS ""{table.TableName}"" 
                ({GetNames(table.Columns.Select(c => $"{c.ColumnName} {GetTypeName(c.DataType)} {(c.Unique ? "PRIMARY KEY" : "")}"))});"
        );

        var adapter = GetTableDataAdapter(table.TableName);
        adapter.Update(table);
        UpdateAdapters();
    }

    public void CreateView(string name, string query, string? schema = null)
    {
        var conn = GetConnection();
        ExecuteNonQuery(conn, $@"CREATE VIEW IF NOT EXISTS ""{name}"" AS {query}");
        UpdateAdapters();
    }

    private void UpdateAdapters()
    {
        var tableAdapters = _adapterTableCashe.ToList();
        _adapterTableCashe.Clear();
        foreach (var kvp in tableAdapters)
        {
            kvp.Value.Dispose();
            GetTableDataAdapter(kvp.Key);
        }

        var viewAdapters = _adapterViewCashe.ToList();
        _adapterViewCashe.Clear();
        foreach (var kvp in viewAdapters)
        {
            kvp.Value.Dispose();
            GetViewDataAdapter(kvp.Key);
        }
    }

    public DataTable GetTable(string name, string? schema = null)
    {
        var table = new DataTable(name);
        FillTable(table, schema);
        return table;
    }

    private SQLiteDataAdapter GetTableDataAdapter(string tableName)
    {
        return _adapterTableCashe.GetOrAdd(tableName, key =>
        {
            SQLiteDataAdapter da = new($"SELECT * FROM \"{tableName}\"", GetConnection());

            SQLiteCommandBuilder builder = new(da);

            da.InsertCommand = builder.GetInsertCommand();
            da.UpdateCommand = builder.GetUpdateCommand();
            da.DeleteCommand = builder.GetDeleteCommand();
            return da;
        });
    }

    private SQLiteDataAdapter GetViewDataAdapter(string tableName)
    {
        return _adapterViewCashe.GetOrAdd(tableName, key => new($"SELECT * FROM \"{tableName}\"", GetConnection()));
    }

    public void FillTable(DataTable table, string? schema = null)
    {
        var adapter = GetTableDataAdapter(table.TableName);
        table.Rows.Clear();

        adapter.Fill(table);
    }

    public DataTable GetView(string name, string? schema = null)
    {
        var table = new DataTable(name);
        FillView(table, schema);
        return table;
    }

    public void FillView(DataTable view, string? schema = null)
    {
        var adapter = GetViewDataAdapter(view.TableName);
        foreach (var row in view.Rows.OfType<DataRow>().ToArray())
        {
            view.Rows.Remove(row);
        }

        adapter.Fill(view);
    }

    public void SaveChanges(DataTable table, string? schema = null)
    {
        if (_adapterTableCashe.TryGetValue(table.TableName, out var adapter))
        {
            adapter.Update(table);
            return;
        }
        throw new Exception();
    }

    public void FillOrCreateTable(DataTable dataTable, string? schema = null)
    {
        if (TableExists(dataTable.TableName))
        {
            //dataTable.Rows.Clear();
            FillTable(dataTable, schema);
            return;
        }

        CreateTable(dataTable, schema);
    }

    public void FillOrCreateView(DataTable dataTable, string query, string? schema = null)
    {
        if (!ViewExists(dataTable.TableName))
        {
            CreateView(dataTable.TableName, query, schema);
        }

        //dataTable.Rows.Clear();
        FillView(dataTable, schema);
    }

    public void Dispose()
    {
        var conn = GetConnection();
        conn.Close();
        conn.Dispose();
        foreach(var adapter in _adapterTableCashe.Values)
        {
            adapter.Dispose();
        }
        foreach(var adapter in _adapterViewCashe.Values)
        {
            adapter.Dispose();
        }

        SQLiteConnection.ClearPool(conn);
        SQLiteConnection.ClearAllPools();
    }
}


