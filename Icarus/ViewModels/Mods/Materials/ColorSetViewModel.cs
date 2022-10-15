﻿using Icarus.Mods;
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

        ColorSetRowViewModel _selectedRow;
        public ColorSetRowViewModel SelectedRow
        {
            get { return _selectedRow; }
            set {
                _selectedRow = value;
                OnPropertyChanged();
                DisplayedDyeData = value.DyeDataViewModel;
            }
        }

        DelegateCommand _onSelected;
        public DelegateCommand OnSelected
        {
            get { return _onSelected ??= new DelegateCommand(_ => SelectedIndex=0); }
        }


        ColorSetRowDyeDataViewModel? _displayedDyeData = null;
        public ColorSetRowDyeDataViewModel? DisplayedDyeData
        {
            get { return _displayedDyeData; }
            set { _displayedDyeData = value; OnPropertyChanged(); }
        }

        ObservableCollection<ColorSetRowViewModel> _colorSetRows = new();
        public ObservableCollection<ColorSetRowViewModel> ColorSetRows
        {
            get { return _colorSetRows; }
            set { _colorSetRows = value; OnPropertyChanged(); }
        }
    }
}
