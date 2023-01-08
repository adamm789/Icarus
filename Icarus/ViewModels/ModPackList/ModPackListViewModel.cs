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
using Icarus.ViewModels.Mods.DataContainers;

namespace Icarus.ViewModels.ModPackList
{
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
        public ObservableCollection<ModPackPageViewModel>? ModPackPages
        {
            get { return SelectedModPack?.ModPackPages; }
        }

        INotifyPropertyChanged? _displayedViewModel;
        public INotifyPropertyChanged? DisplayedViewModel
        {
            get { return _displayedViewModel; }
            set
            {
                _displayedViewModel = value;
                OnPropertyChanged();
                CopyPageCommand.RaiseCanExecuteChanged();
            }
        }

        private Dictionary<ModPackViewModel, int> ModPackSelectedPage = new();

        ModPackViewModel? _selectedModPack;
        public ModPackViewModel? SelectedModPack
        {
            get { return _selectedModPack; }
            set
            {
                _selectedModPack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModPackPages));
                OnPropertyChanged(nameof(AvailablePageIndices));
                if (value != null)
                {
                    SelectedPageIndex = ModPackSelectedPage[value];
                }
                CopyAllPagesCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsEmpty
        {
            get { return ModPacks.Count == 0; }
        }

        public List<string>? AvailablePageIndices => SelectedModPack?.GetAvailablePageIndices();

        int _selectedPageIndex = 0;
        public int SelectedPageIndex
        {
            get { return _selectedPageIndex; }
            set
            {
                if (SelectedModPack == null) return;
                if (value <= -1 || value > SelectedModPack.ModPackPages.Count + 1)
                {
                    return;
                }
                _selectedPageIndex = value;

                ModPackSelectedPage[SelectedModPack] = value;
                if (value == 0)
                {
                    DisplayedViewModel = SelectedModPack.ModPackMetaViewModel;
                }
                else
                {
                    DisplayedViewModel = SelectedModPack.ModPackPages[value - 1];
                }
                OnPropertyChanged();
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
            _logService?.Information($"Clearing all loaded mod packs.");
            ModPacks.Clear();
            SelectedModPack = null;
            DisplayedViewModel = null;
            OnPropertyChanged(nameof(IsEmpty));
            ModPackSelectedPage.Clear();
        }

        DelegateCommand _copyPageCommand;
        public DelegateCommand CopyPageCommand
        {
            get { return _copyPageCommand ??= new DelegateCommand(_ => OnCopyPage(), _ => DisplayedViewModel != null); }
        }

        DelegateCommand _copyAllPagesCommand;
        public DelegateCommand CopyAllPagesCommand
        {
            get { return _copyAllPagesCommand ??= new DelegateCommand(_ => OnCopyAllPages(), _ => ModPackPages != null); }
        }

        private void OnCopyPage()
        {
            if (DisplayedViewModel is ModPackPageViewModel page)
            {
                // TODO: When on meta, copy page... copies everything?
                _modPackViewModel.CopyPage(page);
                // TODO: This does not update the display if the current page is empty
            }
            if (DisplayedViewModel is ModPackMetaViewModel meta)
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
                    _modPackViewModel.CopyPage(page);
                }
            }
        }

        #endregion
        public void Add(ModPack modPack)
        {
            var modPackViewModel = new ModPackViewModel(modPack, _viewModelService, _logService, true, _modsListViewModel);
            //modPackViewModel.SetModPack(modPack);
            ModPacks.Add(modPackViewModel);
            ModPackSelectedPage.Add(modPackViewModel, 0);

            if (SelectedModPack == null)
            {
                SelectedModPack = modPackViewModel;
                SelectedPageIndex = 0;
            }

            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}
