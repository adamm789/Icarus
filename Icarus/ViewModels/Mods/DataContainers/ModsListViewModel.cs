﻿using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModsListViewModel : ViewModelBase, IModsListViewModel, IDropTarget
    {
        public ModPack ModPack { get; }
        readonly ViewModelService _viewModelService;

        NotifyPropertyChanged _displayedMod;
        public NotifyPropertyChanged DisplayedMod
        {
            get { return _displayedMod; }
            set { _displayedMod = value; OnPropertyChanged(); }
        }
        public ModsListViewModel(ModPack modPack, ViewModelService viewModelService, IWindowService windowService, ILogService logService) : base(logService)
        {
            ModPack = modPack;
            _viewModelService = viewModelService;

            FilteredModsList = new(this, logService);
            SetCanExport();
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

        FilteredModsListViewModel _filteredModsList;
        public FilteredModsListViewModel FilteredModsList
        {
            get { return _filteredModsList; }
            set { _filteredModsList = value; OnPropertyChanged(); }
        }

        bool _isAdding = false;
        public bool IsAdding
        {
            get { return _isAdding; }
            set { _isAdding = value; OnPropertyChanged(); }
        }

        #region Public Functions

        public int AddRange(IEnumerable<IMod> mods)
        {
            var numAdded = 0;
            IsAdding = true;
            foreach (var m in mods)
            {
                if (m.ShouldImport)
                {
                    var mvm = Add(m);
                    if (mvm != null)
                    {
                        numAdded++;
                    }
                }
            }
            IsAdding = false;
            return numAdded;
        }

        public ModViewModel? Add(IMod mod)
        {
            var modViewModel = _viewModelService.GetModViewModel(mod);
            if (modViewModel != null)
            {
                Add(modViewModel);

                return modViewModel;
            }
            return null;
        }

        public void AddRange(IEnumerable<ModViewModel> mods)
        {
            var numImported = 0;
            foreach (var m in mods)
            {
                if (m.ShouldImport)
                {
                    Add(m);
                    numImported++;
                }
            }

            _logService.Information($"Added {numImported} mod(s)");
        }

        public void Add(ModViewModel mod)
        {
            ModPack.SimpleModsList.Add(mod.GetMod());
            SimpleModsList.Add(mod);
            mod.Identifier = $"({identifier})";
            identifier++;

            var eh = new PropertyChangedEventHandler(OnPropertyChanged);
            mod.PropertyChanged += eh;
            SetCanExport();
        }

        public bool DeleteMod(ModOptionModViewModel mod)
        {
            return DeleteMod(mod.Mod);
        }

        /// <summary>
        /// Tries to remove the given <paramref name="modViewModel"/> from <see cref="SimpleModsList"/>
        /// and the underlying <see cref="IMod"/> in <see cref="ModPack"/>
        /// </summary>
        /// <param name="modViewModel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool DeleteMod(ModViewModel modViewModel)
        {
            var existingMod = ModExists(modViewModel);
            if (existingMod != null)
            {
                Log.Debug($"Deleting mod from modslist: {modViewModel.FileName}.");

                var b1 = SimpleModsList.Remove(existingMod);
                var b2 = ModPack.SimpleModsList.Remove(modViewModel.GetMod());

                //RemoveMod(mod);

                if (!(b1 && b2))
                {
                    // Hopefully this never happens
                    throw new ArgumentException("Could not remove mod.");
                }

                SetCanExport();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _logService?.Information("Clearing mods list. Removing all mods.");
            ModPack.SimpleModsList.Clear();
            ModPack.ModPackPages.Clear();
            SimpleModsList.Clear();
            CanExport = false;
        }

        public void Move(ModViewModel source, ModViewModel target)
        {
            var sourceIndex = SimpleModsList.IndexOf(source);
            var targetIndex = SimpleModsList.IndexOf(target);

            SimpleModsList.Move(sourceIndex, targetIndex);
            ModPack.MoveMod(sourceIndex, targetIndex);
        }
        #endregion

        #region Private Functions

        /// <summary>
        /// Tries to find the given <paramref name="modViewModel" /> in <see cref="SimpleModsList"/><para />
        /// Comparison is based on equality of the underlying <see cref="ModViewModel.Mod"/>
        /// </summary>
        /// <param name="modViewModel"></param>
        /// <returns>The matching ModViewModel if it exists. Null otherwise</returns>
        private ModViewModel? ModExists(ModViewModel modViewModel)
        {
            foreach (var m in SimpleModsList)
            {
                if (m.GetMod() == modViewModel.GetMod())
                {
                    return m;
                }
            }
            return null;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.CanExport))
            {
                SetCanExport();
            }
            if (e.PropertyName == nameof(ModViewModel.ShouldDelete))
            {
                var mod = sender as ModViewModel;
                DeleteMod(mod);
                Log.Verbose($"Finished deleting: {mod.FileName}.");
            }
            if (e.PropertyName == nameof(ModViewModel.DestinationPath))
            {
                // Update in dictionary
            }
        }

        private void SetCanExport()
        {
            CanExport = GetCanExport();
        }

        /// <summary>
        /// Checks to see if every mod in <see cref="SimpleModsList"/> can be exported.
        /// </summary>
        /// <returns></returns>
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
        #endregion

        #region UI
        void IDropTarget.DragEnter(GongSolutions.Wpf.DragDrop.IDropInfo dropInfo)
        {
            Log.Debug($"Drag enter");
        }
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;
            if (source is ModViewModel && target is ModViewModel)
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
            _logService.Debug($"Drop in {GetType()}");
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModViewModel sourceMod && target is ModViewModel targetMod)
            {
                Move(sourceMod, targetMod);
            }
        }

        #endregion

    }
}
