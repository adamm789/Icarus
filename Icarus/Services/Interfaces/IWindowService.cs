using System;
using System.Windows;
using System.Windows.Forms;

namespace Icarus.Services.Interfaces
{
    public interface IWindowService : IServiceProvider
    {
        void Show<T>(object dataContext) where T : Window, new();

        bool? ShowWindow<T>(object dataContext) where T : Window, new();

        DialogResult ShowOptionWindow<T>(object dataContext) where T : Window, new();

        bool IsWindowOpen<T>(string name = "") where T : Window;
    }
}
