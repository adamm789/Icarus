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

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackListViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ModPackViewModel> ModPacks { get; } = new();
        public ObservableCollection<IModPackMetaViewModel> ModPackMetas { get; } = new();

        public ObservableCollection<ModPackPageViewModel> ModPackPages
        {
            get { return DisplayedModPack?.ModPackPages; }
        }

        IModsListViewModel _modsListViewModel;

        // TODO: What to do if user deletes mods that are part of one of the mods in the ModPackList?
        public ModPackListViewModel(IModsListViewModel modsList, ViewModelService viewModelService)
        {
            _modsListViewModel = modsList;
            _viewModelService = viewModelService;
        }

        INotifyPropertyChanged _modPackPage;
        public INotifyPropertyChanged ModPackPage
        {
            get { return _modPackPage; }
            set { _modPackPage = value; OnPropertyChanged(); }
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
                if( _selectedIndex != value)
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

        readonly ViewModelService _viewModelService;

        ModPackViewModel _displayedModPack;
        public ModPackViewModel DisplayedModPack
        {
            get { return _displayedModPack; }
            set
            {
                _displayedModPack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModPackPages));
                OnPropertyChanged(nameof(AvailablePageIndices));
            }
        }

        public void Add(ModPack modPack)
        {
            var modPackViewModel = new ModPackViewModel(modPack, _viewModelService, true, _modsListViewModel);
            //modPackViewModel.SetModPack(modPack);
            ModPacks.Add(modPackViewModel);
            ModPackMetas.Add(modPackViewModel.ModPackMetaViewModel);

            if (DisplayedModPack == null)
            {
                DisplayedModPack = modPackViewModel;
                SelectedPageIndex = 0;
            }
        }

        public void Add(ModPackViewModel modPack)
        {
            ModPacks.Add(modPack);
        }


    }
}