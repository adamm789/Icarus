using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Mods
{
    /// <summary>
    /// Interaction logic for ModelModView.xaml
    /// </summary>
    public partial class ModelModView : UserControl
    {
        public ModelModView()
        {
            InitializeComponent();
        }

        void OnStretchedHeaderTemplateLoaded(object sender, RoutedEventArgs e)
        {
            // Thank you: https://joshsmithonwpf.files.wordpress.com/2007/02/stretchedexpanderheader_code.PNG
            if (sender is Border rootElem)
            {
                if (rootElem.TemplatedParent is ContentPresenter contentPres)
                {
                    contentPres.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
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
