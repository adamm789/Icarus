using System.Windows;
using System.Windows.Input;

namespace Icarus.Views
{
    /// <summary>
    /// Interaction logic for UserPreferencesWindow.xaml
    /// </summary>
    public partial class UserPreferencesWindow : Window
    {
        public UserPreferencesWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
