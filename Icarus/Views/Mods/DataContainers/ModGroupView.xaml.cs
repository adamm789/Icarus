﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Icarus.Views.Mods.DataContainers
{
    /// <summary>
    /// Interaction logic for ModGroupView.xaml
    /// </summary>
    public partial class ModGroupView : UserControl
    {
        public ModGroupView()
        {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
