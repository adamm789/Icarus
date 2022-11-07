using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Mods;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
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

        public GameFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, ILogService logService) : base(luminaService)
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

        // TODO: Est.GetAllExtraSkeletons(EstType type, XivRace raceFilter = XivRace.All_Races, bool includeNpcs = false) (EST L140)

        public StainingTemplateFile GetStainingTemplateFile()
        {
            var bytes = _lumina.GetFile("chara/base_material/stainingtemplate.stm").Data;
            return new StainingTemplateFile(bytes);
        }

        // public async Task<IGameFile?> GetFileData<T>(IItem? itemArg = null) where T : IMod

        public async Task<IGameFile?> GetFileData(IItem? itemArg = null, Type? type = null)
        {
            if (type == typeof(ModelMod))
            {
                return GetModelFileData(itemArg);
            }
            else if (type == typeof(MaterialMod))
            {
                return await GetMaterialFileData(itemArg);
            }
            else if (type == typeof(TextureMod))
            {
                return await GetTextureFileData(itemArg);
            }
            else if (type == typeof(MetadataMod))
            {
                return await GetMetadata(itemArg);
            }

            return null;
        }

        public List<XivTexType>? GetAvailableTexTypes(IItem? itemArg)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            var materialFileData = Task.Run(() => GetMaterialFileData(item)).Result;
            if (materialFileData != null)
            {
                var ret = new List<XivTexType>();
                var xivMtrl = materialFileData.XivMtrl;
                var mapInfo = xivMtrl.GetAllMapInfos();
                foreach (var info in mapInfo)
                {
                    ret.Add(info.Usage);
                }
                if (ret.Count == 0) return null;

                return ret;
            }
            return null;
        }

        // TODO: race?
        public async Task<IGameFile?> TryGetFileData(string path, Type? callingType = null, string name = "")
        {
            if (XivPathParser.IsMdl(path))
            {
                return TryGetModelFileData(path, name);
            }
            if (XivPathParser.IsMtrl(path))
            {
                return await TryGetMaterialFileData(path, name);
            }
            if (XivPathParser.IsTex(path))
            {
                return await TryGetTextureFileData(path, name);
            }
            _logService.Error($"Returning null from {path}");
            return null;
        }

        public async Task<ITextureGameFile?> GetTextureFileData(IItem? itemArg = null, XivTexType type = XivTexType.Normal, string variant = "a")
        {
            var item = GetItem(itemArg);
            if (item == null) return null;
            var materialFileData = await GetMaterialFileData(item);
            var dat = new Dat(_frameworkGameDirectory);
            if (materialFileData == null)
            {
                return null;
            }
            var xivMtrl = materialFileData.XivMtrl;
            var typeFormatDict = new Dictionary<XivTexType, XivTexFormat>();

            XivTex? savedXivTex = null;

            for (var i = 0; i < xivMtrl.TexturePathList.Count; i++)
            {
                var texturePath = xivMtrl.TexturePathList[i];

                var xivTex = await dat.GetType4Data(texturePath, true);
                if (XivPathParser.GetTexType(texturePath) == type)
                {
                    savedXivTex = xivTex;
                    savedXivTex.TextureTypeAndPath = new()
                    {
                        Type = type,
                        Path = texturePath,
                        DataFile = XivDataFiles.GetXivDataFile(texturePath),
                        Name = materialFileData.Name
                    };
                }
                typeFormatDict.Add(xivMtrl.TextureTypePathList[i].Type, xivTex.TextureFormat);
            }

            var path = item.GetTexPath(type, variant);

            var retVal = new TextureGameFile()
            {
                Name = materialFileData.Name,
                Path = path,
                Category = materialFileData.Category,
                TypeFormatDict = typeFormatDict,
                XivMtrl = xivMtrl,
                XivTex = savedXivTex,
                TexType = type
            };
            return retVal;
        }

        // TODO: Implement with Lumina so user can search for arbitrary .tex files
        public async Task<ITextureGameFile?> TryGetTextureFileData(string path, string itemName = "")
        {
            var result = TryGetItem(path, itemName);

            return await GetTextureFileData(result);
        }

        protected IItem? TryGetItem(string path, string? itemName = null)
        {
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
        }

        // TODO: Split into ModelFileService, MaterialFileService, etc?
        #region Models
        public IModelGameFile? TryGetModelFileData(string path, string itemName = "")
        {
            try
            {
                (var model, var xivMdl) = GetOriginalModel(path);

                _logService.Debug($"Searching for {path} to try and get item name.");
                var name = $"{path} (?)";
                var result = TryGetItem(path, itemName);

                if (result != null)
                {
                    name = result.Name;
                }

                var category = XivPathParser.GetCategory(path);
                var race = XivPathParser.GetRaceFromString(path);

                var ret = new ModelGameFile()
                {
                    Name = name,
                    Path = path,
                    TargetRace = race,
                    TTModel = model,
                    XivMdl = xivMdl,
                    Category = category
                };

                return ret;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public IModelGameFile? GetModelFileData(IItem? itemArg = null, XivRace race = XivRace.Hyur_Midlander_Male)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            string itemPath;
            string category;
            if (item is IGear equip)
            {
                itemPath = equip.GetMdlPath(race);
                category = equip.Slot.ToString();
            }
            else
            {
                itemPath = item.GetMdlPath();
                category = item.Category.ToString();
            }

            (var model, var xivMdl) = GetOriginalModel(itemPath, race);
            xivMdl.MdlPath = itemPath;

            var ret = new ModelGameFile()
            {
                //Item = item,
                Name = item.Name,
                Path = itemPath,
                Category = category,
                TargetRace = race,
                TTModel = model,
                XivMdl = xivMdl
            };

            return ret;
        }

        public List<XivRace> GetAllRaceMdls(IItem? itemArg)
        {
            var mdls = new List<XivRace>();
            var item = GetItem(itemArg);
            if (item is IGear gear)
            {
                foreach (var race in XivRaces.PlayableRaces)
                {
                    var path = gear.GetMdlPath(race);
                    if (_lumina.FileExists(path))
                    {
                        mdls.Add(race);
                    }
                }
            }
            return mdls;
        }

        #endregion
        #region Materials

        public async Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string itemName = "")
        {
            var xivMtrl = await TryGetMaterialFromPath(path);
            var result = TryGetItem(path, itemName);

            if (result != null)
            {
                return await GetMaterialFileData(result);
            }

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

        #endregion
        /*
         * TODO: Return IMetadataFile from TryGetMetadata
        public async Task<IMetadataFile?> TryGetMetadata(string path, string? itemName)
        {
            var itemMetadata = await ItemMetadata.GetMetadata(path, true);
            //var metadata = new MetadataMod(itemMetadata);

            var name = $"{path} (?)";
            var result = TryGetItem(path, itemName);

            if (result != null)
            {
                name = result.Name;
            }
            //metadata.Name = name;
            return new MetadataFile()
            {
                Name = name,
                Path = path,
                Category = itemMetadata.Root.Info.Slot,
                ItemMetadata = itemMetadata
            };
        }
        */

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
                var category = XivPathParser.GetCategory(path);
                var slot = XivPathParser.GetEquipmentSlot(path);

                metadataFile = new MetadataFile()
                {
                    ItemMetadata = itemMetadata,
                    Path = path,
                    Name = name,
                    Category = category
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

        #region Private Functions
        private async Task<XivMtrl?> TryGetMaterialFromPath(string path)
        {
            try
            {
                if (_lumina.FileExists(path))
                {
                    var mtrlFile = _lumina.GetFile<MtrlFile>(path);
                    var xivMtrl = await MtrlExtensions.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, path);
                    _logService.Information($"Successfully found material {path}");
                    return xivMtrl;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logService.Debug($"Could not find material {path}");
            }
            return null;
        }

        private (TTModel, XivMdl) GetOriginalModel(string path, XivRace race = XivRace.Hyur_Midlander_Male)
        {
            //var mdl = TryGetOriginalWithLumina(path);
            //var xivMdl = mdl.GetRawMdlDataLumina();
            var xivMdl = GetXivMdl(path);
            var ttModel = TTModel.FromRaw(xivMdl);

            return (ttModel, xivMdl);
        }

        private XivMdl GetXivMdl(string path)
        {
            if (_lumina.FileExists(path))
            {
                _logService.Verbose($"Successfully got model of {path}");

                var mdlFile = _lumina.GetFile<MdlFile>(path);
                return MdlWithFramework.GetRawMdlDataFramework(path, mdlFile.Data, mdlFile.ModelHeader.MeshCount);
                //return mdlFile.GetXivMdl();
            }

            _logService.Verbose($"Could not resolve model path: {path}.");

            if (XivPathParser.HasSkin(path))
            {
                var skinRacePath = XivPathParser.ChangeToSkinRace(path);
                var midlanderPath = XivPathParser.ChangeToRace(path, XivRace.Hyur_Midlander_Male);

                if (_lumina.FileExists(skinRacePath))
                {
                    _logService.Debug($"Using base race: {skinRacePath}.");
                    _logService.Warning("Make sure metadata is enabled to see this mod correctly.");
                    var mdlFile = _lumina.GetFile<MdlFile>(skinRacePath);
                    //return MdlWithFramework.GetRawMdlDataFramework(skinRacePath, mdlFile.Data, mdlFile.ModelHeader.MeshCount);
                    return mdlFile.GetXivMdl();
                }
                else if (_lumina.FileExists(midlanderPath))
                {
                    _logService.Warning($"Defaulting to midlander race: {midlanderPath}");
                    var mdlFile = _lumina.GetFile<MdlFile>(midlanderPath);
                    //return MdlWithFramework.GetRawMdlDataFramework(midlanderPath, mdlFile.Data, mdlFile.ModelHeader.MeshCount);

                    return mdlFile.GetXivMdl();
                }
            }
            throw new ArgumentException($"The model for {path} could not be found.");
        }

        private XivMdl TryGetOriginalWithLumina(string path)
        {
            if (_lumina.FileExists(path))
            {
                _logService.Verbose($"Successfully got model of {path}");

                var mdl = new MdlWithLumina(_lumina, path);
                return mdl.GetRawMdlDataLumina();
            }

            _logService.Verbose($"Could not resolve model path: {path}.");

            if (XivPathParser.HasSkin(path))
            {
                var skinRacePath = XivPathParser.ChangeToSkinRace(path);
                var midlanderPath = XivPathParser.ChangeToRace(path, XivRace.Hyur_Midlander_Male);

                if (_lumina.FileExists(skinRacePath))
                {
                    _logService.Debug($"Using base race: {skinRacePath}.");
                    _logService.Warning("Make sure metadata is enabled to see this mod correctly.");
                    var mdl = new MdlWithLumina(_lumina, skinRacePath);
                    return mdl.GetRawMdlDataLumina();
                }
                else if (_lumina.FileExists(midlanderPath))
                {
                    _logService.Warning($"Defaulting to midlander race: {midlanderPath}");
                    var mdl = new MdlWithLumina(_lumina, midlanderPath);
                    return mdl.GetRawMdlDataLumina();
                }
            }
            throw new ArgumentException($"The model for {path} could not be found.");
        }
        #endregion
    }
}
