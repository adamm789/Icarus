using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface ITextureFileService : IServiceProvider
    {
        Task<ITextureGameFile?> GetTextureFileData(IItem? item = null);
        Task<ITextureGameFile?> TryGetTextureFileData(string path, string name = "");
    }
}
