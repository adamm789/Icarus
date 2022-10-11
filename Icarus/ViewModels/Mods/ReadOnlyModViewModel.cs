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
            //throw new NotImplementedException();
            _logService.Information("Cannot set item on ReadOnlyMod.");
            return false;
        }
    }
}
