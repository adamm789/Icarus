using System;
using System.Windows;

namespace Icarus.Services.Interfaces
{
    public interface IWindowService : IServiceProvider
    {
        void Show<T>(object dataContext) where T : Window, new();

        void ShowWindow<T>(object dataContext) where T : Window, new();

        bool IsWindowOpen<T>(string name = "") where T : Window;
    }
}
