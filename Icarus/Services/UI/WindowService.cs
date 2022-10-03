using Icarus.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
