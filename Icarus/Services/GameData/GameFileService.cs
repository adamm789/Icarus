﻿using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina;
using Lumina.Data;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Lumina.Models.Materials;
using Lumina.Models.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
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

        public GameFileService(LuminaService luminaService, ItemListService itemDatabaseService, SettingsService settingsService, ILogService logService) : base(luminaService)
        {
            _itemListService = itemDatabaseService;
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
                    _logService.Error("Could not get Item file. None was given.");
                    return null;
                }
            }
            else
            {
                _logService.Verbose($"Using supplied item: {item.Name}.");
                return item;
            }
        }

        // TODO: race?
        public async Task<IGameFile?> TryGetFileData(string path, XivRace race = XivRace.Hyur_Midlander_Male)
        {
            if (XivPathParser.IsMdl(path))
            {
                try
                {
                    return GetModelFileData(path);
                }
                catch (ArgumentException ex)
                {
                    _logService.Error(ex.Message);
                }
            }
            if (XivPathParser.IsMtrl(path))
            {
                return await GetMaterialFileData(path);
            }
            _logService.Error($"Returning null from {path}");
            return null;
        }

        public ModelGameFile? GetModelFileData(string path)
        {
            //var mdl = _lumina.GetFile<MdlFile>(path);
            //var file = _lumina.GetFileMetadata(path);
            (var model, var xivMdl) = TryGetOriginalModel(path);

            var results = _itemListService.Search(path);
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

        public XivTexFormat GetTextureData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return XivTexFormat.INVALID;
            var tex = item.GetTexPath(XivTexType.Multi);
            if (_lumina.FileExists(item.GetTexPath(XivTexType.Multi)))
            {
                var texFile = _lumina.GetFile<TexFile>(item.GetTexPath(XivTexType.Multi));
                var format = texFile.Header.Format;
            }
            return XivTexFormat.INVALID;
        }

        public ModelGameFile? GetModelFileData(IItem? itemArg = null, XivRace race = XivRace.Hyur_Midlander_Male)
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

        public async Task<MaterialGameFile?> GetMaterialFileData(string path)
        {
            var xivMtrl = await TryGetOriginalMaterial(path);
            if (xivMtrl == null)
            {
                return null;
            }
            var category = XivPathParser.GetCategory(path);
            var results = _itemListService.Search(path);
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
            /*
            var itemList = _itemListService.Search(path);
            var name = "";
            if (itemList.Count > 0)
            {
                name = itemList.First().Name;
            }
            var retVal = new MaterialGameFile()
            {
                Name = name
            };
            return retVal;
            */
        }

        public async Task<MaterialGameFile?> GetMaterialFileData(IItem? itemArg = null)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            var equip = item as IGear;
            // TODO: Material when item is not equipment
            if (equip == null) return null;

            var mtrl = new Mtrl(_frameworkGameDirectory);
            var xivMtrl = await mtrl.GetMtrlData(equip.GetMtrlPath());

            var retVal = new MaterialGameFile()
            {
                //Item = item,
                Name = item.Name,
                Path = equip.GetMtrlPath(),
                Category = equip.Category.ToString(),
                XivMtrl = xivMtrl,
            };
            return retVal;
        }

        public async Task<XivMtrl?> TryGetOriginalMaterial(string path)
        {
            _logService.Information($"Trying to get material: {path}.");
            if (_lumina.FileExists(path))
            {
                var mtrlFile = _lumina.GetFile<MtrlFile>(path);
                var xivMtrl = await MtrlExtensions.GetMtrlData(_frameworkGameDirectory, mtrlFile.Data, path);
                return xivMtrl;
            }
            return null;
        }

        public List<XivRace> GetAllRaceMdls(IItem? itemArg)
        {
            var item = GetItem(itemArg);
            if (item == null) return new List<XivRace>();
            return _itemListService.GetAllRaceMdls(item);
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
                _logService.Information($"Successfully got model of {path}");

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
                    _logService.Information($"Using base race: {skinRacePath}.");
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
    }
}