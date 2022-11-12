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
using System.Collections.Specialized;
using Icarus.Services.Interfaces;

namespace Icarus.ViewModels.Mods.DataContainers.ModPackList
{
    // TODO: ModPackList copy option
    public class ModPackListViewModel : ViewModelBase
    {
        readonly IModsListViewModel _modsListViewModel;
        readonly ViewModelService _viewModelService;

        // TODO: What to do if user deletes mods that are part of one of the mods in the ModPackList?
        public ModPackListViewModel(IModsListViewModel modsList, ViewModelService viewModelService, ILogService logService) : base(logService)
        {
            _modsListViewModel = modsList;
            _viewModelService = viewModelService;
        }

        #region Properties
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
                if (_selectedIndex != value)
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

        public DelegateCommand? CopyPageCommand { get; set; }

        private void OnCopyPage()
        {

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
