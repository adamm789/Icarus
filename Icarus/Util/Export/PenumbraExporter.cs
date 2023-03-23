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
using Icarus.Penumbra.GameData;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Variants.FileTypes;

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

            var exportEntries = modPack.SimpleModsList.FindAll(m => m.ShouldExport);
            if (exportEntries.Count == 0)
            {
                _logService?.Information($"No entries were selected for export");
                return "";
            }


            if (toFileStructure)
            {
                var metaJson = GetMetaJson(modPack);
                var defaultJson = await GetDefaultModJson(exportEntries, true);

                File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
                File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);
            }


            var nonMetadataEntries = exportEntries.FindAll(m => m is not MetadataMod);
            var metadataEntries = exportEntries.FindAll(m => m is MetadataMod);

            var tasks = new Task<byte[]>[nonMetadataEntries.Count];
            var byteList = new List<byte[]>();

            for (var i = 0; i < nonMetadataEntries.Count; i++)
            {
                var j = i;
                tasks[j] = WriteToBytes(nonMetadataEntries[j], false);
            }

            await Task.WhenAll(tasks);

            var numFiles = 0;
            var numWritten = 0;

            for (var i = 0; i < nonMetadataEntries.Count; i++)
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

            foreach (var meta in metadataEntries)
            {

            }

            _logService?.Information($"Wrote {numWritten} out of {numFiles} mods.");
            var outputPath = GetOutputPath(modPack, outputDir);
            var finalOutputPath = WriteFiles(tempDir, outputPath, toPmp);

            return finalOutputPath;
        }

        // TODO: Implement Advanced Penumbra mods
        internal async Task<string> ExportToAdvanced(ModPack modPack, string outputDir, bool toPmp = true, bool toFileStructure = true)
        {
            var tempDir = Path.Combine(outputDir, "temp");

            var metaJson = GetMetaJson(modPack);
            var defaultJson = await GetDefaultModJson(modPack.SimpleModsList, false);

            File.WriteAllText(Path.Combine(tempDir, "meta.json"), metaJson);
            File.WriteAllText(Path.Combine(tempDir, "default_mod.json"), defaultJson);

            await WriteGroups(modPack, tempDir);
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
                // TODO: How to handle if this directory already exists?
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
                _logService.Debug($"Deleting temporary directory: {tempDir}");
                Directory.Delete(tempDir, true);
            }
            return ret;
        }

        private async Task WriteGroups(ModPack modPack, string modDir)
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
                            penumbraOption.Files.Add(mods.Path, optionPath);
                            if (mods is MetadataMod meta)
                            {
                                var primaryIdRegex = new Regex(@"[a,e]([0-9]){4}");
                                if (!primaryIdRegex.IsMatch(mods.Path))
                                {
                                    continue;
                                }
                                var manips = await GetManipulations(meta);
                                penumbraOption.Manipulations.AddRange(manips);

                            }
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

        private async Task<string> GetDefaultModJson(IEnumerable<IGameFile> entries, bool isSimple)
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
                            if (entry is MetadataMod meta)
                            {
#if DEBUG
                                var manips = await GetManipulations(meta);
                                def.Manipulations.AddRange(manips);
#endif
                            }
                            else
                            {
                                def.Files.Add(entry.Path, entry.Path);
                            }
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

        private async Task<IList<MetaManipulationContainer>> GetManipulations(MetadataMod meta)
        {
            var ret = new List<MetaManipulationContainer>();
            if (meta.ItemMetadata == null)
            {
                return ret;
            }
            var vanillaMetadata = await ItemMetadata.GetMetadata(meta.Path);
            var setId = meta.ItemMetadata.Root.Info.PrimaryId;
            var slot = meta.Category;

            if (meta.ImcEntries != null)
            {
                for (int i = 0; i < meta.ImcEntries.Count; i++)
                {
                    var imc = meta.ImcEntries[i];

                    var add = true;
                    if (vanillaMetadata != null && vanillaMetadata.ImcEntries != null)
                    {
                        if (i < vanillaMetadata.ImcEntries.Count && i >= 0)
                        {
                            var vanillaImc = vanillaMetadata.ImcEntries[i];
                            if (imc.MaterialSet == vanillaImc.MaterialSet &&
                                imc.Decal == vanillaImc.Decal &&
                                imc.Mask == vanillaImc.Mask &&
                                imc.Vfx == vanillaImc.Vfx &&
                                imc.Animation == vanillaImc.Animation)
                            {
                                add = false;
                                break;
                            }
                        }
                    }

                    /*
                    foreach (var imc in meta.ImcEntries)
                    {
                        var isVanilla = false;
                        if (vanillaMetadata != null && vanillaMetadata.ImcEntries != null)
                        {
                            foreach (var vanillaImc in vanillaMetadata.ImcEntries)
                            {
                                if (imc.MaterialSet == vanillaImc.MaterialSet &&
                                    imc.Decal == vanillaImc.Decal &&
                                    imc.Mask == vanillaImc.Mask &&
                                    imc.Vfx == vanillaImc.Vfx &&
                                    imc.Animation == vanillaImc.Animation &&
                                    (ushort)(imc.Mask & 0x3ff) == (ushort)(vanillaImc.Mask & 0x3ff) &&
                                    (byte)(imc.Mask >> 10) == (byte)(vanillaImc.Mask >> 10))
                                {
                                    isVanilla = true;
                                    break;
                                }
                            }
                        }
                    */
                    if (!add) continue;
                    var secondaryId = meta.ItemMetadata.Root.Info.SecondaryId != null ? (ushort)meta.ItemMetadata.Root.Info.SecondaryId : (ushort)0;

                    var imcEntry = new ImcEntry()
                    {
                        MaterialId = imc.MaterialSet,
                        DecalId = imc.Decal,
                        VfxId = imc.Vfx,
                        MaterialAnimationId = imc.Animation,

                        AttributeMask = (ushort)(imc.Mask & 0x3ff),
                        SoundId = (byte)(imc.Mask >> 10)
                    };
                    var manip = new ImcManipulationContainer()
                    {
                        Type = "Imc",
                        PrimaryId = (ushort)setId,
                        Variant = imc.MaterialSet,
                        SecondaryId = secondaryId,
                        ObjectType = "Equipment",   // TODO: ImcManipulation.ObjectType?
                        EquipSlot = slot,
                        BodySlot = "Unknown",    // TODO: ImcManipulation.BodySlot?
                        Manipulation = new ImcManipulation()
                        {
                            Entry = imcEntry
                        }
                    };

                    ret.Add(manip);
                }
            }

            if (meta.EqdpEntries != null)
            {
                foreach (var eqdp in meta.EqdpEntries)
                {
                    try
                    {
                        var manip = new MetaManipulationContainer()
                        {
                            Type = "Eqdp"
                        };
                        var race = GetRaceString(eqdp.Key);
                        var gender = GetGenderString(eqdp.Key);

                        if (eqdp.Value.b != 0)
                        {
                            var eqdpManip = new EqdpManipulation()
                            {
                                // TODO: Currently eqdp loses accuracies because XivModdingFramework keeps track of two bools, while EqdpEntry keeps track of a ushort and its bits
                                Entry = eqdp.Value.b,
                                Gender = gender,
                                Race = race,
                                SetId = (ushort)setId,
                                Slot = slot
                            };
                            manip.Manipulation = eqdpManip;
                            ret.Add(manip);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            if (meta.EstEntries != null)
            {
                foreach (var est in meta.EstEntries)
                {
                    try
                    {
                        var manip = new MetaManipulationContainer()
                        {
                            Type = "Est"
                        };

                        var race = GetRaceString(est.Key);
                        var gender = GetGenderString(est.Key);

                        if (est.Value.SkelId != 0)
                        {
                            var estManip = new EstManipulation()
                            {
                                Entry = est.Value.SkelId,
                                Gender = gender,
                                Race = race,
                                SetId = est.Value.SetId,
                                Slot = slot
                            };

                            manip.Manipulation = estManip;
                            ret.Add(manip);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (meta.GmpEntry != null)
            {

            }

            if (meta.EqpEntry != null)
            {

            }

            return ret;
        }

        private string GetRaceString(XivRace race)
        {
            switch (race)
            {
                case XivRace.Hyur_Midlander_Male:
                case XivRace.Hyur_Midlander_Female:
                    return "Midlander";
                case XivRace.Hyur_Highlander_Male:
                case XivRace.Hyur_Highlander_Female:
                    return "Highlander";
                case XivRace.Elezen_Male:
                case XivRace.Elezen_Female:
                    return "Elezen";
                case XivRace.Miqote_Male:
                case XivRace.Miqote_Female:
                    return "Miqote";
                case XivRace.Roegadyn_Male:
                case XivRace.Roegadyn_Female:
                    return "Roegadyn";
                case XivRace.Lalafell_Male:
                case XivRace.Lalafell_Female:
                    return "Lalafell";
                case XivRace.AuRa_Male:
                case XivRace.AuRa_Female:
                    return "AuRa";
                case XivRace.Hrothgar_Male:
                case XivRace.Hrothgar_Female:
                    return "Hrothgar";
                case XivRace.Viera_Male:
                case XivRace.Viera_Female:
                    return "Viera";
                default:
                    throw new ArgumentException($"Could not determine race of: {race}");
            }
        }

        private string GetGenderString(XivRace race)
        {
            switch (race)
            {
                case XivRace.Hyur_Midlander_Male:
                case XivRace.Hyur_Highlander_Male:
                case XivRace.Elezen_Male:
                case XivRace.Miqote_Male:
                case XivRace.Roegadyn_Male:
                case XivRace.Lalafell_Male:
                case XivRace.AuRa_Male:
                case XivRace.Hrothgar_Male:
                case XivRace.Viera_Male:
                    return "Male";

                case XivRace.Hyur_Midlander_Female:
                case XivRace.Hyur_Highlander_Female:
                case XivRace.Elezen_Female:
                case XivRace.Miqote_Female:
                case XivRace.Roegadyn_Female:
                case XivRace.Lalafell_Female:
                case XivRace.AuRa_Female:
                case XivRace.Hrothgar_Female:
                case XivRace.Viera_Female:
                    return "Female";
                default:
                    throw new ArgumentException($"Could not determine gender of: {race}");
            }
        }

        private string GetSlot(string s)
        {
            switch(s)
            {
                case "top": return "Body";
                default:
                    return s;
            }
        }
    }
}
