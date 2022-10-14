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

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class FilteredModsListViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ModViewModel> SimpleModsList { get; }

        // TODO:? Allow searching through mods list
        string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { 
                _searchTerm = value;
                OnPropertyChanged();
                Search(value);
            }
        }

        public FilteredModsListViewModel(ObservableCollection<ModViewModel> modsList)
        {
            SimpleModsList = modsList;
            SimpleModsList.CollectionChanged += new(OnCollectionChanged);
            UpdateHeaders();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SimpleModsList));
            /*
            if (e.NewItems != null)
            {
                DisplayedMod = e.NewItems[0] as ModViewModel;
            }
            */
            UpdateHeaders();
        }


        private void UpdateHeaders()
        {
            AllModsHeader = $"All ({SimpleModsList.Count})";
            ModelModsHeader = $"Models ({ModelMods.Cast<ModViewModel>().Count()})";
            MaterialModsHeader = $"Materials ({MaterialMods.Cast<ModViewModel>().Count()})";
            TextureModsHeader = $"Textures({TextureMods.Cast<ModViewModel>().Count()})";
            MetadataModsHeader = $"Metadata({MetadataMods.Cast<ModViewModel>().Count()})";
            ReadOnlyModsHeader = $"ReadOnly ({ReadonlyMods.Cast<ModViewModel>().Count()})";
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

        private bool FilterFunction<T>(object obj) where T: ModViewModel
        {
            if (obj == null)
                return false;
            var correctType = obj is T;
            if (String.IsNullOrWhiteSpace(SearchTerm)) {
                return correctType;
            }
            else
            {
                var x = typeof(T);
                if (obj is T vm)
                {
                    if (vm.FileName.Contains(SearchTerm))
                    {
                        return correctType;
                    }
                    else
                    {
                        return false;
                    }
                }
                return correctType;
            }
        }

        DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand
        {
            get { return _searchCommand ??= new DelegateCommand(_ => Search(SearchTerm)); }
        }

        private void Search(string str)
        {
            //SearchedMods.Refresh();
            ModelMods.Refresh();
            ReadonlyMods.Refresh();

            UpdateHeaders();
        }

        /*
        ICollectionView _searchedMods;
        public ICollectionView SearchedMods
        {
            get
            {
                // TODO?: This doubles the displayed mods for some reason
                _searchedMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _searchedMods.Filter = m => FilterFunction<ModViewModel>(m);
                return _searchedMods;
            }
        }
        */

        ICollectionView _modelMods;
        public ICollectionView ModelMods
        {
            get
            {
                _modelMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _modelMods.Filter = m => FilterFunction<ModelModViewModel>(m);
                return _modelMods;
            }
        }

        ICollectionView _readonlyMods;
        public ICollectionView ReadonlyMods
        {
            get
            {
                _readonlyMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _readonlyMods.Filter = m => FilterFunction<ReadOnlyModViewModel>(m);
                return _readonlyMods;
            }
        }

        ICollectionView _materialMods;
        public ICollectionView MaterialMods
        {
            get
            {
                _materialMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _materialMods.Filter = m => FilterFunction<MaterialModViewModel>(m);
                return _materialMods;
            }
        }

        ICollectionView _textureMods;
        public ICollectionView TextureMods
        {
            get
            {
                _textureMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _textureMods.Filter = m => FilterFunction<TextureModViewModel>(m);
                return _textureMods;
            }
        }

        ICollectionView _metadataMods;
        public ICollectionView MetadataMods
        {
            get
            {
                _metadataMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _metadataMods.Filter = m => FilterFunction<MetadataModViewModel>(m);
                return _metadataMods;
            }
        }
    }
}
