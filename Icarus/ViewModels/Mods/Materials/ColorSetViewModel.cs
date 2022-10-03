using Icarus.ViewModels.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpDXColor = SharpDX.Color;
using WindowsColor = System.Windows.Media.Color;
using Half = SharpDX.Half;

namespace Icarus.ViewModels.Mods
{
    public class ColorSetRowViewModel : NotifyPropertyChanged
    {
        public ColorSetRowViewModel(List<SharpDXColor> values)
        {
            if (values.Count != 4)
            {
                throw new ArgumentException("Could not create ColorSetRow. Length was not four.");
            }

            Diffuse = new(GetColor(values[0]));
            Specular = new(GetColor(values[1]));
            Emissive = new(GetColor(values[2]));
            Red = new(GetColor(values[3]));
        }

        public WindowsColor GetColor(SharpDXColor c)
        {
            return WindowsColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        SolidColorBrush _diffuse;
        public SolidColorBrush Diffuse
        {
            get { return _diffuse; }
            set { _diffuse = value; OnPropertyChanged(); }
        }
        SolidColorBrush _specular;
        public SolidColorBrush Specular
        {
            get { return _specular; }
            set { _specular = value; OnPropertyChanged(); }
        }
        SolidColorBrush _emissive;
        public SolidColorBrush Emissive
        {
            get { return _emissive; }
            set { _emissive = value; OnPropertyChanged(); }
        }
        SolidColorBrush _red;
        public SolidColorBrush Red
        {
            get { return _red; }
            set { _red = value; OnPropertyChanged(); }
        }
    }
}
