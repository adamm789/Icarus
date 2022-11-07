using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods
{
    /// <summary>
    /// ModViewModel provided as a courtesy to be able to include ttmp2 files that are not models, textures, materials, or metadata
    /// Can only be exported to ttmp2
    /// </summary>
    public class ReadOnlyModViewModel : ModViewModel
    {
        public byte[] Data;

        public ReadOnlyModViewModel(IMod mod, IGameFileService gameFileService, ILogService logService)
            : base(mod, gameFileService, logService)
        {
            FileName = mod.ModFileName;
        }

        public override async Task<bool> SetDestinationItem(IItem? item = null)
        {
            _logService.Information("Cannot set item on ReadOnlyMod.");
            return false;
        }
    }
}
