using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;

namespace Icarus.ViewModels.Mods
{
    
    public class MetadataModViewModel : ModViewModel
    {
        public MetadataModViewModel(IMod mod, GameFileService gameFileDataService, ILogService logService)
            : base(mod, gameFileDataService, logService)
        {

        }
    }
}
