using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using Lumina;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Textures.Enums;
using xivModdingFramework.Textures.FileTypes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using xivModdingFramework.Mods.DataContainers;

using Path = System.IO.Path;
using ModPack = Icarus.Mods.DataContainers.ModPack;
using System.Linq;
using xivModdingFramework.Cache;

namespace Icarus.Util
{
    // TODO: It seems like when compressing, the vertex data is incorrectly compressed?
    // Which I don't understand, because it's copy pasted...
    // Even the uncompressed sizes are the same
    // VertexDataBlockLoD Compressed, somewhere in compressedMDLData

    // TODO: For some reason, when export to ttmp2, the sizes differ from one that comes from TexTools
    // Penumbra version seems to be identical (at least sizewise)
    public class Exporter
    {
        protected ILogService _logService;
        protected GameData? _lumina;
        protected DirectoryInfo? _gameDirectoryFramework;

        public Exporter(GameData lumina, ILogService logService)
        {
            _logService = logService;
            _lumina = lumina;
            _gameDirectoryFramework = new DirectoryInfo(Path.Combine(_lumina.DataPath.FullName, "ffxiv"));

            // Set race tree here
            XivRaceTree.GetFullRaceTree();
        }

        // https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Mods/FileTypes/TTMP.cs
        // These file types are forbidden from being included in Modpacks or being imported via modpacks.
        // This is because these file types are re-built from constituent smaller files, and thus importing
        // a complete file would bash the user's current file state in unpredictable ways.
        public static readonly HashSet<string> ForbiddenModTypes = new HashSet<string>()
        {
            ".cmp", ".imc", ".eqdp", ".eqp", ".gmp", ".est"
        };

