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
            //DiffuseColor = GetColor(GetColorSetDataRange(currRange, 0));

            DiffuseColor = new(currRange[0], currRange[1], currRange[2]);

            Diffuse = new SolidColorBrush(GetColor(GetColorSetDataRange(currRange, 0)));
            Specular = new SolidColorBrush(GetColor(GetColorSetDataRange(currRange, 4)));
            Emissive = new SolidColorBrush(GetColor(GetColorSetDataRange(currRange, 8)));

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
            return new Color((byte)Math.Round(values[0]*255), 
                (byte)Math.Round(values[1]*255),
                (byte)Math.Round(values[2]*255), (byte)255);
        }

        public WindowsColor GetColor(Color c)
        {
            return WindowsColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        public ColorViewModel DiffuseColor { get; }

        // TODO: ColorSetRow editor
        SolidColorBrush _diffuse;
        public SolidColorBrush Diffuse
        {
            get { return _diffuse; }
            set {
                _diffuse = value;
                OnPropertyChanged();
                
                // TODO: This changes the color slightly (less than before, but... it does)
                // DiffuseR, DiffuseG, DiffuseB? And have the display color relatively separated from these values?
                _colorsetData[(RowNumber * 16) + 0] = new Half(_diffuse.Color.R/255f);
                _colorsetData[(RowNumber * 16) + 1] = new Half(_diffuse.Color.G/255f);
                _colorsetData[(RowNumber * 16) + 2] = new Half(_diffuse.Color.B/255f);
            }
        }
        SolidColorBrush _specular;
        public SolidColorBrush Specular
        {
            get { return _specular; }
            set {
                _specular = value; 
                OnPropertyChanged();
                _colorsetData[(RowNumber * 16) + 4] = new Half(_specular.Color.R/255f);
                _colorsetData[(RowNumber * 16) + 5] = new Half(_specular.Color.G/255f);
                _colorsetData[(RowNumber * 16) + 6] = new Half(_specular.Color.B/255f);
            }
        }
        SolidColorBrush _emissive;
        public SolidColorBrush Emissive
        {
            get { return _emissive; }
            set { 
                _emissive = value; 
                OnPropertyChanged();
                _colorsetData[(RowNumber * 16) + 8] = new Half(_emissive.Color.R/255f);
                _colorsetData[(RowNumber * 16) + 9] = new Half(_emissive.Color.G/255f);
                _colorsetData[(RowNumber * 16) + 10] = new Half(_emissive.Color.B/255f);
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
