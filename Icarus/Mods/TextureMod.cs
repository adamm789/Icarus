using Icarus.Mods.Interfaces;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods
{
    public class TextureMod : Mod, ITextureGameFile
    {
        // TODO: Assignable TexType?
        public XivTexType TexType { get; set; }

        public Dictionary<XivTexType, XivTexFormat>? TypeFormatDict { get; set; }

        public TextureMod(bool isInternal=false)
        {
            IsInternal = isInternal;
        }
        
        public TextureMod(XivTex tex, bool isInternal = true)
        {
            Path = tex.TextureTypeAndPath.Path;
            IsInternal = isInternal;
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
                foreach (var kvp in TypeFormatDict)
                {
                    if (TexType == kvp.Key)
                    {
                        return kvp.Value;
                    }
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
