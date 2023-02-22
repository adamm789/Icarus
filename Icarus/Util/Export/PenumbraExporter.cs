using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ModPack = Icarus.Mods.DataContainers.ModPack;
using GameData = Lumina.GameData;

namespace Icarus.Util
{
    public class PenumbraExporter : Exporter
    {
        // TODO: Penumbra Metadata export
        // TODO: Penumbra Advanced ModPack export
        public PenumbraExporter(GameData lumina, ILogService logService) : base(lumina, logService)
        {

        }

        public async Task<string> ExportToSimple(ModPack modPack, FileSystemInfo info, bool toFileStructure = true)
        {
            if (info is FileInfo file)
            {
                _logService.Debug($"Exporting file.");
                return await ExportToSimple(modPack, file.Directory.FullName, true);

            }
            else if (info is DirectoryInfo dir)
            {
                _logService.Debug($"Exporting directory with toFileStructure = {toFileStructure}");
                return await ExportToSimple(modPack, dir.FullName, false, toFileStructure);
            }

            throw new NotImplementedException();
        }

        public async Task<string> ExportToAdvanced(ModPack modPack, FileSystemInfo info)
        {
            if (info is FileInfo file)
            {
            }
            else if (info is DirectoryInfo dir)
            {

            }
            throw new NotImplementedException();
        }

        internal async Task<string> ExportToSimple(ModPack modPack, string outputDir, bool toPmp = true, bool toFileStructure = true)
        {
            _logService?.Information("Starting export to simple penumbra modpack.");
            var tempDir = GetTempDirectory(outputDir);

            if (toFileStructure)
            {
                var metaJson = GetMetaJson(modPack);
                var defaultJson = GetDefaultModJson(modPack.SimpleModsList, true);

                File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
                File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);
            }

            var exportEntries = modPack.SimpleModsList.FindAll(m => m.ShouldExport);
            if (exportEntries.Count == 0)
            {
                _logService?.Information($"No entries were selected for export");
                return "";
            }
            var tasks = new Task<byte[]>[exportEntries.Count];
            var byteList = new List<byte[]>();

            for (var i = 0; i < exportEntries.Count; i++)
            {
                var j = i;
                tasks[j] = WriteToBytes(exportEntries[j], false);
            }

            await Task.WhenAll(tasks);

            var numFiles = 0;
            var numWritten = 0;

            for (var i = 0; i < exportEntries.Count; i++)
            {
                try
                {
                    var entry = exportEntries[i];
                    var bytes = tasks[i].Result;
                    if (bytes.Length == 0) continue;

                    var path = Path.Combine(tempDir, entry.Path);
                    var file = new FileInfo(path);
                    if (toPmp || toFileStructure)
                    {
                        file.Directory.Create();
                        if (File.Exists(path))
                        {
                            _logService?.Warning($"{file.Name} already exists and is being overwritten by a newer version.");
                        }
                    }
                    else
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                        var ext = file.Extension;

                        var num = 0;
                        path = Path.Combine(tempDir, file.Name);
                        while (File.Exists(path))
                        {
                            path = Path.Combine(tempDir, $"{fileName} ({num}){ext}");
                            num++;
                        }
                    }
                    File.WriteAllBytes(path, bytes);
                    numFiles++;
                    numWritten++;
                }
                catch (Exception ex)
                {
                    _logService?.Error(ex, $"Could not export entry {i}");
                }
            }

            _logService.Information($"Wrote {numWritten} out of {numFiles} mods.");
            var outputPath = GetOutputPath(modPack, outputDir);
            var finalOutputPath = WriteFiles(tempDir, outputPath, toPmp);

