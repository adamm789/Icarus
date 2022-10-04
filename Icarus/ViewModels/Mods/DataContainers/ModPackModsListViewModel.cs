using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackModsListViewModel : NotifyPropertyChanged, IModsListViewModel, IDropTarget
    {
        public ModPack ModPack { get; }
        ViewModelService _viewModelService;

        public ModPackModsListViewModel(ModPack modPack, ViewModelService modFileService)
        {
            ModPack = modPack;
            _viewModelService = modFileService;

            UpdateHeaders();
        }

        private int identifier = 0;
        bool _canExport = false;
        public bool CanExport
        {
            get { return _canExport; }
            set { _canExport = value; OnPropertyChanged(); }
        }

        ObservableCollection<ModViewModel> _simpleModsList = new();
        public ObservableCollection<ModViewModel> SimpleModsList
        {
            get { return _simpleModsList; }
            set { _simpleModsList = value; OnPropertyChanged(); }
        }

        #region Public Functions

        public int AddRange(IEnumerable<IMod> mods)
        {
            var numAdded = 0;
            foreach (var m in mods)
            {
                numAdded += Add(m);
            }
            return numAdded;
        }

        public int Add(IMod mod)
        {
            var modViewModel = _viewModelService.GetModViewModel(mod);
            if (modViewModel != null)
            {
                Add(modViewModel);
                return 1;
            }
            return 0;
        }

        public void AddRange(IEnumerable<ModViewModel> mods)
        {
            foreach (var m in mods)
            {
                Add(m);
            }
        }

        public void Add(ModViewModel mod)
        {
            ModPack.SimpleModsList.Add(mod.GetMod());
            SimpleModsList.Add(mod);

            mod.Identifier = $"({identifier})";
            identifier++;

            var eh = new PropertyChangedEventHandler(OnPropertyChanged);
            mod.PropertyChanged += eh;
            UpdateHeaders();
        }

        public bool DeleteMod(ModOptionModViewModel mod)
        {
            return DeleteMod(mod.ModViewModel);
        }

        /// <summary>
        /// Removes a mod from the mods list as well as any reference in the modpack pages
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool DeleteMod(ModViewModel mod)
        {
            var existingMod = ModExists(mod);
            if (existingMod != null)
            {
                var b1 = SimpleModsList.Remove(existingMod);
                var b2 = ModPack.SimpleModsList.Remove(mod.GetMod());

                //RemoveMod(mod);

                if (!(b1 && b2))
                {
                    throw new ArgumentException("Could not remove mod.");
                }

                UpdateHeaders();
                return true;
            }
            return false;
        }

        public void Move(ModViewModel source, ModViewModel target)
        {
            var sourceIndex = SimpleModsList.IndexOf(source);
            var targetIndex = SimpleModsList.IndexOf(target);

            SimpleModsList.Move(sourceIndex, targetIndex);
            ModPack.MoveMod(sourceIndex, targetIndex);
            /*
            ModPack.SimpleModsList.RemoveAt(sourceIndex);
            if (targetIndex > ModPack.SimpleModsList.Count)
            {
                targetIndex = ModPack.SimpleModsList.Count - 1;
            }
            ModPack.SimpleModsList.Insert(targetIndex, source.GetMod());
            */
        }
        #endregion

        #region Private Functions

        private ModViewModel? ModExists(ModViewModel mod)
        {
            foreach (var simpleMod in SimpleModsList)
            {
                if (simpleMod.GetMod() == mod.GetMod())
                {
                    return simpleMod;
                }
            }
            return null;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.CanExport))
            {
                UpdateHeaders();
            }
            if (e.PropertyName == nameof(ModViewModel.ShouldDelete))
            {
                var mod = sender as ModViewModel;
                Log.Information($"Deleting mod from modslist: {mod.FileName}.");
                DeleteMod(mod);
                Log.Information($"Finished deleting: {mod.FileName}.");
            }
        }

        private void UpdateHeaders()
        {
            CanExport = GetCanExport();

            AllModsHeader = $"All ({SimpleModsList.Count})";
            ModelModsHeader = $"Models ({ModelMods.Cast<ModViewModel>().Count()})";
            MaterialModsHeader = $"Materials ({MaterialMods.Cast<ModViewModel>().ToArray().Length})";
            TextureModsHeader = $"Textures({TextureMods.Cast<ModViewModel>().ToArray().Length})";
            MetadataModsHeader = $"Metadata({MetadataMods.Cast<ModViewModel>().ToArray().Length})";
            ReadOnlyModsHeader = $"ReadOnly ({ReadOnlyMods.Cast<ModViewModel>().ToArray().Length})";
        }

        private bool GetCanExport()
        {
            if (SimpleModsList.Count == 0)
            {
                return false;
            }
            foreach (var item in SimpleModsList)
            {
                if (!item.CanExport)
                {
                    return false;
                }
            }
            return true;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var target = dropInfo.TargetItem as ModViewModel;
            if (target != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            /*
            var dragInfo = dropInfo.DragInfo.Data;

            //dropInfo.TargetScrollViewer = dragInfo.Ta
            var sourceItem = dropInfo.Data;

            
            if (sourceItem is ModViewModel modViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            */
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as ModViewModel;
            var target = dropInfo.TargetItem as ModViewModel;

            if (source != null && target != null)
            {
                Move(source, target);
            }
        }

        #endregion

        #region CollectionViews
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

        ICollectionView _modelMods;
        public ICollectionView ModelMods
        {
            get
            {
                _modelMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _modelMods.Filter = m => m is ModelModViewModel;
                return _modelMods;
            }
        }

        ICollectionView _readonlyMods;
        public ICollectionView ReadOnlyMods
        {
            get
            {
                _readonlyMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _readonlyMods.Filter = m => m is ReadOnlyModViewModel;
                return _readonlyMods;
            }
        }

        ICollectionView _materialMods;
        public ICollectionView MaterialMods
        {
            get
            {
                _materialMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _materialMods.Filter = m => m is MaterialModViewModel;
                return _materialMods;
            }
        }

        ICollectionView _textureMods;
        public ICollectionView TextureMods
        {
            get
            {
                _textureMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _textureMods.Filter = m => m is TextureModViewModel;
                return _textureMods;
            }
        }

        ICollectionView _metadataMods;
        public ICollectionView MetadataMods
        {
            get
            {
                _metadataMods ??= new CollectionViewSource { Source = SimpleModsList }.View;
                _metadataMods.Filter = m => m is MetadataModViewModel;
                return _metadataMods;
            }
        }
        #endregion
    }
}
