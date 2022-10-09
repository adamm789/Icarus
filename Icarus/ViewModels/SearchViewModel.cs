using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Icarus.ViewModels
{
    //public class SearchViewModel : NotifyPropertyChanged
   // {
        // TODO: On Enter click expand the entries?
        /*
        const int minNumBeforeExpansion = 100;
        readonly ItemListService _itemListService;
        readonly ILogService _logService;
        private PropertyChangedEventHandler eh;

        readonly ItemListViewModelX _itemListViewModel;

        public SearchViewModel(ItemListViewModelX itemListViewModel, ILogService logService)
        {
            _itemListViewModel = itemListViewModel;
            _logService = logService;

            var eh = new PropertyChangedEventHandler(OnItemSelected);
            _itemListViewModel.PropertyChanged += eh;
        }

        private void OnItemSelected(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListViewModelX.SelectedItem) && sender is ItemListViewModelX vm) {
                if (vm.SelectedItem != null)
                {
                    SelectedItem = vm.SelectedItem;
                }
            }
        }

        public SearchViewModel(ItemListService itemListService, ILogService logService)
        {
            _itemListService = itemListService;
            _logService = logService;
            */

            /*
            if (_itemListService.Data == null)
            {
                eh = new(DataPropertySet);
                _itemListService.PropertyChanged += eh;
            }
            else
            {
                BuildList();
            }
            */
        //}
        /*
        ObservableCollection<ItemListViewModel> _itemList = new();
        public ObservableCollection<ItemListViewModel> ItemList
        {
            get { return _itemList; }
            set { _itemList = value; OnPropertyChanged(); }
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
            var entries = _itemListService.GetAllItems();
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
            var searchText = SearchText;

            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            return item.IsMatch(searchText);
        }
        */
        /*
        string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged(); _itemListViewModel.Search(_searchText); }
        }


        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }
        */
        /*
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
        */

        /*
        string? _completePath;
        public string? CompletePath
        {
            get { return _completePath; }
            set { _completePath = value; OnPropertyChanged(); }
        }
        */
        /*
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

            if (matchesFound == 0)
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

        public bool ShouldExpand(int num)
        {
            return num < minNumBeforeExpansion;
        }

        public void ChildChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ItemListViewModel child && e.PropertyName == nameof(ItemListViewModel.SelectedItem))
            {
                SelectedItem = child.SelectedItem;

                if (SelectedItem != null)
                {
                    SelectedItemMdl = SelectedItem.GetMdlFileName();
                    SelectedItemMtrl = SelectedItem.GetMtrlFileName();
                    SelectedItemName = SelectedItem.Name;
                }
            }
        }
        */
    //}
}
