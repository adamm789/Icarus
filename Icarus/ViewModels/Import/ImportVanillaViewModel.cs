using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaViewModel : NotifyPropertyChanged
    {
        readonly IModsListViewModel _modPackViewModel;
        readonly ItemListService _itemListService;
        readonly IGameFileService _gameFileDataService;
        readonly ILogService _logService;

        public ImportVanillaViewModel(IModsListViewModel modPack, ItemListService itemListService, IGameFileService gameFileService, ILogService logService)
        {
            _modPackViewModel = modPack;
            _gameFileDataService = gameFileService;
            _logService = logService;

            _itemListService = itemListService;
            var eh = new PropertyChangedEventHandler(SelectedItemChanged);
            _itemListService.PropertyChanged += eh;
        }

        private void SelectedItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListService.SelectedItem))
            {
                SelectedItem = (sender as ItemListService).SelectedItem;
                AllRacesMdls = new(_itemListService.GetAllRaceMdls());
            }
        }

        string _text = "";
        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }

        DelegateCommand _tryGetPathCommand;
        public DelegateCommand TryGetPathCommand
        {
            get { return _tryGetPathCommand ??= new DelegateCommand(async o => await TryGetPath()); }
        }

        public async Task TryGetPath()
        {
            var gameFile = await _gameFileDataService.TryGetFileData(Text);
            if (gameFile is ModelGameFile model)
            {
                var mod = new ModelMod(model, true);
                _modPackViewModel.Add(mod);
            }
        }

        DelegateCommand _getVanillaModel;
        public DelegateCommand GetVanillaModel
        {
            get { return _getVanillaModel ??= new DelegateCommand(o => GetVanillaMdl()); }
        }

        bool _selectingMdlRace = false;
        public bool SelectingMdlRace
        {
            get { return _selectingMdlRace; }
            set { _selectingMdlRace = value; OnPropertyChanged(); }
        }

        ObservableCollection<XivRace> _allRacesMdls = new();
        public ObservableCollection<XivRace> AllRacesMdls
        {
            get { return _allRacesMdls; }
            set { _allRacesMdls = value; OnPropertyChanged(); }
        }

        XivRace _selectedRace = XivRace.Hyur_Midlander_Male;
        public XivRace SelectedRace
        {
            get { return _selectedRace; }
            set { _selectedRace = value; OnPropertyChanged(); }
        }

        int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; OnPropertyChanged(); }
        }

        IItem _selectedItem;
        public IItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private bool ShouldGetMdlData()
        {
            return (SelectedIndex != -1 && AllRacesMdls.Count > 0) || (AllRacesMdls.Count == 0);
        }

        DelegateCommand _getVanillaMaterial;
        public DelegateCommand GetVanillaMaterial
        {
            get { return _getVanillaMaterial ??= new DelegateCommand(async o => await GetVanillaMtrl()); }
        }

        public ModelMod? GetVanillaMdl()
        {
            // TODO: Trying to get a vanilla item ends up getting model data twice. ?

            if (ShouldGetMdlData())
            {
                var modelGameFile = _gameFileDataService.GetModelFileData(SelectedItem, SelectedRace);
                if (modelGameFile == null) return null;
                var mod = new ModelMod(modelGameFile, true);
                _modPackViewModel.Add(mod);
                return mod;
            }
            return null;
        }

        private async Task<MaterialMod?> GetVanillaMtrl(IItem? item = null)
        {
            // TODO: How to handle SmallClothes, Emperor's series, and skin materials
            // TODO: Most of SmallClothes don't seem to have a material
            var materialGameFile = await _gameFileDataService.GetMaterialFileData(item);
            if (materialGameFile == null) return null;
            if (materialGameFile.XivMtrl == null)
            {
                _logService.Error($"Could not find the material for {item.Name}");
                return null;
            }
            var mod = new MaterialMod(materialGameFile, true);
            _modPackViewModel.Add(mod);
            return mod;
        }

        // TODO: Try to get a vanilla item from user-provider path
        // Will need to add textbox somewhere
        private async Task TryGetVanillaFile()
        {
            // User-provided path
            var gameFile = await _gameFileDataService.TryGetFileData("");
        }
    }
}
