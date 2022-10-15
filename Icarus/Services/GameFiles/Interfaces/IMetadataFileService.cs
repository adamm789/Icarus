using Icarus.Mods;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IMetadataFileService
    {
        Task<MetadataMod> TryGetMetadata(string path, string? itemName = null);
        Task<MetadataMod> GetMetadata(IItem? itemArg = null);
    }
}
