using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;

namespace Icarus.ViewModels.Mods
{
    public class MetadataModViewModel : ModViewModel
    {
        public MetadataModViewModel(IMod mod, ItemListService itemListService, GameFileService gameFileDataService) : base(mod, itemListService, gameFileDataService)
        {

        }
    }
}
