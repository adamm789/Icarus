using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface ITextureFileService : IGameFileService
    {
        List<XivTexType>? GetAvailableTexTypes(IItem? itemArg = null);
        Task<ITextureGameFile?> GetTextureFileData(IItem? item = null, XivTexType type = XivTexType.Normal, string variant="a");
        Task<ITextureGameFile?> TryGetTextureFileData(string path, string name = "");
    }
}
