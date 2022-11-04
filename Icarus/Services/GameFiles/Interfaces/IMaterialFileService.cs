using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Threading.Tasks;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMaterialFileService : IServiceProvider
    {
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item);
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item, string variant);

        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");
        StainingTemplateFile GetStainingTemplateFile();
    }
}
