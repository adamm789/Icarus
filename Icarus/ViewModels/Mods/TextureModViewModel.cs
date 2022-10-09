using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    // TODO: How to allow user to edit texture?
    // drop-down with tex type?
    // change destination path and hope for the best?
    public class TextureModViewModel : ModViewModel
    {
        TextureMod _textureMod;
        public TextureModViewModel(TextureMod mod, ItemListService itemListService, IGameFileService gameFileDataService) : base(mod, itemListService, gameFileDataService)
        {
            _textureMod = mod;
            SetCanExport();
            TexTypeValues = new()
            {
                XivTexType.Normal,
                XivTexType.Multi,
                XivTexType.Diffuse,
                XivTexType.Specular
            };
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
                    Log.Error($"Could not set {_textureMod.TexType} to {DestinationPath}.");
                }
            }
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
        // = Enum.GetValues(typeof(XivTexType)).Cast<XivTexType>().ToList();
    }
}
