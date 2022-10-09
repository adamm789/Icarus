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
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.Enums;
using Path = System.IO.Path;

namespace Icarus.Services.GameFiles
{
    public class GameFileService : LuminaDependentServiceBase<GameFileService>, IGameFileService
    {
        readonly ItemListService _itemListService;
        readonly ILogService _logService;
        readonly string _gameDirectory;
        readonly DirectoryInfo _frameworkGameDirectory;

        public GameFileService(LuminaService luminaService, ItemListService itemListService, SettingsService settingsService, ILogService logService) : base(luminaService)
        {
            _itemListService = itemListService;
            _gameDirectory = settingsService.GameDirectoryLumina;
            _logService = logService;
            _frameworkGameDirectory = new(Path.Combine(_gameDirectory, "ffxiv"));
        }

        private IItem? GetItem(IItem? item = null)
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
            /*
            if (XivPathParser.IsMdl(path) && callingType == typeof(ModelModViewModel))
            {
                return TryGetModelFileData(path, name);
            }
            if (XivPathParser.IsMtrl(path) && callingType == typeof(MaterialModViewModel))
            {
                return await TryGetMaterialFileData(path, name);
            }
            if (XivPathParser.IsTex(path) && callingType == typeof(TextureModViewModel))
            {
                return await TryGetTextureFileData(path, name);
            }
            _logService.Error($"Returning null from {path}");
            return null;
            */
        }

        public async Task<ITextureGameFile?> GetTextureFileData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            var materialFileData = await GetMaterialFileData(item);
            var dat = new Dat(_frameworkGameDirectory);
            if (materialFileData == null)
            {
                return null;
            }
            var xivMtrl = materialFileData.XivMtrl;
            var typeFormatDict = new Dictionary<XivTexType, XivTexFormat>();

            for (var i = 0; i < xivMtrl.TexturePathList.Count; i++)
            {
                var texturePath = xivMtrl.TexturePathList[i];
                var xivTex = await dat.GetType4Data(texturePath, true);
                typeFormatDict.Add(xivMtrl.TextureTypePathList[i].Type, xivTex.TextureFormat);
            }

            var path = item.GetTexPath(XivTexType.Normal);
            var retVal = new TextureGameFile()
            {
                Name = materialFileData.Name,
                Path = path,
                Category = materialFileData.Category,
                TypeFormatDict = typeFormatDict
            };
            return retVal;
        }

        public async Task<ITextureGameFile?> TryGetTextureFileData(string path, string itemName = "")
        {
            List<IItem> results = new();

            if (!String.IsNullOrWhiteSpace(itemName))
            {
                results = _itemListService.Search(itemName);
            }
            if (results.Count == 0)
            {
                results = _itemListService.Search(path);
            }

            return await GetTextureFileData(results[0]);
        }

        // TODO: Split into ModelFileService, MaterialFileService, etc?
        #region Models
        public IModelGameFile? TryGetModelFileData(string path, string itemName = "")
        {
            //var mdl = _lumina.GetFile<MdlFile>(path);
            //var file = _lumina.GetFileMetadata(path);
            try
            {
                (var model, var xivMdl) = TryGetOriginalModel(path);

                List<IItem> results = new();

                _logService.Debug($"Searching for {path} to try and get item name.");
                if (!String.IsNullOrWhiteSpace(itemName))
                {
                    results = _itemListService.Search(itemName);
                }
                if (results.Count == 0)
                {
                    results = _itemListService.Search(path);
                }

                var name = $"{path} (?)";
                if (results.Count > 0)
                {
                    name = results[0].Name;
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

            (var model, var xivMdl) = TryGetOriginalModel(itemPath, race);
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
            List<IItem> results = new();
            if (!String.IsNullOrWhiteSpace(itemName))
            {
                results = _itemListService.Search(itemName);
            }

            if (results.Count == 0)
            {
                results = _itemListService.Search(path);
            }

            if (results.Count > 0)
            {
                return await GetMaterialFileData(results[0]);
            }
            if (xivMtrl != null)
            {
                var category = XivPathParser.GetCategory(path);
                var name = path;
                if (results.Count > 0)
                {
                    name = results[0].Name;
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
            // TODO: Material when item is not equipment
            if (item is IGear equip)
            {
                xivMtrl = await mtrl.GetMtrlData(equip.GetMtrlPath());
                category = equip.Slot.ToString();
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

        #endregion

        #region Private Functions
        private async Task<XivMtrl?> TryGetMaterialFromPath(string path)
        {
            _logService.Information($"Trying to get material: {path}.");
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

        private (TTModel, XivMdl) TryGetOriginalModel(string path, XivRace race = XivRace.Hyur_Midlander_Male)
        {
            var mdl = TryGetOriginalWithLumina(path);
            var retMdl = mdl.GetRawMdlDataLumina();
            var retModel = TTModel.FromRaw(retMdl);

            return (retModel, retMdl);
        }

        private MdlWithLumina TryGetOriginalWithLumina(string path)
        {
            if (_lumina.FileExists(path))
            {
                _logService.Verbose($"Successfully got model of {path}");

                var mdl = new MdlWithLumina(_lumina, path);
                return mdl;
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
                    return mdl;
                }
                else if (_lumina.FileExists(midlanderPath))
                {
                    _logService.Warning($"Defaulting to midlander race: {midlanderPath}");
                    var mdl = new MdlWithLumina(_lumina, midlanderPath);
                    return mdl;
                }
            }
            throw new ArgumentException($"The model for {path} could not be found.");
        }
        #endregion
    }
}
