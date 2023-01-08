﻿using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers;

namespace Icarus.ViewModels.ModPackList
{
    // TODO: Copy to current page if current page has no groups
    // TODO: When copying metadata from a modpack, it doesn't seem to be correct
    // I think it's changing to suit the preferences or settings or current metadata of whatever was there before
    public class ModPackListViewModel : ViewModelBase
    {
        readonly IModsListViewModel _modsListViewModel;
        readonly ViewModelService _viewModelService;
        readonly IModPackViewModel _modPackViewModel;

        // TODO: What to do if user deletes mods that are part of one of the mods in the ModPackList?
        public ModPackListViewModel(IModPackViewModel modPackViewModel, ViewModelService viewModelService, ILogService logService) : base(logService)
        {
            _modPackViewModel = modPackViewModel;
            _modsListViewModel = _modPackViewModel.ModsListViewModel;
            _viewModelService = viewModelService;

            _modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);
        }

        #region Properties
        public ObservableCollection<ModPackViewModel> ModPacks { get; } = new();
        public ObservableCollection<IModPackMetaViewModel> ModPackMetas { get; } = new();
        public ObservableCollection<ModPackPageViewModel>? ModPackPages
        {
            get { return DisplayedModPack?.ModPackPages; }
        }

        INotifyPropertyChanged? _modPackPage;
        public INotifyPropertyChanged? ModPackPage
        {
            get { return _modPackPage; }
            set {
                _modPackPage = value;
                OnPropertyChanged();
                CopyPageCommand.RaiseCanExecuteChanged();
            }
        }

        ModPackViewModel? _displayedModPack;
        public ModPackViewModel? DisplayedModPack
        {
            get { return _displayedModPack; }
            set
            {
                _displayedModPack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModPackPages));
                OnPropertyChanged(nameof(AvailablePageIndices));
                CopyAllPagesCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsEmpty
        {
            get { return ModPacks.Count == 0; }
        }

        public List<int>? AvailablePageIndices
        {
            get { return DisplayedModPack?.GetAvailablePageIndices(); }
        }

        int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value && value >= 0)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    DisplayedModPack = ModPacks[value];
                    SelectedPageIndex = 0;
                }
            }
        }

        int _selectedPageIndex = 0;
        public int SelectedPageIndex
        {
            get { return _selectedPageIndex; }
            set
            {
                _selectedPageIndex = value;
                if (DisplayedModPack == null) return;
                if (value == -1) value = 0;
                OnPropertyChanged();
                DisplayedModPack.PageIndex = value;
                ModPackPage = DisplayedModPack.DisplayedViewModel;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Reset();
            }
        }


        public void Reset()
        {
            _logService.Information($"Clearing all loaded mod packs.");
            ModPacks.Clear();
            ModPackMetas.Clear();
            DisplayedModPack = null;
            ModPackPage = null;
            OnPropertyChanged(nameof(IsEmpty));
        }

        DelegateCommand _copyPageCommand;
        public DelegateCommand CopyPageCommand
        {
            get { return _copyPageCommand ??= new DelegateCommand(_ => OnCopyPage(), _ => ModPackPage != null); }
        }

        DelegateCommand _copyAllPagesCommand;
        public DelegateCommand CopyAllPagesCommand
        {
            get { return _copyAllPagesCommand ??= new DelegateCommand(_ => OnCopyAllPages(), _ => ModPackPages != null); }
        }

        private void OnCopyPage()
        {
            if (ModPackPage is ModPackPageViewModel page)
            {
                // TODO: When on meta, copy page... copies everything?
                _modPackViewModel.CopyPage(page);
                // TODO: This does not update the display if the current page is empty
            }
            if (ModPackPage is ModPackMetaViewModel meta)
            {
                _modPackViewModel.SetMetadata(meta);
            }
        }

        private void OnCopyAllPages()
        {
            if (ModPackPages != null)
            {
                foreach (var page in ModPackPages)
                {
                    _modPackViewModel.AddPage(page);
                }
            }
        }

        #endregion

        public void Add(ModPack modPack)
        {
            var modPackViewModel = new ModPackViewModel(modPack, _viewModelService, _logService, true, _modsListViewModel);
            //modPackViewModel.SetModPack(modPack);
            ModPacks.Add(modPackViewModel);
            ModPackMetas.Add(modPackViewModel.ModPackMetaViewModel);

            if (DisplayedModPack == null)
            {
                DisplayedModPack = modPackViewModel;
                SelectedPageIndex = 0;
            }

            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}