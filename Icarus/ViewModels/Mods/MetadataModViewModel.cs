using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods
{
    public class MetadataModViewModel : ModViewModel
    {
        public MetadataModViewModel(IMod mod, ItemListService itemListService, GameFileService gameFileDataService) : base(mod, itemListService, gameFileDataService)
        {
        }

        public override Task SetDestinationItem(IItem? item = null)
        {
            throw new NotImplementedException();
        }

        public override bool TrySetDestinationPath(string item)
        {
            throw new NotImplementedException();
        }
    }
}
