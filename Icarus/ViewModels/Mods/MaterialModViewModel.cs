using Icarus.Mods;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using System.Collections.ObjectModel;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.ViewModels.Mods
{
    public class MaterialModViewModel : ModViewModel
    {
        MaterialMod _materialMod;
        IWindowService _windowService;
        ShaderInfoViewModel _shaderInfoViewModel;

        // TODO: Include a section that shows the overall edits to all items
        // e.g. Group and display the mods that affect e6111
        public MaterialModViewModel(MaterialMod mod, ItemListService itemListService, IGameFileService gameFileDataService, IWindowService windowService)
            : base(mod, itemListService, gameFileDataService)
        {
            _materialMod = mod;
            _windowService = windowService;
            ShaderInfoViewModel = new ShaderInfoViewModel(mod);

            var colorset = _materialMod.ColorSetData;

            if (colorset.Count == 256)
            {
                for (var i = 0; i < 16; i++)
                {
                    ColorSetViewModels.Add(new ColorSetRowViewModel(colorset.GetRange(i * 16, 16)));
                }
            }

            SetCanExport();
        }

        ObservableCollection<ColorSetRowViewModel> _colorSetViewModels = new();
        public ObservableCollection<ColorSetRowViewModel> ColorSetViewModels
        {
            get { return _colorSetViewModels; }
            set { _colorSetViewModels = value; OnPropertyChanged(); }
        }

        public ShaderInfoViewModel ShaderInfoViewModel
        {
            get { return _shaderInfoViewModel; }
            set { _shaderInfoViewModel = value; OnPropertyChanged(); }
        }

        private ShaderInfo _shaderInfo => _shaderInfoViewModel.ShaderInfo;

        DelegateCommand _openMaterialEditorCommand;
        public DelegateCommand OpenMaterialEditorCommand
        {
            get { return _openMaterialEditorCommand ??= new DelegateCommand(o => OpenMaterialEditor()); }
        }

        public void OpenMaterialEditor()
        {
            _windowService.ShowWindow<ShaderInfoWindow>(this);
        }

        public override void RaiseDestinationPathChanged()
        {
            //ShaderInfoViewModel.ShaderInfoChanged();
            ShaderInfoViewModel.UpdatePaths(DestinationPath);
            base.RaiseDestinationPathChanged();
        }
    }
}
