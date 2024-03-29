﻿using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina;
using Lumina.Data;
using Newtonsoft.Json;
using Serilog;
using SharpDX.Direct2D1;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;
using FrameworkModPack = xivModdingFramework.Mods.DataContainers.ModPack;
using IcarusModPack = Icarus.Mods.DataContainers.ModPack;
using Path = System.IO.Path;

namespace Icarus.Util
{
    public class TexToolsExporter : Exporter
    {
        private const string _currentWizardTTMPVersion = "1.3w";
        private const string _currentSimpleTTMPVersion = "1.3s";
        private const string _minimumAssembly = "1.3.0.0";

        private string _tempDir;
        private string _tempMPD;
        private string _tempMPL;

        public TexToolsExporter(GameData lumina, ILogService logService) : base(lumina, logService)
        {

        }

        public override string GetOutputPath(IcarusModPack modPack, string outputDir)
        {
            var path = Path.Combine(outputDir, $"{modPack.Name}.ttmp2");
            return path;
        }

        public async Task<string> ExportToSimple(IcarusModPack modPack, string outputDir) {

            var fileInfo = new FileInfo(Path.Combine(outputDir, $"{modPack.Name}.ttmp2"));
            return await ExportToSimple(modPack, fileInfo);
        }

        public async Task<string> ExportToAdvanced(IcarusModPack modPack, string outputDir)
        {
            var fileInfo = new FileInfo(Path.Combine(outputDir, $"{modPack.Name}.ttmp2"));
            return await ExportToSimple(modPack, fileInfo);
        }

        // TODO: Export to Standard...?
        // TODO: CancellationToken for TexTools ExportToSimple
        // TODO: IProgress for TexTools ExportToSimple
        public async Task<string> ExportToSimple(IcarusModPack modPack, FileInfo fileInfo,
            CancellationToken? cancellationToken = null, IProgress<(int, int)>? progress = null)
        {
            _logService.Information("Exporting to simple textools modpack.");
            var entries = modPack.SimpleModsList;
            if (entries == null || entries.Count == 0)
            {
                var err = "SimpleModsList/Entries is null or empty. Returning.";
                _logService?.Error(err);
                throw new ArgumentNullException(err);
            }

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _tempDir = GetTempDirectory(fileInfo.Directory.FullName);
            _tempMPD = GetMPDPath(_tempDir);
            _tempMPL = GetMPLPath(_tempDir);

            var modPackJson = GetModPackJson(modPack);
            modPackJson.TTMPVersion = _currentSimpleTTMPVersion;
            modPackJson.SimpleModsList = new();

            var exportEntries = entries.FindAll(m => m.ShouldExport);
            // TODO: For "additional paths mods" add to "numexported"?
            if (exportEntries.Count == 0)
            {
                _logService.Information("No entries were selected for export.");
                return "";
            }
            var tasks = new Task<byte[]>[exportEntries.Count];
            var byteList = new List<byte[]>();

            for (var i = 0; i < exportEntries.Count; i++)
            {
                var j = i;

                _logService?.Debug($"On entry {i}");
                tasks[j] = Task.Run(() => GetBytes(exportEntries[j], j));
                //byteList.Add(await GetBytes(entries[i]));
            }

            // TODO: Report progress
            await Task.WhenAll(tasks);

            var offset = 0;
            var numMods = 0;
            var numModsWritten = 0;
            _logService?.Debug($"Writing to {_tempMPD}");

            using (var bw = new BinaryWriter(File.Open(_tempMPD, FileMode.Create)))
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    numMods++;
                    var entry = exportEntries[i];
                    var bytes = await tasks[i];
                    //var bytes = byteList[i];

                    if (bytes == Array.Empty<byte>())
                    {
                        _logService?.Error($"Could not get bytes for {entry.Name}. Skipping entry {i}.");
                        continue;
                    }

                    /*
                    var modsJson = GetModsJson(entry, offset, bytes.Length, modPack);
                    List<ModsJson>? pathsModJson = null;

                    if (entry is IAdditionalPathsMod pathsMod && pathsMod.HasAdditionalPaths)
                    {
                        pathsModJson = GetAdditionalPaths(pathsMod, offset, bytes.Length, modPack);
                    }

                    if (modsJson != null)
                    {
                        modPackJson.SimpleModsList.Add(modsJson);

                        if (pathsModJson != null)
                        {
                            modPackJson.SimpleModsList.AddRange(pathsModJson);
                        }

                        offset += bytes.Length;
                        bw.Write(bytes);
                        numModsWritten++;
                    }
                    */

                    if (entry is IAdditionalPathsMod pathsMod && pathsMod.AssignToAllPaths)
                    {
                        var pathsModJson = GetAllPathsModJson(pathsMod, offset, bytes.Length, modPack);
                        modPackJson.SimpleModsList.AddRange(pathsModJson);
                    }
                    else
                    {
                        var modJson = GetModsJson(entry, offset, bytes.Length, modPack);
                        modPackJson.SimpleModsList.Add(modJson);
                    }
                    offset += bytes.Length;
                    bw.Write(bytes);
                    _logService?.Debug($"Wrote {bytes.Length} bytes");
                    numModsWritten++;
                }
            }

