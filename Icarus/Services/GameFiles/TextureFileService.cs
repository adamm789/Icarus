using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using SharpDX.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Services.GameFiles
{
    public class TextureFileService : GameFileService, ITextureFileService
    {
        protected IMaterialFileService _materialFileService;
        public TextureFileService(LuminaService luminaService, IItemListService itemListService,
            ISettingsService settingsService, ILogService logService, IMaterialFileService materialFileService) 
            : base(luminaService, itemListService, settingsService, logService)
        {
            _materialFileService = materialFileService;
        }

        public async Task<ITextureGameFile?> GetTextureFileData(IMaterialGameFile materialFileData, XivTexType type = XivTexType.Normal, string variant = "a")
        {
            var dat = new Dat(_frameworkGameDirectory);
            var xivMtrl = materialFileData.XivMtrl;
            var typeFormatDict = new Dictionary<XivTexType, XivTexFormat>();

            XivTex? savedXivTex = null;
            string? savedPath = "";
            foreach (var texTypePath in xivMtrl.TextureTypePathList)
            {
                if (texTypePath.Type == XivTexType.ColorSet) continue;
                else
                {
                    var xivTex = await dat.GetType4Data(texTypePath.Path, true);
                    if (texTypePath.Type == type)
                    {
                        savedXivTex = xivTex;
                        savedPath = texTypePath.Path;
                        savedXivTex.TextureTypeAndPath = texTypePath;
                    }
                    typeFormatDict.TryAdd(texTypePath.Type, xivTex.TextureFormat);
                }
            }

            if (savedXivTex == null)
            {
                _logService.Error($"Could not get texsture for {materialFileData.Path}");
                return null;
            }

            if (String.IsNullOrWhiteSpace(savedPath))
            {
                savedPath = XivPathParser.ChangeTexVariant(savedPath, variant);
            }

            var retVal = new TextureGameFile()
            {
                Name = materialFileData.Name,
                Path = savedPath,
                Category = materialFileData.Category,
                TypeFormatDict = typeFormatDict,
                XivMtrl = xivMtrl,
                XivTex = savedXivTex,
                TexType = type
            };
            return retVal;
        }

        public async Task<ITextureGameFile?> GetTextureFileData(IItem? itemArg = null, XivTexType type = XivTexType.Normal, string variant = "a")
        {
            var item = GetItem(itemArg);
            if (item == null) return null;
            var materialFileData = await _materialFileService.GetMaterialFileData(item);
            var dat = new Dat(_frameworkGameDirectory);
            if (materialFileData == null)
            {
                return null;
            }
            return await GetTextureFileData(materialFileData, type, variant);
            /*
            var xivMtrl = materialFileData.XivMtrl;
            var typeFormatDict = new Dictionary<XivTexType, XivTexFormat>();

            XivTex? savedXivTex = null;
            string? savedPath = "";

            for (var i = 0; i < xivMtrl.TexturePathList.Count; i++)
            {
                var texturePath = xivMtrl.TexturePathList[i];
                var xivTex = await dat.GetType4Data(texturePath, true);
                if (XivPathParser.GetTexType(texturePath) == type)
                {
                    savedXivTex = xivTex;
                    savedPath = texturePath;
                    savedXivTex.TextureTypeAndPath = new()
                    {
                        Type = type,
                        Path = texturePath,
                        DataFile = XivPathParser.GetXivDataFileFromPath(texturePath),
                        Name = materialFileData.Name
                    };
                }
                if (i < xivMtrl.TextureTypePathList.Count) {
                    typeFormatDict.TryAdd(xivMtrl.TextureTypePathList[0].Type, xivTex.TextureFormat);
                }
                else
                {
                    typeFormatDict.TryAdd(XivTexType.Other, xivTex.TextureFormat);
                }
            }

            if (String.IsNullOrWhiteSpace(savedPath))
            {
                savedPath = item.GetTexPath(type, variant);
            }

            var retVal = new TextureGameFile()
            {
                Name = materialFileData.Name,
                Path = savedPath,
                Category = materialFileData.Category,
                TypeFormatDict = typeFormatDict,
                XivMtrl = xivMtrl,
                XivTex = savedXivTex,
                TexType = type
            };
            return retVal;
            */
        }

        public async Task<ITextureGameFile?> TryGetTextureFileData(string path, string itemName = "")
        {
            var result = TryGetItem(path, itemName);

            if (result != null)
            {
                return await GetTextureFileData(result);
            }

            var xivTex = await TryGetTextureFromPath(path);
            if (xivTex != null)
            {
                var typeFormatDict = new Dictionary<XivTexType, XivTexFormat>() { { xivTex.TextureTypeAndPath.Type, xivTex.TextureFormat } };

                var name = TryGetName(path, itemName);
                var textureGameFile = new TextureGameFile()
                {
                    Path = path,
                    XivTex = xivTex,
                    TexType = xivTex.TextureTypeAndPath.Type,
                    TexFormat = xivTex.TextureFormat,
                    Name = name,
                    TypeFormatDict = typeFormatDict,
                    Category = XivPathParser.GetCategoryFromPath(path)
                };
                return textureGameFile;
            }
            return null;
        }

        private async Task<XivTex?> TryGetTextureFromPath(string path)
        {
            try
            {
                var dat = new Dat(_frameworkGameDirectory);
                var xivTex = await dat.GetType4Data(path, true);
                var type = XivTexType.Normal;

                try
                {
                    type = XivPathParser.GetTexType(path);
                }
                catch (ArgumentException)
                {
                    _logService.Error("Using TexType Normal");
                }
                var dataFile = XivPathParser.GetXivDataFileFromPath(path);

                xivTex.TextureTypeAndPath = new()
                {
                    Path = path,
                    Type = type,
                    DataFile = dataFile
                };
                return xivTex;
            }
            catch (Exception)
            {

            }
            return null;
        }

        public async Task<List<XivTexType>?> GetAvailableTexTypes(IItem? itemArg)
        {
            var item = GetItem(itemArg);
            if (item == null) return null;

            var materialFileData = await _materialFileService.GetMaterialFileData(item);
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

        public async Task<List<XivTexType>?> GetAvailableTexTypes(IMaterialGameFile? material)
        {
            if (material == null) return null;
            else
            {
                var ret = new List<XivTexType>();
                var xivMtrl = material.XivMtrl;
            }

            return null;
        }
    }
}
