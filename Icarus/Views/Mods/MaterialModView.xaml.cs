using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Mods
{
    /// <summary>
    /// Interaction logic for MaterialModView.xaml
    /// </summary>

    public partial class MaterialModView : UserControl
    {
        public MaterialModView()
        {
            InitializeComponent();
        }
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled && !this.IsMouseCaptureWithin)
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
