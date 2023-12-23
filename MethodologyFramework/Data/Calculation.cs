using System.Data;

namespace MethodologyFramework.Data;

public class Calculation : DataTable
{
    private readonly Action<Calculation> _updator;

    public Calculation(string name, Action<Calculation> updator, string? title = null)
        : base(name)
    {
        _updator = updator;
        Title = title ?? name;
    }

    public string Title { get; }
    public void Update()
    {
        _updator(this);
    }
}
