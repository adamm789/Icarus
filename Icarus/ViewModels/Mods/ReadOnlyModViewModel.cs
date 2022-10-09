using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using ItemDatabase.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods
{

    public class ReadOnlyModViewModel : ModViewModel
    {

        public byte[] Data;

        public ReadOnlyModViewModel(IMod mod, ItemListService itemListService, IGameFileService gameFileDataService)
            : base(mod, itemListService, gameFileDataService)
        {
            FileName = mod.ModFileName;
        }

        public override async Task<bool> SetDestinationItem(IItem? item = null)
        {
            //throw new NotImplementedException();
            Log.Information("Cannot set item on ReadOnlyMod.");
            return false;
        }
    }
}
