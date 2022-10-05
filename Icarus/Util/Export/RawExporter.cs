using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Lumina;
using Lumina.Data.Files;
using Lumina.Models.Materials;
using Lumina.Models.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using xivModdingFramework.Textures.FileTypes;

namespace Icarus.Util.Export
{
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

        private void ApplyOptions(ModelMod mm)
        {
            var ttModel = mm.ImportedModel;
            var ogMdl = mm.XivMdl;
            var options = mm.Options;
            var ogPath = ogMdl.MdlPath;

            options.Apply(ttModel, ogMdl, null, _logService.LoggingFunction);
            ModelModifiers.FixUpSkinReferences(ttModel, ogPath);
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
            if (mod is ModelMod mdlMod)
            {
                ApplyOptions(mdlMod);
                await _converterService.ModelModToFbx(mdlMod, outputDirectory, outputFileName);
                var outputPath = outputDirectory.FullName;

                var model = mdlMod.ImportedModel;

                // TODO: Implement ShouldExportMaterials
                var shouldExportMaterial = true;
                if (shouldExportMaterial && mod.IsInternal)
                {
                    var textureOutputPath = Path.Combine(outputPath, "textures");
                    _logService.Information("Trying to get Materials.");
                    // TODO: Textures for (only?) skins, seems wrong
                    await Mdl.ExportMaterialsForModel(model, textureOutputPath, _gameDirectoryFramework);
                    _logService.Information("Done with getting materials.");
                }
            }
            if (mod is MaterialMod mtrlMod)
            {

            }
        }
    }
}
