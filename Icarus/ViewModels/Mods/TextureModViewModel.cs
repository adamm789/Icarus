using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    // TODO: Combo box available before assignment of item

    // TODO: How to allow user to edit texture?
    // drop-down with tex type?
    // change destination path and hope for the best?

    // TODO: Try to change texture destination path while no item is selected, doesn't work?
    public class TextureModViewModel : ModViewModel
    {
        TextureMod _textureMod;
        readonly ITextureFileService _textureFileService;
        public TextureModViewModel(TextureMod mod, ITextureFileService textureFileService, ILogService logService)
            : base(mod, textureFileService, logService)
        {
            _textureMod = mod;
            _textureFileService = textureFileService;
            _textureVariant = XivPathParser.GetTexVariant(mod.Path);
            _texType = _textureMod.TexType;
            SetCanExport();
            TexTypeValues = new()
            {
                XivTexType.Normal,
                XivTexType.Multi,
                XivTexType.Diffuse,
                XivTexType.Specular
            };
        }

        //ObservableCollection<string> _additionalPaths = new();
        public ObservableCollection<string> AdditionalPaths = new();

        // TODO: Should this just hide the TexType combo box?
        bool _canParseTexType = true;
        public bool CanParseTexType
        {
            get { return _canParseTexType; }
            set { _canParseTexType = value; OnPropertyChanged(); }
        }

        string _textureVariant = "a";
        public string TextureVariant
        {
            get { return _textureVariant; }
            set
            {
                _textureVariant = value;
                OnPropertyChanged();
                var path = XivPathParser.ChangeTexVariant(base.DestinationPath, value);
                path = XivPathParser.ChangeTexType(path, TexType);
                base.DestinationPath = path;
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
                    OnPropertyChanged(nameof(TexType));
                    CanParseTexType = true;
                }
                catch (ArgumentException) {
                    CanParseTexType = false;
                }
                base.DestinationPath = value;
                
            }
        }

        XivTexType _texType = XivTexType.Normal;
        public XivTexType TexType
        {
            get { return _texType; }
            set
            {
                _texType = value;
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

        int _materialSet = 1;
        public int MaterialSet
        {
            get { return _materialSet; }
            set { _materialSet = value; OnPropertyChanged(); }
        }

        public override async Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            // Change this so it basically only gets the path?
            var ret = await _textureFileService.GetTextureFileData(itemArg, TexType, TextureVariant);
            if (ret == null)
            {
                ret = await _textureFileService.GetTextureFileData(itemArg, XivTexType.Normal, TextureVariant);
                if (ret != null)
                {
                    ret.TexType = TexType;
                    ret.Path = XivPathParser.ChangeTexType(ret.Path, TexType);
                    ret.Path = XivPathParser.ChangeTexVariant(ret.Path, TextureVariant);
                    ret.XivTex.TextureTypeAndPath.Type = TexType;
                    ret.XivTex.TextureTypeAndPath.Path = ret.Path;
                }
            }
            return ret;
        }

        public override async Task<IGameFile?> GetFileData(string path, string name= "")
        {
            return await _textureFileService.TryGetTextureFileData(path, name);
        }

        public override bool SetModData(IGameFile gameFile)
        {
            if (gameFile is not ITextureGameFile texGameFile)
            {
                return false;
            }
            //_logService.Debug($"Adding: {texGameFile.Path}");
            //_textureMod.AdditionalPaths.Add(texGameFile.Path);
            CanParseTexType = true;
            return base.SetModData(texGameFile);
        }

        public List<XivTexType> TexTypeValues { get; }

        // TODO: Remove duplicate, make static?
        public List<string> VariantList { get; } = new()
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        protected override bool HasValidPathExtension(string path)
        {
            return Path.GetExtension(path) == ".tex";
        }
    }
}
