using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Import
{
    // TODO: Clean up this view model
    // Split into ImportVanillaModelViewModel and ImportVanillaMaterialViewModel?
    public class ImportVanillaViewModel : NotifyPropertyChanged
    {
        readonly IModsListViewModel _modPackViewModel;
        readonly ItemListViewModel _itemListService;
        readonly IGameFileService _gameFileDataService;
        readonly ILogService _logService;

        public ImportVanillaViewModel(IModsListViewModel modPack, ItemListViewModel itemListService, IGameFileService gameFileService, ILogService logService)
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
            var itemList = sender as ItemListViewModel;
            if (e.PropertyName == nameof(ItemListViewModel.SelectedItem) && itemList != null)
            {
                SelectedItem = itemList.SelectedItem;
                if (SelectedItem != null)
                {
                    HasSkin = XivPathParser.HasSkin(SelectedItem.GetMdlPath());
                    AllRacesMdls = new(_gameFileDataService.GetAllRaceMdls(SelectedItem));
                    if (AllRacesMdls.Count > 0)
                    {
                        SelectedRace = AllRacesMdls[0];
                    }

                    SelectedItemMdl = SelectedItem.GetMdlFileName();
                    SelectedItemMtrl = SelectedItem.GetMtrlFileName();
                    SelectedItemName = SelectedItem.Name;

                    CanImportMdl = true;
                    CanImportMtrl = true;
                    CanImportMeta = true;
                }
                else
                {
                    HasSkin = false;
                    AllRacesMdls = new();

                    SelectedItemMdl = "";
                    SelectedItemMtrl = "";
                    SelectedItemName = "";
                }
            }
            // TODO: Behavior: Type in invalid path, CanImportMdl/Mtrl will still be enabled
            if (e.PropertyName == nameof(ItemListViewModel.CompletePath) && itemList != null)
            {
                _completePath = itemList.CompletePath;
                if (_completePath != null)
                {
                    CanImportMdl = XivPathParser.IsMdl(_completePath);
                    if (CanImportMdl)
                    {
                        SelectedItemMdl = _completePath;
                    }

                    CanImportMtrl = XivPathParser.IsMtrl(_completePath);
                    if (CanImportMtrl)
                    {
                        SelectedItemMtrl = _completePath;
                    }
                }
                else if (SelectedItem != null)
                {
                    CanImportMdl = true;
                    CanImportMtrl = true;
                    CanImportMeta = true;
                }
                else
                {
                    CanImportMdl = false;
                    CanImportMtrl = false;
                    CanImportMeta = false;
                }
            }
        }

        bool _hasSkin = false;
        public bool HasSkin
        {
            get { return _hasSkin; }
            set { _hasSkin = value; OnPropertyChanged(); }
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
            set
            {
                _selectedRace = value;
                OnPropertyChanged();
                if (SelectedItem is IGear gear)
                {
                    SelectedItemMdl = gear.GetMdlFileName(_selectedRace);
                }
            }
        }

        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        string _selectedItemMdl;
        public string SelectedItemMdl
        {
            get { return _selectedItemMdl; }
            set { _selectedItemMdl = value; OnPropertyChanged(); }
        }

        string _selectedItemMtrl;
        public string SelectedItemMtrl
        {
            get { return _selectedItemMtrl; }
            set { _selectedItemMtrl = value; OnPropertyChanged(); }
        }

        string _selectedItemName;
        public string SelectedItemName
        {
            get { return _selectedItemName; }
            set { _selectedItemName = value; OnPropertyChanged(); }
        }

        string? _completePath;

        bool _canImportMdl = false;
        public bool CanImportMdl
        {
            get { return _canImportMdl; }
            set { _canImportMdl = value; OnPropertyChanged(); }
        }

        bool _canImportMtrl = false;
        public bool CanImportMtrl
        {
            get { return _canImportMtrl; }
            set { _canImportMtrl = value; OnPropertyChanged(); }
        }

        bool _canImportMeta = false;
        public bool CanImportMeta
        {
            get { return _canImportMeta; }
            set { _canImportMeta = value; OnPropertyChanged(); }
        }

        DelegateCommand _getVanillaModel;
        public DelegateCommand GetVanillaModel
        {
            get { return _getVanillaModel ??= new DelegateCommand(_ => GetVanillaMdl()); }
        }

        DelegateCommand _getVanillaMaterial;
        public DelegateCommand GetVanillaMaterial
        {
            get { return _getVanillaMaterial ??= new DelegateCommand(async _ => await GetVanillaMtrl()); }
        }

        DelegateCommand _getVanillaMetadata;
        public DelegateCommand GetVanillaMetadata
        {
            get { return _getVanillaMetadata ??= new DelegateCommand(async _ => await GetVanillaMeta()); }
        }

        // chara/human/c1301/obj/face/f0001/model/c1301f0001_fac.mdl

        /*
        public async Task<IGameFile?> GetVanillaItem()
        {
            IGameFile? gameFile = null;
            if (_completePath != null)
            {
                gameFile = await _gameFileDataService.TryGetFileData(_completePath);
            }
            else
            {
                if (CanImportMdl)
                {
                    gameFile = _gameFileDataService.GetModelFileData(SelectedItem, SelectedRace);
                }
                else if (CanImportMtrl)
                {
                    gameFile = await _gameFileDataService.GetMaterialFileData(SelectedItem);
                }
            }

            if (gameFile != null)
            {
                var modViewModel = ServiceManager.GetRequiredService<ViewModelService>().GetModViewModel(gameFile);
                _modPackViewModel.Add(modViewModel);
                modViewModel.SetModData(gameFile);
            }
            return gameFile;
        }
        */

        private async Task GetVanillaMeta()
        {
            MetadataMod? mod;
            if (SelectedItem == null) return;
            if (_completePath != null)
            {
                mod = await _gameFileDataService.TryGetMetadata(_completePath, SelectedItemName);
                mod.Path = _completePath;
            }
            else
            {
                mod = await _gameFileDataService.GetMetadata(SelectedItem);
                mod.Path = SelectedItem.GetMetadataPath();
            }
            var modViewModel = _modPackViewModel.Add(mod);
        }

        public ModelMod? GetVanillaMdl()
        {
            IModelGameFile? modelGameFile;
            if (_completePath != null)
            {
                modelGameFile = _gameFileDataService.TryGetModelFileData(_completePath, SelectedItemName);
            }
            else
            {
                modelGameFile = _gameFileDataService.GetModelFileData(SelectedItem, SelectedRace);
            }

            if (modelGameFile != null)
            {
                var mod = new ModelMod(modelGameFile, true);
                var modViewModel = _modPackViewModel.Add(mod);
                if (modViewModel == null)
                {
                    _logService.Fatal($"Failed to get ViewModel for vanilla mdl: {mod.Name}");
                    return null;
                }
                modViewModel.SetModData(modelGameFile);
                return mod;
            }
            return null;
            //return null;
        }

        private async Task<MaterialMod?> GetVanillaMtrl(IItem? item = null)
        {
            // TODO: How to handle SmallClothes, Emperor's series, and skin materials
            // TODO: Most of SmallClothes don't seem to have a material
            IMaterialGameFile? materialGameFile;
            if (_completePath != null)
            {
                materialGameFile = await _gameFileDataService.TryGetMaterialFileData(_completePath, SelectedItemName);
            }
            else
            {
                materialGameFile = await _gameFileDataService.GetMaterialFileData(item);
            }
            if (materialGameFile != null)
            {
                if (materialGameFile.XivMtrl == null)
                {
                    _logService.Error($"Could not find the material for {item.Name}");
                    return null;
                }
                var mod = new MaterialMod(materialGameFile, true);
                var modViewModel = _modPackViewModel.Add(mod);
                if (modViewModel == null)
                {
                    _logService.Fatal($"Failed to get ViewModel vanilla mtrl: {mod.Name}");
                    return null;
                }
                modViewModel.SetModData(materialGameFile);
                return mod;
            }
            return null;
        }
    }
}
