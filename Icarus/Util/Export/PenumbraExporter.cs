using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Ionic.Zip;
using Lumina;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using ModPack = Icarus.Mods.DataContainers.ModPack;

namespace Icarus.Util
{
    public class PenumbraExporter : Exporter
    {
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
            _logService.Information("Starting export to simple penumbra modpack.");
            var tempDir = GetTempDirectory(outputDir);

            if (toFileStructure)
            {
                var metaJson = GetMetaJson(modPack);
                var defaultJson = GetDefaultModJson(modPack.SimpleModsList, true);

                File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
                File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);
            }

            var numFiles = 0;
            var numWritten = 0;
            foreach (var entry in modPack.SimpleModsList)
            {
                // Maybe unecessary? But just to be safe, i guess
                if (ForbiddenModTypes.Contains(entry.Path)) continue;
                if (!entry.ShouldExport) continue;

                try
                {
                    var bytes = await WriteToBytes(entry, false);
                    numFiles++;
                    if (bytes.Length == 0) continue;

                    var path = Path.Combine(tempDir, entry.Path);
                    var file = new FileInfo(path);
                    if (toPmp || toFileStructure)
                    {
                        file.Directory.Create();
                        if (File.Exists(path))
                        {
                            _logService.Warning($"{file.Name} already exists and is being overwritten by a newer version.");
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
                    numWritten++;
                } catch (Exception ex)
                {
                    _logService.Error(ex, "An exception has occurred");
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
                                // TODO: Put into "Manipulations"
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
                        // TODO: What to do here?
                        def.Files.Add(entry.Path, entry.Path);
                    }

                    if (entry is IAdditionalPathsMod pathsMod)
                    {

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
