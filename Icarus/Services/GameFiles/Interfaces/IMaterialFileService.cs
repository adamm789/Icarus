using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMaterialFileService : IServiceProvider
    {
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item);
        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");

    }
}
