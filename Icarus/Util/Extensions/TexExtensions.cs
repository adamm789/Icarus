using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeximpNet.Compression;
using TeximpNet.DDS;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using xivModdingFramework.Textures.FileTypes;


namespace Icarus.Util.Extensions
{
    public static class TexExtensions
    {
        // xivModdingFramework.Textures.FileTypes.Tex
        public static XivTexFormat GetDDSTexFormat(BinaryReader ddsStream)
        {
            ddsStream.BaseStream.Seek(12, SeekOrigin.Begin);

            var newHeight = ddsStream.ReadInt32();
            var newWidth = ddsStream.ReadInt32();
            ddsStream.ReadBytes(8);
            var newMipCount = ddsStream.ReadInt32();

            if (newHeight % 2 != 0 || newWidth % 2 != 0)
            {
                throw new Exception("Resolution must be a multiple of 2");
            }

            ddsStream.BaseStream.Seek(80, SeekOrigin.Begin);

            var textureFlags = ddsStream.ReadInt32();
            var texType = ddsStream.ReadInt32();
            XivTexFormat textureType;

            if (DDSType.ContainsKey(texType))
            {
                textureType = DDSType[texType];
            }
            else
            {
                throw new Exception($"DDS Type ({texType}) not recognized.");
            }

            switch (textureFlags)
            {
                case 2 when textureType == XivTexFormat.A8R8G8B8:
                    textureType = XivTexFormat.A8;
                    break;
                case 65 when textureType == XivTexFormat.A8R8G8B8:
                    var bpp = ddsStream.ReadInt32();
                    if (bpp == 32)
                    {
                        textureType = XivTexFormat.A8R8G8B8;
                    }
                    else
                    {
                        var red = ddsStream.ReadInt32();

                        switch (red)
                        {
                            case 31744:
                                textureType = XivTexFormat.A1R5G5B5;
                                break;
                            case 3840:
                                textureType = XivTexFormat.A4R4G4B4;
                                break;
                        }
                    }

                    break;
            }
            return textureType;
        }

        /// <summary>
        /// A dictionary containing the int represntations of known file types for DDS
        /// </summary>
        private static readonly Dictionary<int, XivTexFormat> DDSType = new Dictionary<int, XivTexFormat>
        {
            //DXT1
            {827611204, XivTexFormat.DXT1 },

            //DXT3
            {861165636, XivTexFormat.DXT3 },

            //DXT5
            {894720068, XivTexFormat.DXT5 },

            //ARGB 16F
            {113, XivTexFormat.A16B16G16R16F },

            //Uncompressed RGBA
            {0, XivTexFormat.A8R8G8B8 }

        };

        /// <summary>
        /// Creates the header for the texture info from the data to be imported.
        /// </summary>
        /// <param name="xivTex">Data for the currently displayed texture.</param>
        /// <param name="newWidth">The width of the DDS texture to be imported.</param>
        /// <param name="newHeight">The height of the DDS texture to be imported.</param>
        /// <param name="newMipCount">The number of mipmaps the DDS texture to be imported contains.</param>
        /// <returns>The created header data.</returns>
        public static List<byte> MakeTextureInfoHeader(XivTexFormat format, int newWidth, int newHeight, int newMipCount)
        {
            var headerData = new List<byte>();

            headerData.AddRange(BitConverter.GetBytes((short)0));
            headerData.AddRange(BitConverter.GetBytes((short)128));
            headerData.AddRange(BitConverter.GetBytes(short.Parse(format.GetTexFormatCode())));
            headerData.AddRange(BitConverter.GetBytes((short)0));
            headerData.AddRange(BitConverter.GetBytes((short)newWidth));
            headerData.AddRange(BitConverter.GetBytes((short)newHeight));
            headerData.AddRange(BitConverter.GetBytes((short)1));
            headerData.AddRange(BitConverter.GetBytes((short)newMipCount));


            headerData.AddRange(BitConverter.GetBytes(0));
            headerData.AddRange(BitConverter.GetBytes(1));
            headerData.AddRange(BitConverter.GetBytes(2));

            int mipLength;

            switch (format)
            {
                case XivTexFormat.DXT1:
                    mipLength = (newWidth * newHeight) / 2;
                    break;
                case XivTexFormat.DXT5:
                case XivTexFormat.A8:
                    mipLength = newWidth * newHeight;
                    break;
                case XivTexFormat.A1R5G5B5:
                case XivTexFormat.A4R4G4B4:
                    mipLength = (newWidth * newHeight) * 2;
                    break;
                case XivTexFormat.L8:
                case XivTexFormat.A8R8G8B8:
                case XivTexFormat.X8R8G8B8:
                case XivTexFormat.R32F:
                case XivTexFormat.G16R16F:
                case XivTexFormat.G32R32F:
                case XivTexFormat.A16B16G16R16F:
                case XivTexFormat.A32B32G32R32F:
                case XivTexFormat.DXT3:
                case XivTexFormat.D16:
                default:
                    mipLength = (newWidth * newHeight) * 4;
                    break;
            }

            var combinedLength = 80;

            for (var i = 0; i < newMipCount; i++)
            {
                headerData.AddRange(BitConverter.GetBytes(combinedLength));
                combinedLength = combinedLength + mipLength;

                if (mipLength > 16)
                {
                    mipLength = mipLength / 4;
                }
                else
                {
                    mipLength = 16;
                }
            }

            var padding = 80 - headerData.Count;

            headerData.AddRange(new byte[padding]);

            return headerData;
        }

