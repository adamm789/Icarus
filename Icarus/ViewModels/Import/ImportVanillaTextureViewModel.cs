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
        public ImportVanillaTextureViewModel(IModsListViewModel modPack, ItemListViewModel itemListViewModel, ITextureFileService textureFileService, ILogService logService)
            : base(modPack, itemListViewModel, logService)
        {
            _textureFileService = textureFileService;
        }

        protected override void CompletePathSet()
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

        protected override void SelectedItemSet()
        {
            base.SelectedItemSet();
            if (SelectedItem == null)
            {
                AvailableTexTypes = null;
                CanChooseTexType = false;
            }
            else
            {
                var texTypes = _textureFileService.GetAvailableTexTypes(SelectedItem);
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
            await GetVanillaTex();
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
