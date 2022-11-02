using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Helpers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Util.Extensions
{
    public class DDSExtensions
    {
        public static async Task<(List<byte> compressedDDS, List<short> mipPartOffsets, List<short> mipPartCounts)>
            ReadDDS(BinaryReader br, XivTexFormat format, int newWidth, int newHeight, int newMipCount, bool shouldCompress = true)
        {
            var compressedDDS = new List<byte>();
            var mipPartOffsets = new List<short>();
            var mipPartCount = new List<short>();

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

            br.BaseStream.Seek(128, SeekOrigin.Begin);

            for (var i = 0; i < newMipCount; i++)
            {
                var mipParts = (int)Math.Ceiling(mipLength / 16000f);
                mipPartCount.Add((short)mipParts);

                if (mipParts > 1)
                {
                    for (var j = 0; j < mipParts; j++)
                    {
                        int uncompLength;
                        var comp = true;

                        if (j == mipParts - 1)
                        {
                            uncompLength = mipLength % 16000;
                        }
                        else
                        {
                            uncompLength = 16000;
                        }

                        var uncompBytes = br.ReadBytes(uncompLength);
                        byte[] compressed;
                        if (shouldCompress)
                        {
                            compressed = await IOUtil.Compressor(uncompBytes);
                        }
                        else
                        {
                            compressed = uncompBytes;
                            comp = false;
                        }

                        if (compressed.Length > uncompLength)
                        {
                            compressed = uncompBytes;
                            comp = false;
                        }

                        compressedDDS.AddRange(BitConverter.GetBytes(16));
                        compressedDDS.AddRange(BitConverter.GetBytes(0));

                        compressedDDS.AddRange(!comp
                            ? BitConverter.GetBytes(32000)
                            : BitConverter.GetBytes(compressed.Length));

                        compressedDDS.AddRange(BitConverter.GetBytes(uncompLength));
                        compressedDDS.AddRange(compressed);

                        var padding = 128 - (compressed.Length % 128);

                        compressedDDS.AddRange(new byte[padding]);

                        mipPartOffsets.Add((short)(compressed.Length + padding + 16));
                    }
                }
                else
                {
                    int uncompLength;
                    var comp = true;

                    if (mipLength != 16000)
                    {
                        uncompLength = mipLength % 16000;
                    }
                    else
                    {
                        uncompLength = 16000;
                    }

                    var uncompBytes = br.ReadBytes(uncompLength);
                    byte[] compressed;
                    if (shouldCompress)
                    {
                        compressed = await IOUtil.Compressor(uncompBytes);
                    }
                    else
                    {
                        compressed = uncompBytes;
                        comp = false;
                    }

                    if (compressed.Length > uncompLength)
                    {
                        compressed = uncompBytes;
                        comp = false;
                    }

                    compressedDDS.AddRange(BitConverter.GetBytes(16));
                    compressedDDS.AddRange(BitConverter.GetBytes(0));

                    compressedDDS.AddRange(!comp
                        ? BitConverter.GetBytes(32000)
                        : BitConverter.GetBytes(compressed.Length));

                    compressedDDS.AddRange(BitConverter.GetBytes(uncompLength));
                    compressedDDS.AddRange(compressed);

                    var padding = 128 - (compressed.Length % 128);

                    compressedDDS.AddRange(new byte[padding]);

                    mipPartOffsets.Add((short)(compressed.Length + padding + 16));
                }

                if (mipLength > 32)
                {
                    mipLength = mipLength / 4;
                }
                else
                {
                    mipLength = 8;
                }
            }

            return (compressedDDS, mipPartOffsets, mipPartCount);
        }
    }
}
