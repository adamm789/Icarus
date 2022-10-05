using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Serilog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.Enums;
using Half = SharpDX.Half;

namespace Icarus.ViewModels.Mods
{
    public class MaterialModViewModel : ModViewModel
    {
        // TODO: Overhaul MaterialModViewModel so that it does not constantly update the associated MaterialMod and consequently the XiVMtrl
        // There are destructive operations in xivMtrl.SetShaderInfo()


        // TODO: Need to handle case of importing .dds file
        // TODO: call SetCanExport... at some point

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

            /*
            var colorSetData = new List<List<Color>>();
            for (var i = 0; i < 16; i++)
            {
                colorSetData.Add(new List<Color>());
                colorSetData[i] = new ColorSetRowViewModel()
                for (int j = 0; j < 4; j++)
                {
                    var color = GetColorSetDataRange(_materialMod.GetColorSetData(), (i * 16) + (j * 4));
                    colorSetData[i].Add(color);
                }
            }
            */
            var colorSetData = new List<List<Color>>();
            var colorset = _materialMod.GetColorSetData();

            if (colorset.Count == 256)
            {
                for (var i = 0; i < 16; i++)
                {
                    ColorSetViewModels.Add(new ColorSetRowViewModel(colorset.GetRange(i * 16, 16)));
                }
            }
        }

        /*
        public override MaterialMod GetMod()
        {
            _materialMod.ShaderInfo = _shaderInfo;
            
            var colorSetData = new List<Half>();
            foreach(var data in ColorSetViewModels)
            {
                colorSetData.AddRange(data.GetList());
            }
            _materialMod.ColorSetData = colorSetData;
            
            return _materialMod;
        }
        */

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
