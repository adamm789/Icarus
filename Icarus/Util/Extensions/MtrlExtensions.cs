using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Helpers;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;
using Half = SharpDX.Half;
using Index = xivModdingFramework.SqPack.FileTypes.Index;

namespace Icarus.Util.Extensions
{
    public static class MtrlExtensions
    {
        //https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Materials/FileTypes/Mtrl.cs#L408
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameDirectory">The framework game directory (ends in ffxiv)</param>
        /// <param name="mtrlData"></param>
        /// <param name="mtrlPath"></param>
        /// <returns></returns>
       /*
        public static async Task<XivMtrl> GetMtrlData(DirectoryInfo gameDirectory, byte[] mtrlData, string mtrlPath)
        {
            var dat = new Dat(gameDirectory);
            var index = new Index(gameDirectory);

            // Get uncompressed mtrl data
            XivDataFile df = XivDataFile._00_Common;
            try
            {
                df = IOUtil.GetDataFileFromPath(mtrlPath);
            }
            catch (Exception ex)
            {

            }
            XivMtrl xivMtrl = null;
            using (var br = new BinaryReader(new MemoryStream(mtrlData)))
            {
                xivMtrl = new XivMtrl
                {
                    Signature = br.ReadInt32(),
                    FileSize = br.ReadInt16(),
                };
                var colorSetDataSize = br.ReadUInt16();
                xivMtrl.MaterialDataSize = br.ReadUInt16();
                xivMtrl.TexturePathsDataSize = br.ReadUInt16();
                xivMtrl.TextureCount = br.ReadByte();
                xivMtrl.MapCount = br.ReadByte();
                xivMtrl.ColorSetCount = br.ReadByte();
                xivMtrl.UnknownDataSize = br.ReadByte();
                xivMtrl.MTRLPath = mtrlPath;

                var pathSizeList = new List<int>();

                // get the texture path offsets
                xivMtrl.TexturePathOffsetList = new List<int>(xivMtrl.TextureCount);
                xivMtrl.TexturePathUnknownList = new List<short>(xivMtrl.TextureCount);
                for (var i = 0; i < xivMtrl.TextureCount; i++)
                {
                    xivMtrl.TexturePathOffsetList.Add(br.ReadInt16());
                    xivMtrl.TexturePathUnknownList.Add(br.ReadInt16());

                    // add the size of the paths
                    if (i > 0)
                    {
                        pathSizeList.Add(
                            xivMtrl.TexturePathOffsetList[i] - xivMtrl.TexturePathOffsetList[i - 1]);
                    }
                }

                // get the map path offsets
                xivMtrl.MapPathOffsetList = new List<int>(xivMtrl.MapCount);
                xivMtrl.MapPathUnknownList = new List<short>(xivMtrl.MapCount);
                for (var i = 0; i < xivMtrl.MapCount; i++)
                {
                    xivMtrl.MapPathOffsetList.Add(br.ReadInt16());
                    xivMtrl.MapPathUnknownList.Add(br.ReadInt16());

                    // add the size of the paths
                    if (i > 0)
                    {
                        pathSizeList.Add(xivMtrl.MapPathOffsetList[i] - xivMtrl.MapPathOffsetList[i - 1]);
                    }
                    else
                    {
                        if (xivMtrl.TextureCount > 0)
                        {
                            pathSizeList.Add(xivMtrl.MapPathOffsetList[i] -
                                             xivMtrl.TexturePathOffsetList[xivMtrl.TextureCount - 1]);
                        }
                    }
                }

                // get the color set offsets
                xivMtrl.ColorSetPathOffsetList = new List<int>(xivMtrl.ColorSetCount);
                xivMtrl.ColorSetPathUnknownList = new List<short>(xivMtrl.ColorSetCount);
                for (var i = 0; i < xivMtrl.ColorSetCount; i++)
                {
                    xivMtrl.ColorSetPathOffsetList.Add(br.ReadInt16());
                    xivMtrl.ColorSetPathUnknownList.Add(br.ReadInt16());

                    // add the size of the paths
                    if (i > 0)
                    {
                        pathSizeList.Add(xivMtrl.ColorSetPathOffsetList[i] -
                                         xivMtrl.ColorSetPathOffsetList[i - 1]);
                    }
                    else
                    {
                        pathSizeList.Add(xivMtrl.ColorSetPathOffsetList[i] -
                                         xivMtrl.MapPathOffsetList[xivMtrl.MapCount - 1]);
                    }
                }

                pathSizeList.Add(xivMtrl.TexturePathsDataSize -
                                 xivMtrl.ColorSetPathOffsetList[xivMtrl.ColorSetCount - 1]);

                var count = 0;

                // get the texture path strings
                xivMtrl.TexturePathList = new List<string>(xivMtrl.TextureCount);
                for (var i = 0; i < xivMtrl.TextureCount; i++)
                {
                    var texturePath = Encoding.UTF8.GetString(br.ReadBytes(pathSizeList[count]))
                        .Replace("\0", "");
                    var dx11FileName = Path.GetFileName(texturePath).Insert(0, "--");

                    if (string.IsNullOrEmpty(texturePath)) continue;

                    if (await index.FileExists(Path.GetDirectoryName(texturePath).Replace("\\", "/") + "/" + dx11FileName,
                        df))
                    {
                        texturePath = texturePath.Insert(texturePath.LastIndexOf("/") + 1, "--");
                    }

                    xivMtrl.TexturePathList.Add(texturePath);
                    count++;
                }

                // get the map path strings
                xivMtrl.MapPathList = new List<string>(xivMtrl.MapCount);
                for (var i = 0; i < xivMtrl.MapCount; i++)
                {
                    xivMtrl.MapPathList.Add(Encoding.UTF8.GetString(br.ReadBytes(pathSizeList[count]))
                        .Replace("\0", ""));
                    count++;
                }

                // get the color set path strings
                xivMtrl.ColorSetPathList = new List<string>(xivMtrl.ColorSetCount);
                for (var i = 0; i < xivMtrl.ColorSetCount; i++)
                {
                    xivMtrl.ColorSetPathList.Add(Encoding.UTF8.GetString(br.ReadBytes(pathSizeList[count]))
                        .Replace("\0", ""));
                    count++;
                }

                var shaderPathSize = xivMtrl.MaterialDataSize - xivMtrl.TexturePathsDataSize;

                xivMtrl.Shader = Encoding.UTF8.GetString(br.ReadBytes(shaderPathSize)).Replace("\0", "");

                xivMtrl.Unknown2 = br.ReadBytes(xivMtrl.UnknownDataSize);

                xivMtrl.ColorSetData = new List<Half>();
                xivMtrl.ColorSetDyeData = null;
                if (colorSetDataSize > 0)
                {
                    // Color Data is always 512 (6 x 14 = 64 x 8bpp = 512)
                    var colorDataSize = 512;

                    for (var i = 0; i < colorDataSize / 2; i++)
                    {
                        xivMtrl.ColorSetData.Add(new Half(br.ReadUInt16()));
                    }

                    // If the color set is 544 in length, it has an extra 32 bytes at the end
                    if (colorSetDataSize == 544)
                    {
                        xivMtrl.ColorSetDyeData = br.ReadBytes(32);
                    }
                }

                var originalShaderParameterDataSize = br.ReadUInt16();

                var originalTextureUsageCount = br.ReadUInt16();

                var originalShaderParameterCount = br.ReadUInt16();

                var originalTextureDescriptorCount = br.ReadUInt16();

                xivMtrl.ShaderNumber = br.ReadUInt16();

                xivMtrl.Unknown3 = br.ReadUInt16();

                xivMtrl.TextureUsageList = new List<TextureUsageStruct>(originalTextureUsageCount);
                for (var i = 0; i < originalTextureUsageCount; i++)
                {
                    xivMtrl.TextureUsageList.Add(new TextureUsageStruct
                    {
                        TextureType = br.ReadUInt32(),
                        Unknown = br.ReadUInt32()
                    });
                }

                xivMtrl.ShaderParameterList = new List<ShaderParameterStruct>(originalShaderParameterCount);
                for (var i = 0; i < originalShaderParameterCount; i++)
                {
                    xivMtrl.ShaderParameterList.Add(new ShaderParameterStruct
                    {
                        ParameterID = (MtrlShaderParameterId)br.ReadUInt32(),
                        Offset = br.ReadInt16(),
                        Size = br.ReadInt16()
                    });
                }

                xivMtrl.TextureDescriptorList = new List<TextureDescriptorStruct>(originalTextureDescriptorCount);
                for (var i = 0; i < originalTextureDescriptorCount; i++)
                {
                    xivMtrl.TextureDescriptorList.Add(new TextureDescriptorStruct
                    {
                        TextureType = br.ReadUInt32(),
                        FileFormat = br.ReadInt16(),
                        Unknown = br.ReadInt16(),
                        TextureIndex = br.ReadUInt32()
                    });
                }


                var bytesRead = 0;
                foreach (var shaderParam in xivMtrl.ShaderParameterList)
                {
                    var offset = shaderParam.Offset;
                    var size = shaderParam.Size;
                    shaderParam.Args = new List<float>();
                    if (bytesRead + size <= originalShaderParameterDataSize)
                    {
                        for (var idx = offset; idx < offset + size; idx += 4)
                        {
                            var arg = br.ReadSingle();
                            shaderParam.Args.Add(arg);
                            bytesRead += 4;
                        }
                    }
                    else
                    {
                        // Just use a blank array if we have missing/invalid shader data.
                        shaderParam.Args = new List<float>(new float[size / 4]);
                    }
                }

                // Chew through any remaining padding.
                while (bytesRead < originalShaderParameterDataSize)
                {
                    br.ReadByte();
                    bytesRead++;
                }


            }

            return xivMtrl;
        }
       */
        //https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Materials/FileTypes/Mtrl.cs#L753
        /*
        public static byte[] CreateMtrlFile(XivMtrl xivMtrl)
        {
            var mtrlBytes = new List<byte>();

            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.Signature));

            var fileSizePointer = mtrlBytes.Count;
            mtrlBytes.AddRange(BitConverter.GetBytes((ushort)0)); //Backfilled later
            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.ColorSetDataSize));

            var materialDataSizePointer = mtrlBytes.Count;
            mtrlBytes.AddRange(BitConverter.GetBytes((ushort)0)); //Backfilled later

            var texturePathsDataSizePointer = mtrlBytes.Count;
            mtrlBytes.AddRange(BitConverter.GetBytes((ushort)0)); //Backfilled later

            mtrlBytes.Add((byte)xivMtrl.TexturePathList.Count);
            mtrlBytes.Add((byte)xivMtrl.MapPathList.Count);
            mtrlBytes.Add((byte)xivMtrl.ColorSetPathList.Count);
            mtrlBytes.Add(xivMtrl.UnknownDataSize);

            // Regenerate offset list as we build the string list.
            xivMtrl.TexturePathOffsetList.Clear();
            xivMtrl.MapPathOffsetList.Clear();
            xivMtrl.ColorSetPathOffsetList.Clear();

            var stringListBytes = new List<byte>();
            var texIdx = 0;
            foreach (var texPathString in xivMtrl.TexturePathList)
            {
                xivMtrl.TexturePathOffsetList.Add(stringListBytes.Count);
                var path = texPathString;
                // This is an old style DX9/DX11 Mixed Texture reference, make sure to clean it up if needed.
                if (xivMtrl.TexturePathUnknownList[texIdx] != 0)
                {
                    path = path.Replace("--", string.Empty);
                }

                stringListBytes.AddRange(Encoding.UTF8.GetBytes(path));
                stringListBytes.Add(0);
                texIdx++;
            }

            foreach (var mapPathString in xivMtrl.MapPathList)
            {
                xivMtrl.MapPathOffsetList.Add(stringListBytes.Count);
                stringListBytes.AddRange(Encoding.UTF8.GetBytes(mapPathString));
                stringListBytes.Add(0);
            }

            foreach (var colorSetPathString in xivMtrl.ColorSetPathList)
            {
                xivMtrl.ColorSetPathOffsetList.Add(stringListBytes.Count);
                stringListBytes.AddRange(Encoding.UTF8.GetBytes(colorSetPathString));
                stringListBytes.Add(0);
            }

            xivMtrl.TexturePathsDataSize = (ushort)stringListBytes.Count;

            stringListBytes.AddRange(Encoding.UTF8.GetBytes(xivMtrl.Shader));
            stringListBytes.Add(0);

            var padding = stringListBytes.Count % 8;
            if (padding != 0)
            {
                padding = 8 - padding;
            }

            stringListBytes.AddRange(new byte[padding]);
            xivMtrl.MaterialDataSize = (ushort)stringListBytes.Count;

            // Write the new offset list.
            for (var i = 0; i < xivMtrl.TexturePathOffsetList.Count; i++)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes((short)xivMtrl.TexturePathOffsetList[i]));
                mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.TexturePathUnknownList[i]));
            }

            for (var i = 0; i < xivMtrl.MapPathOffsetList.Count; i++)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes((short)xivMtrl.MapPathOffsetList[i]));
                mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.MapPathUnknownList[i]));
            }

            for (var i = 0; i < xivMtrl.ColorSetPathOffsetList.Count; i++)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes((short)xivMtrl.ColorSetPathOffsetList[i]));
                mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.ColorSetPathUnknownList[i]));
            }

            // Write the actual string list (including padding).
            mtrlBytes.AddRange(stringListBytes);

            // Don't know what these (4) bytes do, but hey, whatever.
            mtrlBytes.AddRange(xivMtrl.Unknown2);

            foreach (var colorSetHalf in xivMtrl.ColorSetData)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes(colorSetHalf.RawValue));
            }

            if (xivMtrl.ColorSetDataSize == 544)
            {
                mtrlBytes.AddRange(xivMtrl.ColorSetDyeData);
            }

            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.ShaderParameterDataSize));
            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.TextureUsageCount));
            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.ShaderParameterCount));
            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.TextureDescriptorCount));
            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.ShaderNumber));

            mtrlBytes.AddRange(BitConverter.GetBytes(xivMtrl.Unknown3));

            foreach (var dataStruct1 in xivMtrl.TextureUsageList)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes(dataStruct1.TextureType));
                mtrlBytes.AddRange(BitConverter.GetBytes(dataStruct1.Unknown));
            }

            var offset = 0;
            foreach (var parameter in xivMtrl.ShaderParameterList)
            {
                // Ensure we're writing correctly calculated data.
                parameter.Offset = (short)offset;
                parameter.Size = (short)parameter.Args.Count;
                offset += parameter.Size * 4;
                short byteSize = (short)(parameter.Size * 4);

                mtrlBytes.AddRange(BitConverter.GetBytes((uint)parameter.ParameterID));
                mtrlBytes.AddRange(BitConverter.GetBytes(parameter.Offset));
                mtrlBytes.AddRange(BitConverter.GetBytes(byteSize));
            }

            foreach (var parameterStruct in xivMtrl.TextureDescriptorList)
            {
                mtrlBytes.AddRange(BitConverter.GetBytes(parameterStruct.TextureType));
                mtrlBytes.AddRange(BitConverter.GetBytes(parameterStruct.FileFormat));
                mtrlBytes.AddRange(BitConverter.GetBytes(parameterStruct.Unknown));
                mtrlBytes.AddRange(BitConverter.GetBytes(parameterStruct.TextureIndex));
            }

            var shaderBytes = new List<byte>();
            foreach (var shaderParam in xivMtrl.ShaderParameterList)
            {
                foreach (var f in shaderParam.Args)
                {
                    shaderBytes.AddRange(BitConverter.GetBytes(f));
                }
            }

            // Pad out if we're missing anything.
            if (shaderBytes.Count < xivMtrl.ShaderParameterDataSize)
            {
                shaderBytes.AddRange(new byte[xivMtrl.ShaderParameterDataSize - shaderBytes.Count]);
            }
            mtrlBytes.AddRange(shaderBytes);

            // Backfill the actual file size data.
            xivMtrl.FileSize = (short)mtrlBytes.Count;
            IOUtil.ReplaceBytesAt(mtrlBytes, BitConverter.GetBytes(xivMtrl.FileSize), fileSizePointer);

            xivMtrl.MaterialDataSize = (ushort)stringListBytes.Count;
            IOUtil.ReplaceBytesAt(mtrlBytes, BitConverter.GetBytes(xivMtrl.MaterialDataSize), materialDataSizePointer);

            IOUtil.ReplaceBytesAt(mtrlBytes, BitConverter.GetBytes(xivMtrl.TexturePathsDataSize), texturePathsDataSizePointer);
            return mtrlBytes.ToArray();
        }
        */
        /*
        //https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Materials/FileTypes/Mtrl.cs#L640
        public static XivTex MtrlToXivTex(XivMtrl xivMtrl, TexTypePath ttp)
        {
            var colorSetData = new List<byte>();

            foreach (var colorSetHalf in xivMtrl.ColorSetData)
            {
                colorSetData.AddRange(BitConverter.GetBytes(colorSetHalf.RawValue));
            }

            var xivTex = new XivTex
            {
                Width = 4,
                Height = 16,
                MipMapCount = 0,
                TexData = colorSetData.ToArray(),
                TextureFormat = XivTexFormat.A16B16G16R16F,
                TextureTypeAndPath = ttp
            };

            return xivTex;
        }
        */

    }
}
