using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.Primitives;

namespace Icarus.ViewModels.Items
{
    public class ItemTreeNodeViewModel : NotifyPropertyChanged
    {
        string _header;
        public string Header { get; set; }
        public IItem? Item;
        public int NumLeaves;
        public ObservableCollection<ItemTreeNodeViewModel> Children { get; } = new();

        public ItemTreeNodeViewModel(ITreeNode<(string Header, IItem? Item)> node)
        {
            _header = node.Value.Header;
            Item = node.Value.Item;

            if (node.Children.Count == 0)
            {
                NumLeaves = 1;
                Header = _header;
            }
            else
            {
                NumLeaves = 0;
                foreach (var child in node.Children)
                {
                    var vm = new ItemTreeNodeViewModel(child);
                    NumLeaves += vm.NumLeaves;
                    vm.PropertyChanged += new(OnChildSelected);

                    Children.Add(vm);
                }
                Header = FormatHeader(_header, NumLeaves);

            }
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Children);
            view.Filter = ChildSearchFilter;
        }

        string _searchText = "";
        private bool ChildSearchFilter(object o)
        {
            var vm = o as ItemTreeNodeViewModel;
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return true;
            }
            return vm.HasMatch(_searchText) > 0;
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

        private void OnChildSelected(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem) && sender is ItemTreeNodeViewModel vm)
            {
                SelectedItem = vm.SelectedItem;
            }
        }

        public void ExpandAll(bool value)
        {
            IsExpanded = value;
            foreach (var child in Children)
            {
                child.ExpandAll(value);
            }
        }

        private static string FormatHeader(string header, int value)
        {
            return $"{header} ({value})";
        }

        DelegateCommand _isSelectedCommand;
        public DelegateCommand IsSelectedCommand
        {
            get { return _isSelectedCommand ??= new DelegateCommand(_ => OnSelect()); }
        }

        public int Search(string term)
        {
            _searchText = term;

            if (Children.Count == 0)
            {
                if (ChildSearchFilter(this))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            var items = (CollectionView)CollectionViewSource.GetDefaultView(Children);
            items.Filter = ChildSearchFilter;
            var childMatches = 0;

            foreach (var child in Children)
            {
                childMatches += child.Search(term);
            }

            if (Item == null)
            {
                Header = FormatHeader(_header, childMatches);
            }
            return childMatches;
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

        public int CompareTo(object? obj)
        {
            if (obj is ItemTreeNodeViewModel other)
            {
                return Header.CompareTo(other.Header);
            }
            throw new ArgumentException();
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
            set
            {
                _isExpanded = value; OnPropertyChanged();
            }
        }

        public string Tooltip { get; } = "";
    }
}
