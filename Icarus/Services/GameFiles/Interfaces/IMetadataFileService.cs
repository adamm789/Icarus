using Icarus.Mods;
using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMetadataFileService : IGameFileService
    {
        Task<IMetadataFile?> TryGetMetadata(string path, string? itemName = null);
        Task<IMetadataFile?> GetMetadata(IItem? itemArg = null);
    }
}
