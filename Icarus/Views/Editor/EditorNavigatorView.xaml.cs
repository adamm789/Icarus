using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Icarus.Views.Editor
{
    /// <summary>
    /// Interaction logic for EditorNavigatorView.xaml
    /// </summary>
    public partial class EditorNavigatorView : UserControl
    {
        public EditorNavigatorView()
        {
            InitializeComponent();
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }

        private void PreviewTextInput(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }
    }
}
