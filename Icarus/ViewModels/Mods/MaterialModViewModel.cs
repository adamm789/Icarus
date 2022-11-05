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
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.ViewModels.Mods
{
    public class MaterialModViewModel : ModViewModel
    {
        // TODO: ComboBox for variants
        MaterialMod _material;
        IWindowService _windowService;
        ShaderInfoViewModel _shaderInfoViewModel;

        // TODO: Include a section that shows the overall edits to all items
        // e.g. Group and display the mods that affect e6111
        public MaterialModViewModel(MaterialMod mod, IGameFileService gameFileDataService, IWindowService windowService, ILogService logService)
            : base(mod, gameFileDataService, logService)
        {
            _material = mod;
            _materialVariant = XivPathParser.GetMtrlVariant(_material.Path);
            _windowService = windowService;
            var stainingTemplateFile = gameFileDataService.GetStainingTemplateFile();

            ShaderInfoViewModel = new(_material);
            if (_material.ColorSetData.Count > 0)
            {
                ColorSetViewModel = new(_material, stainingTemplateFile);
            }

            SetCanExport();
        }

        // TODO: Update MaterialMod with color set stuff

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

        public void OpenMaterialEditor()
        {
            _windowService.Show<ShaderInfoWindow>(this);
        }

        protected override void RaiseDestinationPathChanged()
        {
            ShaderInfoViewModel.UpdatePaths(DestinationPath);
            base.RaiseDestinationPathChanged();
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

        // TODO: Changing an item changes the path such that it ignores the material variant
        public override async Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return await _gameFileService.GetMaterialFileData(itemArg, MaterialVariant);
        }

        public List<string> VariantList { get; } = new()
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };
    }
}
