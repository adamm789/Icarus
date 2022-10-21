using GongSolutions.Wpf.DragDrop;
using Icarus.ViewModels.Util;
using ItemDatabase;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace Icarus.ViewModels.Items
{
    public class TreeItemViewModel : NotifyPropertyChanged, IItemViewModel
    {
        const int minNumBeforeExpansion = 100;

        string _header;
        public IItem? Item { get; }
        public List<TreeItemViewModel> Children { get; } = new();

        public TreeItemViewModel(string header, IDictionary<string, IItem> values)
        {
            _header = header;

            Header = $"{_header} ({values.Count})";
            foreach (var kvp in values)
            {
                var vm = new TreeItemViewModel(kvp.Value);
                var eh = new PropertyChangedEventHandler(OnChildSelected);
                vm.PropertyChanged += eh;

                Children.Add(vm);
            }

            var view = (CollectionView)CollectionViewSource.GetDefaultView(Children);
            view.Filter = ChildSearchFilter;
        }

        public TreeItemViewModel(IItem item)
        {
            Item = item;
            _header = item.Name;
            Header = item.Name;

            Tooltip = item.Name;
            if (item is IGear gear)
            {
                Tooltip += $" ({gear.VariantCode})";
            }
        }


        string _searchText = "";
        private bool ChildSearchFilter(object o)
        {
            var vm = o as TreeItemViewModel;
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return true;
            }
            return vm.HasMatch(_searchText) > 0;
        }

        private void OnChildSelected(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem) && sender is TreeItemViewModel vm)
            {
                SelectedItem = vm.SelectedItem;
            }
        }

        public void SetNumVisible(int i)
        {
            if (Item != null)
            {
                return;
            }
            Header = $"{_header} ({i})";

            if (i == 0)
            {
                IsExpanded = false;
            }
            else
            {
                IsExpanded = true;
            }
        }

        public int Search(string term)
        {
            _searchText = term;
            if (Children == null)
            {
                return 0;
            }
            var items = (CollectionView)CollectionViewSource.GetDefaultView(Children);
            items.Filter = ChildSearchFilter;

            var numMatches =  items.Count;

            if (Item == null)
            {
                Header = $"{_header} ({numMatches})";
            }
            return numMatches;
        }

        public int HasMatch(string name)
        {
            if (Item == null)
            {
                var count = 0;
                foreach (var child in Children)
                {
                    count += child.HasMatch(name);
                }
                return count;
            }
            else
            {
                return Item.IsMatch(name) ? 1 : 0;
            }
        }

        public string Header { get; set; }

        DelegateCommand _isSelectedCommand;
        public DelegateCommand IsSelectedCommand
        {
            get { return _isSelectedCommand ??= new DelegateCommand(_ => OnSelect()); }
        }

        public void OnSelect()
        {
            if (Item != null)
            {
                SelectedItem = Item;
            }
            else
            {
                IsExpanded = !IsExpanded;
            }
        }

        int _numMatches = 0;
        public int NumMatches
        {
            get { return _numMatches; }
        }

        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }
        public bool HasItem => Item != null;

        bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        public string Tooltip { get; }
    }
}
