using System;
using System.Windows;

namespace Icarus.Services.Interfaces
{
    public interface IWindowService : IServiceProvider
    {
        void ShowWindow<T>(object dataContext) where T : Window, new();
    }
}
