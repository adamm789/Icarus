using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Threading.Tasks;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMaterialFileService : IGameFileService
    {
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item, string variant = "a");

        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");
        StainingTemplateFile GetStainingTemplateFile();
    }
}
