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
        // chara/equipment/e0387/material/v0001/mt_c0101e0387_top_a.mtrl

        readonly IMaterialFileService _materialFileService;
        public ImportVanillaMaterialViewModel(IModsListViewModel modPack, IMaterialFileService materialFileService, ILogService logService)
            : base(modPack, logService)
        {
            _materialFileService = materialFileService;
            MaterialSetText = $"Add all 0 variant(s)";
        }

        public bool HasOneMaterial
        {
            get { return MaterialFiles != null && MaterialFiles.Count == 1; }
        }

        public bool HasMoreThanOneMaterial
        {
            get { return MaterialFiles != null && MaterialFiles.Count > 1; }
        }

        List<IMaterialGameFile>? _materialFiles;
        public List<IMaterialGameFile>? MaterialFiles
        {
            get { return _materialFiles; }
            set
            {
                _materialFiles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasOneMaterial));
                OnPropertyChanged(nameof(HasMoreThanOneMaterial));
            }
        }

        IMaterialGameFile? _selectedMaterialFile;
        public IMaterialGameFile? SelectedMaterialFile
        {
            get { return _selectedMaterialFile; }
            set {
                _selectedMaterialFile = value;
                if (_selectedMaterialFile != null)
                {
                    SelectedMaterialPath = _selectedMaterialFile.Path;
                }
                OnPropertyChanged();
            }
        }

        string _selectedMaterialPath;
        public string SelectedMaterialPath
        {
            get { return _selectedMaterialPath; }
            set { _selectedMaterialPath = value; OnPropertyChanged(); }
        }

        DelegateCommand _getVanillaMaterial;
        public DelegateCommand GetVanillaMaterial
        {
            get { return _getVanillaMaterial ??= new DelegateCommand(_ => GetVanillaMtrl(), _ => CanImport == true); }
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

        int _selectedMaterialSet = 1;
        public int SelectedMaterialSet
        {
            get { return _selectedMaterialSet; }
            set { _selectedMaterialSet = value; OnPropertyChanged(); }
        }

        List<int> _materialSetList = new();
        public List<int> MaterialSetList
        {
            get { return _materialSetList; }
            set { _materialSetList = value; OnPropertyChanged(); }
        }

        string _materialSetText = "";
        public string MaterialSetText
        {
            get { return _materialSetText; }
            set { _materialSetText = value; OnPropertyChanged(); }
        }

        protected override void DoImport()
        {
            GetVanillaMtrl();
        }

        public async override Task SetCompletePath(string? path)
        {
            await base.SetCompletePath(path);
            MaterialFiles = null;
            if (!XivPathParser.IsMtrl(path))
            {
                CanImport = false;
                SelectedMaterialFile = null;
            }
            else
            {
                SelectedMaterialFile = await _materialFileService.TryGetMaterialFileData(path);
                CanImport = SelectedMaterialFile != null;
            }
        }

        public override async Task SetItem(IItem? item)
        {
            MaterialFiles = await _materialFileService.GetMaterialSet(item);
            if (MaterialFiles != null)
            {
                if (item is IGear gear)
                {
                    SelectedMaterialFile = MaterialFiles.FirstOrDefault(m => m.MaterialSet == gear.MaterialId);
                }
                else
                {
                    SelectedMaterialFile = MaterialFiles[0];
                }
            }

            CanImport = MaterialFiles != null;
            //return MaterialFiles;
        }

        private MaterialMod? GetVanillaMtrl()
        {
            // TODO: How to handle SmallClothes, Emperor's series, and skin materials
            // TODO: Most of SmallClothes don't seem to have a material
            var materialGameFile = SelectedMaterialFile;

            if (materialGameFile != null)
            {
                if (materialGameFile.XivMtrl == null)
                {
                    _logService.Error($"Could not find the material");
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
            if (_completePath == null)
            {
                //var materials = await _materialFileService.GetMaterialAndVariantsFileData(itemArg);
                var materials = await _materialFileService.GetMaterialSet(itemArg);
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
