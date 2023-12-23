using System;

namespace Attestator.ViewModels;

public interface IFileViewModel : IDisposable
{
    string Name { get; }
}