            return finalOutputPath;
        }

        // TODO: Implement Advanced Penumbra mods
        internal async Task<string> ExportToAdvanced(ModPack modPack, string outputDir, bool toPmp = true, bool toFileStructure = true)
        {
            var tempDir = Path.Combine(outputDir, "temp");

            var metaJson = GetMetaJson(modPack);
            var defaultJson = GetDefaultModJson(modPack.SimpleModsList, false);

            File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
            File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);

            WriteGroups(modPack, tempDir);
            var outputPath = GetOutputPath(modPack, outputDir);
            var finalOutputPath = WriteFiles(tempDir, outputPath, toPmp);

            return await Task.Run(() => finalOutputPath);
        }

        private string WriteFiles(string tempDir, string outputPath, bool toPmp = true)
        {
            _logService.Verbose($"Writing from {tempDir} to {outputPath}.");
            var ret = outputPath;

            if (toPmp)
            {
                var pmpOutputPath = outputPath + ".pmp";
                var zf = new ZipFile();
                zf.AddDirectory(tempDir);
                zf.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zf.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                zf.Save(pmpOutputPath);

                ret = pmpOutputPath;
            }
            else
            {
                if (Directory.Exists(outputPath))
                {
                    _logService.Debug($"Deleting {outputPath}.");
                    Directory.Delete(outputPath, true);
                }
                //Directory.Copy(tempDir, outputPath);
                FileSystem.MoveDirectory(tempDir, outputPath);
            }

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            return ret;
        }

        private void WriteGroups(ModPack modPack, string modDir)
        {
            _logService.Verbose("Writing groups.");
            var groupIndex = 1;
            foreach (var page in modPack.ModPackPages)
            {
                foreach (var group in page.ModGroups)
                {
                    var groupString = groupIndex.ToString().PadLeft(3, '0');
                    var jsonFileName = "group_" + groupString + "_" + group.GroupName.ToLower() + ".json";

                    var penumbraGroup = new PenumbraModGroup()
                    {
                        Name = group.GroupName,
                        Description = "",
                        Priority = 0,
                        Type = "Single"
                    };

                    foreach (var option in group.OptionList)
                    {
                        var penumbraOption = new PenumbraModOption()
                        {
                            Name = option.Name,
                        };
                        foreach (var mods in option.Mods)
                        {
                            var optionPath = Path.Combine(group.GroupName.ToLower(), mods.Path);
                            //if (!penumbraOption.Files.ContainsKey(mods.Path))
                            //{
                            penumbraOption.Files.Add(mods.Path, optionPath);
                            if (mods is MetadataMod meta)
                            {
                                var primaryIdRegex = new Regex(@"[a,e]([0-9]){4}");
                                if (!primaryIdRegex.IsMatch(mods.Path))
                                {
                                    continue;
                                }
                                var matches = primaryIdRegex.Matches(mods.Path);
                                var item = new PenumbraManipulation()
                                {
                                    Type = "Imc"
                                };
                                //penumbraOption.Manipulations.Add(item);
                                // TODO: Put into "Manipulations"
                                foreach (var imc in meta.ImcEntries)
                                {
                                    var entry = new PenumbraImcEntry()
                                    {
                                        MaterialId = imc.MaterialSet,
                                        DecalId = imc.Decal,
                                        VfxId = imc.Vfx
                                    };
                                }

                            }
                            //}
                        }
                        penumbraGroup.Options.Add(penumbraOption);
                    }
                    groupIndex++;
                    var str = JsonConvert.SerializeObject(penumbraGroup);
                    File.WriteAllText(Path.Combine(modDir, jsonFileName), str);
                }
            }
        }

        private string GetOptionsJson()
        {
            throw new NotImplementedException();
        }

        private string GetDefaultModJson(IEnumerable<IGameFile> entries, bool isSimple)
        {
            // TODO: If Advanced Penumbra mod, seems like default_mod is essentially blank
            // TODO: If Simple Penumbra mod, meta goes into Manipulations here
            var def = new PenumbraDefaultMod();
            if (isSimple)
            {
                foreach (var entry in entries)
                {
                    // TODO: Dictionary to prevent redundantly adding mod files?
                    if (def.Files.ContainsKey(entry.Path))
                    {

                    }
                    else
                    {
                        if (entry is IAdditionalPathsMod pathsMod && pathsMod.AssignToAllPaths)
                        {
                            foreach (var kvp in pathsMod.AllPathsDictionary)
                            {
                                var gamePath = kvp.Key;
                                def.Files.Add(gamePath, entry.Path);
                            }
                        }
                        else
                        {
                            def.Files.Add(entry.Path, entry.Path);
                        }
                    }

                }
            }

            var json = JsonConvert.SerializeObject(def);
            return json;
        }

        private string GetMetaJson(ModPack modPack)
        {
            var meta = new PenumbraMeta(modPack); ;
            var json = JsonConvert.SerializeObject(meta);
            return json;
        }
    }
}
