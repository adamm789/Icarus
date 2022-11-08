using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaViewModel : ViewModelBase
    {
        public ImportVanillaModelViewModel ImportVanillaModelViewModel { get; set; }
        public ImportVanillaMaterialViewModel ImportVanillaMaterialViewModel { get; set; }
        public ImportVanillaMetadataViewModel ImportVanillaMetadataViewModel { get; set; }
        public ImportVanillaTextureViewModel ImportVanillaTextureViewModel { get; set; }

        public ImportVanillaViewModel(IModsListViewModel modPack, ItemListViewModel itemListService, VanillaFileService vanillaFileService, ILogService logService) 
            : base(logService)
        {
            ImportVanillaModelViewModel = new(modPack, itemListService, vanillaFileService.ModelFileService, logService);
            ImportVanillaMaterialViewModel = new(modPack, itemListService, vanillaFileService.MaterialFileService, logService);
            ImportVanillaMetadataViewModel = new(modPack, itemListService, vanillaFileService.MetadataFileService, logService);
            ImportVanillaTextureViewModel = new(modPack, itemListService, vanillaFileService.TextureFileService, logService);
        }
    }
}
