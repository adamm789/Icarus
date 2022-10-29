using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina.Data;
using Lumina.Data.Files;
using Lumina.Data.Structs;
using Icarus.Util.Import;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Mods.DataContainers;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using Mod = Icarus.Mods.Mod;
using ModGroup = Icarus.Mods.DataContainers.ModGroup;
using ModOption = Icarus.Mods.DataContainers.ModOption;
using ModPack = Icarus.Mods.DataContainers.ModPack;

namespace Icarus.Services.Files
{
    public class TexToolsModPackImporter
    {
        private string _projectDirectory;
        private string _gameDirectory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="gameDirectory">The framework game directory (ends in /ffxiv)</param>
        public TexToolsModPackImporter(string projectDirectory, string gameDirectory)
        {
            _projectDirectory = projectDirectory;
            _gameDirectory = gameDirectory;
        }

        public async Task<ModPack> ExtractTexToolsModPack(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var fileInfo = new FileInfo(filePath);

            var tempDir = Path.Combine(_projectDirectory, "temp");
            Directory.CreateDirectory(tempDir);

            var dir = new DirectoryInfo(tempDir);

            using var zfs = fileInfo.OpenRead();
            using var zip = ZipFile.Read(zfs);

            string mplPath = dir + "\\ttmpl.mpl";
            string mpdPath = dir + "\\ttmpd.mpd";

            ZipEntry mpl = zip.Entries.First(x => x.FileName.EndsWith(".mpl"));
            mpl.Extract(tempDir, ExtractExistingFileAction.OverwriteSilently);

            ZipEntry mpd = zip.Entries.First(x => x.FileName.EndsWith(".mpd"));
            mpd.Extract(tempDir, ExtractExistingFileAction.OverwriteSilently);

            zfs.Close();
            zip.Dispose();

            ModPackJson? modPack = JsonConvert.DeserializeObject<ModPackJson>(File.ReadAllText(mplPath));

            if (modPack == null)
            {
                var err = $"Could not deserialize {filePath}.";
                Log.Error(err);
                throw new ArgumentNullException(err);
            }

            var mpdFileInfo = new FileInfo(mpdPath);
            var retModPack = new ModPack(modPack);

            using (var pack = new SqPackStream(mpdFileInfo))
            {
                if (modPack.SimpleModsList != null)
                {
                    foreach (ModsJson mods in modPack.SimpleModsList)
                    {
                        var file = await Extract(mods, pack, mods.Name, filePath);
                        if (file == null)
                        {
                            Log.Error($"Could not extract mod {mods.Name}.");
                        }
                        else
                        {
                            retModPack.SimpleModsList.Add(file);
                            
                        }
                    }
                }

                if (modPack.ModPackPages != null)
                {
                    foreach (ModPackPageJson page in modPack.ModPackPages)
                    {
                        var copyPage = new ModPackPage(page);
                        foreach (ModGroupJson group in page.ModGroups)
                        {
                            var groupNumber = 0;
                            var copyGroup = new ModGroup(group);
                            foreach (ModOptionJson option in group.OptionList)
                            {
                                var optionNumber = 0;
                                var copyOption = new ModOption(option);
                                foreach (ModsJson mods in option.ModsJsons)
                                {
                                    // TODO: Extract asynchronously?
                                    var fileName = $"Page{page.PageIndex}/{group.GroupName}/{option.Name}: ({mods.Name})";
                                    Log.Verbose($"Extracting {fileName}");

                                    var mod = await Extract(mods, pack, fileName, filePath);

                                    if (mod == null)
                                    {
                                        Log.Error($"Could not extract mod. Skipping {fileName}");
                                    }
                                    else
                                    {
                                        copyOption.AddMod(mod);

                                        // TODO: Check to see if it already exists?
                                        if (!retModPack.SimpleModsList.Contains(mod))
                                        {
                                            retModPack.SimpleModsList.Add(mod);
                                        }
                                        else
                                        {
                                            Log.Warning($"{mods.Name} already exists. Skipping.");
                                        }
                                    }
                                }
                                optionNumber++;
                                copyGroup.AddOption(copyOption);
                            }
                            groupNumber++;
                            copyPage.AddGroup(copyGroup);
                        }
                        retModPack.ModPackPages.Add(copyPage);
                    }
                }
            }
            return retModPack;
        }

        private async Task<IMod?> Extract(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            var dat = pack.GetFileMetadata(mods.ModOffset);
            IMod? result = null;

            switch (dat.Type)
            {
                case FileType.Model:
                    result = await Task.Run(() => ExtractModel(mods, pack, modFileName, modFilePath));
                    break;
                case FileType.Texture:
                    result = await ExtractTexture(mods, pack, modFileName, modFilePath);
                    break;
                default:
                    result = await ExtractStandard(mods, pack, modFileName, modFilePath);
                    break;
            }
            result ??= ExtractReadOnlyMod(mods, pack, modFileName, modFilePath);
            return result;
        }

