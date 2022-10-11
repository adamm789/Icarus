using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Color = SharpDX.Color;
using Half = SharpDX.Half;
using WindowsColor = System.Windows.Media.Color;

namespace Icarus.ViewModels.Mods
{
    public class ColorSetRowViewModel : NotifyPropertyChanged
    {
        List<Half> _colorsetData;
        public ColorSetRowViewModel(int rowNumber, List<Half> values)
        {
            _colorsetData = values;
            RowNumber = rowNumber;
            Diffuse = new(GetColor(GetColorSetDataRange(values, 0)));
            Specular = new(GetColor(GetColorSetDataRange(values, 4)));
            Emissive = new(GetColor(GetColorSetDataRange(values, 8)));
            Red = new(GetColor(GetColorSetDataRange(values, 12)));
        }
        public ColorSetRowViewModel(List<Color> values)
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

        private Color GetColorSetDataRange(List<Half> values, int index)
        {
            return ListToColor(values.GetRange(index, 4));
        }

        private Color ListToColor(List<Half> values)
        {
            if (values.Count != 4)
            {
                throw new ArgumentException("List was not of length four.");
            }
            return new Color(values[0], values[1], values[2], values[3]);
        }

        public List<Half> GetList()
        {
            return _colorsetData;
        }

        public WindowsColor GetColor(Color c)
        {
            return WindowsColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        // TODO: Get an edited Colorset row
        public List<Color> GetRow()
        {
            var retVal = new List<Color>();
            //retVal.Add(new SharpDXColor(Diffuse.Color));

            return retVal;
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
        int _rowNumber;
        public int RowNumber
        {
            get { return _rowNumber; }
            set { _rowNumber = value; OnPropertyChanged(); }
        }
    }
}
