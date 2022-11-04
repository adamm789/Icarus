using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    // TODO: How to allow user to edit texture?
    // drop-down with tex type?
    // change destination path and hope for the best?
    public class TextureModViewModel : ModViewModel
    {
        TextureMod _textureMod;
        public TextureModViewModel(TextureMod mod, IGameFileService gameFileService, ILogService logService)
            : base(mod, gameFileService, logService)
        {
            _textureMod = mod;
            _textureVariant = XivPathParser.GetTexVariant(mod.Path);
            SetCanExport();
            TexTypeValues = new()
            {
                XivTexType.Normal,
                XivTexType.Multi,
                XivTexType.Diffuse,
                XivTexType.Specular
            };
        }

        string _textureVariant = "a";
        public string TextureVariant
        {
            get { return _textureVariant; }
            set
            {
                _textureVariant = value;
                OnPropertyChanged();
                base.DestinationPath = XivPathParser.ChangeTexVariant(base.DestinationPath, value);
            }
        }

        public override string DestinationPath
        {
            get => base.DestinationPath;
            set
            {
                TextureVariant = XivPathParser.GetTexVariant(value);
                try
                {
                    _textureMod.TexType = XivPathParser.GetTexType(value);
                }
                catch (ArgumentException) { }
                base.DestinationPath = value;
            }
        }

        public XivTexType TexType
        {
            get { return _textureMod.TexType; }
            set
            {
                _textureMod.TexType = value;
                OnPropertyChanged();
                try
                {
                    // TODO: When I cannot parse the path to change, what should I Change the TexType to?
                    // Should this even be an option to directly change? or just something calculated at export time?
                    var newPath = XivPathParser.ChangeTexType(DestinationPath, _textureMod.TexType);
                    DestinationPath = newPath;
                }
                catch (ArgumentOutOfRangeException)
                {
                    _logService.Error($"Could not set {_textureMod.TexType} to {DestinationPath}.");
                }
            }
        }

        public override async Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return await _gameFileService.GetTextureFileData(itemArg, TextureVariant);
        }

        public override bool SetModData(IGameFile gameFile)
        {
            if (gameFile is not ITextureGameFile texGameFile)
            {
                return false;
            }
            return base.SetModData(texGameFile);
        }

        public List<XivTexType> TexTypeValues { get; }

        // TODO: Remove duplicate, make static?
        public List<string> VariantList { get; } = new()
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };
    }
}