        private ModelMod? ExtractModel(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            try
            {
                var mdlFile = pack.ReadFile<MdlFile>(mods.ModOffset);
                var xivMdl = MdlWithFramework.GetRawMdlDataFramework(mods.FullPath, mdlFile.Data, mdlFile.Meshes.Length);
                var imported = TTModel.FromRaw(xivMdl);
                var ret = new ModelMod(mods.FullPath, imported, ImportSource.TexToolsModPack)
                {
                    ModFileName = modFileName,
                    ModFilePath = modFilePath,

                    Path = mods.FullPath,
                    Name = mods.Name,
                    Category = mods.Category
                };
                return ret;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not extract model");
                return null;
            }
        }

        private async Task<TextureMod?> ExtractTexture(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            if (mods.FullPath.Contains(".tex"))
            {
                try
                {
                    var bytes = new byte[mods.ModSize];
                    pack.BaseStream.Seek(mods.ModOffset, SeekOrigin.Begin);
                    pack.BaseStream.Read(bytes, 0, mods.ModSize);

                    var xivTex = await DatExtensions.GetType4Data(bytes);
                    var type = XivTexType.Normal;

                    try
                    {
                        type = XivPathParser.GetTexType(mods.FullPath);
                    }
                    catch (ArgumentException)
                    {
                        Log.Error("Using TexType Normal");
                    }

                    var dataFile = XivDataFiles.GetXivDataFile(mods.FullPath);

                    var texTypePath = new TexTypePath()
                    {
                        Path = mods.FullPath,
                        Name = mods.Name,
                        Type = type,
                        DataFile = dataFile
                    };
                    xivTex.TextureTypeAndPath = texTypePath;

                    return new TextureMod(xivTex, ImportSource.TexToolsModPack)
                    {
                        ModFileName = modFileName,
                        ModFilePath = modFilePath,

                        Path = mods.FullPath,
                        Name = mods.Name,
                        Category = mods.Category,

                        TexType = type
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not extract texture");
                }
            }
            return null;
        }

        // TODO: Something akin to: ImportStandard(byte[] bytes, ...) ?
        private async Task<Mod?> ExtractStandard(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            if (mods.FullPath.Contains(".mtrl"))
            {
                return await ExtractMaterial(mods, pack, modFileName, modFilePath);
            }
            else if (mods.FullPath.Contains(".meta"))
            {
                return await ExtractMetadata(mods, pack, modFileName, modFilePath);
            }
            else
            {
                Log.Error($"Unknown \"standard\" mod {mods.FullPath}");
                return null;
            }
        }

        private async Task<MaterialMod?> ExtractMaterial(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            try
            {
                var file = pack.ReadFile<MtrlFile>(mods.ModOffset);
                var gameDirectory = new DirectoryInfo(_gameDirectory);

                var xivMtrl = await MtrlExtensions.GetMtrlData(gameDirectory, file.Data, mods.FullPath);

                var mtrlMod = new MaterialMod(xivMtrl)
                {
                    ModFileName = modFileName,
                    ModFilePath = modFilePath,

                    Path = mods.FullPath,
                    Name = mods.Name,
                    Category = mods.Category
                };
                return mtrlMod;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not extract material");
            }
            return null;
        }

        private async Task<MetadataMod?> ExtractMetadata(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            try
            {
                //var file = pack.ReadFile<FileResource>(mods.ModOffset);
                var bytes = new byte[mods.ModSize];

                pack.BaseStream.Seek(mods.ModOffset, SeekOrigin.Begin);
                pack.BaseStream.Read(bytes, 0, mods.ModSize);

                var uncompressedBytes = await DatExtensions.GetType2Data(bytes);
                var meta = await ItemMetadata.Deserialize(uncompressedBytes);

                var metaMod = new MetadataMod(meta)
                {
                    ModFileName = modFileName,
                    ModFilePath = modFilePath,

                    Path = mods.FullPath,
                    Name = mods.Name,
                    Category = mods.Category
                };
                return metaMod;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not extract metadata");
            }
            return null;
        }

        private ReadOnlyMod? ExtractReadOnlyMod(ModsJson mods, SqPackStream pack, string modFileName, string modFilePath)
        {
            try
            {
                var bytes = new byte[mods.ModSize];
                var startPosition = pack.BaseStream.Position;

                pack.BaseStream.Seek(mods.ModOffset, SeekOrigin.Begin);
                pack.BaseStream.Read(bytes, 0, mods.ModSize);

                pack.BaseStream.Position = startPosition;

                var ret = new ReadOnlyMod()
                {
                    ModFileName = modFileName,
                    ModFilePath = modFilePath,

                    Name = mods.Name,
                    Path = mods.FullPath,
                    Category = mods.Category,
                    Data = bytes
                };
                return ret;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not extract ???");
                return null;
            }
        }
    }
}
