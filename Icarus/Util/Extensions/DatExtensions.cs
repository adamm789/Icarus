using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Helpers;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Util.Extensions
{
    public static class DatExtensions
    {
        /// <summary>
        /// Type4 data is textures
        /// data is data from XivTex
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static async Task<XivTex> GetType4Data(byte[] data, long offset = 0)
        {
            var xivTex = new XivTex();
            var decompressedData = new List<byte>();

            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                var headerLength = br.ReadInt32();
                var fileType = br.ReadInt32();
                var uncompressedFileSize = br.ReadInt32();
                var ikd1 = br.ReadInt32();
                var ikd2 = br.ReadInt32();
                xivTex.MipMapCount = br.ReadInt32();

                var endOfHeader = offset + headerLength;
                var mipMapInfoOffset = offset + 24;

                br.BaseStream.Seek(endOfHeader + 4, SeekOrigin.Begin);

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

                    var mipMapPartOffset = endOfHeader + offsetFromHeaderEnd;

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

                if (decompressedData.Count < uncompressedFileSize)
                {
                    var difference = uncompressedFileSize - decompressedData.Count;
                    var padding = new byte[difference];
                    Array.Clear(padding, 0, difference);
                    decompressedData.AddRange(padding);
                }
            }
            xivTex.TexData = decompressedData.ToArray();

            return xivTex;
        }

        public static async Task<byte[]> GetType2Data(byte[] data, long offset = 0)
        {
            var type2Bytes = new List<byte>();
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                var headerLength = br.ReadInt32();

                br.ReadBytes(16);

                var dataBlockCount = br.ReadInt32();

                for (var i = 0; i < dataBlockCount; i++)
                {
                    br.BaseStream.Seek(offset + (24 + (8 * i)), SeekOrigin.Begin);

                    var dataBlockOffset = br.ReadInt32();

                    br.BaseStream.Seek(offset + headerLength + dataBlockOffset, SeekOrigin.Begin);

                    br.ReadBytes(8);

                    var compressedSize = br.ReadInt32();
                    var uncompressedSize = br.ReadInt32();

                    // When the compressed size of a data block shows 32000, it is uncompressed.
                    if (compressedSize == 32000)
                    {
                        type2Bytes.AddRange(br.ReadBytes(uncompressedSize));
                    }
                    else
                    {
                        var compressedData = br.ReadBytes(compressedSize);

                        var decompressedData = await IOUtil.Decompressor(compressedData, uncompressedSize);

                        type2Bytes.AddRange(decompressedData);
                    }
                }
            }

            return type2Bytes.ToArray();
        }

        public static async Task<byte[]> CreateType2Data(byte[] dataToCreate, bool shouldCompress = true)
        {
            var newData = new List<byte>();
            var headerData = new List<byte>();
            var dataBlocks = new List<byte>();

            // Header size is defaulted to 128, but may need to change if the data being imported is very large.
            headerData.AddRange(BitConverter.GetBytes(128));
            headerData.AddRange(BitConverter.GetBytes(2));
            headerData.AddRange(BitConverter.GetBytes(dataToCreate.Length));

            var dataOffset = 0;
            var totalCompSize = 0;
            var uncompressedLength = dataToCreate.Length;

            var partCount = (int)Math.Ceiling(uncompressedLength / 16000f);

            headerData.AddRange(BitConverter.GetBytes(partCount));

            var remainder = uncompressedLength;

            using (var binaryReader = new BinaryReader(new MemoryStream(dataToCreate)))
            {
                binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

                for (var i = 1; i <= partCount; i++)
                {
                    if (i == partCount)
                    {
                        byte[] compressedData;
                        if (shouldCompress)
                        {
                            compressedData = await IOUtil.Compressor(binaryReader.ReadBytes(remainder));
                        }
                        else
                        {
                            compressedData = binaryReader.ReadBytes(remainder);
                        }

                        var padding = 128 - (compressedData.Length + 16) % 128;

                        dataBlocks.AddRange(BitConverter.GetBytes(16));
                        dataBlocks.AddRange(BitConverter.GetBytes(0));
                        dataBlocks.AddRange(BitConverter.GetBytes(compressedData.Length));
                        dataBlocks.AddRange(BitConverter.GetBytes(remainder));
                        dataBlocks.AddRange(compressedData);
                        dataBlocks.AddRange(new byte[padding]);

                        headerData.AddRange(BitConverter.GetBytes(dataOffset));
                        headerData.AddRange(BitConverter.GetBytes((short)(compressedData.Length + 16 + padding)));
                        headerData.AddRange(BitConverter.GetBytes((short)remainder));

                        totalCompSize = dataOffset + compressedData.Length + 16 + padding;
                    }
                    else
                    {
                        byte[] compressedData;
                        if (shouldCompress)
                        {
                            compressedData = await IOUtil.Compressor(binaryReader.ReadBytes(16000));
                        }
                        else
                        {
                            compressedData = binaryReader.ReadBytes(16000);
                        }
                        var padding = 128 - (compressedData.Length + 16) % 128;

                        dataBlocks.AddRange(BitConverter.GetBytes(16));
                        dataBlocks.AddRange(BitConverter.GetBytes(0));
                        dataBlocks.AddRange(BitConverter.GetBytes(compressedData.Length));
                        dataBlocks.AddRange(BitConverter.GetBytes(16000));
                        dataBlocks.AddRange(compressedData);
                        dataBlocks.AddRange(new byte[padding]);

                        headerData.AddRange(BitConverter.GetBytes(dataOffset));
                        headerData.AddRange(BitConverter.GetBytes((short)(compressedData.Length + 16 + padding)));
                        headerData.AddRange(BitConverter.GetBytes((short)16000));

                        dataOffset += compressedData.Length + 16 + padding;
                        remainder -= 16000;
                    }
                }
            }

            headerData.InsertRange(12, BitConverter.GetBytes(totalCompSize / 128));
            headerData.InsertRange(16, BitConverter.GetBytes(totalCompSize / 128));

            var headerSize = headerData.Count;
            var rem = headerSize % 128;
            if (rem != 0)
            {
                headerSize += 128 - rem;
            }

            headerData.RemoveRange(0, 4);
            headerData.InsertRange(0, BitConverter.GetBytes(headerSize));

            var headerPadding = rem == 0 ? 0 : 128 - rem;
            headerData.AddRange(new byte[headerPadding]);

            newData.AddRange(headerData);
            newData.AddRange(dataBlocks);
            return newData.ToArray();
        }

        public static byte[] MakeType4DatHeader(XivTexFormat format, List<short> mipPartOffsets, List<short> mipPartCount, int uncompressedLength, int newMipCount, int newWidth, int newHeight)
        {
            var headerData = new List<byte>();

            var headerSize = 24 + (newMipCount * 20) + (mipPartOffsets.Count * 2);
            var headerPadding = 128 - (headerSize % 128);

            headerData.AddRange(BitConverter.GetBytes(headerSize + headerPadding));
            headerData.AddRange(BitConverter.GetBytes(4));
            headerData.AddRange(BitConverter.GetBytes(uncompressedLength + 80));
            headerData.AddRange(BitConverter.GetBytes(0));
            headerData.AddRange(BitConverter.GetBytes(0));
            headerData.AddRange(BitConverter.GetBytes(newMipCount));


            var partIndex = 0;
            var mipOffsetIndex = 80;
            var uncompMipSize = newHeight * newWidth;

            switch (format)
            {
                case XivTexFormat.DXT1:
                    uncompMipSize = (newWidth * newHeight) / 2;
                    break;
                case XivTexFormat.DXT5:
                case XivTexFormat.A8:
                    uncompMipSize = newWidth * newHeight;
                    break;
                case XivTexFormat.A1R5G5B5:
                case XivTexFormat.A4R4G4B4:
                    uncompMipSize = (newWidth * newHeight) * 2;
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
                    uncompMipSize = (newWidth * newHeight) * 4;
                    break;
            }

            for (var i = 0; i < newMipCount; i++)
            {
                headerData.AddRange(BitConverter.GetBytes(mipOffsetIndex));

                var paddedSize = 0;

                for (var j = 0; j < mipPartCount[i]; j++)
                {
                    paddedSize = paddedSize + mipPartOffsets[j + partIndex];
                }

                headerData.AddRange(BitConverter.GetBytes(paddedSize));

                headerData.AddRange(uncompMipSize > 16
                    ? BitConverter.GetBytes(uncompMipSize)
                    : BitConverter.GetBytes(16));

                uncompMipSize = uncompMipSize / 4;

                headerData.AddRange(BitConverter.GetBytes(partIndex));
                headerData.AddRange(BitConverter.GetBytes((int)mipPartCount[i]));

                partIndex = partIndex + mipPartCount[i];
                mipOffsetIndex = mipOffsetIndex + paddedSize;
            }

            foreach (var part in mipPartOffsets)
            {
                headerData.AddRange(BitConverter.GetBytes(part));
            }

            headerData.AddRange(new byte[headerPadding]);

            return headerData.ToArray();
        }

    }
}
