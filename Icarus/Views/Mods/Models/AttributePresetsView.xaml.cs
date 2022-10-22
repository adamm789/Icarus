using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Models
{
    /// <summary>
    /// Interaction logic for BodyAttributesView.xaml
    /// </summary>
    public partial class AttributePresetsView : UserControl
    {
        public AttributePresetsView()
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
