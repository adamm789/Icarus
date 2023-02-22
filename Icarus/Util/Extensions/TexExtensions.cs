using Lumina.Data.Files;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeximpNet.Compression;
using TeximpNet.DDS;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Helpers;
using xivModdingFramework.SqPack.FileTypes;
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
                        if (shouldCompress)
                        {
                            newTex.AddRange(DatExtensions.MakeType4DatHeader(texFormat, DDSInfo.mipPartOffsets, DDSInfo.mipPartCounts, (int)uncompressedLength, newMipCount, newWidth, newHeight));
                        }

                        newTex.AddRange(Tex.MakeTextureInfoHeader(texFormat, newWidth, newHeight, newMipCount));
                        newTex.AddRange(DDSInfo.dds);

                        return newTex.ToArray();
                    }
                    else
                    {
                        br.BaseStream.Seek(128, SeekOrigin.Begin);
                        newTex.AddRange(Tex.MakeTextureInfoHeader(texFormat, newWidth, newHeight, newMipCount));
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

        public static async Task<XivTex> GetXivTex(byte[] data)
        {
            var xivTex = new XivTex();
            var decompressedData = new List<byte>();

            using var br = new BinaryReader(new MemoryStream(data));
            var mipMapInfoOffset = 24;

            br.BaseStream.Seek(4, SeekOrigin.Begin);
            var val = br.ReadInt32();
            xivTex.TextureFormat = Dat.TextureTypeDictionary[val];
            xivTex.Width = br.ReadInt16();
            xivTex.Height = br.ReadInt16();
            var pos = br.BaseStream.Position;
            xivTex.Layers = br.ReadInt16();
            var imageCount2 = br.ReadInt16();

            for (int i = 0, j = 0; i < xivTex.MipMapCount; i++)
            {
                br.BaseStream.Seek(mipMapInfoOffset + j, SeekOrigin.Begin);

                var offsetFromHeaderEnd = br.ReadInt32();
                var mipMapLength = br.ReadInt32();
                var mipMapSize = br.ReadInt32();
                var mipMapStart = br.ReadInt32();
                var mipMapParts = br.ReadInt32();

                var mipMapPartOffset = offsetFromHeaderEnd;

                br.BaseStream.Seek(mipMapPartOffset, SeekOrigin.Begin);

                br.ReadBytes(8);
                var compressedSize = br.ReadInt32();
                var uncompressedSize = br.ReadInt32();

                if (mipMapParts > 1)
                {
                    var compressedData = br.ReadBytes(compressedSize);

                    var decompressedPartData = await IOUtil.Decompressor(compressedData, uncompressedSize);

                    decompressedData.AddRange(decompressedPartData);

                    for (var k = 1; k < mipMapParts; k++)
                    {
                        var check = br.ReadByte();
                        while (check != 0x10)
                        {
                            check = br.ReadByte();
                        }

                        br.ReadBytes(7);
                        compressedSize = br.ReadInt32();
                        uncompressedSize = br.ReadInt32();

                        // When the compressed size of a data block shows 32000, it is uncompressed.
                        if (compressedSize != 32000)
                        {
                            compressedData = br.ReadBytes(compressedSize);
                            decompressedPartData =
                                await IOUtil.Decompressor(compressedData, uncompressedSize);

                            decompressedData.AddRange(decompressedPartData);
                        }
                        else
                        {
                            decompressedPartData = br.ReadBytes(uncompressedSize);
                            decompressedData.AddRange(decompressedPartData);
                        }
                    }
                }
                else
                {
                    // When the compressed size of a data block shows 32000, it is uncompressed.
                    if (compressedSize != 32000)
                    {
                        var compressedData = br.ReadBytes(compressedSize);

                        var uncompressedData = await IOUtil.Decompressor(compressedData, uncompressedSize);

                        decompressedData.AddRange(uncompressedData);
                    }
                    else
                    {
                        var decompressedPartData = br.ReadBytes(uncompressedSize);
                        decompressedData.AddRange(decompressedPartData);
                    }
                }

                j = j + 20;
            }

            /*
            if (decompressedData.Count < uncompressedFileSize)
            {
                var difference = uncompressedFileSize - decompressedData.Count;
                var padding = new byte[difference];
                Array.Clear(padding, 0, difference);
                decompressedData.AddRange(padding);
            }
            */
            xivTex.TexData = decompressedData.ToArray();

            return xivTex;
        }
    }
}

