using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.FileTypes;

namespace Icarus.Services.GameFiles
{
    public class MetadataFileService : GameFileService, IMetadataFileService
    {
        public MetadataFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, ILogService logService) 
            : base(luminaService, itemListService, settingsService, logService)
        {
        }

        public async Task<IMetadataFile?> TryGetMetadata(string path, string? itemName = null)
        {
            IMetadataFile? metadataFile = null;

            try
            {
                var itemMetadata = await ItemMetadata.GetMetadata(path, true);
                var name = $"{path} (?)";
                var item = TryGetItem(path, itemName);

                if (item != null)
                {
                    name = item.Name;
                }
                var category = XivPathParser.GetCategoryFromPath(path);
                var slot = XivPathParser.GetEquipmentSlot(path);

                metadataFile = new MetadataFile()
                {
                    ItemMetadata = itemMetadata,
                    Path = path,
                    Name = name,
                    Category = category,
                    Slot = slot.GetShortHandSlot(false)
                };
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Exception caught while trying to get metadata: {path}.");
            }

            return metadataFile;
        }

        public async Task<IMetadataFile?> GetMetadata(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;
            var metadata = await TryGetMetadata(item.GetMetadataPath(), item.Name);
            return metadata;
        }
    }
}
