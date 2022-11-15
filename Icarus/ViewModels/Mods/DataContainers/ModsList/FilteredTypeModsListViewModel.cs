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
    public abstract class FilteredTypeModsListViewModel<T> : ViewModelBase where T : ModViewModel
    {
        FilteredModsListViewModel? _parent;
        protected string _type = "";
        ObservableCollection<ModViewModel> SimpleModsList;

        public FilteredTypeModsListViewModel(FilteredModsListViewModel parent, string header, ILogService logService) : base(logService)
        {
            _parent = parent;
            _type = header;
            _parent.PropertyChanged += new(OnParentPropertyChanged);

            SimpleModsList = parent.SimpleModsList;


            SimpleModsList.CollectionChanged += new(OnCollectionChanged);
        }

        private void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_parent.SearchTerm))
            {
                UpdateHeaders();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateHeaders();
        }

        private void UpdateHeaders()
        {
            Header = $"{_type} ({ModsList.Cast<ModViewModel>().Count()})";
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
                _modsList.Filter = m => FilterFunction(m);
                return _modsList;
            }
        }

        private bool FilterFunction(object o)
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
