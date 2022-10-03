﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;

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
    }
}
