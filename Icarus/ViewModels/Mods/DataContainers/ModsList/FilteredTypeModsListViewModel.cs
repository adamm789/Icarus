using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Icarus.ViewModels.Mods.DataContainers.ModsList
{
    public class FilteredTypeModsListViewModel<T> : ViewModelBase where T : ModViewModel
    {
        FilteredModsListViewModel? _parent;
        protected string _type = "";
        ObservableCollection<ModViewModel> SimpleModsList;

        public Type ModType;

        public int TotalNum = 0;
        public int NumSelected = 0;

        public FilteredTypeModsListViewModel(FilteredModsListViewModel parent, string header, ILogService logService) : base(logService)
        {
            _parent = parent;
            _type = header;
            _parent.PropertyChanged += new(OnParentPropertyChanged);

            SimpleModsList = parent.SimpleModsList;
            SimpleModsList.CollectionChanged += new(OnCollectionChanged);

            UpdateList();
            ModType = typeof(T);
        }

        public bool AllSelected => TotalNum == NumSelected;

        private void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_parent.SearchTerm))
            {
                UpdateList();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateList();
        }

        public Func<ModViewModel, bool>? FilterFunction;
        public Func<FilteredTypeModsListViewModel<T>, string>? HeaderFunction;

        public void UpdateList()
        {
            TotalNum = ModsList.Cast<ModViewModel>().Count();
            if (FilterFunction == null)
            {
                NumSelected = TotalNum;
            }
            else
            {
                NumSelected = ModsList.Cast<ModViewModel>().Where(FilterFunction).Count();
            }
            UpdateHeader();
        }

        public void UpdateHeader()
        {
            TotalNum = ModsList.Cast<ModViewModel>().Count();

            if (FilterFunction == null)
            {
                NumSelected = TotalNum;
                Header = $"{_type} ({TotalNum})";
            }
            else
            {
                if (typeof(T) == typeof(ModViewModel))
                {

                }
                NumSelected = ModsList.Cast<ModViewModel>().Where(FilterFunction).Count();
                Header = $"{_type} ({NumSelected}/{TotalNum})";
            }
            OnPropertyChanged(nameof(ModsList));
        }

        string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; OnPropertyChanged(); }
        }

        ICollectionView _modsList;
        public ICollectionView ModsList
        {
            get
            {
                _modsList = new CollectionViewSource { Source = SimpleModsList }.View;
                _modsList.Filter = m => SearchFilterFunction(m);
                return _modsList;
            }
        }

        private bool SearchFilterFunction(object o)
        {
            if (_parent != null)
            {
                return o is T && _parent.SearchFilterFunction(o);
            }
            else
            {
                return o is T;
            }
        }
    }
}
