using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using ItemDatabase.Interfaces;
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
        // TODO: Need to handle case of importing .dds file
        // TODO: call SetCanExport... at some point

        MaterialMod _materialMod;
        IWindowService _windowService;
        ShaderInfoViewModel _shaderInfoViewModel;
        public ShaderInfoViewModel ShaderInfoViewModel
        {
            get { return _shaderInfoViewModel; }
            set { _shaderInfoViewModel = value; OnPropertyChanged(); }
        }

        // TODO: Include a section that shows the overall edits to all items
        // e.g. Group and display the mods that affect e6111
        public MaterialModViewModel(MaterialMod mod, ItemListService itemListService, IGameFileService gameFileDataService, IWindowService windowService)
            : base(mod, itemListService, gameFileDataService)
        {
            _materialMod = mod;
            _windowService = windowService;
            ShaderInfoViewModel = new ShaderInfoViewModel(mod);

            var colorSetData = new List<List<Color>>();
            for (var i = 0; i < 16; i++)
            {
                colorSetData.Add(new List<Color>());
                for (int j = 0; j < 4; j++)
                {
                    var color = GetColorSetDataRange(_materialMod.GetColorSetData(), (i * 16) + (j * 4));
                    colorSetData[i].Add(color);
                }
            }

            foreach (var set in colorSetData)
            {
                ColorSetViewModels.Add(new ColorSetRowViewModel(set));
            }
        }

        private Color GetColorSetDataRange(List<Half> values, int index)
        {
            return ListToColor(values.GetRange(index, 4));
        }

        private Color ListToColor(List<Half> values)
        {
            if (values.Count != 4)
            {
                throw new ArgumentException("List was not of length four.");
            }
            return new Color(values[0], values[1], values[2], values[3]);
        }

        ObservableCollection<ColorSetRowViewModel> _colorSetViewModels = new();
        public ObservableCollection<ColorSetRowViewModel> ColorSetViewModels
        {
            get { return _colorSetViewModels; }
            set { _colorSetViewModels = value; OnPropertyChanged(); }
        }
        
        public override string DestinationPath
        {
            get { return _mod.Path; }
            set
            {
                if (MaterialMod.IsValidMtrlPath(value))
                {
                    _mod.Path = value;
                    OnPropertyChanged();
                    ShaderInfoViewModel.UpdatePaths(value);
                }
            }
        }
        
        public override async Task SetDestinationItem(IItem? item = null)
        {
            var modData = await _gameFileService.GetMaterialFileData(item);
            var path = _itemListService.SelectedItem;
            if (path == null)
            {
                return;
            }
            DestinationPath = path.GetMtrlPath();
            
        }        

        private ShaderInfo _shaderInfo => _shaderInfoViewModel.ShaderInfo;

        public override bool TrySetDestinationPath(string item)
        {
            throw new NotImplementedException();
        }

        DelegateCommand _openMaterialEditorCommand;
        public DelegateCommand OpenMaterialEditorCommand
        {
            get { return _openMaterialEditorCommand ??= new DelegateCommand(o => OpenMaterialEditor()); }
        }

        public void OpenMaterialEditor()
        {
            _windowService.ShowWindow<MaterialEditorWindow>(this);
        }
    }
}
