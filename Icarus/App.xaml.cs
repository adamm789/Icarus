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
        public static GameData _lumina;
        public static string _projectDirectory;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var _serviceManager = new ServiceManager();
            _projectDirectory = GetProjectDirectory();
        }

        public static string GetProjectDirectory()
        {
#if DEBUG
            try
            {

                return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/";
            }
            catch (Exception ex)
            {
            }
#endif
            return _projectDirectory = Directory.GetCurrentDirectory() + "/";

        }
    }
}
