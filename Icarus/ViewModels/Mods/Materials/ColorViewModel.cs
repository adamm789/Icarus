using Icarus.ViewModels.Util;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Half = SharpDX.Half;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorViewModel : NotifyPropertyChanged
    {
        public ColorViewModel(Half r, Half g, Half b)
        {
            R = r;
            G = g;
            B = b;
        }

        Half _r;
        public Half R
        {
            get { return _r; }
            set { _r = value; OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor)); }
        }
        Half _g;
        public Half G
        {
            get { return _g; }
            set { _g = value; OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor)); }
        }
        Half _b;
        public Half B
        {
            get { return _b; }
            set { _b = value; OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor)); }
        }
        public SolidColorBrush BrushColor
        {
            get
            {
                var color = Color.FromArgb(255, (byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
                return new SolidColorBrush(color);
            }
        }
    }
}
