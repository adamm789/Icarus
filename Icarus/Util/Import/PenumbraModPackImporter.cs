using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Mods.Penumbra;
using Icarus.Penumbra.GameData;
using Icarus.Util.Extensions;
using Ionic.Zip;
using ItemDatabase.Paths;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Variants.DataContainers;

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

        // TODO: Asynchronous import

        public bool IsValidPenumbraDirectory(string dirPath)
        {

            if (Directory.Exists(dirPath))
            {
                var dir = new DirectoryInfo(dirPath);
                var files = dir.EnumerateFiles();
                var defaultMod = files.Where(x => x.Name.Contains("default_mod.json")).FirstOrDefault();
                var meta = files.Where(x => x.Name.Contains("meta.json")).FirstOrDefault();
                return (defaultMod != null) && (meta != null);
            }
            return false;
        }

        private async Task<ModPack?> TryReadDefaultMod(DirectoryInfo dir)
        {
            var dirPath = dir.FullName;
            var fileName = dir.Name;
            var defaultModFile = dir.EnumerateFiles().Where(x => x.Name.Contains("default_mod.json")).FirstOrDefault();
            if (defaultModFile != null)
            {
                // TODO: Check this for "simple" mods
                var defaultModText = File.ReadAllText(defaultModFile.FullName);
                if (!String.IsNullOrWhiteSpace(defaultModText))
                {
                    var contents = JsonConvert.DeserializeObject<PenumbraDefaultMod>(defaultModText);

                    if (contents != null)
                    {
                        var ret = new ModPack();

                        if (contents.Files.Count > 0)
                        {
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
                        }

                        if (contents.Manipulations.Count > 0) { 
                            // TODO: Shouldn't this return a list of metadata mods?
                            var mod = await TryImportMetadata(contents.Manipulations);
                            if (mod != null)
                            {
                                ret.SimpleModsList.Add(mod);
                            }
                        }

                        return ret;
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

            Log.Verbose("Trying to read Penumbra groups");
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

                                var metadataMod = await TryImportMetadata(option.Manipulations);
                                if (metadataMod != null && metadataMod.ItemMetadata != null)
                                {
                                    metadataMod.ModFileName = $"metadataMod.ModFileName - {option.Name}";
                                    ret.SimpleModsList.Add(metadataMod);
                                    modOption.AddMod(metadataMod);
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
            Log.Verbose($"Processing {filePath}");
            // TODO: Need "Name" of the item
            var ext = Path.GetExtension(filePath);
            IMod? ret = null;

            if (ext == ".mdl")
            {
                //var mdl = TryImportMdl(gameFilePath, filePath);
                ret = TryImportMdl(filePath, gameFilePath);
            }
            else if (ext == ".mtrl")
            {
                ret = await TryImportMtrl(filePath, gameFilePath);
            }
            else if (ext == ".tex")
            {
                ret = await TryImportTex(filePath, gameFilePath);
            }
            ret ??= TryImportReadOnlyMod(filePath, gameFilePath);
            return ret;
        }

        public ModelMod? TryImportMdl(string filePath, string gameFilePath = "")
        {
            try
            {
                var mdlData = File.ReadAllBytes(filePath);
                var xivMdl = Mdl.GetRawMdlData(gameFilePath, mdlData);
                var imported = TTModel.FromRaw(xivMdl);
                imported.Source = gameFilePath;
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
                    var ret = new ModelMod(gameFilePath, imported, ImportSource.PenumbraModPack)
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
                    var mod = new MaterialMod(mtrl, ImportSource.PenumbraModPack)
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

                var ret = new TextureMod(tex, ImportSource.PenumbraModPack)
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

        public async Task<MetadataMod?> TryImportMetadata(List<MetaManipulation> manipulations)
        {
            try
            {
                Dictionary<(ushort id, string slot), ItemMetadata> dict = new();
                var metadataMod = new MetadataMod(ImportSource.PenumbraModPack);
                ItemMetadata? itemMetadata = null;
                List<XivImc>? imcs = null;
                EquipmentParameter? eqpEntry = null;
                foreach (var manipulation in manipulations)
                {
                    // TODO: MetadataMod
                    // TODO: e.g. one manipulation that manipulates two different items simultaneously (e.g. body item and shoe item)
                    Log.Error($"Metadata import not currently implemented.");
                    if (manipulation.Type == "Imc")
                    {
                        var imcManipulation = JsonConvert.DeserializeObject<ImcManipulation>(manipulation.Manipulation.ToString());
                        var imcEntry = imcManipulation.Entry;
                        var imc = new XivImc()
                        {
                            MaterialSet = imcEntry.MaterialId,
                            Decal = imcEntry.DecalId,
                            Mask = imcEntry.AttributeAndSound,
                            Vfx = imcEntry.VfxId,
                            Animation = imcEntry.MaterialAnimationId
                        };

                        if (itemMetadata == null)
                        {
                            itemMetadata = await TryGetItemMetadata(imcManipulation);
                        }

                        if (imcs == null)
                        {
                            imcs = new();
                        }
                        imcs.Add(imc);
                    }
                    if (manipulation.Type == "Eqp")
                    {
                        var eqpManipulation = JsonConvert.DeserializeObject<EqpManipulation>(manipulation.Manipulation.ToString());
                        if (eqpManipulation != null)
                        {
                            var bytes = BitConverter.GetBytes((byte)eqpManipulation.Entry);
                            // TODO: EqpEntry, get bytes[] for EquipmentParamenter
                            eqpEntry = new EquipmentParameter(EquipSlotToShort(eqpManipulation.Slot), bytes);
                        }
                    }
                } // end for

                if (itemMetadata == null)
                {
                    return null;
                }
                else
                {
                    metadataMod.ItemMetadata = itemMetadata;
                    metadataMod.ModFileName = itemMetadata.Root.ToRawItem().Name;
                    metadataMod.ModFilePath = itemMetadata.Root.ToString();
                    metadataMod.Path = itemMetadata.Root.ToString();
                    metadataMod.Slot = itemMetadata.Root.Info.Slot;
                }

                if (imcs != null)
                {
                    metadataMod.ImcEntries = imcs;
                }
                if (eqpEntry != null)
                {
                    metadataMod.EqpEntry = eqpEntry;
                }

                return metadataMod;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Could not import metadata.");
                return null;
            }
        }

        private async Task<ItemMetadata?> TryGetItemMetadata(MetaManipulation manipulation)
        {
            ItemMetadata? itemMetadata = null;
            string slot = "";
            ushort? id = null;
            if (manipulation is ImcManipulation imcManipulation)
            {
                if (imcManipulation.ObjectType == "Equipment")
                {
                    slot = EquipSlotToShort(imcManipulation.EquipSlot);
                    id = imcManipulation.PrimaryId;
                }
            }
            else if (manipulation is EqpManipulation eqpManipulation)
            {
                slot = EquipSlotToShort(eqpManipulation.Slot);
                id = eqpManipulation.SetId;
            }

            if (!String.IsNullOrWhiteSpace(slot) && id != null)
            {
                var path = $"chara/equipment/e{id}_{slot}.meta";
                itemMetadata = await ItemMetadata.GetMetadata(path);
            }
            return itemMetadata;
        }

        private string EquipSlotToShort(string slot)
        {
            return slot switch
            {
                "Head" => "met",
                "Body" => "top",
                "Hands" => "glv",
                "Legs" => "dwn",
                "Feet" => "sho",
                _ => ""
            };
        }

        public ReadOnlyMod TryImportReadOnlyMod(string filePath, string gamePath)
        {
            Log.Warning($"Fell through to ReadOnlyMod trying to import {filePath}");
            var data = File.ReadAllBytes(filePath);
            var ret = new ReadOnlyMod(ImportSource.PenumbraModPack)
            {
                ModFileName = filePath,
                ModFilePath = filePath,
                Path = gamePath,
                Data = data
            };
            return ret;
        }
    }
}
