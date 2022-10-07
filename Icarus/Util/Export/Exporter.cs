using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Lumina;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeximpNet.DDS;
using xivModdingFramework.Textures.Enums;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.FileTypes;
using Icarus.Util.Extensions;
using TeximpNet.Compression;
using TeximpNet;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Cache;
using System.Drawing;
using ItemDatabase.Paths;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Icarus.Util
{
    public class Exporter
    {
        protected ILogService _logService;
        protected GameData? _lumina;
        protected DirectoryInfo? _gameDirectoryFramework;

        public Exporter(ILogService logService)
        {
            _logService = logService;
        }

        public Exporter(GameData lumina, ILogService logService)
        {
            _logService = logService;
            _lumina = lumina;
            _gameDirectoryFramework = new DirectoryInfo(Path.Combine(_lumina.DataPath.FullName, "ffxiv"));
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
        /// Writes file to byte array
        /// If compressed, it is to be used in a .ttmp2 file
        /// If not, it is to be used with Penumbra
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="shouldCompress">If true, to be used in a ttmp2 file. If false, produces the raw file.</param>
        /// <returns></returns>
        public async Task<byte[]> WriteToBytes(IMod mod, bool shouldCompress, int counter = 0)
        {
            try
            {
                switch (mod)
                {
                    case ModelMod mdlMod:
                        return await WriteModelToBytes(mdlMod, shouldCompress, counter);
                    case MaterialMod mtrlMod:
                        return await WriteMaterialToBytes(mtrlMod, shouldCompress);
                    case TextureMod texMod:
                        return await WriteTextureToBytes(texMod, shouldCompress);
                    case ReadOnlyMod readonlyMod:
                        _logService.Warning("Trying to export a \"Read Only Mod\" to Penumbra.");
                        // TODO: What to do here?
                        break;
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Exception thrown during WriteToBytes with {mod.Name}.");
                return Array.Empty<byte>();
            }

            _logService.Warning($"Currently unknown export method. Skipping {mod.Name}.");
            return Array.Empty<byte>();
        }

        public async Task<byte[]> WriteTextureToBytes(TextureMod mod, bool shouldCompress)
        {
            if (mod.IsInternal)
            {
                throw new NotImplementedException("Unknown export method for internal textures.");
            }
            if (!shouldCompress)
            {
                throw new NotImplementedException("Unknown export method for Penumbra texture (.tex)");
            }

            var ddsContainer = new DDSContainer();
            var isDds = Path.GetExtension(mod.ModFilePath).ToLower() == ".dds";
            var texFormat = mod.GetTexFormat();
            var externalPath = mod.ModFilePath;
            var internalPath = mod.Path;

            if (!File.Exists(externalPath))
            {
                throw new FileNotFoundException($"The external texture path: {externalPath} could not be found.");
            }

            // MakeTexData
            // https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Textures/FileTypes/Tex.cs#L905
            try
            {
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
                            throw new Exception($"Format {texFormat} is not currently supported for BMP import\n\nPlease use the DDS import option instead.");
                        }
                        break;
                }

                if (!isDds)
                {
                    using (var surface = Surface.LoadFromFile(externalPath))
                    {
                        if (surface == null)
                            throw new FormatException($"Unsupported texture format");

                        surface.FlipVertically();

                        var maxMipCount = 1;
                        // TODO: root?

                        //if (root != null)
                        //{
                        // For things that have real roots (things that have actual models/aren't UI textures), we always want mipMaps, even if the existing texture only has one.
                        // (Ex. The Default Mat-Add textures)
                        maxMipCount = -1;
                        //}

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
                }

                // If we're not a DDS, write the DDS to file temporarily.
                // TODO: Where to write temp file?... To Temp folder?
                var ddsFilePath = externalPath;
                if (!isDds)
                {
                    var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dds");
                    ddsContainer.Write(tempFile, DDSFlags.None);
                    ddsFilePath = tempFile;
                }

                return await TexExtensions.DDSToTex(ddsFilePath, internalPath, texFormat);
            }
            finally
            {
                ddsContainer.Dispose();
            }
        }

        protected void ApplyModelOptions(ModelMod mm)
        {
            _logService.Information($"Applying ModelModifiers for {mm.Name}");
            var ttModel = mm.ImportedModel;
            var ogMdl = mm.XivMdl;
            var options = mm.Options;
            var ogPath = ogMdl.MdlPath;

            options.Apply(ttModel, ogMdl, null, _logService.LoggingFunction);
            ModelModifiers.FixUpSkinReferences(ttModel, ogPath);
        }

        /// <summary>
        /// Writes a model to bytes
        /// </summary>
        /// <param name="file"></param>
        /// <param name="shouldCompress">Whether to compress the vertex information or not</param>
        /// <returns></returns>
        protected async Task<byte[]> WriteModelToBytes(ModelMod file, bool shouldCompress, int counter = 0)
        {
            _logService.Verbose($"{file.Name} ({counter}) has started.");
            /*
            TTModel ttModel;
            XivMdl ogMdl = file.XivMdl;
            string ogPath = ogMdl.MdlPath;
            ModelModifierOptions options = new();

            ttModel = file.ImportedModel;
            options = file.Options;

            options.Apply(ttModel, ogMdl, null, _logService.LoggingFunction);

            try
            {
                ModelModifiers.FixUpSkinReferences(ttModel, ogPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            */
            var ttModel = file.ImportedModel;
            var ogMdl = file.XivMdl;
            ApplyModelOptions(file);

            var mdl = new Mdl(ttModel, ogMdl);
            var bytes = await mdl.MakeNewMdlFileLumina(shouldCompress);

            _logService.Verbose($"{file.Name} ({counter}) has finished.");
            return bytes;
        }

        /// <summary>
        /// Writes a material to bytes
        /// If ShouldCompress, the resulting file is with TexTools
        /// If not, the resulting file is used with Penumbra
        /// </summary>
        /// <param name="file"></param>
        /// <param name="shouldCompress"></param>
        /// <returns></returns>
        protected async Task<byte[]> WriteMaterialToBytes(MaterialMod file, bool shouldCompress = true)
        {
            var xivMtrl = file.GetMtrl();
            var bytes = MtrlExtensions.CreateMtrlFile(xivMtrl);

            if (!shouldCompress)
            {
                return bytes;
            }

            // To be read by the game, must be made into "Type 2" data
            var datBytes = await DatExtensions.CreateType2Data(bytes);
            return datBytes;
        }
    }
}
