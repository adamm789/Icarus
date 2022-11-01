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
        public ColorSetRowViewModel(int rowNumber, MaterialMod material, StainingTemplateFile stainingTemplateFile)
        {
            _rowNumber = rowNumber;

            DiffuseColor = new(material, rowNumber * 16);
            SpecularColor = new(material, rowNumber * 16 + 4);
            EmissiveColor = new(material, rowNumber * 16 + 8);

            EditorViewModel = new ColorSetRowEditorViewModel(this, material, stainingTemplateFile);

            ToolTip = $"Row {DisplayedRowNumber}";
        }

        public void CopyRow(ColorSetRowViewModel other)
        {
            DiffuseColor.Copy(other.DiffuseColor);
            EmissiveColor.Copy(other.EmissiveColor);
            SpecularColor.Copy(other.SpecularColor);
        }

        public string ToolTip { get; }
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
    }
}
