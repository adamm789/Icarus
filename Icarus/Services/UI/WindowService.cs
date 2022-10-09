using Icarus.Services.Interfaces;
using System.Linq;
using System.Windows;

namespace Icarus.Services.UI
{
    public class WindowService : ServiceBase<WindowService>, IWindowService
    {
        public void Show<T>(object dataContext) where T : Window, new()
        {
            var child = new T();
            child.DataContext = dataContext;
            child.Show();
        }

        public void ShowWindow<T>(object dataContext) where T : Window, new()
        {
            var child = new T();

            child.DataContext = dataContext;

            child.ShowDialog();
        }

        public bool IsWindowOpen<T>(string name ="") where T: Window
        {
            var any = Application.Current.Windows.OfType<T>().Any();
            var anyName = Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
            return string.IsNullOrEmpty(name) ? any : anyName;
        }
    }
}
