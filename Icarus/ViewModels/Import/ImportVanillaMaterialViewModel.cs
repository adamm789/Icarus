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
        }

        protected override void SelectedItemSet()
        {
            base.SelectedItemSet();
            if (SelectedItem != null)
            {
                SelectedItemMtrl = SelectedItem.GetMtrlFileName();
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
    }
}
