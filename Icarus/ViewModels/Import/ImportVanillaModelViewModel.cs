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
using Lumina.Models.Models;
using Icarus.Mods.GameFiles;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaModelViewModel : ImportVanillaFileViewModel
    {
        // chara/human/c1301/obj/face/f0001/model/c1301f0001_fac.mdl
        IModelFileService _modelFileService;
        IModelGameFile? _selectedModel;

        public ImportVanillaModelViewModel(IModsListViewModel modPackViewModel, IModelFileService modelFileService, ILogService logService) :
            base(modPackViewModel, logService)
        {
            _modelFileService = modelFileService;

        }

        public override Task SetCompletePath(string? path)
        {
            base.SetCompletePath(path);

            if (String.IsNullOrWhiteSpace(path) || !XivPathParser.IsMdl(path))
            {
                HasSkin = false;
                SelectedItemMdl = "";
                _completePath = null;
                CanImport = false;
            }
            else
            {
                SelectedItemMdl = path;
                _selectedModel = _modelFileService.TryGetModelFileData(path);
                CanImport = _selectedModel != null;
                _completePath = path;
            }
            return Task.CompletedTask;
        }

        public override Task SetItem(IItem? item)
        {
            //List<IModelGameFile>? ret = null;
            base.SetItem(item);
            var modelGameFile = _modelFileService.GetModelFileData(item);

            _selectedModel = modelGameFile;
            SelectedItemName = _selectedModel?.Name;

            if (_selectedModel != null)
            {
                HasSkin = XivPathParser.HasSkin(_selectedModel.Path);
                AllRacesMdls = new(_modelFileService.GetAllRaceMdls(item));

                if (AllRacesMdls.Count > 0)
                {
                    SelectedRace = AllRacesMdls[0];
                }
                SelectedItemMdl = _selectedModel.Name;
            }
            else
            {
                HasSkin = false;
                AllRacesMdls = new();
                SelectedItemMdl = "";
            }
            CanImport = modelGameFile != null;
            return Task.CompletedTask;
        }

        public async Task<ModelMod?> GetVanillaMdl()
        {
            IModelGameFile? modelGameFile = _selectedModel;

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
            set { _selectedRace = value; OnPropertyChanged(); }
        }

        bool _hasSkin = false;
        public bool HasSkin
        {
            get { return _hasSkin; }
            set { _hasSkin = value; OnPropertyChanged(); }
        }
    }
}
