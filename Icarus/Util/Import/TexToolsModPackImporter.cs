using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Util;
using Ionic.Zip;
using Lumina.Data;
using Lumina.Data.Files;
using Lumina.Data.Structs;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Mods.DataContainers;
using xivModdingFramework.Mods.FileTypes;
using Icarus.Util.Extensions;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using ItemDatabase.Paths;

using Mod = Icarus.Mods.Mod;
using ModPack = Icarus.Mods.DataContainers.ModPack;
using ModGroup = Icarus.Mods.DataContainers.ModGroup;
using ModOption = Icarus.Mods.DataContainers.ModOption;

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
                        var file = await Extract(mods, pack);
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
                            var copyGroup = new ModGroup(group);
                            foreach (ModOptionJson option in group.OptionList)
                            {
                                var copyOption = new ModOption(option);
                                foreach (ModsJson mods in option.ModsJsons)
                                {
                                    // TODO: Extract asynchronously?
                                    var file = await Extract(mods, pack, fileInfo.Name, option);

                                    if (file == null)
                                    {
                                        Log.Error($"Could not extract mod. Skipping {mods.Name}.");
                                    }
                                    else
                                    {
                                        copyOption.AddMod(file);

                                        // TODO: Check to see if it already exists?
                                        if (!retModPack.SimpleModsList.Contains(file))
                                        {
                                            retModPack.SimpleModsList.Add(file);
                                        }
                                        else
                                        {
                                            Log.Warning($"{mods.Name} already exists. Skipping.");
                                        }
                                    }
                                }
                                copyGroup.AddOption(copyOption);
                            }
                            copyPage.AddGroup(copyGroup);
                        }
                        retModPack.ModPackPages.Add(copyPage);
                    }
                }
            }
            return retModPack;
        }

        private async Task<IMod?> Extract(ModsJson mods, SqPackStream pack, string fileName = "", ModOptionJson? option = null)
        {
            Log.Verbose("Extracting {mod}.", mods.FullPath);
            var dat = pack.GetFileMetadata(mods.ModOffset);

            IMod? result = null;

            switch (dat.Type)
            {
                case FileType.Model:
                    result = await Task.Run(() => ExtractModel(mods, pack, option));
                    break;
                case FileType.Texture:
                    result = await ExtractTexture(mods, pack, option);
                    break;
                default:
                    result = await ExtractStandard(mods, pack, option);
                    break;
            }
            result ??= ExtractReadOnlyMod(mods, pack, option);
            return result;
        }

        private string GetModFileName(ModsJson mods, ModOptionJson? option)
        {
            var fileName = mods.FullPath;
            var file = new FileInfo(mods.FullPath);
            if (file != null)
            {
                fileName = file.Name;
            }
            if (option != null)
            {
                return $"{option.GroupName} - {option.Name}: {mods.Name} ({fileName})";
            }
            return $"{mods.Name} ({fileName})";
        }

        private ModelMod? ExtractModel(ModsJson mods, SqPackStream pack, ModOptionJson? option = null)
        {
            var mdlFile = pack.ReadFile<MdlFile>(mods.ModOffset);
            var xivMdl = MdlWithFramework.GetRawMdlDataFramework(mods.FullPath, mdlFile.Data, mdlFile.Meshes.Length);
            var imported = TTModel.FromRaw(xivMdl);
            var modFileName = GetModFileName(mods, option);
            var ret = new ModelMod(mods.FullPath, imported)
            {
                ModFileName = modFileName,
                Path = mods.FullPath,
                Name = mods.Name,
                Category = mods.Category
            };
            return ret;
        }

        private async Task<Mod?> ExtractTexture(ModsJson mods, SqPackStream pack, ModOptionJson? option = null)
        {
            if (mods.FullPath.Contains(".tex"))
            {
                var bytes = new byte[mods.ModSize];
                pack.BaseStream.Seek(mods.ModOffset, SeekOrigin.Begin);
                pack.BaseStream.Read(bytes, 0, mods.ModSize);
                var xivTex = await DatExtensions.GetType4Data(bytes);
                var type = XivTexType.Normal;

                try
                {
                    type = XivPathParser.GetTexType(mods.FullPath);
                } catch (ArgumentException)
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

                var modFileName = GetModFileName(mods, option);
                return new TextureMod(xivTex, false)
                {
                    ModFileName = modFileName,
                    ModFilePath = mods.FullPath,
                    Path = mods.FullPath,
                    Name = mods.Name,
                    Category = mods.Category
                };
            }
            return null;
        }

        // TODO: Something akin to: ImportStandard(byte[] bytes, ...) ?
        private async Task<Mod?> ExtractStandard(ModsJson mods, SqPackStream pack, ModOptionJson? option = null)
        {
            if (mods.FullPath.Contains(".mtrl"))
            {                
                try
                {
                    var file = pack.ReadFile<MtrlFile>(mods.ModOffset);
                    var gameDirectory = new DirectoryInfo(_gameDirectory);
                    // TODO: Does seem to be creating correct color set data...?
                    var xivMtrl = await MtrlExtensions.GetMtrlData(gameDirectory, file.Data, mods.FullPath);

                    var modFileName = GetModFileName(mods, option);
                    var mtrlMod = new MaterialMod(xivMtrl)
                    {
                        ModFileName = mods.Name,
                        ModFilePath = mods.FullPath,

                        Path = mods.FullPath,
                        Name = mods.Name,
                        Category = mods.Category
                    };
                    return mtrlMod;
                } catch (NotImplementedException ex)
                {
                    Log.Error(ex, "");
                    return null;
                }

            }
            else if (mods.FullPath.Contains(".meta"))
            {
                var file = pack.ReadFile<FileResource>(mods.ModOffset);
                var meta = await ItemMetadata.Deserialize(file.Data);
                var x = meta.EqdpEntries[XivRace.Hyur_Midlander_Male];
            }
            return ExtractReadOnlyMod(mods, pack);
        }

        private ReadOnlyMod? ExtractReadOnlyMod(ModsJson mods, SqPackStream pack, ModOptionJson? option = null)
        {
            var bytes = new byte[mods.ModSize];
            var startPosition = pack.BaseStream.Position;

            pack.BaseStream.Seek(mods.ModOffset, SeekOrigin.Begin);
            pack.BaseStream.Read(bytes, 0, mods.ModSize);

            pack.BaseStream.Position = startPosition;

            var modFileName = GetModFileName(mods, option);
            var ret = new ReadOnlyMod()
            {
                ModFileName = modFileName,
                ModFilePath = mods.FullPath,

                Name = mods.Name,
                Path = mods.FullPath,
                Category = mods.Category,
                Data = bytes
            };
            return ret;
        }
    }
}
