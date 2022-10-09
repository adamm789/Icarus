using Icarus.Services.Interfaces;
using System.Windows;

namespace Icarus.Services.UI
{
    public class WindowService : ServiceBase<WindowService>, IWindowService
    {
        public void ShowWindow<T>(object dataContext) where T : Window, new()
        {
            var child = new T();

            child.DataContext = dataContext;

            child.ShowDialog();
        }
    }
}
