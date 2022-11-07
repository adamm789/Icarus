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
        // TODO: Re-purpose this so as to not have two "GetMaterialFileData" functions
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item, string variant = "a");

        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");
        StainingTemplateFile GetStainingTemplateFile();
    }
}
