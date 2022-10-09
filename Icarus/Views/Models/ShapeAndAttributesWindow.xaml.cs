using System.Windows;
using System.Windows.Input;

namespace Icarus.Views.Models
{
    /// <summary>
    /// Interaction logic for ShapeAndAttributesView.xaml
    /// </summary>
    public partial class ShapeAndAttributeWindow : Window
    {
        public ShapeAndAttributeWindow()
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
