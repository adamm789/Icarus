using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Util.Extensions;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.DataContainers;

namespace Icarus.Util.Import
{
    public class PenumbraModPackImporter
    {
        private string _gameDirectory;
        private GameData _lumina;

        public PenumbraModPackImporter(string gameDirectory)
        {

            _gameDirectory = gameDirectory;
            _lumina = new(_gameDirectory);
        }

        private async Task<ModPack?> TryReadDefaultMod(DirectoryInfo dir)
        {
            var dirPath = dir.FullName;
            var fileName = dir.Name;
            var defaultModFile = dir.EnumerateFiles().Where(x => x.Name.Contains("default_mod")).FirstOrDefault();
            if (defaultModFile != null)
            {
                // TODO: Check this for "simple" mods
                var defaultModText = File.ReadAllText(defaultModFile.FullName);
                if (!String.IsNullOrWhiteSpace(defaultModText))
                {
                    var contents = JsonConvert.DeserializeObject<PenumbraDefaultMod>(defaultModText);

                    if (contents != null)
                    {
                        if (contents.Files.Count > 0)
                        {
                            var ret = new ModPack();

                            foreach (var kvp in contents.Files)
                            {
                                var gameFilePath = kvp.Key;
                                var filePath = Path.Combine(dirPath, kvp.Value);
                                var mod = await ProcessMod(filePath, gameFilePath);
                                if (mod != null)
                                {
                                    mod.ModFileName = Path.Combine(fileName, kvp.Value);
                                    mod.ModFilePath = Path.Combine(fileName, kvp.Value);
                                    ret.SimpleModsList.Add(mod);
                                }
                            }
                            return ret;
                        }
                    }
                    else
                    {
                        Log.Information($"Could not deserialize default_mod.json");
                    }
                }
                else
                {
                    Log.Information($"No text was present in default_mod.json");
                }
            }
            else
            {
                Log.Error($"Could not find default_mod.json");
            }
            return null;
        }

