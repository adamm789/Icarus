using Icarus.Services;
using Lumina;
using System;
using System.IO;
using System.Windows;

namespace Icarus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var _serviceManager = new ServiceManager();
        }
    }
}
