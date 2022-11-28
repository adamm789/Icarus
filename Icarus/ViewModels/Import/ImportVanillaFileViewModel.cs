using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Import
{
    public abstract class ImportVanillaFileViewModel : ViewModelBase
    {
        protected readonly IModsListViewModel _modPackViewModel;
        protected readonly ItemListViewModel _itemListService;

        public ImportVanillaFileViewModel(IModsListViewModel modPack, ItemListViewModel itemListViewModel, ILogService logService)
            : base(logService)
        {
            _modPackViewModel = modPack;
            _logService = logService;

            _itemListService = itemListViewModel;
            var eh = new PropertyChangedEventHandler(SelectedItemChanged);
            _itemListService.PropertyChanged += eh;
        }

        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        string _selectedItemName;
        public string SelectedItemName
        {
            get { return _selectedItemName; }
            set { _selectedItemName = value; OnPropertyChanged(); }
        }

        protected string? _completePath;

        bool _canImport = false;
        public bool CanImport
        {
            get { return _canImport; }
            set { _canImport = value; OnPropertyChanged(); RaiseCanExecuteChanged(); }
        }

        DelegateCommand _importVanillaFileCommand;
        public DelegateCommand ImportVanillaFileCommand
        {
            get { return _importVanillaFileCommand ??= new DelegateCommand(async _ => await DoImport(), _ => CanImport); }
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            ImportVanillaFileCommand.RaiseCanExecuteChanged();
        }

        protected abstract Task DoImport();

        protected virtual void CompletePathSet()
        {
            SelectedItem = null;
            SelectedItemName = "";
        }

        protected virtual void SelectedItemSet()
        {
            if (_completePath != null)
            {
                return;
            }
            if (SelectedItem == null)
            {
                CanImport = false;
            }
            else if (SelectedItem != null)
            {
                SelectedItemName = SelectedItem.Name;
                _logService.Debug($"Selected item is set: {SelectedItemName}");
                CanImport = true;
            }
        }

        private void SelectedItemChanged(object sender, PropertyChangedEventArgs e)
        {
            var itemList = sender as ItemListViewModel;
            if (e.PropertyName == nameof(ItemListViewModel.SelectedItem) && itemList != null)
            {
                if (SelectedItem != itemList.SelectedItem)
                {
                    SelectedItem = itemList.SelectedItem;
                    SelectedItemSet();
                }
            }
            if (e.PropertyName == nameof(ItemListViewModel.CompletePath) && itemList != null)
            {
                _completePath = itemList.CompletePath;
                if (_completePath != null)
                {
                    var results = itemList.Search(_completePath);
                    CompletePathSet();
                }
                else
                {
                    //SelectedItemSet();
                }
            }
        }
    }
}
