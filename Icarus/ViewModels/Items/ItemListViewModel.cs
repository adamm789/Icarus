﻿using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;

namespace Icarus.ViewModels.Items
{
    // TODO: Add one more parent category
    // i.e. "Gear" -> list of equip slot -> list of items
    public class ItemListViewModel : ViewModelBase
    {
        const int minNumBeforeExpansion = 100;
        readonly IItemListService _itemListService;
        readonly PropertyChangedEventHandler eh;

        public ItemListViewModel(IItemListService itemListService, ILogService logService) : base(logService)
        {
            _itemListService = itemListService;
            _logService = logService;
            if (!_itemListService.IsLoaded)
            {
                eh = new(ItemListServiceInitialized);
                _itemListService.PropertyChanged += eh;
            }
            else
            {
                BuildList();
            }
        }

        ObservableCollection<ItemTreeNodeViewModel> _itemList = new();
        public ObservableCollection<ItemTreeNodeViewModel> ItemList
        {
            get { return _itemList; }
            set { _itemList = value; OnPropertyChanged(); }
        }

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
                FilterSearch(_searchText);
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

        private void ItemListServiceInitialized(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IItemListService.IsLoaded) && sender is IItemListService service)
            {
                if (service.IsLoaded)
                {
                    BuildList();
                    service.PropertyChanged -= eh;
                }
            }
        }

        private void BuildList()
        {
            var root = _itemListService.CreateList();
            foreach (var child in root.Children)
            {
                var vm = new ItemTreeNodeViewModel(child);
                var eh = new PropertyChangedEventHandler(OnChildChanged);
                vm.PropertyChanged += eh;
                ItemList.Add(vm);
            }
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            view.Filter = ChildSearchFilter;
            /*
            _logService.Information("Building item list");
            //var entries = _itemListService.GetAllItems2();
            var entries = _itemListService.GetAllItems2();

            foreach (var entry in entries)
            {
                var vm = new TreeItemViewModel(entry.Key, entry.Value);
                var eh = new PropertyChangedEventHandler(OnChildChanged);
                vm.PropertyChanged += eh;

                ItemList.Add(vm);
            }
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            view.Filter = ChildSearchFilter;
            */
        }

        public List<IItem> Search(string str, bool exactMatch = false) => _itemListService.Search(str, exactMatch);

        private void FilterSearch(string term)
        {
            var numMatches = 0;

            foreach (var item in ItemList)
            {
                var numVisible = item.Search(term);
                numMatches += numVisible;
            }
            if (numMatches == 1)
            {
                // TODO: Time-limit this so it's not searching on every key-stroke
                _logService.Debug($"Searching for {SearchText}");
                var results = _itemListService.Search(SearchText);
                if (results.Count == 1 && SelectedItem != results[0])
                {
                    SelectedItem = results[0];
                }
            }
            if (numMatches == 0)
            {
                if (_itemListService.TrySearch(SearchText))
                {
                    SelectedItem = null;
                    CompletePath = SearchText;
                }
                else
                {
                    CompletePath = null;
                }
            }
            else
            {
                CompletePath = null;
            }

            foreach (var item in ItemList)
            {
                item.ExpandAll(ShouldExpand(term, numMatches));
            }

            var items = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            items.Refresh();
        }

        private bool ShouldExpand(string search, int numMatches)
        {
            return !String.IsNullOrWhiteSpace(search) && numMatches < minNumBeforeExpansion;
        }

        private bool ChildSearchFilter(object o)
        {
            var vm = o as ItemTreeNodeViewModel;
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return true;
            }
            return vm.HasMatch(_searchText) > 0;
        }

        private void OnChildChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemTreeNodeViewModel.SelectedItem) && sender is ItemTreeNodeViewModel vm)
            {
                if (vm.SelectedItem != null)
                {
                    SelectedItem = vm.SelectedItem;
                }
            }
        }
    }
}