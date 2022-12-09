using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using Lumina.Data.Parsing;
using Lumina.Models.Models;
using SharpDX;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.IO;
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
        Mtrl _mtrl;

        Dictionary<string, List<string>> _materialCache = new();
        public MaterialFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, IModelFileService modelFileService, ILogService logService)
            : base(luminaService, itemListService, settingsService, logService)
        {
            _modelFileService = modelFileService;
        }

        protected override void OnLuminaSet()
        {
            base.OnLuminaSet();
            _mtrl = new(_frameworkGameDirectory);
        }

        public StainingTemplateFile GetStainingTemplateFile()
        {
            var bytes = _lumina.GetFile("chara/base_material/stainingtemplate.stm").Data;
            return new StainingTemplateFile(bytes);
        }

        // TODO: When itemList.Lenght == 1, we get the file data, but it isn't saved so when Add Mtrl is requested, it does this whole thing again
        public async Task<IMaterialGameFile?> GetMaterialFileData(IItem? itemArg = null, int materialSet = 1)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            try
            {
                var mtrl = new Mtrl(_frameworkGameDirectory);
                var mdlPath = item.GetMdlPath();
                var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(mdlPath));
                var ttModel = await mdl.GetModel(mdlPath);
                var mats = await mdl.GetReferencedMaterialNames(mdlPath);

                if (item is IGear gear)
                {
                    var mtrlPath = mtrl.GetMtrlPath(gear.GetMdlPath(), gear.GetMtrlFileName(), gear.MaterialId);
                    var xivMtrl = await mtrl.GetMtrlData(mtrlPath, gear.MaterialId);
                    return new MaterialGameFile()
                    {
                        Name = item.Name,
                        Path = mtrlPath,
                        Category = XivPathParser.GetCategoryFromPath(mtrlPath),
                        XivMtrl = xivMtrl
                    };
                    /*
                    var imcPath = gear.GetImcPath();
                    var imcFile = _lumina.GetFile<ImcFile>(imcPath);

                    var variant = gear.Variant;

                    if (gear.Slot == ItemDatabase.Enums.EquipmentSlot.Body)
                    {
                        var x = gear.MaterialId;
                        var matId = imcFile.GetPart(1).Variants[variant - 1].MaterialId;
                        var mtrlPath = mtrl.GetMtrlPath(item.GetMdlPath(), item.GetMtrlFileName(), matId);
                        var xivMtrl = await mtrl.GetMtrlData(mtrlPath, matId);
                        return new MaterialGameFile()
                        {
                            Name = item.Name,
                            Path = mtrlPath,
                            Category = XivPathParser.GetCategoryFromPath(mtrlPath),
                            XivMtrl = xivMtrl
                        };
                    }
                    */
                }
                else
                {

                    var mdlFile = _lumina.GetFile<MdlFile>(item.GetMdlPath());
                    var model = new Model(mdlFile);
                    var materials = model.Materials;

                    // TODO: Seems like I may have to look at the IMC to see what material a particular model uses

                    var mtrlPath = mtrl.GetMtrlPath(item.GetMdlPath(), materials[0].MaterialPath, materialSet);
                    var xivMtrl = await mtrl.GetMtrlData(mtrlPath, materialSet);

                    var materialPaths = await mdl.GetReferencedMaterialPaths(mdlPath, includeSkin: false);

                    return new MaterialGameFile()
                    {
                        Name = item.Name,
                        Path = mtrlPath,
                        Category = XivPathParser.GetCategoryFromPath(mtrlPath),
                        XivMtrl = xivMtrl
                    };
                }
            }
            catch (Exception ex)
            {

            }
            return null;
            /*
            var gear = item as IGear;
            var mtrl = new Mtrl(_frameworkGameDirectory);
            var xivMtrl = await mtrl.GetMtrlData(path);

            if (xivMtrl != null)
            {
                var category = XivPathParser.GetCategoryFromPath(path);

                return new MaterialGameFile()
                {
                    Name = item.Name,
                    Path = path,
                    Category = category,
                    XivMtrl = xivMtrl
                };
            }
            else
            {
                return null;
            }

            */
            /*
            var path = item.GetMtrlPath();
            var itemName = item.Name;
            var retVal = await TryGetMaterialFileData(path, itemName);
            var mtrl = new Mtrl(_frameworkGameDirectory);
            var x = await mtrl.GetMtrlData(path);
            if (item is IGear gear)
            {
                var y = await mtrl.GetMtrlData(path, gear.Variant);
            }
            if (retVal == null)
            {
                retVal = await TryGetMaterialFromModel(item.GetMdlPath());
            }
            return retVal;
            */
        }
        public async Task<int> GetNumMaterialSets(IItem? itemArg = null)
        {
            // TODO: This is currently wrong
            var item = GetItem(itemArg);
            if (item == null) return 0;
            var mdlPath = item.GetMdlPath();
            if (_materialCache.ContainsKey(mdlPath))
            {
                return _materialCache[mdlPath].Count;
            }
            else
            {
                var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(mdlPath));
                try
                {
                    var materialPaths = await mdl.GetReferencedMaterialPaths(mdlPath, includeSkin: false);
                    _materialCache.Add(mdlPath, materialPaths);
                    return materialPaths.Count;
                }
                catch (Exception ex)
                {
                    _logService.Debug(ex, "Caught exception during GetNumMaterialSets");
                    return -1;
                }
            }
        }

        public async Task<List<IMaterialGameFile>?> GetMaterialAndVariantsFileData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            var mtrl = new Mtrl(_frameworkGameDirectory);
            var modelPath = item.GetMdlPath();

            List<IMaterialGameFile> retVal = new();
            try
            {
                var mdlFile = _lumina.GetFile<MdlFile>(modelPath);
                var model = new Model(mdlFile);
                foreach (var material in model.Materials)
                {
                    var mtrlPath = mtrl.GetMtrlPath(modelPath, material.MaterialPath);
                    var xivMtrl = await mtrl.GetMtrlData(mtrlPath);
                    var category = XivPathParser.GetCategoryFromPath(mtrlPath);
                    if (xivMtrl != null)
                    {
                        var mat = new MaterialGameFile()
                        {
                            Name = item.Name,
                            Path = mtrlPath,
                            XivMtrl = xivMtrl,
                            Category = category
                        };
                        retVal.Add(mat);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return retVal;
            /*

            var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(modelPath));
            var xivMdl = await mdl.GetRawMdlData(modelPath, true);
            var ttModel = TTModel.FromRaw(xivMdl);

            var mtrlFileName = item.GetMtrlFileName();

            return new List<IMaterialGameFile>();
            */
            /*
            var set = _itemListService.GetMaterialSet(item);
            if (set != null)
            {
                var list = new List<IMaterialGameFile>();
                foreach (var m in set)
                {
                    var data = await GetMaterialFileData(m);
                    if (data != null)
                    {
                        list.Add(data);
                    }
                }
                return list;
            }
            return null;
            */
        }

        public async Task<List<IMaterialGameFile>?> GetMaterialSet(IItem? itemArg = null)
        {
            try
            {
                var item = GetItem(itemArg);
                if (item == null) return null;

                var mdlPath = item.GetMdlPath();
                var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(mdlPath));

                var retVal = new List<IMaterialGameFile>();
                var category = XivPathParser.GetCategoryFromPath(mdlPath);

                List<string> materialPaths;

                if (item is IGear gear)
                {
                    var sharedModels = _itemListService.GetSharedModels(gear);
                    if (sharedModels == null) return null;

                    materialPaths = new();
                    var ret = new List<IMaterialGameFile>();
                    var seenPaths = new List<string>();
                    foreach (var m in sharedModels)
                    {
                        var paths = await mdl.GetReferencedMaterialPaths(m.GetMdlPath(), m.MaterialId, includeSkin: false);
                        foreach (var path in paths)
                        {
                            if (XivPathParser.IsSkinMtrl(path)) continue;
                            if (seenPaths.Contains(path)) continue;
                            var xivMtrl = await _mtrl.GetMtrlData(path);

                            if (xivMtrl == null) continue;
                            var mat = new MaterialGameFile()
                            {
                                Name = m.Name,
                                Path = path,
                                XivMtrl = xivMtrl,
                                Category = category,
                                MaterialSet = m.MaterialId,
                                Variant = XivPathParser.GetMtrlVariant(path)
                            };
                            ret.Add(mat);
                            seenPaths.Add(path);
                        }
                    }
                    return ret;
                }

                var model = await mdl.GetModel(mdlPath, true);
                materialPaths = model.Materials;

                foreach (var path in materialPaths)
                {
                    var xivMtrl = await _mtrl.GetMtrlData(path);
                    if (xivMtrl == null) continue;
                    var materialSet = XivPathParser.GetMtrlSetVariant(path);

                    var mat = new MaterialGameFile()
                    {
                        Name = item.Name,
                        Path = path,
                        XivMtrl = xivMtrl,
                        Category = category,
                        MaterialSet = materialSet,
                        Variant = XivPathParser.GetMtrlVariant(path)
                    };
                    if (mat != null)
                    {
                        retVal.Add(mat);
                    }
                }
                return retVal;
            }
            catch (Exception)
            {
                return null;
            }
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
                var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(modelPath));
                var mtrl = new Mtrl(_frameworkGameDirectory);
                var xivMdl = await mdl.GetRawMdlData(modelPath, true);
                var ttModel = TTModel.FromRaw(xivMdl);
                // TODO: What to do when a model has multiple materials?

                var path = mtrl.GetMtrlPath(modelPath, ttModel.Materials[0]);
                var xivMtrl = await mtrl.GetMtrlData(path);
                return new MaterialGameFile()
                {
                    Name = ttModel.Source,
                    Category = XivPathParser.GetCategoryFromPath(path),
                    Path = path,
                    XivMtrl = xivMtrl
                };
            }
            catch (Exception ex)
            {
                _logService.Error(ex, "An exception was caught when trying to get a material from a model.");
                return null;
            }
            /*
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
                    if (mtrlFile == null)
                    {
                        return null;
                    }
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
            catch (Exception ex)
            {
                _logService.Debug(ex, "Exception thrown when trying to get material from model");
            }
            return null;
            */
        }
    }
}
