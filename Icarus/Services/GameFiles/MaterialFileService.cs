using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using Lumina.Models.Models;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Cache;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.ModelTextures;

namespace Icarus.Services.GameFiles
{
    public class MaterialFileService : GameFileService, IMaterialFileService
    {
        // TODO: Aurum Jacket has "variant" = 106, but uses material 45
        IModelFileService _modelFileService;
        public MaterialFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, IModelFileService modelFileService, ILogService logService)
            : base(luminaService, itemListService, settingsService, logService)
        {
            _modelFileService = modelFileService;
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

            var retVal = await TryGetMaterialFileData(path, itemName);
            if (retVal == null)
            {
                retVal = await TryGetMaterialFromModel(item.GetMdlPath());

            }
            return retVal;
        }

        public async Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string itemName = "")
        {
            var xivMtrl = await TryGetMaterialFromPath(path);
            if (xivMtrl == null)
            {
                var tempPath = XivPathParser.ChangeMtrlVariant(path);
                xivMtrl = await TryGetMaterialFromPath(tempPath);
            }
            if (xivMtrl != null)
            {
                var category = XivPathParser.GetCategoryFromPath(path);
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
                _logService.Debug($"Could not find material via lumina {path}");
            }

            try
            {
                var mtrl = new Mtrl(_frameworkGameDirectory);
                var xivMtrl = await mtrl.GetMtrlData(path);
            }
            catch (Exception ex)
            {
                _logService.Debug(ex, $"Exception occurred while trying to get material from path: {path}");
            }
            return null;
        }

        // TODO: This still currently seems to not select the correct material for e.g. Aurum Jacket
        // For now, it at least allows the option to get the variant 1
        private async Task<IMaterialGameFile?> TryGetMaterialFromModel(string modelPath)
        {
            try
            {
                if (_lumina.FileExists(modelPath))
                {
                    var mtrl = new Mtrl(_frameworkGameDirectory);

                    var mdlFile = _lumina.GetFile<MdlFile>(modelPath);
                    var model = new Model(mdlFile);
                    var material = model.Materials.FirstOrDefault();
                    material.Update(_lumina);

                    var mtrlFile = _lumina.GetFile<MtrlFile>(material.ResolvedPath);
                    var xivMtrl = await MtrlExtensions.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, material.ResolvedPath);

                    var retVal = new MaterialGameFile()
                    {
                        Name = TryGetName(modelPath),
                        Category = XivPathParser.GetCategoryFromPath(modelPath),
                        Path = material.ResolvedPath,
                        XivMtrl = xivMtrl
                    };
                    return retVal;

                }
            }
            catch (ArgumentOutOfRangeException) { }
            return null;
        }
    }
}
