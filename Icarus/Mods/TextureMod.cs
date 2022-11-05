using Icarus.Mods.Interfaces;
using Icarus.Util.Import;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods
{
    public class TextureMod : Mod, ITextureGameFile
    {
        // TODO: Assignable TexType?
        public XivTexType TexType { get; set; }

        public Dictionary<XivTexType, XivTexFormat>? TypeFormatDict { get; set; }

        // Only set when source is from ttmp2
        public XivTex? XivTex { get; }

        public TextureMod(ImportSource source) : base(source)
        {

        }

        public TextureMod(XivTex tex, ImportSource source = ImportSource.Vanilla) : base(source)
        {
            XivTex = tex;
            Path = tex.TextureTypeAndPath.Path;
        }

        public override bool IsComplete()
        {
            return !String.IsNullOrWhiteSpace(Path) && !String.IsNullOrWhiteSpace(ModFilePath);
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not ITextureGameFile texGameFile)
            {
                throw new ArgumentException($"ModData for texture was not of MaterialGameFile. It was {gameFile.GetType()}.");
            }
            //base.SetModData(gameFile);
            TypeFormatDict = texGameFile.TypeFormatDict;
            Path = XivPathParser.ChangeTexType(gameFile.Path, TexType);
            Name = gameFile.Name;
            Category = gameFile.Category;
        }

        public XivTexFormat GetTexFormat()
        {
            if (TypeFormatDict != null)
            {
                // Try to get the format from the parent material
                var found = TypeFormatDict.TryGetValue(TexType, out var format);
                if (found)
                {
                    Log.Debug($"{Path} found TexType: {TexType} and produced {format}");
                    return format;
                }
            }

            if (TexType == XivTexType.Normal)
            {
                return XivTexFormat.DXT5;
            }
            if (TexType == XivTexType.Multi)
            {
                return XivTexFormat.DXT1;
            }

            return XivTexFormat.A8R8G8B8;
        }
    }
}
