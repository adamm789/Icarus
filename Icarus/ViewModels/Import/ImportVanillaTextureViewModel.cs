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
using ItemDatabase.Interfaces;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaTextureViewModel : ImportVanillaFileViewModel
    {
        // chara/human/c0101/obj/face/f0001/texture/--c0101f0001_fac_d.tex
        readonly ITextureFileService _textureFileService;
        private IMaterialGameFile? _selectedMaterial;


        public ImportVanillaTextureViewModel(IModsListViewModel modPack, ITextureFileService textureFileService, ILogService logService)
    : base(modPack, logService)
        {
            _textureFileService = textureFileService;
        }


        public void SetMaterial(IMaterialGameFile? material)
        {
            _selectedMaterial = material;

            if (material == null)
            {
                AvailableTexTypes = null;
            }
            else
            {
                _selectedTexture = null;
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
            CanImport = AvailableTexTypes != null;
            CanChooseTexType = AvailableTexTypes != null;
        }

        bool _canChooseTexType = true;
        public bool CanChooseTexType
        {
            get { return _canChooseTexType; }
            set { _canChooseTexType = value; OnPropertyChanged(); }
        }

        public override async Task SetCompletePath(string? path)
        {
            await base.SetCompletePath(path);
            if (!XivPathParser.IsTex(path))
            {
                _selectedTexture = null;
                CanImport = false;
                CanChooseTexType = false;
            }
            else
            {
                _selectedMaterial = null;
                _selectedTexture = await _textureFileService.TryGetTextureFileData(path);
                CanImport = _selectedTexture != null;
                CanChooseTexType = false;
                AvailableTexTypes = null;
            }
        }
        ITextureGameFile? _selectedTexture;

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
            set {
                _selectedTexType = value;
                OnPropertyChanged();
                if (_selectedMaterial != null)
                {
                    var info = _selectedMaterial.XivMtrl.GetMapInfo(_selectedTexType, false);
                    SelectedTexturePath = info.Path;
                }
            }
        }

        string? _selectedTexturePath;
        public string? SelectedTexturePath
        {
            get { return _selectedTexturePath; }
            set { _selectedTexturePath = value; OnPropertyChanged(); }
        }

        protected override void DoImport()
        {
            if (_selectedMaterial != null)
            {
                var textureGameFile = Task.Run(() => _textureFileService.GetTextureFileData(_selectedMaterial, SelectedTexType)).Result;
                if (textureGameFile != null && textureGameFile.XivTex != null)
                {
                    var mod = new TextureMod(textureGameFile, ImportSource.Vanilla);
                    var modViewModel = _modPackViewModel.Add(mod);
                }
            }
            else if (_selectedTexture != null)
            {
                var mod = new TextureMod(_selectedTexture, ImportSource.Vanilla);
                _modPackViewModel.Add(mod);
            }
        }
    }
}