        public virtual string GetOutputPath(ModPack modPack, string outputDir)
        {
            if (String.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            var path = Path.Combine(outputDir, modPack.Name);
            return path;
        }

        /// <summary>
        /// Writes mod to byte array
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="forTexTools">If true, to be used in a ttmp2 file. If false, produces the raw file.</param>
        /// <returns>The array of bytes, if successful. An empty array of bytes if not.</returns>
        public async Task<byte[]> WriteToBytes(IMod mod, bool forTexTools, int counter = 0)
        {
            switch (mod)
            {
                case ModelMod mdlMod:
                    return await TryWriteModelToBytes(mdlMod, forTexTools, counter);
                case MaterialMod mtrlMod:
                    return await TryWriteMaterialToBytes(mtrlMod, forTexTools);
                case TextureMod texMod:
                    return await TryWriteTextureToBytes(texMod, forTexTools);
                case MetadataMod metaMod:
                    return await TryWriteMetadataToBytes(metaMod);
                case ReadOnlyMod readonlyMod:
                    if (forTexTools)
                    {
                        return readonlyMod.Data;
                    }
                    else
                    {
                        // Readonly mods can only be written to ttmp2 files
                        _logService.Warning("Trying to export a \"Read Only Mod\" to Penumbra.");
                        return Array.Empty<byte>();
                    }
            }
            _logService.Error($"Failed to get bytes: {mod.ModFileName} - {mod.ModFilePath}.");
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the temporary directory based off of the argument.
        /// Deletes the directory if it exists, then creates it.
        /// </summary>
        /// <param name="outputDir"></param>
        /// <returns></returns>
        protected string GetTempDirectory(string outputDir)
        {
            /*
            try
            {
                return Path.Combine(Path.GetTempPath(), "temp");
            }
            catch (SecurityException ex)
            {
                _logService.Error(ex, "Could not get temporary path.");
                return Path.Combine(outputDir, "temp");
            }
            */
            var tempDir = Path.Combine(outputDir, "temp");
            if (!Directory.Exists(tempDir))
            {
                _logService.Verbose($"Creating temporary directory.");
                Directory.CreateDirectory(tempDir);
            }

            return tempDir;
        }

        /// <summary>
        /// Returns the current directory if the argument is an invalid directory.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <param name="outputDir"></param>
        /// <returns></returns>
        protected string GetOutputDirectory(string outputDir)
        {
            if (String.IsNullOrWhiteSpace(outputDir) || !Directory.Exists(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            return outputDir;
        }

        protected async Task<byte[]> TryWriteMetadataToBytes(MetadataMod mod)
        {
            try
            {
                _logService.Verbose($"Exporting metadata: {mod.ModFileName}.");
                var itemMetadata = mod.ItemMetadata;
                var bytes = await ItemMetadata.Serialize(itemMetadata);
                return await DatExtensions.CreateType2Data(bytes);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Exception thrown while writing metadata.");
            }
            return Array.Empty<byte>();
        }

        // MakeTexData: https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Textures/FileTypes/Tex.cs#L905
        protected async Task<byte[]> TryWriteTextureToBytes(TextureMod mod, bool forTexTools)
        {
            // TODO: Export texture to .pmp
            // TODO: Extract writing texture to bytes into its own function or move to another class

            _logService.Verbose($"Exporting texture: {mod.ModFileName} with forTexTools={forTexTools}");

            if (!forTexTools)
            {
                _logService.Error("Unknown export method for Penumbra texture (.tex)");
                return Array.Empty<byte>();
            }

            var isDds = Path.GetExtension(mod.ModFilePath).ToLower() == ".dds";
            var texFormat = mod.GetTexFormat();
            var externalPath = mod.ModFilePath;
            var internalPath = mod.Path;
            var tempImageFile = Path.GetTempFileName();

            if (mod.XivTex != null)
            {
                // Utilizing mod.XivTex.TexData does not seem to work
                externalPath = Path.ChangeExtension(tempImageFile, ".dds");
                TexExtensions.SaveTexAsDDS(externalPath, mod.XivTex);
                texFormat = mod.XivTex.TextureFormat;
                isDds = true;
            }

            if (!File.Exists(externalPath))
            {
                throw new FileNotFoundException($"The external texture path: {externalPath} could not be found.");
            }

            // Check if the texture being imported has been imported before
            CompressionFormat compressionFormat = CompressionFormat.BGRA;

            switch (texFormat)
            {
                case XivTexFormat.DXT1:
                    compressionFormat = CompressionFormat.BC1a;
                    break;
                case XivTexFormat.DXT5:
                    compressionFormat = CompressionFormat.BC3;
                    break;
                case XivTexFormat.A8R8G8B8:
                    compressionFormat = CompressionFormat.BGRA;
                    break;
                default:
                    if (!isDds)
                    {
                        _logService.Error($"Format {texFormat} is not currently supported for BMP import\nPlease use the DDS import option instead.");
                        return Array.Empty<byte>();
                    }
                    break;
            }

            DDSContainer? ddsContainer = null;

            if (!isDds)
            {
                using (var surface = Surface.LoadFromFile(externalPath))
                {
                    if (surface == null)
                    {
                        _logService.Error($"Unsupported texture format.");
                        _logService.Debug($"Surface.LoadFromFile({externalPath}) return null.");
                        return Array.Empty<byte>();
                    }

                    surface.FlipVertically();

                    var maxMipCount = 1;

                    // For things that have real roots (things that have actual models/aren't UI textures), we always want mipMaps, even if the existing texture only has one.
                    // (Ex. The Default Mat-Add textures)
                    var root = await XivCache.GetFirstRoot(internalPath);
                    if (root != null)
                    {
                        maxMipCount = -1;
                    }

                    using (var compressor = new Compressor())
                    {
                        // UI/Paintings only have a single mipmap and will crash if more are generated, for everything else generate max levels
                        compressor.Input.SetMipmapGeneration(true, maxMipCount);
                        compressor.Input.SetData(surface);
                        compressor.Compression.Format = compressionFormat;
                        compressor.Compression.SetBGRAPixelFormat();

                        compressor.Process(out ddsContainer);
                    }
                }
                if (ddsContainer == null)
                {
                    _logService.Error($"DDSContainer was null.");
                    return Array.Empty<byte>();
                }
            }

            // If we're not a DDS, write the DDS to file temporarily.
            var ddsFilePath = externalPath;
            string tempDdsFile = "";
            if (!isDds)
            {
                tempDdsFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dds");
                bool success = false;

                try
                {
                    success = ddsContainer.Write(tempDdsFile, DDSFlags.None);
                    _logService.Debug($"ddsContainer.Write result: {success}");
                }
                catch (Exception ex)
                {
                    _logService.Error(ex, $"Exception caught during ddsContainer.Write()");
                }

                if (!success)
                {
                    if (File.Exists(tempDdsFile))
                    {
                        _logService.Verbose($"Deleting temp dds file: {tempDdsFile}");
                        File.Delete(tempDdsFile);
                    }
                    if (File.Exists(tempImageFile))
                    {
                        _logService.Verbose($"Deleting temp path: {tempImageFile}");
                        File.Delete(tempImageFile);
                    }

                    if (ddsContainer != null)
                    {
                        ddsContainer.Dispose();
                    }
                    return Array.Empty<byte>();
                }

                _logService.Debug($"{tempDdsFile}: {success}");

                ddsFilePath = tempDdsFile;
            }

            var bytes = await TexExtensions.DDSToTex(ddsFilePath, internalPath, texFormat, forTexTools);
            
            if (File.Exists(tempImageFile))
            {
                _logService.Verbose($"Deleting temp path: {tempImageFile}");
                File.Delete(tempImageFile);
            }
            if (File.Exists(tempDdsFile))
            {
                _logService.Verbose($"Deleting temp dds file: {tempDdsFile}");
                File.Delete(tempDdsFile);
            }
            if (ddsContainer != null)
            {
                ddsContainer.Dispose();
            }
            return bytes;
        }

        /// <summary>
        /// Creates a copy of the TTModel and returns it with model options applied
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        protected TTModel ApplyModelOptions(ModelMod mm)
        {
            _logService.Verbose($"Applying ModelModifiers to {mm.Name}");

            var ttModel = mm.ImportedModel;
            var copy = ttModel.DeepCopy();

            var ogMdl = mm.XivMdl;
            var options = mm.Options;
            var ogPath = ogMdl.MdlPath;

            // Change the path so that ModelModifiers correctly converts the race (if necessary)
            ogMdl.MdlPath = mm.Path;

            options.Apply(copy, ogMdl, ogMdl, _logService.LoggingFunction);
            CheckRaceConversion(mm);

            // Removed to be able to use other race's materials
            //ModelModifiers.FixUpSkinReferences(copy, ogPath, _logService.LoggingFunction);
            ogMdl.MdlPath = ogPath;
            return copy;
        }

        private void CheckRaceConversion(ModelMod mm)
        {
            var sourceRace = mm.Options.SourceRace;
            var targetRace = mm.TargetRace;

            // Because GetModelPriorityList throws an exception if this is the case
            if (sourceRace == XivRace.All_Races || targetRace == XivRace.All_Races) return;

            var sourceModelPriority = sourceRace.GetModelPriorityList().FirstOrDefault();
            var targetModelPriority = targetRace.GetModelPriorityList().FirstOrDefault();

            if (sourceRace == XivRace.Hyur_Midlander_Male && targetRace == XivRace.Hyur_Highlander_Male && mm.Category != "Body")
            {
                // This conversion should be fine
            }
            else
            {
                if (sourceModelPriority != targetModelPriority)
                {
                    _logService.Warning($"Conversion from {sourceRace} to {targetRace} may not convert as expected.");
                }
            }
        }

        /// <summary>
        /// Writes a model to bytes
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="shouldCompress">Whether to compress the vertex information or not</param>
        /// <returns></returns>
        protected async Task<byte[]> TryWriteModelToBytes(ModelMod mod, bool shouldCompress, int counter = 0)
        {
            try
            {
                _logService.Verbose($"Exporting model: {mod.ModFileName} with shouldCompress={shouldCompress}");

                var copy = ApplyModelOptions(mod);

                var ogMdl = mod.XivMdl;
                var mdl = new Mdl(copy, ogMdl);
                var bytes = await mdl.MakeNewMdlFileLumina(shouldCompress);
                //var bytes = await Mdl.MakeNewMdlFiles(copy, ogMdl);

                _logService.Verbose($"{mod.Name} ({counter}) has finished.");
                return bytes;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Exception thrown while writing model.");
            }
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Writes a material to bytes
        /// If shouldCompress is true, the resulting file is for TexTools
        /// If not, the resulting file is used with Penumbra
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="shouldCompress"></param>
        /// <returns></returns>
        protected async Task<byte[]> TryWriteMaterialToBytes(MaterialMod mod, bool shouldCompress = true)
        {
            try
            {
                _logService.Verbose($"Exporting material: {mod.ModFileName} with shouldCompress={shouldCompress}");

                var xivMtrl = mod.GetMtrl();
                var bytes = MtrlExtensions.CreateMtrlFile(xivMtrl);

                if (!shouldCompress)
                {
                    return bytes;
                }

                // To be read by the game, must be made into "Type 2" data
                var datBytes = await DatExtensions.CreateType2Data(bytes);
                return datBytes;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Exception thrown while writing material.");
            }
            return Array.Empty<byte>();
        }
    }
}
