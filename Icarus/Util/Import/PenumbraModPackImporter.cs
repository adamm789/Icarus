using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Util.Extensions;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina;
using Lumina.Data;
using Lumina.Data.Files;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using static System.Net.Mime.MediaTypeNames;

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

        // TODO: Advanced mod packs
        public async Task<ModPack?> ImportPenumbraDirectory(DirectoryInfo dir)
        {
            var dirPath = dir.FullName;
            var ret = new ModPack();

            var defaultModFile = dir.EnumerateFiles().Where(x => x.Name.Contains("default_mod")).FirstOrDefault();
            if (defaultModFile != null)
            {
                // TODO: Check this for "simple" mods
                var defaultModText = File.ReadAllText(defaultModFile.FullName);
                if (defaultModText != null)
                {
                    var contents = JsonConvert.DeserializeObject<PenumbraDefaultMod>(defaultModText);

                    if (contents != null)
                    {
                        if (contents.Files.Count > 0)
                        {
                            foreach (var kvp in contents.Files)
                            {
                                var gameFilePath = kvp.Key;
                                var filePath = Path.Combine(dirPath, kvp.Value);
                                var mod = await ProcessMod(gameFilePath, filePath);
                                if (mod != null)
                                {
                                    ret.SimpleModsList.Add(mod);
                                }
                            }
                        }
                    }
                }
                return ret;
            }
            else
            {
                Log.Error($"Could not find default_mod.json");
            }

            return null;
        }

        public async Task<ModPack?> ExtractPenumbraModPack(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

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

            /*
            try
            {
                Log.Debug($"Beginning extraction");
                zip.ExtractAll(tempPath);
                Log.Debug($"Finished extraction");
                var defaultModFile = tempDir.EnumerateFiles().Where(x => x.Name.Contains("default_mod")).FirstOrDefault();
                if (defaultModFile != null)
                {
                    // TODO: Check this for "simple" mods
                    var defaultModText = File.ReadAllText(defaultModFile.FullName);
                    if (defaultModText != null)
                    {
                        var contents = JsonConvert.DeserializeObject<PenumbraDefaultMod>(defaultModText);
                        if (contents != null)
                        {
                            foreach (var kvp in contents.Files)
                            {
                                var mod = await ProcessMod(kvp, tempPath);
                                if (mod != null)
                                {
                                    ret.SimpleModsList.Add(mod);
                                }
                            }
                        }
                    }
                    return ret;
                }
                else
                {
                    Log.Error($"Could not find default_mod");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
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
            */

            return null;
        }

        private async Task<IMod?> ProcessMod(string gameFilePath, string filePath)
        {
            // TODO: Need "Name" of the item
            var ext = Path.GetExtension(filePath);

            if (ext == ".mdl")
            {
                var mdl = TryImportMdl(gameFilePath, filePath);
                if (mdl != null)
                {
                    return mdl;
                }
            }
            else if (ext == ".mtrl")
            {
                var mtrl = await TryImportMtrl(gameFilePath, filePath);
                if (mtrl != null)
                {
                    return mtrl;
                }
            }
            else if (ext == ".tex")
            {

            }
            return null;
        }

        public ModelMod? TryImportIMdl(string filePath)
        {
            var mdlData = File.ReadAllBytes(filePath);
            try
            {
                //var xivMdl = MdlWithFramework.GetRawMdlDataFramework("", mdlData);
                var xivMdl = Mdl.GetRawMdlData("", mdlData);
                var imported = TTModel.FromRaw(xivMdl);
                // TODO: Try to guess the target model based off the mtrl?
                var ret = new ModelMod(filePath, imported, ImportSource.RawGameFile)
                {
                    ModFilePath = filePath,
                    ModFileName = Path.GetFileName(filePath)
                };
                return ret;
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="tempPath"></param>
        /// <returns></returns>
        private ModelMod? TryImportMdl(string gameFilePath, string filePath)
        {
            var mdlData = File.ReadAllBytes(filePath);

            try
            {
                //var xivMdl = MdlWithFramework.GetRawMdlDataFramework(gameFilePath, mdlData);
                var xivMdl = Mdl.GetRawMdlData(gameFilePath, mdlData);
                var imported = TTModel.FromRaw(xivMdl);

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
            catch (Exception ex)
            {
                Log.Debug($"Could not process model: {filePath}");
                Log.Debug(ex.Message);
            }
            return null;
        }

        private async Task<MaterialMod?> TryImportMtrl(string gamePath, string filePath)
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
                        Category = XivPathParser.GetCategoryFromPath(filePath)
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
    }
}
