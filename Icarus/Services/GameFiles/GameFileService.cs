using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Mods;
using ItemDatabase;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using Lumina.Models.Materials;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Mods.DataContainers;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using xivModdingFramework.Textures.FileTypes;
using Path = System.IO.Path;

namespace Icarus.Services.GameFiles
{
    public class GameFileService : LuminaDependentServiceBase<GameFileService>, IGameFileService
    {
        protected readonly IItemListService _itemListService;
        protected readonly ILogService _logService;
        protected readonly ISettingsService _settingsService;
        protected DirectoryInfo _frameworkGameDirectory;

        public GameFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, ILogService logService) 
            : base(luminaService)
        {
            _itemListService = itemListService;
            _settingsService = settingsService;
            _logService = logService;
        }

        protected override void OnLuminaSet()
        {
            var gameDirectory = _settingsService.GameDirectoryLumina;
            _frameworkGameDirectory = new(Path.Combine(gameDirectory, "ffxiv"));
        }

        public IItem? GetItem(IItem? item = null)
        {
            if (item == null)
            {
                if (_itemListService.SelectedItem != null)
                {
                    var retItem = _itemListService.SelectedItem;
                    _logService.Verbose($"Using ItemList item: {retItem.Name}");
                    return retItem;
                }
                else
                {
                    _logService.Debug("Could not get Item file. None was given.");
                    return null;
                }
            }
            else
            {
                _logService.Verbose($"Using supplied item: {item.Name}.");
                return item;
            }
        }

        protected IItem? TryGetItem(string path, string itemName = "")
        {
            return _itemListService.TryGetItem(path, itemName);
            /*
            List<IItem> results = new();
            if (!String.IsNullOrWhiteSpace(itemName))
            {
                results = _itemListService.Search(itemName, true);
                foreach (var r in results)
                {
                    if (r.Name == itemName)
                    {
                        return r;
                    }
                }
            }

            if (results.Count == 0)
            {
                results = _itemListService.Search(path);
            }
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
            */
        }

        protected string TryGetName(string path, string itemName="")
        {
            var result = _itemListService.TryGetName(path, itemName);
            if (String.IsNullOrWhiteSpace(result))
            {
                return "???";
            }
            else
            {
                return result;
            }
        }
    }
}
