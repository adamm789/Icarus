using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Icarus.Services.Interfaces
{
    public interface IWindowService : IServiceProvider
    {
        void ShowWindow<T>(object dataContext) where T : Window, new();
    }
}
