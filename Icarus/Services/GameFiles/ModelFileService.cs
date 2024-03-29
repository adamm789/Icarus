﻿using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;

namespace Icarus.Services.GameFiles
{
    public class ModelFileService : GameFileService, IModelFileService
    {
        readonly IMetadataFileService _metadataFileService;
        public ModelFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, IMetadataFileService metadataFileService, ILogService logService)
        : base(luminaService, itemListService, settingsService, logService)
        {
            _metadataFileService = metadataFileService;
        }

        public IModelGameFile? SelectedModelFile { get; set; }

        public List<IGear>? GetSharedModels(IGear gear)
        {
            return _itemListService.GetSharedModels(gear);
        }

        public IModelGameFile? TryGetModelFileData(string path, string itemName = "")
        {
            try
            {
                (var model, var xivMdl) = GetOriginalModel(path);

                _logService.Debug($"Searching for {path} to try and get item name.");
                var result = TryGetItem(path, itemName);

                var name = TryGetName(path, itemName);

                var category = XivPathParser.GetCategoryFromPath(path);
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

        public IModelGameFile? GetModelFileData(IItem? itemArg = null)
        {
            if (SelectedModelFile != null && itemArg == null)
            {
                _logService.Debug($"Returning loaded model file.");
                return SelectedModelFile;
            }

            var item = GetItem(itemArg);
            if (item == null) return null;
            var metadata = Task.Run(() =>_metadataFileService.GetMetadata(item)).Result;

            string path;
            XivRace race = XivRace.Hyur_Midlander_Male;

            if (metadata != null && item is IGear gear && metadata.ItemMetadata?.EqdpEntries?.Count > 0)
            {
                foreach (var kvp in metadata.ItemMetadata.EqdpEntries)
                {
                    if (kvp.Value.bit1)
                    {
                        race = kvp.Key;
                        break;
                    }
                }
                path = gear.GetMdlPath(race);
            }
            else
            {
                path = item.GetMdlPath();
                race = XivPathParser.GetRaceFromString(path);
            }

            return GetModelFileData(itemArg, race);
        }

        public Dictionary<XivRace, IModelGameFile> GetModelFileDataDict(IItem? itemArg = null)
        {
            var races = GetAllRaceMdls(itemArg);
            if (races.Count == 0)
            {
                races.Add(XivRace.All_Races);
            }
            var ret = new Dictionary<XivRace, IModelGameFile>();
            foreach (var race in races)
            {
                var data = GetModelFileData(itemArg, race);
                if (data != null)
                {
                    ret.Add(race, data);
                }
            }
            return ret;
        }

        public IModelGameFile? GetModelFileData(IItem? itemArg = null, XivRace race = XivRace.Hyur_Midlander_Male)
        {
            /*
            if (SelectedModelFile != null && itemArg == null && SelectedModelFile.TargetRace == race)
            {
                _logService.Debug($"Returning loaded model file.");
                return SelectedModelFile;
            }
            */
            var item = GetItem(itemArg);
            if (item == null) return null;

            try
            {
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
            catch (Exception ex)
            {
                _logService.Error(ex, $"Model for \"{item.Name}\" could not be found.");
                return null;
            }
        }

        public List<XivRace> GetAllRaceMdls(IItem? itemArg)
        {
            var mdls = new List<XivRace>();
            var item = GetItem(itemArg);

            if (item == null) return mdls;

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
            else
            {
                var race = XivPathParser.GetRaceFromString(item.GetMdlPath());
                if (race != XivRace.All_Races)
                {
                    mdls.Add(race);
                }
            }
            return mdls;
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
                if (mdlFile != null)
                {
                    _logService.Debug($"Mesh Count = {mdlFile.ModelHeader.MeshCount}, VertexDeclarations = {mdlFile.FileHeader.VertexDeclarationCount}");
                    //return MdlWithFramework.GetRawMdlDataFramework(path, mdlFile.Data, mdlFile.ModelHeader.MeshCount);
                    return Mdl.GetRawMdlData(path, mdlFile.Data, mdlFile.ModelHeader.MeshCount);
                }
                //return mdlFile.GetXivMdl();
            }

            _logService.Verbose($"Could not resolve model path: {path}.");

            if (XivPathParser.HasSkin(path))
            {
                var skinRacePath = XivPathParser.ChangeToSkinRace(path);
                var midlanderPath = XivPathParser.ChangeToRace(path, XivRace.Hyur_Midlander_Male);
                var midlanderFemalePath = XivPathParser.ChangeToRace(path, XivRace.Hyur_Highlander_Female);

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
                else if (_lumina.FileExists(midlanderFemalePath))
                {
                    _logService.Information($"Using midlander female path: {midlanderFemalePath}");
                    var mdlFile = _lumina.GetFile<MdlFile>(midlanderFemalePath);
                    return mdlFile.GetXivMdl();
                }
            }
            throw new ArgumentException($"The model for {path} could not be found.");
        }
    }
}
