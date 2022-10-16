using Icarus.Mods;
using Icarus.ViewModels.Util;
using Lumina.Data.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using Color = SharpDX.Color;
using Half = SharpDX.Half;
using WindowsColor = System.Windows.Media.Color;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorSetRowViewModel : NotifyPropertyChanged
    {
        List<Half> _colorsetData;
        public string ToolTip { get; }
        public ColorSetRowViewModel(int rowNumber, MaterialMod material, StainingTemplateFile stainingTemplateFile)
        {
            RowNumber = rowNumber;
            _colorsetData = material.ColorSetData;

            var currRange = _colorsetData.GetRange(rowNumber * 16, 16);

            Diffuse = new(GetColor(GetColorSetDataRange(currRange, 0)));
            Specular = new(GetColor(GetColorSetDataRange(currRange, 4)));
            Emissive = new(GetColor(GetColorSetDataRange(currRange, 8)));

            DyeDataViewModel = new ColorSetRowDyeDataViewModel(rowNumber, material, stainingTemplateFile);
        }

        public ColorSetRowDyeDataViewModel DyeDataViewModel { get; }

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
            return new Color(values[0], values[1], values[2], 255);
        }

        public WindowsColor GetColor(Color c)
        {
            return WindowsColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        SolidColorBrush _diffuse;
        public SolidColorBrush Diffuse
        {
            get { return _diffuse; }
            set {
                _diffuse = value;
                OnPropertyChanged();
                _colorsetData[(RowNumber * 16) + 0] = _diffuse.Color.R;
                _colorsetData[(RowNumber * 16) + 1] = _diffuse.Color.G;
                _colorsetData[(RowNumber * 16) + 2] = _diffuse.Color.B;
            }
        }
        SolidColorBrush _specular;
        public SolidColorBrush Specular
        {
            get { return _specular; }
            set {
                _specular = value; 
                OnPropertyChanged();
                _colorsetData[(RowNumber * 16) + 4] = _specular.Color.R;
                _colorsetData[(RowNumber * 16) + 5] = _specular.Color.G;
                _colorsetData[(RowNumber * 16) + 6] = _specular.Color.B;
            }
        }
        SolidColorBrush _emissive;
        public SolidColorBrush Emissive
        {
            get { return _emissive; }
            set { 
                _emissive = value; 
                OnPropertyChanged();
                _colorsetData[(RowNumber * 16) + 8] = _emissive.Color.R;
                _colorsetData[(RowNumber * 16) + 9] = _emissive.Color.G;
                _colorsetData[(RowNumber * 16) + 10] = _emissive.Color.B;
            }
        }

        int _rowNumber;
        public int RowNumber
        {
            get { return _rowNumber; }
            set { _rowNumber = value; OnPropertyChanged(); }
        }
    }
}