        private async Task<ModPack?> TryReadGroups(DirectoryInfo dir)
        {
            var dirPath = dir.FullName;
            var fileName = dir.Name;

            var groups = dir.EnumerateFiles().Where(x => x.Name.Contains("group"));
            if (groups != null)
            {
                var ret = new ModPack();
                var pageIndex = 1;
                foreach (var file in groups)
                {
                    var modPackPage = new ModPackPage(pageIndex);
                    var modGroup = new ModGroup();
                    var groupText = File.ReadAllText(file.FullName);
                    if (!String.IsNullOrWhiteSpace(groupText))
                    {
                        var contents = JsonConvert.DeserializeObject<PenumbraModGroup>(groupText);
                        if (contents != null)
                        {
                            modGroup.GroupName = contents.Name;
                            modGroup.SelectionType = contents.Type;
                            foreach (var option in contents.Options)
                            {
                                var modOption = new ModOption();
                                modOption.GroupName = modGroup.GroupName;
                                modOption.Name = option.Name;
                                modOption.SelectionType = contents.Type;
                                foreach (var kvp in option.Files)
                                {
                                    var gameFilePath = kvp.Key;
                                    var filePath = Path.Combine(dirPath, kvp.Value);
                                    var mod = await ProcessMod(filePath, gameFilePath);
                                    if (mod != null)
                                    {
                                        mod.ModFileName = Path.Combine(fileName, kvp.Value);
                                        mod.ModFilePath = Path.Combine(fileName, kvp.Value);
                                        ret.SimpleModsList.Add(mod);
                                        modOption.AddMod(mod);
                                    }
                                }

                                foreach (var manipulation in option.Manipulations)
                                {
                                    // TODO: MetadataMod
                                }

                                if (modOption.Mods.Count > 0)
                                {
                                    modGroup.AddOption(modOption);
                                }
                            }
                            modPackPage.AddGroup(modGroup);
                            ret.ModPackPages.Add(modPackPage);
                        }
                        else
                        {
                            Log.Error($"Could not parse mod group");
                        }
                    }
                    else
                    {
                        Log.Error($"No text was present in {file.Name}");
                    }
                }
                if (ret.ModPackPages.Count > 0)
                {
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Log.Error($"Could not find groups.");
            }
            return null;
        }

        private void TryReadMeta(DirectoryInfo dir, ModPack modPack)
        {
            var dirPath = dir.FullName;
            var fileName = dir.Name;

            var meta = dir.EnumerateFiles().Where(x => x.Name.Contains("meta.json")).FirstOrDefault();
            if (meta != null)
            {
                var metaText = File.ReadAllText(meta.FullName);
                if (!String.IsNullOrWhiteSpace(metaText))
                {
                    var contents = JsonConvert.DeserializeObject<PenumbraMeta>(metaText);
                    if (contents != null)
                    {
                        modPack.SetMetadata(contents);
                    }
                    else
                    {
                        Log.Error($"Could not meta");
                    }
                }
                else
                {
                    Log.Error($"No text was present in meta.json");
                }
            }
            else
            {
                Log.Error($"Could not find meta.json in {dirPath}");
            }
        }

        // TODO: Advanced mod packs
        public async Task<ModPack?> ImportPenumbraDirectory(DirectoryInfo dir)
        {
            // TODO: Read meta.json
            ModPack? modPack = null;
            modPack = await TryReadDefaultMod(dir);

            if (modPack == null)
            {
                modPack = await TryReadGroups(dir);
            }

            if (modPack != null)
            {
                TryReadMeta(dir, modPack);
                return modPack;
            }

            return null;
        }

        public async Task<ModPack?> ExtractPenumbraModPack(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempDir = Directory.CreateDirectory(tempPath);

            using var zfs = fileInfo.OpenRead();
            using var zip = ZipFile.Read(zfs);

            var ret = new ModPack();

            try
            {
                Log.Debug($"Beginning extraction");
                zip.ExtractAll(tempPath);
                Log.Debug($"Finished extraction");
                return await ImportPenumbraDirectory(tempDir);
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }
            finally
            {
                zfs.Close();
                if (tempDir.Exists)
                {
                    Log.Verbose($"Deleting temporary directory: {tempDir.FullName}");
                    tempDir.Delete(true);
                }
            }

            return null;
        }

        private async Task<IMod?> ProcessMod(string filePath, string gameFilePath)
        {
            // TODO: Need "Name" of the item
            var ext = Path.GetExtension(filePath);

            if (ext == ".mdl")
            {
                //var mdl = TryImportMdl(gameFilePath, filePath);
                var mdl = TryImportMdl(filePath, gameFilePath);
                return mdl;
            }
            else if (ext == ".mtrl")
            {
                var mtrl = await TryImportMtrl(filePath, gameFilePath);
                return mtrl;
            }
            else if (ext == ".tex")
            {
                var tex = await TryImportTex(filePath, gameFilePath);
                return tex;
            }
            return null;
        }

        public ModelMod? TryImportMdl(string filePath, string gameFilePath = "")
        {
            try
            {
                var mdlData = File.ReadAllBytes(filePath);
                var xivMdl = Mdl.GetRawMdlData(gameFilePath, mdlData);
                var imported = TTModel.FromRaw(xivMdl);
                if (String.IsNullOrWhiteSpace(gameFilePath))
                {
                    var ret = new ModelMod(filePath, imported, ImportSource.RawGameFile)
                    {
                        ModFilePath = filePath,
                        ModFileName = Path.GetFileName(filePath)
                    };
                    return ret;
                }
                else
                {
                    var category = XivPathParser.GetCategoryFromPath(gameFilePath);
                    var ret = new ModelMod(gameFilePath, imported, ImportSource.TexToolsModPack)
                    {
                        Path = gameFilePath,
                        Category = category,

                        TTModel = imported,
                        XivMdl = xivMdl
                    };

                    return ret;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Could not process model: {filePath}");
                Log.Error(ex.Message);
            }
            return null;
        }

        public async Task<MaterialMod?> TryImportMtrl(string filePath, string gamePath)
        {
            var frameworkDir = new DirectoryInfo(Path.Combine(_gameDirectory, "ffxiv"));
            XivMtrl? mtrl = null;
            try
            {
                var mtrlData = File.ReadAllBytes(filePath);

                //mtrl = await MtrlExtensions.GetMtrlData(frameworkDir, mtrlData, gamePath);
                mtrl = await Mtrl.GetMtrlData(frameworkDir, mtrlData, gamePath);
                if (mtrl != null)
                {
                    var mod = new MaterialMod(mtrl)
                    {
                        ModFileName = filePath,
                        ModFilePath = filePath,
                        Category = XivPathParser.GetCategoryFromPath(gamePath)
                    };
                    return mod;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Could not process material: {filePath}");
                Log.Error(ex.Message);
            }

            return null;
        }

        public async Task<TextureMod?> TryImportTex(string filePath, string gamePath)
        {
            try
            {
                // TODO: This may still be unable to read certain tex files
                var texData = File.ReadAllBytes(filePath);
                //var tex = await DatExtensions.GetType4Data(texData);
                var tex = await TexExtensions.GetXivTex(texData);

                var texType = XivPathParser.GetTexType(gamePath);
                var dataType = XivPathParser.GetXivDataFileFromPath(gamePath);
                var texTypePath = new TexTypePath()
                {
                    Type = texType,
                    Path = gamePath,
                    DataFile = dataType
                };
                tex.TextureTypeAndPath = texTypePath;

                var ret = new TextureMod(tex, ImportSource.RawGameFile)
                {
                    ModFileName = filePath,
                    ModFilePath = filePath,
                    Path = gamePath
                };
                return ret;
            }
            catch (Exception ex)
            {
                Log.Error($"Could not process texture: {filePath}");
                Log.Error(ex.Message);
            }
            return null;
        }
    }
}
