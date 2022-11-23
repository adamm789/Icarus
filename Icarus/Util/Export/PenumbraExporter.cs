using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Ionic.Zip;
using Lumina;
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

        public async Task<string> ExportToSimple(ModPack modPack, FileSystemInfo info)
        {
            if (info is FileInfo file)
            {

            }
            else if (info is DirectoryInfo dir)
            {

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

        internal async Task<string> ExportToSimple(ModPack modPack, string outputDir, bool toPmp = true)
        {
            _logService.Information("Starting export to simple penumbra modpack.");
            var tempDir = GetTempDirectory(outputDir);

            var metaJson = GetMetaJson(modPack);
            var defaultJson = GetDefaultModJson(modPack.SimpleModsList, true);

            foreach (var entry in modPack.SimpleModsList)
            {
                // Maybe unecessary? But just to be safe, i guess
                if (ForbiddenModTypes.Contains(entry.Path)) continue;
                var bytes = await WriteToBytes(entry, false);

                if (bytes.Length == 0) continue;

                var path = Path.Combine(tempDir, entry.Path);
                var file = new FileInfo(path);
                file.Directory.Create();
                File.WriteAllBytes(path, bytes);
            }
            File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
            File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);

            var outputPath = GetOutputPath(modPack, outputDir);
            var finalOutputPath = WriteFiles(tempDir, outputPath, toPmp);

            return finalOutputPath;
        }

        // TODO: Implement Advanced Penumbra mods
        internal async Task<string> ExportToAdvanced(ModPack modPack, string outputDir, bool toPmp = true)
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
                    Directory.Delete(outputPath, true);
                }
                Directory.Move(tempDir, outputPath);
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
