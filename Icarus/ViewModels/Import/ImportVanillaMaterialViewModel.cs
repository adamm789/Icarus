using Icarus.Mods.Interfaces;
using Icarus.Mods;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemDatabase.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaMaterialViewModel : ImportVanillaFileViewModel
    {
        readonly IMaterialFileService _materialFileService;
        public ImportVanillaMaterialViewModel(IModsListViewModel modPack, ItemListViewModel itemListService, IMaterialFileService materialFileService, ILogService logService)
    : base(modPack, itemListService, logService)
        {
            _materialFileService = materialFileService;
            MaterialSetText = $"Add all 0 variant(s)";
        }

        protected override void SelectedItemSet()
        {
            base.SelectedItemSet();
            if (SelectedItem != null)
            {
                SelectedItemMtrl = SelectedItem.GetMtrlFileName();
                MaterialSetText = $"Add all {_materialFileService.GetNumMaterialSets(SelectedItem)} variant(s)";
            }
            else
            {
                SelectedItemMtrl = "";
            }
        }

        protected override void CompletePathSet()
        {
            base.CompletePathSet();
            CanImport = XivPathParser.IsMtrl(_completePath);
            if (CanImport)
            {
                SelectedItemMtrl = _completePath;
            }
        }

        string _selectedItemMtrl;
        public string SelectedItemMtrl
        {
            get { return _selectedItemMtrl; }
            set { _selectedItemMtrl = value; OnPropertyChanged(); }
        }

        DelegateCommand _getVanillaMaterial;
        public DelegateCommand GetVanillaMaterial
        {
            get { return _getVanillaMaterial ??= new DelegateCommand(async _ => await GetVanillaMtrl(), _ => CanImport == true); }
        }

        DelegateCommand _importMaterialSetCommand;
        public DelegateCommand ImportMaterialSetCommand
        {
            get { return _importMaterialSetCommand ??= new DelegateCommand(async _ => await GetVanillaMaterialAndVariants(), _ => CanImport == true); }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            ImportMaterialSetCommand.RaiseCanExecuteChanged();
        }

        string _materialSetText = "";
        public string MaterialSetText
        {
            get { return _materialSetText; }
            set { _materialSetText = value; OnPropertyChanged(); }
        }

        protected override async Task DoImport()
        {
            await GetVanillaMtrl(SelectedItem);
        }

        private async Task<MaterialMod?> GetVanillaMtrl(IItem? item = null)
        {
            // TODO: How to handle SmallClothes, Emperor's series, and skin materials
            // TODO: Most of SmallClothes don't seem to have a material
            IMaterialGameFile? materialGameFile;
            if (_completePath != null)
            {
                materialGameFile = await _materialFileService.TryGetMaterialFileData(_completePath, SelectedItemName);
            }
            else
            {
                materialGameFile = await _materialFileService.GetMaterialFileData(item);
            }
            if (materialGameFile != null)
            {
                if (materialGameFile.XivMtrl == null)
                {
                    _logService.Error($"Could not find the material for {item.Name}");
                    return null;
                }
                var mod = new MaterialMod(materialGameFile, ImportSource.Vanilla);
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

        private async Task<List<MaterialMod>?> GetVanillaMaterialAndVariants(IItem? itemArg = null)
        {
            if(_completePath == null)
            {
                var materials = await _materialFileService.GetMaterialAndVariantsFileData(itemArg);
                if (materials != null)
                {
                    var materialMods = new List<MaterialMod>();
                    foreach (var mat in materials)
                    {
                        var mod = new MaterialMod(mat, ImportSource.Vanilla);
                        var modViewModel = _modPackViewModel.Add(mod);
                        if (modViewModel == null)
                        {
                            _logService.Fatal($"Failed to get view model for vanilla mtrl :{mod.Name}");
                        }
                        materialMods.Add(mod);
                        modViewModel.SetModData(mat);
                    }
                    return materialMods;
                }
            }
            return null;
        }
    }
}
