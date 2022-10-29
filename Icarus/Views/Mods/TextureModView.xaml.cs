using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Mods
{
    /// <summary>
    /// Interaction logic for TextureView.xaml
    /// </summary>
    public partial class TextureModView : UserControl
    {
        public TextureModView()
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