        public static void SaveTexAsDDS(string path, XivTex xivTex)
        {
            path = Path.ChangeExtension(path, ".dds");
            DDS.MakeDDS(xivTex, path);
        }

        public static void SaveTexAsDDS(string path, XivTex xivTex, DirectoryInfo saveDirectory)
        {
            Directory.CreateDirectory(path);
            var savePath = Path.Combine(path, Path.GetFileNameWithoutExtension(xivTex.TextureTypeAndPath.Path) + ".dds");
            DDS.MakeDDS(xivTex, savePath);
        }

        // MakeTexData
        // https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Textures/FileTypes/Tex.cs#L905
        public static async Task<byte[]> DDSToTex(string ddsFilePath, string internalPath, XivTexFormat texFormat, bool shouldCompress = true)
        {
            try
            {
                using (var br = new BinaryReader(File.OpenRead(ddsFilePath)))
                {
                    br.BaseStream.Seek(12, SeekOrigin.Begin);

                    var newHeight = br.ReadInt32();
                    var newWidth = br.ReadInt32();
                    br.ReadBytes(8);
                    var newMipCount = br.ReadInt32();

                    if (newHeight % 2 != 0 || newWidth % 2 != 0)
                    {
                        throw new Exception("Resolution must be a multiple of 2");
                    }

                    br.BaseStream.Seek(80, SeekOrigin.Begin);

                    var textureFlags = br.ReadInt32();
                    var texType = br.ReadInt32();

                    var uncompressedLength = (int)new FileInfo(ddsFilePath).Length - 128;
                    var newTex = new List<byte>();

                    if (!internalPath.Contains(".atex"))
                    {
                        //var DDSInfo = await DDS.ReadDDS(br, texFormat, newWidth, newHeight, newMipCount);
                        var DDSInfo = await DDSExtensions.ReadDDS(br, texFormat, newWidth, newHeight, newMipCount, shouldCompress);

                        newTex.AddRange(DatExtensions.MakeType4DatHeader(texFormat, DDSInfo.mipPartOffsets, DDSInfo.mipPartCounts, (int)uncompressedLength, newMipCount, newWidth, newHeight));
                        newTex.AddRange(TexExtensions.MakeTextureInfoHeader(texFormat, newWidth, newHeight, newMipCount));
                        newTex.AddRange(DDSInfo.dds);

                        return newTex.ToArray();
                    }
                    else
                    {
                        br.BaseStream.Seek(128, SeekOrigin.Begin);
                        newTex.AddRange(TexExtensions.MakeTextureInfoHeader(texFormat, newWidth, newHeight, newMipCount));
                        newTex.AddRange(br.ReadBytes((int)uncompressedLength));
                        var data = await DatExtensions.CreateType2Data(newTex.ToArray(), shouldCompress);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Exception has occurred.");
            }
            return Array.Empty<byte>();
        }
        public static async Task<byte[]> GetImageData(DirectoryInfo gameDirectory, XivTex xivTex, int layer = -1)
        {
            var tex = new Tex(gameDirectory);
            // TODO: Extract GetImageData so that it does not depend on game directory (?)
            return await tex.GetImageData(xivTex, layer);
        }
    }
}
