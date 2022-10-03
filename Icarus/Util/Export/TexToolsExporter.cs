﻿using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.ViewModels.Mods;
using Ionic.Zip;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

using IcarusModPack = Icarus.Mods.DataContainers.ModPack;
using FrameworkModPack = xivModdingFramework.Mods.DataContainers.ModPack;
using Path = System.IO.Path;
using Icarus.Mods.Interfaces;
using System.Collections.Concurrent;
using Icarus.Services.Interfaces;
using ItemDatabase.Paths;
using Lumina;
using System.Threading;

namespace Icarus.Util
{
    public class TexToolsExporter : Exporter
    {
        private const string _currentWizardTTMPVersion = "1.3w";
        private const string _currentSimpleTTMPVersion = "1.3s";
        private const string _minimumAssembly = "1.3.0.0";

        public TexToolsExporter(ILogService logService) : base(logService)
        {
            
        }

        public override string GetOutputPath(IcarusModPack modPack, string outputDir)
        {
            var path = Path.Combine(outputDir, $"{modPack.Name}.ttmp2");
            return path;
        }

        // TODO: CancellationToken for TexTools ExportToSimple
        // TODO: IProgress for TexTools ExportToSimple
        internal async Task<string> ExportToSimple(IcarusModPack modPack, string outputDir,
            CancellationToken? cancellationToken = null, IProgress<(int, int)>? progress = null)
        {
            _logService.Information("Starting export to simple textools modpack.");
            var entries = modPack.SimpleModsList;
            if (entries == null || entries.Count == 0)
            {
                var err = "SimpleModsList/Entries is null or empty. Returning.";
                _logService.Error(err);
                throw new ArgumentNullException(err);
            }
            if (String.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            var tempDir = Path.Combine(outputDir, "temp");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            string _tempMPD = Path.Combine(tempDir, "TTMPD.mpd");
            string _tempMPL = Path.Combine(tempDir, "TTMPL.mpl");

            var modPackJson = new ModPackJson
            {
                TTMPVersion = _currentSimpleTTMPVersion,
                Name = modPack.Name,
                Author = modPack.Author,
                Version = modPack.Version,
                MinimumFrameworkVersion = _minimumAssembly,
                Url = modPack.Url,
                Description = modPack.Description,
                SimpleModsList = new List<ModsJson>()
            };

            _logService.Verbose("Creating ModPackJson.");
            var tasks = new Task<byte[]>[entries.Count];

            for (var i = 0; i < entries.Count; i++)
            {
                var j = i;
                tasks[j] = Task.Run(() => GetBytes(entries[j], j));
            }

            await Task.WhenAll(tasks);

            var offset = 0;
            var numMods = 0;
            using (var bw = new BinaryWriter(File.Open(_tempMPD, FileMode.Create)))
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    // TODO: Remove duplicate code of writing mods by extracting into separate function?
                    var entry = entries[i];
                    var name = entry.Name;
                    var category = entry.Category;
                    var path = entry.Path;

                    var bytes = await tasks[i];
                    if (bytes == Array.Empty<Byte>())
                    {
                        _logService.Error($"Could not get bytes for {name}. Skipping entry {i}.");
                        continue;
                    }

                    // Maybe unecessary? But just to be safe, I guess
                    if (ForbiddenModTypes.Contains(path))
                    {
                        _logService.Error($"{path} is a forbidden mod type. Skipping");
                        continue;
                    }

                    numMods++;
                    var datFile = XivPathParser.GetDatFile(path);

                    var modsJson = new ModsJson
                    {
                        Name = name,
                        Category = category,
                        FullPath = path,
                        ModOffset = offset,
                        ModSize = bytes.Length,
                        DatFile = datFile,
                        IsDefault = false,
                        ModPackEntry = new FrameworkModPack
                        {
                            name = modPack.Name,
                            author = modPack.Author,
                            version = modPack.Version,
                            url = modPack.Url
                        }
                    };

                    modPackJson.SimpleModsList.Add(modsJson);
                    modsJson.ModOffset = offset;
                    offset += bytes.Length;
                    bw.Write(bytes);
                }
            }

            try
            {
                var modPackPath = GetOutputPath(modPack, outputDir);
                _logService.Information($"Saving ttmp2 file to {modPackPath}.");

                File.WriteAllText(_tempMPL, JsonConvert.SerializeObject(modPackJson));

                var zf = new ZipFile();
                zf.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zf.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                zf.AddFile(_tempMPL, "");
                zf.AddFile(_tempMPD, "");

                if (File.Exists(modPackPath))
                {
                    File.Delete(modPackPath);
                }
                zf.Save(modPackPath);
                _logService.Verbose($"Wrote {offset} bytes.");
                if (numMods == entries.Count)
                {
                    _logService.Information($"Sucessfully saved {modPackPath}.");
                }
                else
                {
                    _logService.Warning($"Saved {modPackPath}. But only wrote {numMods} out of {entries.Count} mod(s).");
                }
                return modPackPath;
            }
            catch (ArgumentException ex)
            {
                Log.Error("Caught exception while writing to directory. {ex}.", ex);
            }
            finally
            {
                Log.Verbose($"Deleting {tempDir}.");
                Directory.Delete(tempDir, true);
            }
            return "";
        }

