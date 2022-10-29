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

            DiffuseColor = new(material, rowNumber * 16);
            SpecularColor = new(material, rowNumber * 16 + 4);
            EmissiveColor = new(material, rowNumber * 16 + 8);


            //DiffuseColor = new(material.ColorSetData[rowNumber * 16 + 0], material.ColorSetData[rowNumber * 16 + 1], material.ColorSetData[rowNumber * 16 + 2]);
            //SpecularColor = new(currRange[4], currRange[5], currRange[6]);
            //EmissiveColor = new(currRange[8], currRange[9], currRange[10]);

            EditorViewModel = new ColorSetRowEditorViewModel(this, material, stainingTemplateFile);
        }

        public void CopyRow(ColorSetRowViewModel other)
        {
            DiffuseColor.Copy(other.DiffuseColor);
            EmissiveColor.Copy(other.EmissiveColor);
            SpecularColor.Copy(other.SpecularColor);
        }

        public ColorSetRowEditorViewModel EditorViewModel { get; }
        public ColorViewModel DiffuseColor { get; }
        public ColorViewModel EmissiveColor { get; }
        public ColorViewModel SpecularColor { get; }

        public int DisplayedRowNumber { get { return RowNumber + 1; } }

        int _rowNumber;
        public int RowNumber
        {
            get { return _rowNumber; }
            set { _rowNumber = value; OnPropertyChanged(); }
        }

        /*
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
        */
    }
}
