using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMaterialFileService : IGameFileService
    {
        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item, int materialSet = 1);
        Task<Dictionary<string, List<IMaterialGameFile>>?> GetMaterialSetsDict(IItem? itemArg = null);
        Task<List<IMaterialGameFile>?> GetMaterialSet(IItem? itemArg = null);

        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");
        Task<IMaterialGameFile?> TryGetMaterialFromName(string name);
        Task<List<IMaterialGameFile>?> GetMaterials(IModelGameFile modelGameFile);
        Task<IMaterialGameFile> CopyMaterialGameFile(IMaterialGameFile materialGameFile);
        StainingTemplateFile GetStainingTemplateFile();
        IMaterialGameFile? SelectedMaterialFile { get; set; }
        List<IMaterialGameFile>? MaterialSet { get; set; }
    }
}