        internal async Task<string> ExportToAdvanced(IcarusModPack modPack, string outputDir)
        {
            Log.Information("Exporting to advanced textools modpack.");
            if (String.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            var tempDir = Path.Combine(outputDir, "temp");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            string _tempMPD = Path.Combine(tempDir, "TTMPD.mpd");
            string _tempMPL = Path.Combine(tempDir, "TTMPL.mpl");

            // TODO: Advanced modpack image list...?
            var imageList = new HashSet<string>();

            var modPackJson = new ModPackJson
            {
                TTMPVersion = _currentWizardTTMPVersion,
                Name = modPack.Name,
                Author = modPack.Author,
                Version = modPack.Version,
                MinimumFrameworkVersion = _minimumAssembly,
                Url = modPack.Url,
                Description = modPack.Description,
                ModPackPages = new List<ModPackPageJson>()
            };

            var entries = modPack.SimpleModsList;
            var tasks = new Task<byte[]>[entries.Count];
            // var tasks = new Task< Dictionary<IMod, byte[]> >[entries.Count];

            var concurrentDict = new ConcurrentDictionary<IMod, Task<byte[]>>();

            for (var i = 0; i < entries.Count; i++)
            {
                var j = i;
                concurrentDict.TryAdd(entries[j], Task.Run(() => GetBytes(entries[j], j)));
            }

            await Task.WhenAll(concurrentDict.Values);

            var offsetDict = new Dictionary<IMod, int>();

            var offset = 0;
            using (var bw = new BinaryWriter(File.Open(_tempMPD, FileMode.Create)))
            {
                foreach (var modPackPage in modPack.ModPackPages)
                {
                    var modPackPageJson = new ModPackPageJson
                    {
                        PageIndex = modPackPage.PageIndex,
                        ModGroups = new List<ModGroupJson>()
                    };

                    modPackJson.ModPackPages.Add(modPackPageJson);
                    foreach (var modGroup in modPackPage.ModGroups)
                    {
                        var modGroupJson = new ModGroupJson
                        {
                            GroupName = modGroup.GroupName,
                            SelectionType = modGroup.SelectionType,
                            OptionList = new List<ModOptionJson>()
                        };

                        modPackPageJson.ModGroups.Add(modGroupJson);
                        foreach (var modOption in modGroup.OptionList)
                        {
                            var imageFileName = "";
                            if (!String.IsNullOrEmpty(modOption.ImagePath))
                            {
                                var fname = Path.GetFileName(modOption.ImagePath);
                                imageFileName = Path.Combine(tempDir, fname);
                                File.Copy(modOption.ImagePath, imageFileName, true);
                                imageList.Add(imageFileName);
                            }
                            var fn = imageFileName == "" ? "" : "images/" + Path.GetFileName(imageFileName);
                            var modOptionJson = new ModOptionJson
                            {
                                Name = modOption.Name,
                                Description = modOption.Description,
                                ImagePath = fn,
                                GroupName = modOption.GroupName,
                                SelectionType = modOption.SelectionType,
                                IsChecked = modOption.IsChecked,
                                ModsJsons = new List<ModsJson>()
                            };

                            if (String.IsNullOrEmpty(modOptionJson.GroupName))
                            {
                                modOptionJson.GroupName = modGroup.GroupName;
                            }

                            modGroupJson.OptionList.Add(modOptionJson);
                            foreach (var modOptionMod in modOption.Mods)
                            {
                                var name = modOptionMod.Name;
                                var category = modOptionMod.Category;
                                var path = modOptionMod.Path;

                                // Maybe unecessary? But just to be safe, I guess
                                if (ForbiddenModTypes.Contains(path))
                                {
                                    _logService.Error($"{path} is a forbidden mod type. Skipping");
                                    continue;
                                }
                                var datFile = XivPathParser.GetDatFile(path);
                                byte[] bytes = await concurrentDict[modOptionMod];

                                if (bytes == Array.Empty<Byte>())
                                {
                                    if (bytes == Array.Empty<Byte>())
                                    {
                                        _logService.Error($"Could not get bytes for {name}. Skipping.");
                                        continue;
                                    }
                                }
                                var modOffset = offset;

                                // Don't write duplicate mods
                                if (offsetDict.ContainsKey(modOptionMod))
                                {
                                    modOffset = offsetDict[modOptionMod];
                                } else
                                {
                                    offsetDict.Add(modOptionMod, offset);
                                }
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
                                if (modOffset == offset)
                                {
                                    bw.Write(bytes);
                                    offset += bytes.Length;
                                }
                            }
                        }
                    }
                }
            }

            string modPackPath = "";
            try
            {
                _logService.Information("Saving ttmp2 file.");
                modPackPath = GetOutputPath(modPack, outputDir);
                File.WriteAllText(_tempMPL, JsonConvert.SerializeObject(modPackJson));

                var zf = new ZipFile();
                zf.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zf.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                zf.AddFile(_tempMPL, "");
                zf.AddFile(_tempMPD, "");

                if (File.Exists(modPackPath))
                {
                    File.Delete(modPackPath);
                }
                zf.Save(modPackPath);

                foreach (var image in imageList)
                {
                    zf.AddFile(image, "images");
                }
                zf.Save(modPackPath);

                // TODO: Report if pack was not "fully" exported, i.e. skipped mods and/or pages
                _logService.Information($"Sucessfully saved {modPackPath}.");
                return modPackPath;
            }
            catch (ArgumentException ex)
            {
                _logService.Error(ex.Message);
            }
            finally
            {
                _logService.Verbose($"Deleting {tempDir}.");
                Directory.Delete(tempDir, true);
            }
            return modPackPath;
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