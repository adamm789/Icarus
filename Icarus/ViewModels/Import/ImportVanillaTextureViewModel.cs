using Icarus.Mods.Interfaces;
using Icarus.Mods;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;
using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaTextureViewModel : ImportVanillaFileViewModel
    {
        // chara/human/c0101/obj/face/f0001/texture/--c0101f0001_fac_d.tex
        readonly ITextureFileService _textureFileService;
        readonly ImportVanillaMaterialViewModel _materialViewModel;
        private IMaterialGameFile? _selectedMaterial;

        public ImportVanillaTextureViewModel(IModsListViewModel modPack, ImportVanillaMaterialViewModel importVanillaMaterialViewModel, ItemListViewModel itemListViewModel, ITextureFileService textureFileService, ILogService logService)
    : base(modPack, itemListViewModel, logService)
        {
            _materialViewModel = importVanillaMaterialViewModel;
            _textureFileService = textureFileService;
        }

        public ImportVanillaTextureViewModel(IModsListViewModel modPack, ItemListViewModel itemListViewModel, ITextureFileService textureFileService, ILogService logService)
            : base(modPack, itemListViewModel, logService)
        {
            _textureFileService = textureFileService;
        }

        public void SetMaterial(IMaterialGameFile? material)
        {
            if (material == null)
            {
                AvailableTexTypes = null;
            }
            else
            {
                var xivMtrl = material.XivMtrl;
                var texTypes = new List<XivTexType>();
                foreach (var texTypePath in xivMtrl.TextureTypePathList)
                {
                    if (texTypePath.Type == XivTexType.ColorSet) continue;
                    texTypes.Add(texTypePath.Type);
                }
                if (texTypes == null || texTypes.Count == 0)
                {
                    AvailableTexTypes = null;
                }
                else
                {
                    AvailableTexTypes = new(texTypes);
                    SelectedTexType = AvailableTexTypes[0];
                }
            }
            _selectedMaterial = material;
            CanImport = AvailableTexTypes != null;
            CanChooseTexType = AvailableTexTypes != null;
        }

        public override void CompletePathSet()
        {
            base.CompletePathSet();
            CanImport = XivPathParser.IsTex(_completePath);
        }

        bool _canChooseTexType = true;
        public bool CanChooseTexType
        {
            get { return _canChooseTexType; }
            set { _canChooseTexType = value; OnPropertyChanged(); }
        }

        /*
        public override async Task SelectedItemSetAsync()
        {
            await base.SelectedItemSetAsync();
            if (SelectedItem == null)
            {
                AvailableTexTypes = null;
                CanChooseTexType = false;
            }
            else
            {
                var texTypes = await _textureFileService.GetAvailableTexTypes(SelectedItem);
                if (texTypes == null || texTypes.Count == 0)
                {
                    AvailableTexTypes = null;
                }
                else
                {
                    AvailableTexTypes = new(texTypes);
                    SelectedTexType = AvailableTexTypes[0];
                    CanChooseTexType = true;
                }
            }
        }
        */

        ObservableCollection<XivTexType>? _availableTexTypes;
        public ObservableCollection<XivTexType>? AvailableTexTypes
        {
            get { return _availableTexTypes; }
            set { _availableTexTypes = value; OnPropertyChanged(); }
        }

        XivTexType _selectedTexType = XivTexType.Normal;
        public XivTexType SelectedTexType
        {
            get { return _selectedTexType; }
            set { _selectedTexType = value; OnPropertyChanged(); }
        }

        protected override async Task DoImport()
        {
            //await GetVanillaTex();
            if (_selectedMaterial == null) return;
            var textureGameFile = await _textureFileService.GetTextureFileData(_selectedMaterial, SelectedTexType);
            if (textureGameFile != null && textureGameFile.XivTex != null)
            {
                var mod = new TextureMod(textureGameFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
            }
        }

        private async Task<TextureMod?> GetVanillaTex()
        {
            ITextureGameFile? textureGameFile;

            if (_completePath != null)
            {
                textureGameFile = await _textureFileService.TryGetTextureFileData(_completePath, SelectedItemName);
            }
            else
            {
                textureGameFile = await _textureFileService.GetTextureFileData(SelectedItem, SelectedTexType);
            }

            if (textureGameFile != null && textureGameFile.XivTex != null)
            {
                var mod = new TextureMod(textureGameFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
                SelectedTexType = textureGameFile.TexType;
                return mod;
            }

            return null;
        }
    }
}
