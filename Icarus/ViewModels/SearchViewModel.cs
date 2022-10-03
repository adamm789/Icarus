using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace Icarus.ViewModels
{
    public class SearchViewModel : NotifyPropertyChanged
    {
        // TODO: On Enter click expand the entries?

        const int minNumBeforeExpansion = 100;
        readonly ItemListService _itemDatabaseService;
        readonly ILogService _logService;
        private PropertyChangedEventHandler eh;

        public SearchViewModel(ItemListService itemDatabaseService, ILogService logService)
        {
            _itemDatabaseService = itemDatabaseService;
            _logService = logService;

            if (_itemDatabaseService.Data == null)
            {
                eh = new(DataPropertySet);
                _itemDatabaseService.PropertyChanged += eh;
            }
            else
            {
                BuildList();
            }
        }
        ObservableCollection<ItemListViewModel> _itemList = new();
        public ObservableCollection<ItemListViewModel> ItemList
        {
            get { return _itemList; }
            set
            {
                _itemList = value;
                OnPropertyChanged();
            }
        }
        private async void DataPropertySet(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListService.Data) && sender is ItemListService service)
            {
                BuildList();
                service.PropertyChanged -= eh;
            }
        }

        public void BuildList()
        {
            // TODO: Further categorize indoor furniture?
            //ItemList = await Task.Run( () => {
            _logService.Information("Building item list view models.");
            var entries = _itemDatabaseService.GetAllItems();
            ObservableCollection<ItemListViewModel> items = new();

            foreach (var entry in entries)
            {
                var vm = new ItemListViewModel(entry.Key, entry.Value.Values.ToList());
                items.Add(vm);

                PropertyChangedEventHandler eh = new(ChildChanged);
                vm.PropertyChanged += eh;
            };

            var view = (CollectionView)CollectionViewSource.GetDefaultView(items);
            view.Filter = SearchFilter;

            foreach (var list in items)
            {
                view = (CollectionView)CollectionViewSource.GetDefaultView(list.Items);
                view.Filter = ChildSearchFilter;
            }
            //return items;

            ItemList = items;
            //});
        }

        private bool SearchFilter(object o)
        {
            var item = (ItemListViewModel)o;
            return item.NumMatches > 0;
        }

        private bool ChildSearchFilter(object o)
        {
            var item = ((ItemViewModel)o).Item;
            var searchText = ItemText;

            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            return item.IsMatch(searchText);
        }

        string _itemText;
        public string ItemText
        {
            get { return _itemText; }
            set { _itemText = value; OnPropertyChanged(); Search(); }
        }

        public IItem SelectedItem
        {
            get { return _itemDatabaseService.SelectedItem; }
            set { _itemDatabaseService.SelectedItem = value; OnPropertyChanged(); }
        }

        string _selectedItemMdl;
        public string SelectedItemMdl
        {
            get { return _selectedItemMdl; }
            set { _selectedItemMdl = value; OnPropertyChanged(); }
        }

        string _selectedItemMtrl;
        public string SelectedItemMtrl
        {
            get { return _selectedItemMtrl; }
            set { _selectedItemMtrl = value; OnPropertyChanged(); }
        }

        string _selectedItemName;
        public string SelectedItemName
        {
            get { return _selectedItemName; }
            set { _selectedItemName = value; OnPropertyChanged(); }
        }

        // TODO: Expand if there are only X amount of matching items?
        public void Search()
        {
            var matchesFound = 0;
            foreach (var list in ItemList)
            {
                var itemsList = (CollectionView)CollectionViewSource.GetDefaultView(list.Items);
                itemsList.Refresh();

                list.SetNumMatches(itemsList.Count);
                matchesFound += itemsList.Count;
            }
            var listView = (CollectionView)CollectionViewSource.GetDefaultView(ItemList);
            listView.Refresh();
            Expand(matchesFound);
        }

        public bool ShouldExpand(int num)
        {
            return num < minNumBeforeExpansion;
        }

        // TODO: Behavior: Search is empty, user expands a category, user searches, should the category stay expanded?
        public void Expand(int num)
        {
            bool val = ShouldExpand(num);

            foreach (var list in ItemList)
            {
                list.IsExpanded = val;
            }
        }

        public void ChildChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ItemListViewModel child && e.PropertyName == nameof(ItemListViewModel.SelectedItem))
            {
                SelectedItem = child.SelectedItem;

                if (SelectedItem != null)
                {
                    SelectedItemMdl = SelectedItem.GetMdlPath();
                    SelectedItemMtrl = SelectedItem.GetMtrlPath();
                    SelectedItemName = SelectedItem.Name;
                }
            }
        }
    }
}
