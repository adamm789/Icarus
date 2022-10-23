using Icarus.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Icarus.Views
{
    /// <summary>
    /// Interaction logic for WatermarkTextBox.xaml
    /// </summary>
    public partial class WatermarkTextBox : UserControl
    {
        public WatermarkTextBox()
        {
            InitializeComponent();
        }
        /*
        void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {

            if (myTextBox.Text == "")
            {
                // Create an ImageBrush.
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(@"TextBoxBackground.gif", UriKind.Relative)
                    );
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.Stretch = Stretch.None;
                // Use the brush to paint the button's background.
                myTextBox.Background = textImageBrush;
            }
            else
            {

                myTextBox.Background = null;
            }
        }
        */
        
        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WatermarkTextBox), new PropertyMetadata(null));

        
        public string WatermarkText
        {
            get { return (string)base.GetValue(WatermarkTextProperty); }
            set { base.SetValue(WatermarkTextProperty, value); }

        }

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), typeof(WatermarkTextBox), new PropertyMetadata(null));
        
    }
}