            var modPackPath = TrySaveFile(modPack, fileInfo, modPackJson);
            LogNumModsWritten(numMods, numModsWritten);
            return modPackPath;
        }

        public async Task<string> ExportToAdvanced(IcarusModPack modPack, FileInfo fileInfo,
            CancellationToken? cancellationToken = null, IProgress<(int, int)>? progress = null)
        {
            _logService?.Information("Exporting to advanced textools modpack.");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (fileInfo.Exists)
            {
                _logService?.Debug($"Deleting {fileInfo.FullName} while exporting to advanced textools.");
                fileInfo.Delete();
            }

            _tempDir = GetTempDirectory(fileInfo.Directory.FullName);
            _tempMPD = GetMPDPath(_tempDir);
            _tempMPL = GetMPLPath(_tempDir);

            // TODO: Advanced modpack image list...?
            var imageList = new HashSet<string>();

            var modPackJson = GetModPackJson(modPack);
            modPackJson.TTMPVersion = _currentWizardTTMPVersion;
            modPackJson.ModPackPages = new();

            //var entries = modPack.SimpleModsList;
            var entries = new List<IMod>();
            // var tasks = new Task< Dictionary<IMod, byte[]> >[entries.Count];

            foreach (var page in modPack.ModPackPages)
            {
                foreach (var group in page.ModGroups)
                {
                    foreach (var option in group.OptionList)
                    {
                        foreach (var mod in option.Mods)
                        {
                            // Only convert the ones that are actually used
                            entries.Add(mod);
                        }
                    }
                }
            }
            var tasks = new Task<byte[]>[entries.Count];
            var concurrentDict = new ConcurrentDictionary<IMod, Task<byte[]>>();

            for (var i = 0; i < entries.Count; i++)
            {
                var j = i;
                concurrentDict.TryAdd(entries[j], Task.Run(() => GetBytes(entries[j], j)));
            }

            await Task.WhenAll(concurrentDict.Values);

            var offsetDict = new Dictionary<IMod, int>();

            var offset = 0;
            var numMods = 0;
            var numModsWritten = 0;
            var options = 0;
            using (var bw = new BinaryWriter(File.Open(_tempMPD, FileMode.Create)))
            {
                foreach (var modPackPage in modPack.ModPackPages)
                {
                    // Disallow pages without groups
                    if (modPackPage.ModGroups.Count <= 0)
                    {
                        _logService?.Warning($"Page {modPackPage.PageIndex} has no groups. Skipping page.");
                        continue;
                    }
                    var modPackPageJson = new ModPackPageJson
                    {
                        PageIndex = modPackPage.PageIndex,
                        ModGroups = new List<ModGroupJson>()
                    };

                    modPackJson.ModPackPages.Add(modPackPageJson);
                    foreach (var modGroup in modPackPage.ModGroups)
                    {
                        // Disallow groups with no options
                        if (modGroup.OptionList.Count <= 0)
                        {
                            _logService?.Warning($"{modGroup.GroupName} has no options. Skipping group.");
                            continue;
                        }

                        var modGroupJson = new ModGroupJson
                        {
                            GroupName = modGroup.GroupName,
                            SelectionType = modGroup.SelectionType,
                            OptionList = new List<ModOptionJson>()
                        };

                        modPackPageJson.ModGroups.Add(modGroupJson);
                        foreach (var modOption in modGroup.OptionList)
                        {
                            // Allow options to have no mods
                            var imageFileName = "";
                            if (!String.IsNullOrEmpty(modOption.ImagePath))
                            {
                                var fname = Path.GetFileName(modOption.ImagePath);
                                imageFileName = Path.Combine(_tempDir, fname);
                                if (File.Exists(imageFileName))
                                {
                                    File.Copy(modOption.ImagePath, imageFileName, true);
                                    imageList.Add(imageFileName);
                                }
                                else
                                {
                                    _logService?.Error($"Could not add {imageFileName} to imageList.");
                                }
                            }
                            var fn = imageFileName == "" ? "" : "images/" + Path.GetFileName(imageFileName);
                            var modOptionJson = new ModOptionJson
                            {
                                Name = modOption.Name,
                                Description = modOption.Description,
                                ImagePath = fn,
                                GroupName = modOption.GroupName,
                                SelectionType = modOption.SelectionType,
                                IsChecked = false,
                                ModsJsons = new List<ModsJson>()
                            };

                            if (String.IsNullOrEmpty(modOptionJson.GroupName))
                            {
                                modOptionJson.GroupName = modGroup.GroupName;
                            }

                            modGroupJson.OptionList.Add(modOptionJson);
                            foreach (var modOptionMod in modOption.Mods)
                            {
                                numMods++;
                                byte[] bytes = await concurrentDict[modOptionMod];

                                if (bytes == Array.Empty<byte>())
                                {
                                    _logService?.Error($"Could not get bytes for {modOptionMod.Name}. " 
                                        + $"Skipping page {modPackPage.PageIndex}, group {modGroup.GroupName}, option {options}");
                                    continue;
                                }
                                var modOffset = offset;

                                // TODO?: Don't write duplicate mods?
                                /*if (offsetDict.ContainsKey(modOptionMod))
                                {
                                    modOffset = offsetDict[modOptionMod];
                                    var modsJson = new ModsJson
                                    {
                                        Name = name,
                                        Category = category,
                                        FullPath = path,
                                        ModOffset = modOffset,
                                        ModSize = bytes.Length,
                                        DatFile = datFile,
                                        IsDefault = false,
                                    };
                                    modOptionJson.ModsJsons.Add(modsJson);
                                }
                                else
                                {
                                
                                    offsetDict.Add(modOptionMod, offset);
                                */
                                /*
                                var modsJson = GetModsJson(modOptionMod, offset, bytes.Length);

                                List<ModsJson>? pathsModJson = null;
                                if (modOptionMod is IAdditionalPathsMod pathsMod && pathsMod.HasAdditionalPaths)
                                {
                                    pathsModJson = GetAdditionalPaths(pathsMod, offset, bytes.Length, modPack);
                                }

                                if (modsJson != null)
                                {
                                    modOptionJson.ModsJsons.Add(modsJson);
                                    bw.Write(bytes);
                                    offset += bytes.Length;
                                    numModsWritten++;
                                    options++;
                                }
                                */
                                var success = false;
                                if (modOptionMod is IAdditionalPathsMod pathsMod && pathsMod.AssignToAllPaths)
                                {
                                    var pathsModJson = GetAllPathsModJson(pathsMod, offset, bytes.Length, modPack);
                                    modOptionJson.ModsJsons.AddRange(pathsModJson);
                                    success = true;
                                }
                                else
                                {
                                    var modsJson = GetModsJson(modOptionMod, offset, bytes.Length, modPack);
                                    if (modsJson != null)
                                    {
                                        modOptionJson.ModsJsons.Add(modsJson);
                                        success = true;
                                    }
                                }
                                if (success)
                                {
                                    offset += bytes.Length;
                                    bw.Write(bytes);
                                    numModsWritten++;
                                    options++;
                                }
                            }
                        }
                    }
                }
            }
            var modPackPath = TrySaveFile(modPack, fileInfo, modPackJson, imageList);
            LogNumModsWritten(numMods, numModsWritten);

            return modPackPath;
        }

        private string GetMPLPath(string tempDir)
        {
            return Path.Combine(tempDir, "TTMPL.mpl");
        }

        private string GetMPDPath(string tempDir)
        {
            return Path.Combine(tempDir, "TTMPD.mpd");
        }

        private void LogNumModsWritten(int numMods, int numModsWritten)
        {
            if (numMods == numModsWritten)
            {
                _logService?.Information($"Wrote {numModsWritten} out of {numMods} mods.");
            }
            else
            {
                _logService?.Warning($"But only wrote {numModsWritten} out of {numMods}.");
            }
        }

        private string TrySaveFile(IcarusModPack modPack, FileInfo file, ModPackJson modPackJson, HashSet<string>? imageList = null)
        {
            _logService?.Information("Trying to save ttmp2 file");
            var zf = new ZipFile
            {
                UseZip64WhenSaving = Zip64Option.AsNecessary,
                CompressionLevel = Ionic.Zlib.CompressionLevel.None
            };
            try
            {
                var fileInfo = new FileInfo(_tempMPD);
                if (fileInfo.Length == 0)
                {
                    return "";
                }

                File.WriteAllText(_tempMPL, JsonConvert.SerializeObject(modPackJson));

                _logService?.Debug($"_tempMPL = {_tempMPL} - _tempMPD = {_tempMPD}");

                zf.AddFile(_tempMPL, "");
                zf.AddFile(_tempMPD, "");

                if (imageList != null)
                {
                    foreach (var image in imageList)
                    {
                        zf.AddFile(image, "images");
                    }
                }
                zf.Save(file.FullName);

                var entries = modPack.SimpleModsList;
                _logService?.Information($"Successfully wrote to {file.FullName}.");
                return file.FullName;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, "An exception has been thrown while trying to save the ttmp2 file.");
            }
            finally
            {
                if (Directory.Exists(_tempDir))
                {
                    _logService?.Debug($"Deleting temporary directory: {_tempDir}");
                    Directory.Delete(_tempDir, true);
                }
                zf.Dispose();
            }
            return "";

        }

        private ModPackJson GetModPackJson(IcarusModPack modPack)
        {
            string? description = null;
            if (!String.IsNullOrWhiteSpace(modPack.Description))
            {
                description = modPack.Description;
            }
            return new ModPackJson()
            {
                Name = modPack.Name,
                Author = modPack.Author,
                Version = modPack.Version,
                MinimumFrameworkVersion = _minimumAssembly,
                Url = modPack.Url,
                Description = description
            };
        }

        private ModsJson? GetModsJson(IMod entry, long offset, int modSize, IcarusModPack? modPack = null)
        {
            var name = entry.Name;
            var category = entry.Category;
            var path = entry.Path;
            var isDefault = entry.IsDefault;

            // Maybe unecessary? But just to be safe, I guess
            if (ForbiddenModTypes.Contains(path))
            {
                _logService?.Error($"{path} is a forbidden mod type. Skipping");
                return null;
            }

            var datFile = XivPathParser.GetDatFile(path);

            var modsJson = new ModsJson
            {
                Name = name,
                Category = category,
                FullPath = path,
                ModSize = modSize,
                DatFile = datFile,
                IsDefault = isDefault,
                ModOffset = offset
            };

            if (modPack != null)
            {
                modsJson.ModPackEntry = new FrameworkModPack
                {
                    name = modPack.Name,
                    author = modPack.Author,
                    version = modPack.Version,
                    url = modPack.Url
                };
            }

            return modsJson;
        }

        private List<ModsJson> GetAllPathsModJson(IAdditionalPathsMod mod, long offset, int modSize, IcarusModPack? modPack = null)
        {
            _logService?.Debug($"Applying to all {mod.AllPathsDictionary.Count} variants.");
            var ret = new List<ModsJson>();
            var category = mod.Category;
            var isDefault = mod.IsDefault;

            foreach (var (path, name) in mod.AllPathsDictionary)
            {
                if (ForbiddenModTypes.Contains(path))
                {
                    _logService?.Error($"{path} is a forbidden mod type. Skipping");
                    continue;
                }
                var datFile = XivPathParser.GetDatFile(path);
                var modsJson = new ModsJson
                {
                    Name = name,
                    Category = category,
                    FullPath = path,
                    ModSize = modSize,
                    DatFile = datFile,
                    IsDefault = isDefault,
                    ModOffset = offset
                };

                isDefault = false;

                if (modPack != null)
                {
                    modsJson.ModPackEntry = new FrameworkModPack
                    {
                        name = modPack.Name,
                        author = modPack.Author,
                        version = modPack.Version,
                        url = modPack.Url
                    };
                }
                ret.Add(modsJson);
            }

            return ret;
        }

        private async Task<byte[]> GetBytes(IMod entry, int counter = 0)
        {
            if (entry is ReadOnlyMod readOnly)
            {
                return readOnly.Data;
            }
            else
            {
                return await WriteToBytes(entry, true, counter);
            }
        }
    }
}
