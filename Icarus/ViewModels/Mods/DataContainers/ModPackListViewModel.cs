using Icarus.Mods.DataContainers;
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

        INotifyPropertyChanged _modPackPage;
        public INotifyPropertyChanged ModPackPage
        {
            get { return _modPackPage; }
            set { _modPackPage = value; OnPropertyChanged(); }
        }

        int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                DisplayedModPack = ModPacks[value];
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
                DisplayedModPack.PageIndex = value;
                OnPropertyChanged(nameof(ModPackPage));
            }
        }

        readonly ViewModelService _viewModelService;
        public ModPackListViewModel(ViewModelService viewModelService)
        {
            _viewModelService = viewModelService;
        }

        ModPackViewModel _displayedModPack;
        public ModPackViewModel DisplayedModPack
        {
            get { return _displayedModPack; }
            set
            {
                _displayedModPack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModPackPages));
            }
        }

        public void Add(ModPack modPack)
        {
            var modPackViewModel = new ModPackViewModel(modPack, _viewModelService);
            //modPackViewModel.SetModPack(modPack);
            ModPacks.Add(modPackViewModel);
            ModPackMetas.Add(modPackViewModel.ModPackMetaViewModel);

            if (DisplayedModPack == null)
            {
                DisplayedModPack = modPackViewModel;
            }
        }

        public void Add(ModPackViewModel modPack)
        {
            ModPacks.Add(modPack);
        }
    }
}
