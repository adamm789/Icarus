using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Import
{
    /// <summary>
    /// Interaction logic for MeshGroupView.xaml
    /// </summary>
    public partial class MeshGroupView : UserControl
    {
        public MeshGroupView()
        {
            InitializeComponent();
        }
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
