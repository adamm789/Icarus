using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles
{
    public class MaterialFileService : GameFileService, IMaterialFileService
    {
        public MaterialFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, ILogService logService) 
            : base(luminaService, itemListService, settingsService, logService)
        {

        }

        public StainingTemplateFile GetStainingTemplateFile()
        {
            var bytes = _lumina.GetFile("chara/base_material/stainingtemplate.stm").Data;
            return new StainingTemplateFile(bytes);
        }

        public async Task<IMaterialGameFile?> GetMaterialFileData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            // We actually don't want 
            var path = item.GetMtrlPath();
            var itemName = item.Name;
            return await TryGetMaterialFileData(path, itemName);
        }

        public async Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string itemName = "")
        {
            var xivMtrl = await TryGetMaterialFromPath(path);
            if (xivMtrl != null)
            {
                var category = XivPathParser.GetCategory(path);
                /*
                var name = "???";
                if (!String.IsNullOrWhiteSpace(itemName))
                {
                    name = itemName;
                }
                else
                {
                    var result = TryGetItem(path, itemName);
                    if (result != null)
                    {
                        name = result.Name;
                    }
                }
                */
                var name = TryGetName(path, itemName);
                var retVal = new MaterialGameFile()
                {
                    Name = name,
                    Path = path,
                    Category = category,
                    XivMtrl = xivMtrl
                };

                return retVal;
            }
            return null;
        }

        public async Task<IMaterialGameFile?> TryGetMaterialFromName(string name)
        {
            var results = _itemListService.Search(name, true);
            if (results.Count > 0)
            {
                return await GetMaterialFileData(results[0]);
            }
            else
            {
                return null;
            }
        }

        private async Task<XivMtrl?> TryGetMaterialFromPath(string path)
        {
            try
            {
                if (_lumina.FileExists(path))
                {
                    var mtrlFile = _lumina.GetFile<MtrlFile>(path);
                    var xivMtrl = await MtrlExtensions.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, path);
                    return xivMtrl;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logService.Debug($"Could not find material {path}");
            }
            return null;
        }

        /*
        public async Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string itemName = "")
        {
            var result = TryGetItem(path, itemName);
            if (result != null)
            {
                return await GetMaterialFileData(result);
            }

            var xivMtrl = await TryGetMaterialFromPath(path);
            if (xivMtrl != null)
            {
                var category = XivPathParser.GetCategory(path);
                var name = path;
                if (result != null)
                {
                    name = result.Name;
                }
                // TODO: Try/Catch, null checks, etc.
                var retVal = new MaterialGameFile()
                {
                    Name = name,
                    Path = path,
                    Category = category,
                    XivMtrl = xivMtrl
                };

                return retVal;
            }
            return null;
        }

        public async Task<IMaterialGameFile?> GetMaterialFileData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            var mtrl = new Mtrl(_frameworkGameDirectory);

            XivMtrl? xivMtrl;
            string path = "";
            var category = "";
            if (item is IGear gear)
            {
                xivMtrl = await mtrl.GetMtrlData(gear.GetMtrlPath());
                category = gear.Slot.ToString();
            }
            else
            {
                xivMtrl = await TryGetMaterialFromPath(item.GetMtrlPath());
                category = XivPathParser.GetCategory(item.GetMtrlPath());
            }

            if (xivMtrl == null) return null;

            var retVal = new MaterialGameFile()
            {
                //Item = item,
                Name = item.Name,
                Path = item.GetMtrlPath(),
                Category = category,
                XivMtrl = xivMtrl,
            };
            return retVal;
        }

        public async Task<IMaterialGameFile?> GetMaterialFileData(IItem? itemArg = null, string variant = "a")
        {
            var gameFile = await GetMaterialFileData(itemArg);
            if (gameFile != null)
            {
                gameFile.Path = XivPathParser.ChangeMtrlVariant(gameFile.Path, variant);
                gameFile.XivMtrl.MTRLPath = XivPathParser.ChangeMtrlVariant(gameFile.XivMtrl.MTRLPath, variant);
            }

            return gameFile;
        }
        */
    }
}
