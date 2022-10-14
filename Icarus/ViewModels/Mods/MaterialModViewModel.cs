using Icarus.Mods;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.Materials;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using System.Collections.ObjectModel;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.ViewModels.Mods
{
    public class MaterialModViewModel : ModViewModel
    {
        MaterialMod _material;
        IWindowService _windowService;
        ShaderInfoViewModel _shaderInfoViewModel;

        // TODO: Include a section that shows the overall edits to all items
        // e.g. Group and display the mods that affect e6111
        public MaterialModViewModel(MaterialMod mod, IGameFileService gameFileDataService, IWindowService windowService, ILogService logService)
            : base(mod, gameFileDataService, logService)
        {
            _material = mod;
            _windowService = windowService;
            ShaderInfoViewModel = new(_material);
            ColorSetViewModel = new(_material.ColorSetData);

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

        public void OpenMaterialEditor()
        {
            _windowService.Show<ShaderInfoWindow>(this);
        }

        protected override void RaiseDestinationPathChanged()
        {
            ShaderInfoViewModel.UpdatePaths(DestinationPath);
            base.RaiseDestinationPathChanged();
        }
    }
}
