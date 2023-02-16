using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.Materials;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using Icarus.Views.Mods.Materials;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.ViewModels.Mods
{
    public class MaterialModViewModel : ModViewModel
    {
        MaterialMod _material;
        IWindowService _windowService;
        ShaderInfoViewModel _shaderInfoViewModel;

        IMaterialFileService _materialFileService;

        // TODO: Include a section that shows the overall edits to all items
        // e.g. Group and display the mods that affect e6111

        // TODO: "Apply to all variants"
        public MaterialModViewModel(MaterialMod mod, IMaterialFileService materialFileService, IWindowService windowService, ILogService logService)
            : base(mod, materialFileService, logService)
        {
            _material = mod;
            _materialVariant = XivPathParser.GetMtrlVariant(_material.Path);
            _windowService = windowService;
            _materialFileService = materialFileService;

            var stainingTemplateFile = materialFileService.GetStainingTemplateFile();

            ShaderInfoViewModel = new(_material);
            if (_material.ColorSetData.Count > 0)
            {
                ColorSetViewModel = new(_material, stainingTemplateFile);
            }

            SetCanExport();
        }

        ColorSetViewModel _colorSetViewModel;
        public ColorSetViewModel ColorSetViewModel
        {
            get { return _colorSetViewModel; }
            set { _colorSetViewModel = value; OnPropertyChanged(); }
        }

        public ShaderInfoViewModel ShaderInfoViewModel
        {
            get { return _shaderInfoViewModel; }
            set { _shaderInfoViewModel = value; OnPropertyChanged(); }
        }

        DelegateCommand _openMaterialEditorCommand;
        public DelegateCommand OpenMaterialEditorCommand
        {
            get { return _openMaterialEditorCommand ??= new DelegateCommand(o => OpenMaterialEditor()); }
        }

        public bool AssignToAllPaths
        {
            get { return _material.AssignToAllPaths; }
            set { _material.AssignToAllPaths = value; OnPropertyChanged(); }
        }

        public bool HasMultipleMaterialSets => _material.AllPathsDictionary.Count > 1;

        public void OpenMaterialEditor()
        {
            _windowService.Show<ShaderInfoWindow>(this);
        }

        protected override void RaiseDestinationPathChanged()
        {
            base.DestinationPath = XivPathParser.ChangeMtrlVariant(base.DestinationPath, MaterialVariant);
            ShaderInfoViewModel.UpdatePaths(DestinationPath);
            base.RaiseDestinationPathChanged();
        }

        int _selectedMaterialSet = 1;
        public int SelectedMaterialSet
        {
            get { return _selectedMaterialSet; }
            set { _selectedMaterialSet = value; OnPropertyChanged(); }
        }
        string _materialVariant = "a";
        public string MaterialVariant
        {
            get { return _materialVariant; }
            set
            {
                _materialVariant = value;
                OnPropertyChanged();
                base.DestinationPath = XivPathParser.ChangeMtrlVariant(base.DestinationPath, value);
                _material.Variant = value;
            }
        }

        public override string DestinationPath
        {
            get => base.DestinationPath;
            set
            {
                MaterialVariant = XivPathParser.GetMtrlVariant(value);
                base.DestinationPath = XivPathParser.ChangeMtrlVariant(value, MaterialVariant);
            }
        }

        public override async Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {

            var ret = await _materialFileService.GetMaterialFileData(itemArg);
            if (ret != null && _materialFileService.MaterialSet != null)
            {
                _material.SetAllPaths(_materialFileService.MaterialSet);
            }

            return ret;
        }

        public override async Task<IGameFile?> GetFileData(string path, string name = "")
        {
            return await _materialFileService.TryGetMaterialFileData(path, name);
        }

        public List<string> VariantList { get; } = new()
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        protected override bool HasValidPathExtension(string path)
        {
            return Path.GetExtension(path) == ".mtrl";
        }
    }
}
