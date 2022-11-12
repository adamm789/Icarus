using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using ItemDatabase;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class FilteredModsListViewModel : NotifyPropertyChanged
    {
        // TODO: Figure out how I want to handle searching within the mods list
        public ObservableCollection<ModViewModel> SimpleModsList { get; }

        Timer timer = new();
        // TODO:? Allow searching through mods list
        string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                Search();
                //timer.Stop();
                //timer.Start();
            }
        }

        int numCalled = 0;

        public FilteredModsListViewModel(ModsListViewModel modsListViewModel)
        {
            SimpleModsList = modsListViewModel.SimpleModsList;
            modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);
            modsListViewModel.PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            UpdateHeaders();

            /*
            var view = (CollectionView)CollectionViewSource.GetDefaultView(SimpleModsList);
            view.Filter += SearchFilter;

            timer.Tick += Timer_Tick;
            timer.Interval = 300;
            */
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var items = (CollectionView)CollectionViewSource.GetDefaultView(SimpleModsList);
            items.Refresh();
           
            UpdateHeaders();
            //OnPropertyChanged(nameof(SimpleModsList));
            OnPropertyChanged(nameof(AllMods));
            OnPropertyChanged(nameof(ModelMods));
            OnPropertyChanged(nameof(ReadOnlyMods));
            OnPropertyChanged(nameof(MaterialMods));
            OnPropertyChanged(nameof(TextureMods));
            OnPropertyChanged(nameof(MetadataMods));
            timer.Stop();
        }

        private void Search()
        {
            OnPropertyChanged(nameof(AllMods));
            OnPropertyChanged(nameof(ModelMods));
            OnPropertyChanged(nameof(ReadOnlyMods));
            OnPropertyChanged(nameof(MaterialMods));
            OnPropertyChanged(nameof(TextureMods));
            OnPropertyChanged(nameof(MetadataMods));
            UpdateHeaders();
        }

        private bool SearchFilter(object o)
        {
            if (string.IsNullOrEmpty(_searchTerm))
            {
                return true;
            }
            if (o is ModViewModel mvm)
            {
                return mvm.DisplayedHeader.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModsListViewModel.IsAdding) && sender is ModsListViewModel modsListViewModel && !modsListViewModel.IsAdding)
            {
                OnPropertyChanged(nameof(SimpleModsList));
                UpdateHeaders();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SimpleModsList));

            if (e.NewItems != null)
            {
                var newItems = e.NewItems.Cast<ModViewModel>();
                foreach (var item in newItems)
                {
                    item.PropertyChanged += new(OnExportStatusChanged);
                }
                UpdateHeaders();
            }
            if (e.OldItems != null)
            {
                UpdateHeaders();
            }
        }

        // TODO: Track some sort of "incompleteness"
        private void OnExportStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModViewModel mod && e.PropertyName == nameof(ModViewModel.CanExport))
            {
                UpdateHeaders();
                //IncompleteMods.Refresh();
            }
        }

        private void UpdateHeaders()
        {
            AllModsHeader = $"All ({((CollectionView)AllMods).Count})";
            ModelModsHeader = $"Models ({ModelMods.Cast<ModViewModel>().Count()})";
            MaterialModsHeader = $"Materials ({MaterialMods.Cast<ModViewModel>().Count()})";
            TextureModsHeader = $"Textures({TextureMods.Cast<ModViewModel>().Count()})";
            MetadataModsHeader = $"Metadata({MetadataMods.Cast<ModViewModel>().Count()})";
            ReadOnlyModsHeader = $"ReadOnly ({ReadOnlyMods.Cast<ModViewModel>().Count()})";
        }

        string _allModsHeader = "";
        public string AllModsHeader
        {
            get { return _allModsHeader; }
            set { _allModsHeader = value; OnPropertyChanged(); }
        }

        string _modelModsHeader = "";
        public string ModelModsHeader
        {
            get { return _modelModsHeader; }
            set { _modelModsHeader = value; OnPropertyChanged(); }
        }

        string _materialModsHeader = "";
        public string MaterialModsHeader
        {
            get { return _materialModsHeader; }
            set { _materialModsHeader = value; OnPropertyChanged(); }
        }

        string _readOnlyModsHeader = "";
        public string ReadOnlyModsHeader
        {
            get { return _readOnlyModsHeader; }
            set { _readOnlyModsHeader = value; OnPropertyChanged(); }
        }

        string _textureModsHeader = "";
        public string TextureModsHeader
        {
            get { return _textureModsHeader; }
            set { _textureModsHeader = value; OnPropertyChanged(); }
        }

        string _metadataModsHeader = "";
        public string MetadataModsHeader
        {
            get { return _metadataModsHeader; }
            set { _metadataModsHeader = value; OnPropertyChanged(); }
        }

        string _incompleteModsHeader = "";
        public string IncompleteModsHeader
        {
            get { return _incompleteModsHeader; }
            set { _incompleteModsHeader = value; OnPropertyChanged(); }
        }

        private bool FilterFunction<T>(object o) where T : ModViewModel
        {
            if (o is T mvm)
            {

                // TODO: More robust matching
                //return mvm.DisplayedHeader.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
                return mvm.HasMatch(SearchTerm);
            }
            else
            {
                return false;
            }
        }

        ICollectionView _allMods;
        public ICollectionView AllMods
        {
            get
            {
                _allMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _allMods.Filter = m => FilterFunction<ModViewModel>(m);
                return _allMods;
            }
        }
        
        ICollectionView _modelMods;
        public ICollectionView ModelMods
        {
            get
            {
                _modelMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _modelMods.Filter = m => FilterFunction<ModelModViewModel>(m);
                return _modelMods;
            }
        }
        
        ICollectionView _readonlyMods;
        public ICollectionView ReadOnlyMods
        {
            get
            {
                _readonlyMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _readonlyMods.Filter = m => FilterFunction<ReadOnlyModViewModel>(m);
                return _readonlyMods;
            }
        }

        ICollectionView _materialMods;
        public ICollectionView MaterialMods
        {
            get
            {
                _materialMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _materialMods.Filter = m => FilterFunction<MaterialModViewModel>(m);
                return _materialMods;
            }
        }

        ICollectionView _textureMods;
        public ICollectionView TextureMods
        {
            get
            {
                _textureMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _textureMods.Filter = m => FilterFunction<TextureModViewModel>(m);
                return _textureMods;
            }
        }

        ICollectionView _metadataMods;
        public ICollectionView MetadataMods
        {
            get
            {
                _metadataMods = new CollectionViewSource { Source = SimpleModsList }.View;
                _metadataMods.Filter = m => FilterFunction<MetadataModViewModel>(m);
                return _metadataMods;
            }
        }


        // TODO: "Incomplete" mods
        ICollectionView _incompleteMods;
        public ICollectionView IncompleteMods
        {
            // TODO: This needs a different "trigger"
            // CollectionChanged does not get called when a destination path changes
            get
            {
                _incompleteMods = new CollectionViewSource { Source = SimpleModsList }.View;
                //_incompleteMods.Filter = m => !(m as ModViewModel).CanExport;
                return _incompleteMods;
            }
        }
    }
}
