﻿using System.Windows;
using System.Windows.Input;

namespace Icarus.Views.Mods.Materials
{
    /// <summary>
    /// Interaction logic for MaterialEditorWindow.xaml
    /// </summary>
    public partial class ShaderInfoWindow : Window
    {
        public ShaderInfoWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            DataContext = this;

        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void window1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            grid1.Focus();
        }
    }
}
