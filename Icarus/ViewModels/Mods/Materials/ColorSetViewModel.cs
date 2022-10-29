using Icarus.Mods;
using Icarus.ViewModels.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using Half = SharpDX.Half;

namespace Icarus.ViewModels.Mods.Materials
{
    // TODO: Figure out where to put all of this stuff
    // DyeData, ColorSet Rows, the checkboxes
    public class ColorSetViewModel : NotifyPropertyChanged
    {
        public ColorSetViewModel(MaterialMod material, StainingTemplateFile stainingTemplateFile)
        {
            for (var i = 0; i < 16; i++)
            {
                var row = new ColorSetRowViewModel(i, material, stainingTemplateFile);
                ColorSetRows.Add(row);
            }
            SelectedRow = ColorSetRows[0];
        }

        int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                SelectedRow = ColorSetRows[value];
            }
        }

        ColorSetRowEditorViewModel _rowEditorViewModel;

        ColorSetRowViewModel _selectedRow;
        public ColorSetRowViewModel SelectedRow
        {
            get { return _selectedRow; }
            set {
                _selectedRow = value;
                OnPropertyChanged();
                DisplayedRow = value.EditorViewModel;
            }
        }

        ColorSetRowEditorViewModel? _displayedRow = null;
        public ColorSetRowEditorViewModel? DisplayedRow
        {
            get { return _displayedRow; }
            set { _displayedRow = value; OnPropertyChanged(); }
        }

        ObservableCollection<ColorSetRowViewModel> _colorSetRows = new();
        public ObservableCollection<ColorSetRowViewModel> ColorSetRows
        {
            get { return _colorSetRows; }
            set { _colorSetRows = value; OnPropertyChanged(); }
        }
    }
}
