﻿using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using Lumina;
using System;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Helpers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using xivModdingFramework.Textures.DataContainers;

namespace Icarus.Util.Export
{
    // Class that exports to common file extensions, i.e. .fbx, .dds
    public class RawExporter : Exporter
    {
        // TODO: Theoretically provide options for output files
        // Specifically, textures and png/dds
        readonly ConverterService _converterService;
        public RawExporter(ConverterService converter, GameData lumina, ILogService logService) : base(lumina, logService)
        {
            _converterService = converter;
        }

        public async Task<string> ExportToSimple(ModPack modPack, string outputDir)
        {
            _logService.Information("Starting export to simple.");
            var dir = GetOutputPath(modPack, outputDir);
            Directory.CreateDirectory(dir);
            var outputDirectory = new DirectoryInfo(dir);

            if (modPack.SimpleModsList != null)
            {
                foreach (var mod in modPack.SimpleModsList)
                {
                    await ExportMod(outputDirectory, mod);
                }
            }
            return dir;
        }

        private string GetOutputFileName(IMod mod, ModOption? option = null)
        {
            var retVal = mod.ModFileName;
            if (option != null)
            {
                retVal = option.GroupName + " - " + option.Name;
            }

            retVal = String.Join("_", retVal.Split(Path.GetInvalidFileNameChars()));
            return retVal;
        }

        public async Task<string> ExportToAdvanced(ModPack modPack, string outputDir)
        {
            _logService.Information("Starting export to advanced.");
            var dir = GetOutputPath(modPack, outputDir);
            Directory.CreateDirectory(dir);
            var outputDirectory = new DirectoryInfo(dir);
            if (modPack.ModPackPages != null)
            {
                foreach (var page in modPack.ModPackPages)
                {
                    foreach (var group in page.ModGroups)
                    {
                        foreach (var option in group.OptionList)
                        {
                            foreach (var mod in option.Mods)
                            {
                                await ExportMod(outputDirectory, mod, option);
                            }
                        }
                    }
                }
            }
            return dir;
        }

        public async Task ExportMod(DirectoryInfo outputDirectory, IMod mod, ModOption? option = null)
        {
            string outputFileName = GetOutputFileName(mod, option);
            var outputPath = outputDirectory.FullName;

            if (mod is ModelMod mdlMod)
            {
                var src = mdlMod.TTModel.Source;
                if (Path.GetExtension(src) == ".db")
                {
                    // Check to see if the original file was an .fbx file
                    // Why would someone want to export to an fbx when they have the original?
                    _logService.Error($"Cannot export mod {mdlMod.Name}.");
                    return;
                }
                var copy = ApplyModelOptions(mdlMod);
                try
                {
                    await _converterService.TTModelToFbx(copy, outputDirectory, outputFileName);
                }
                catch (Exception ex)
                {
                    _logService.Error(ex, $"Could not export model to fbx. {mdlMod.Name}");
                    return;
                }

                var model = mdlMod.ImportedModel;

                // TODO: Implement ShouldExportMaterials
                var shouldExportMaterial = true;
                if (shouldExportMaterial && mod.IsInternal)
                {
                    var textureOutputPath = Path.Combine(outputPath, "textures");
                    _logService.Information("Trying to get Materials.");
                    // TODO?: Textures for (only?) skins, seems wrong
                    await Mdl.ExportMaterialsForModel(model, textureOutputPath, _gameDirectoryFramework);
                    _logService.Verbose("Done with getting materials.");
                }
            }
            if (mod is MaterialMod mtrlMod)
            {
                // TODO: Only allow .dds file as raw exports for materials?
                var xivMtrl = mtrlMod.GetMtrl();
                var df = IOUtil.GetDataFileFromPath(xivMtrl.MTRLPath);
                var ttp = new TexTypePath
                {
                    Path = mod.Path,
                    Type = xivModdingFramework.Textures.Enums.XivTexType.ColorSet,
                    DataFile = df
                };

                var texData = MtrlExtensions.MtrlToXivTex(xivMtrl, ttp);
                TexExtensions.SaveTexAsDDS(outputPath, texData, outputDirectory);
            }
            if (mod is TextureMod texMod)
            {
                if (mod.IsInternal)
                {
                    // If/when to export raw textures?
                }
            }
        }
    }
}
