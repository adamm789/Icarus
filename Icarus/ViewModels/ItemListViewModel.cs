using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Icarus.ViewModels
{
    public class ItemListViewModel : NotifyPropertyChanged
    {
        public ItemListViewModel(string header, List<IItem> items)
        {
            Header = header;
            LoadItems(items);
            NumMatches = Items.Count;
        }

        bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        ObservableCollection<ItemViewModel> _items = new();
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(); }
        }

        string _header = "";
        public string Header
        {
            get { return _header; }
            set { _header = value; OnPropertyChanged(); }
        }

        public void LoadItems(List<IItem> items)
        {
            foreach (var i in items)
            {
                var vm = new ItemViewModel(i);
                Items.Add(vm);

                var eh = new PropertyChangedEventHandler(OnChildChanged);
                vm.PropertyChanged += eh;
            }
        }

        public void SetNumMatches(int num)
        {
            NumMatches = num;
        }

        public void OnChildChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ItemViewModel vm)
            {
                if (e.PropertyName == nameof(ItemViewModel.IsSelected))
                {
                    SelectedItem = vm.Item;
                }
            }
        }

        int _numMatches;
        public int NumMatches
        {
            get { return _numMatches; }
            set { _numMatches = value; OnPropertyChanged(); }
        }

        IItem _selectedItem;
        public IItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }
    }
}