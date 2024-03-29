﻿using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using Lumina.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.FileTypes;

namespace Icarus.Services.GameFiles
{
    public class MaterialFileService : GameFileService, IMaterialFileService
    {
        IModelFileService _modelFileService;
        Mtrl _mtrl;

        public IMaterialGameFile? SelectedMaterialFile { get; set; }
        public List<IMaterialGameFile>? MaterialSet { get; set; }

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

        public async Task<IMaterialGameFile> CopyMaterialGameFile(IMaterialGameFile materialGameFile)
        {
            var mtrl = new Mtrl(_frameworkGameDirectory);
            var xivMtrl = await mtrl.GetMtrlData(materialGameFile.Path);
            if (xivMtrl == null)
            {
                _logService.Fatal($"Could not get mtrl from {materialGameFile.Path}");
            }
            return new MaterialGameFile()
            {
                Name = materialGameFile.Name,
                Path = materialGameFile.Path,
                Category = materialGameFile.Category,
                XivMtrl = xivMtrl,
                MaterialSet = materialGameFile.MaterialSet,
                Variant = materialGameFile.Variant
            };
        }

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

                string name = item.Name;
                string? mtrlPath;
                XivMtrl? xivMtrl;

                if (item is IGear gear)
                {
                    var mats = await mdl.GetReferencedMaterialNames(mdlPath);
                    mtrlPath = mtrl.GetMtrlPath(gear.GetMdlPath(), gear.GetMtrlFileName(), gear.MaterialId);
                    xivMtrl = await mtrl.GetMtrlData(mtrlPath, gear.MaterialId);
                }
                else
                {
                    // TODO: How to handle "GetMaterialFileData" for character assets, which contain multiple different materials

                    var mdlFile = _lumina.GetFile<MdlFile>(item.GetMdlPath());
                    var model = new Model(mdlFile);
                    var materials = model.Materials;

                    mtrlPath = mtrl.GetMtrlPath(item.GetMdlPath(), materials[0].MaterialPath, materialSet);
                    xivMtrl = await mtrl.GetMtrlData(mtrlPath, materialSet);
                }

                if (xivMtrl == null) return null;

                return new MaterialGameFile()
                {
                    Name = name,
                    Path = mtrlPath,
                    Category = XivPathParser.GetCategoryFromPath(mtrlPath),
                    XivMtrl = xivMtrl,
                    MaterialSet = materialSet,
                    Variant = XivPathParser.GetMtrlVariant(mtrlPath)
                };
            }
            catch (Exception ex)
            {
                _logService.Error(ex, "Exception caught while getting Material File Data.");
            }
            return null;
        }

        public async Task<Dictionary<string, List<IMaterialGameFile>>?> GetMaterialSetsDict(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;
            var mdlPath = item.GetMdlPath();
            var mdl = new Mdl(_frameworkGameDirectory, XivPathParser.GetXivDataFileFromPath(mdlPath));

            var category = XivPathParser.GetCategoryFromPath(mdlPath);
            SortedDictionary<string, SortedList<string, IMaterialGameFile>> sortedDict = new();
            try
            {
                if (item is IGear gear)
                {
                    var sharedModels = _itemListService.GetSharedModels(gear);
                    if (sharedModels == null) return null;
                    foreach (var m in sharedModels)
                    {
                        var races = _modelFileService.GetAllRaceMdls(m);
                        var race = XivRace.Hyur_Midlander_Male;
                        if (races.Count > 0)
                        {
                            race = races[0];
                        }
                        var includeSkin = XivPathParser.IsSmallClothesOrEmperorSeries(m.GetMdlPath(race));
                        var paths = await mdl.GetReferencedMaterialPaths(m.GetMdlPath(race), m.MaterialId, includeSkin: includeSkin);

                        foreach (var path in paths)
                        {
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
                            if (mat == null) continue;


                            if (!sortedDict.ContainsKey(path))
                            {
                                sortedDict[path] = new();
                            }

                            sortedDict[path].Add(mat.Name, mat);
                        }
                    }
                    var ret = new Dictionary<string, List<IMaterialGameFile>>();
                    foreach (var kvp in sortedDict)
                    {
                        ret.Add(kvp.Key, new List<IMaterialGameFile>(kvp.Value.Values));
                    }
                    return ret;
                }
                else
                {
                    var model = await mdl.GetModel(mdlPath, true);
                    var ret = new Dictionary<string, List<IMaterialGameFile>>();
                    if (model == null)
                    {
                        return null;
                    }
                    foreach (var path in model.Materials)
                    {
                        var xivMtrl = await _mtrl.GetMtrlData(path);
                        if (xivMtrl == null) continue;

                        if (!ret.ContainsKey(path))
                        {
                            ret[path] = new();
                        }
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
                            ret[path].Add(mat);
                        }
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Getting material set for {item.Name} threw an exception");
            }

            return null;
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
                    var ret = new SortedList<string, IMaterialGameFile>();
                    var seenPaths = new List<string>();

                    foreach (var m in sharedModels)
                    {
                        var races = _modelFileService.GetAllRaceMdls(m);
                        var race = XivRace.Hyur_Midlander_Male;
                        if (races.Count > 0)
                        {
                            race = races[0];
                        }
                        var includeSkin = XivPathParser.IsSmallClothesOrEmperorSeries(m.GetMdlPath(race));
                        var paths = await mdl.GetReferencedMaterialPaths(m.GetMdlPath(race), m.MaterialId, includeSkin: includeSkin);
                        foreach (var path in paths)
                        {
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
                            ret.TryAdd(m.Name, mat);
                            //seenPaths.Add(path);
                        }
                    }
                    return ret.Values.ToList();
                }
                else
                {
                    var model = await mdl.GetModel(mdlPath, true);
                    if (model == null)
                    {
                        return null;
                    }
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
            }
            catch (Exception ex)
            {
                _logService.Debug(ex);
                return null;
            }
        }

        public async Task<List<IMaterialGameFile>?> GetMaterials(IModelGameFile modelGameFile)
        {
            var ttModel = modelGameFile.TTModel;
            var mdlPath = modelGameFile.Path;
            var ret = new List<IMaterialGameFile>();
            foreach (var material in ttModel.Materials)
            {
                var mtrlPath = _mtrl.GetMtrlPath(mdlPath, material);
                var materialGameFile = await TryGetMaterialFileData(mtrlPath);
                if (materialGameFile != null)
                {
                    ret.Add(materialGameFile);
                }
            }
            return ret;
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
                    //var xivMtrl = await MtrlExtensions.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, path);
                    var xivMtrl = await Mtrl.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, path);
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
    }
}
