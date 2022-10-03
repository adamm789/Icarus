using Serilog;
using System;
using System.Windows;
using System.Windows.Input;

namespace Icarus
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            Log.Information("=== Closing ===");
            base.OnClosed(e);
            Application.Current.Dispatcher.InvokeShutdown();
        }
    }
}
