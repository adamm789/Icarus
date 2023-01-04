using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.UI
{
    public class ScrollListView : ListView
    {
        public ScrollListView() : base()
        {
            PreviewMouseWheel += HandlePreviewMouseWheel;
        }

        public void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                try
                {
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                    eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                    eventArg.Source = sender;
                    var parent = ((Control)sender).Parent as UIElement;
                    if (parent != null)
                    {
                        e.Handled = true;

                        parent.RaiseEvent(eventArg);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception in ScrollListView.HandlePreviewMouseWheel", ex);
                }
            }
        }
    }
}
