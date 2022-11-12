using Icarus.Mods.Interfaces;
using Icarus.Mods;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ItemDatabase.Paths;
using ItemDatabase.Interfaces;
using System.Collections.ObjectModel;
using xivModdingFramework.General.Enums;
using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaModelViewModel : ImportVanillaFileViewModel
    {
        // chara/human/c1301/obj/face/f0001/model/c1301f0001_fac.mdl
        IModelFileService _modelFileService;
        public ImportVanillaModelViewModel(IModsListViewModel modPack, ItemListViewModel itemListService, IModelFileService modelFileService, ILogService logService) :
            base(modPack, itemListService, logService)
        {
            _modelFileService = modelFileService;
        }

        protected override void CompletePathSet()
        {
            base.CompletePathSet();
            CanImport = XivPathParser.IsMdl(_completePath);
            if (CanImport)
            {
                SelectedItemMdl = _completePath;
            }
        }

        protected override void SelectedItemSet()
        {
            base.SelectedItemSet();
            if (SelectedItem != null)
            {
                HasSkin = XivPathParser.HasSkin(SelectedItem.GetMdlPath());
                AllRacesMdls = new(_modelFileService.GetAllRaceMdls(SelectedItem));

                if (AllRacesMdls.Count > 0)
                {
                    SelectedRace = AllRacesMdls[0];
                }
                SelectedItemMdl = SelectedItem.GetMdlFileName();
            }
            else
            {
                HasSkin = false;
                AllRacesMdls = new();
                SelectedItemMdl = "";
            }
        }

        public async Task<ModelMod?> GetVanillaMdl()
        {
            IModelGameFile? modelGameFile;
            if (_completePath != null)
            {
                modelGameFile = _modelFileService.TryGetModelFileData(_completePath, SelectedItemName);
            }
            else
            {
                modelGameFile = _modelFileService.GetModelFileData(SelectedItem, SelectedRace);
            }

            if (modelGameFile != null)
            {
                var mod = new ModelMod(modelGameFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
                if (modViewModel == null)
                {
                    _logService.Fatal($"Failed to get ViewModel for vanilla mdl: {mod.Name}");
                    return await Task.FromResult<ModelMod?>(null);
                }
                modViewModel.SetModData(modelGameFile);
                return await Task.FromResult<ModelMod?>(mod);
            }
            return await Task.FromResult<ModelMod?>(null);
            //return null;
        }

        protected async override Task DoImport()
        {
            await GetVanillaMdl();
            //return Task.CompletedTask;
        }

        string _selectedItemMdl;
        public string SelectedItemMdl
        {
            get { return _selectedItemMdl; }
            set { _selectedItemMdl = value; OnPropertyChanged(); }
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

        bool _hasSkin = false;
        public bool HasSkin
        {
            get { return _hasSkin; }
            set { _hasSkin = value; OnPropertyChanged(); }
        }
    }
}
