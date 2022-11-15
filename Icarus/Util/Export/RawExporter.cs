﻿using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using ItemDatabase.Paths;
using Lumina;
using SharpDX.Direct2D1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Markup;
using xivModdingFramework.Helpers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using SixLaborsImage = SixLabors.ImageSharp.Image;

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
                    if (mod.ShouldExport)
                    {
                        await ExportMod(outputDirectory, mod);
                    }
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
            if (Path.HasExtension(retVal))
            {
                var ext = Path.GetExtension(retVal);
                retVal = retVal.Replace(ext, "");
            }
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
                _logService.Verbose($"Beginning mdl to fbx export.");
                if (mdlMod.ImportSource == Import.ImportSource.Raw)
                {
                    // Check to see if the original file was from a .db file (which came from an .fbx file)
                    // Why would someone want to export to an fbx when they have the original?
                    _logService.Error($"Cannot export mod {mdlMod.Name} because it originated from a raw file.");
                    return;
                }

                var copy = ApplyModelOptions(mdlMod);
                try
                {
                    await _converterService.TTModelToFbx(copy, outputDirectory, outputFileName);
                    _logService.Information($"Saved model to {outputFileName}");
                }
                catch (Exception ex)
                {
                    _logService.Error(ex, $"Could not export model to fbx. {mdlMod.Name}");
                    return;
                }
            }
            else if (mod is MaterialMod mtrlMod)
            {
                _logService.Verbose($"Beginning mtrl to dds export.");
                // TODO: Only allow .dds file as raw exports for materials?
                var xivMtrl = mtrlMod.GetMtrl();
                var df = IOUtil.GetDataFileFromPath(xivMtrl.MTRLPath);
                var ttp = new TexTypePath
                {
                    Path = mod.Path,
                    Type = XivTexType.ColorSet,
                    DataFile = df
                };

                var xivTex = MtrlExtensions.MtrlToXivTex(xivMtrl, ttp);
                outputPath = Path.Combine(outputPath, outputFileName);
                var ogPath = outputPath;
                var i = 0;
                while (File.Exists(Path.ChangeExtension(outputPath, ".dds")))
                {
                    outputPath = $"{ogPath} ({i})";
                    i++;
                }
                TexExtensions.SaveTexAsDDS(outputPath, xivTex);
            }
            else if (mod is TextureMod texMod)
            {
                // TODO: Allow export to png, dds, and bmp..?
                _logService.Verbose($"Beginning tex to ... dds? export.");
                if (texMod.XivTex != null)
                {
                    outputPath = Path.Combine(outputPath, outputFileName);
                    var i = 0;
                    var texType = texMod.TexType;

                    var texAbbreviation = "";
                    switch (texType)
                    {
                        case XivTexType.Diffuse:
                            texAbbreviation = "d";
                            break;
                        case XivTexType.Specular:
                            texAbbreviation = "s";
                            break;
                        case XivTexType.Multi:
                            texAbbreviation = "m";
                            break;
                        case XivTexType.Normal:
                            texAbbreviation = "n";
                            break;
                        default:
                            break;
                    }
                    var ogPath = outputPath;
                    while (File.Exists(Path.ChangeExtension(outputPath, ".dds")))
                    {
                        outputPath = $"{ogPath} ({i})";
                        i++;
                    }
                    // TODO: Allow export to png
                    /*
                    Path.ChangeExtension(outputPath, ".png");

                    using (Image<Rgba32> img = SixLaborsImage.LoadPixelData<Rgba32>(texMod.XivTex.TexData, texMod.XivTex.Width, texMod.XivTex.Height))
                    {
                        img.Save(outputPath, new PngEncoder());
                    }
                    */
                    TexExtensions.SaveTexAsDDS(outputPath, texMod.XivTex);
                }
            }
            _logService.Debug($"Wrote to {outputPath}");

        }
    }
}
