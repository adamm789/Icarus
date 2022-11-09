using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Threading.Tasks;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMaterialFileService : IGameFileService
    {
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item);

        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");
        Task<IMaterialGameFile?> TryGetMaterialFromName(string name);
        StainingTemplateFile GetStainingTemplateFile();
    }
}
