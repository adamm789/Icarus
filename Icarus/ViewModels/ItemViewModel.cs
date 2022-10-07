using ItemDatabase.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Windows;
using System.ComponentModel;
using GongSolutions.Wpf.DragDrop;

namespace Icarus.ViewModels
{
    public class ItemViewModel : NotifyPropertyChanged
    {
        public IItem Item;
        public ItemViewModel(IItem item)
        {
            Item = item;
        }

        public string Name
        {
            get { return Item.Name; }
        }

        DelegateCommand _isSelectedCommand;
        public DelegateCommand IsSelectedCommand
        {
            get { return _isSelectedCommand ??= new DelegateCommand(o => IsSelected = true); }
        }

        bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); }
        }

        bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged(); }
        }
    }
}
