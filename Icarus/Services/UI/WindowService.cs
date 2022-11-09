using Icarus.Services.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WindowsApplication = System.Windows.Application;

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

        public DialogResult ShowOptionWindow<T>(object dataContext) where T : Window, new()
        {
            var child = new T();
            child.DataContext = dataContext;
            var ret = child.ShowDialog();
            if (ret is bool val)
            {
                if (val)
                {
                    return DialogResult.Yes;
                }
            }
            return DialogResult.No;
        }

        public bool IsWindowOpen<T>(string name = "") where T : Window
        {
            var any = WindowsApplication.Current.Windows.OfType<T>().Any();
            var anyName = WindowsApplication.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
            return string.IsNullOrEmpty(name) ? any : anyName;
        }
    }
}
