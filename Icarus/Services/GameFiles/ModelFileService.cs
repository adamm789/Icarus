using Icarus.Mods.GameFiles;
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

namespace Icarus.Services.GameFiles
{
    public class ModelFileService : GameFileService, IModelFileService
    {
        public ModelFileService(LuminaService luminaService, IItemListService itemListService, ISettingsService settingsService, ILogService logService)
        : base(luminaService, itemListService, settingsService, logService)
        {

        }

        public IModelGameFile? TryGetModelFileData(string path, string itemName = "")
        {
            try
            {
                (var model, var xivMdl) = GetOriginalModel(path);

                _logService.Debug($"Searching for {path} to try and get item name.");
                var result = TryGetItem(path, itemName);

                var name = TryGetName(path, itemName);

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
    }
}
