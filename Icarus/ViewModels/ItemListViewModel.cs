using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;

namespace Icarus.ViewModels
{
    public class ItemListViewModel : NotifyPropertyChanged
    {
        const int minNumBeforeExpansion = 100;
        readonly ItemListService _itemListService;
        readonly ILogService _logService;
        readonly PropertyChangedEventHandler eh;

        ObservableCollection<TreeItemViewModel> _itemList = new();
        public ObservableCollection<TreeItemViewModel> ItemList
        {
            get { return _itemList; }
            set { _itemList = value; OnPropertyChanged(); }
        }

        IItem _selectedItem;
        public IItem? SelectedItem
        {
            get { return _itemListService.SelectedItem; }
            set { _itemListService.SelectedItem = value; OnPropertyChanged(); }
        }

        string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                Search(_searchText);
            }
        }

        /// <summary>
        /// Contains the SearchText if it is a full, valid ffxiv in-game path
        /// and not in the ItemList
        /// Null, otherwise
        /// </summary>
        string? _completePath;
        public string? CompletePath
        {
            get { return _completePath; }
            set { _completePath = value; OnPropertyChanged(); }
        }

        public ItemListViewModel(ItemListService itemListService, ILogService logService)
        {
            _itemListService = itemListService;
            _logService = logService;
            if (_itemListService.Data == null)
            {
                eh = new(ItemListServiceInitialized);
                _itemListService.PropertyChanged += eh;
            }
            else
            {
                BuildList();
            }
        }

        private void ItemListServiceInitialized(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListService.Data) && sender is ItemListService service)
            {
                BuildList();
                service.PropertyChanged -= eh;
            }
        }

        private void BuildList()
        {
            _logService.Information("Build item list");
            var entries = _itemListService.GetAllItems();

            foreach (var entry in entries)
            {
                var vm = new TreeItemViewModel(entry.Key, entry.Value);
                var eh = new PropertyChangedEventHandler(OnChildChanged);
                vm.PropertyChanged += eh;

                ItemList.Add(vm);
            }

            var view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            view.Filter = ChildSearchFilter;
        }

        public void Search(string term)
        {
            /*
            var matchesFound = 0;
            _searchText = term;

            var items = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            items.Refresh();

            if (items.Count < minNumBeforeExpansion)
            {
                Expand();
            }
            */
            var numVisible = 0;

            foreach (var item in ItemList)
            {
                var itemVisible = item.Search(term);
                item.SetNumVisible(itemVisible);
                numVisible += itemVisible;
            }

            if (String.IsNullOrWhiteSpace(term))
            {
                foreach (var item in ItemList)
                {
                    item.IsExpanded = false;
                }
            }

            if (numVisible == 0)
            {
                if (_itemListService.TrySearch(SearchText))
                {
                    CompletePath = SearchText;
                    SelectedItem = null;
                }
            }
            else
            {
                CompletePath = null;
            }

            if (numVisible < minNumBeforeExpansion)
            {
                foreach (var item in ItemList)
                {
                    item.IsExpanded = true;
                }
            }
            else
            {
                foreach (var item in ItemList)
                {
                    item.IsExpanded = false;
                }
            }

            var items = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            items.Refresh();
        }

        private bool ChildSearchFilter(object o)
        {
            var vm = o as TreeItemViewModel;
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return true;
            }
            return vm.HasMatch(_searchText) > 0;
        }

        private void OnChildChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName == nameof(TreeItemViewModel.IsSelected) && sender is TreeItemViewModel vm)
            {
                SelectedItem = vm.Item;
            }
            */
            if (e.PropertyName == nameof(TreeItemViewModel.SelectedItem) && sender is TreeItemViewModel vm)
            {
                if (vm.SelectedItem != null)
                {
                    SelectedItem = vm.SelectedItem;
                }
            }
        }
    }
    /*
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
    */
}