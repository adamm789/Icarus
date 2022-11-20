using HelixToolkit.SharpDX.Core;
using Lumina;
using Lumina.Data.Files;
using Lumina.Models.Models;
using Newtonsoft.Json.Linq;
using Serilog;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Enums;
using static Lumina.Data.Parsing.MdlStructs;
using static Lumina.Models.Models.Model;

namespace Icarus.Util
{
    public class MdlWithLumina
    {
        private static readonly bool _modded = false;   // *should* always be false because we're only loading original game data (I hope)

        // The new, user-provided Mdl file
        private readonly XivMdl ImportedXivMdl = new();

        // The Mdl file that is being replaced
        private readonly MdlFile OriginalMdlFile;

        private readonly Dictionary<int, string> OffsetToName = new();
        readonly Dictionary<VertexUsageType, VertexDataType> VertexDict = new();

        public MdlWithLumina(GameData _lumina, string mdlPath)
        {
            if (_lumina == null)
            {
                var err = "Lumina was not instantiated.";
                Log.Error(err);
                throw new NullReferenceException(err);
            }

            OriginalMdlFile = _lumina.GetFile<MdlFile>(mdlPath);

            if (OriginalMdlFile == null)
            {
                var err = string.Format("Count not find mdl file: {0}.", mdlPath);
                throw new ArgumentException(err);
            }
            ImportedXivMdl.MdlPath = mdlPath;
        }

        public MdlWithLumina(MdlFile mdlFile)
        {
            OriginalMdlFile = mdlFile;
            ImportedXivMdl.MdlPath = mdlFile.FilePath;
        }

        public MdlWithLumina(MdlFile mdlFile, string mdlPath)
        {
            OriginalMdlFile = mdlFile;
            ImportedXivMdl.MdlPath = mdlPath;
        }

        public static XivMdl GetRawMdlDataLumina(GameData _lumina, string mdlPath)
        {
            XivMdl ret = new();

            return ret;
        }

        // https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Models/FileTypes/Mdl.cs#L515
        // MDl.GetRawMdlData(string mdlPath, bool getOriginal = false, long offset = 0) but trying with Lumina
        // MdlFile https://github.com/NotAdam/Lumina/blob/master/src/Lumina/Data/Files/MdlFile.cs
        public XivMdl GetRawMdlDataLumina()
        {
            SetModelData();
            SetPathData();

            SetUnknownData0();

            bool getShapeData = SetLoDList();
            SetExtraLoDList();

            SetVertexDataStruct();
            SetMeshDataInformation();

            SetAttributeDataBlock();
            SetUnknownData1();

            SetMeshParts();
            SetUnknownData2();
            SetMaterialNameOffsets();
            SetBoneNameOffsets();

            SetBoneLists();
            SetMeshShapeData();
            AssignMeshAndLoDNumber();

            ImportedXivMdl.HasShapeData = ImportedXivMdl.ModelData.ShapeCount > 0 && getShapeData;

            SetBoneIndices();

            // Does this number matter or can I just set it to 7?
            // I assume that these two items have to sum to a number divisible by 8?
            ImportedXivMdl.PaddingSize = 7;
            ImportedXivMdl.PaddedBytes = new byte[ImportedXivMdl.PaddingSize];

            SetBoundingBoxes();
            SetBoneTransformData();
            SetVertexData(getShapeData);

            return ImportedXivMdl;
        }

        private byte ToBytes(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits is not 8 bits");
            }

            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        private void SetModelData()
        {
            var mdlHeader = OriginalMdlFile.ModelHeader;

            var b1 = new bool[] { mdlHeader.ShadowDisabled, mdlHeader.LightShadowDisabled,
                                    mdlHeader.WavingAnimationDisabled, mdlHeader.BgLightingReflectionEnabled,
                                    mdlHeader.Unknown1, mdlHeader.RainOcclusionEnabled,
                                    mdlHeader.SnowOcclusionEnabled, mdlHeader.DustOcclusionEnabled};
            var u1 = ToBytes(new BitArray(b1));

            var b2 = new bool[]
            {
                mdlHeader.Unknown2, mdlHeader.BgUvScrollEnabled, mdlHeader.EnableForceNonResident,
                mdlHeader.ExtraLodEnabled, mdlHeader.ShadowMaskEnabled, mdlHeader.ForceLodRangeEnabled,
                mdlHeader.EdgeGeometryEnabled, mdlHeader.Unknown3
            };
            var u2 = ToBytes(new BitArray(b2));

            var u3 = new byte[] { mdlHeader.TerrainShadowMeshCount, u2 };

            var u11 = new byte[] { mdlHeader.BGCrestChangeMaterialIndex, mdlHeader.Unknown6 };

            var mdlModelData = new MdlModelData
            {
                Unknown0 = BitConverter.ToInt32(BitConverter.GetBytes(mdlHeader.Radius), 0),

                MeshCount = (short)mdlHeader.MeshCount,
                AttributeCount = (short)mdlHeader.AttributeCount,
                MeshPartCount = (short)mdlHeader.SubmeshCount,
                MaterialCount = (short)mdlHeader.MaterialCount,
                BoneCount = (short)mdlHeader.BoneCount,
                BoneListCount = (short)mdlHeader.BoneTableCount,
                ShapeCount = (short)mdlHeader.ShapeCount,
                ShapePartCount = (short)mdlHeader.ShapeMeshCount,
                ShapeDataCount = mdlHeader.ShapeValueCount,
                LoDCount = mdlHeader.LodCount,

                Unknown1 = u1,
                Unknown2 = (short)mdlHeader.ElementIdCount,
                Unknown3 = BitConverter.ToInt16(u3),
                Unknown4 = BitConverter.ToInt16(BitConverter.GetBytes(mdlHeader.ModelClipOutDistance), 2),
                Unknown5 = BitConverter.ToInt16(BitConverter.GetBytes(mdlHeader.ModelClipOutDistance), 0),
                Unknown6 = BitConverter.ToInt16(BitConverter.GetBytes(mdlHeader.ShadowClipOutDistance), 2),
                Unknown7 = BitConverter.ToInt16(BitConverter.GetBytes(mdlHeader.ShadowClipOutDistance), 0),
                Unknown8 = (short)mdlHeader.Unknown4,
                Unknown9 = (short)mdlHeader.TerrainShadowSubmeshCount,
                Unknown10a = 0,     // Unknown5, which is private
                Unknown10b = mdlHeader.BGChangeMaterialIndex,
                Unknown11 = BitConverter.ToInt16(u11),
                Unknown12 = (short)mdlHeader.Unknown7,
                Unknown13 = (short)mdlHeader.Unknown8,
                Unknown14 = (short)mdlHeader.Unknown9,
                Unknown15 = 0,  // Padding
                Unknown16 = 0,
                Unknown17 = 0
            };
            ImportedXivMdl.ModelData = mdlModelData;
        }


        private void SetPathData()
        {
            var mdlPathData = new MdlPathData()
            {
                PathCount = OriginalMdlFile.StringCount,
                PathBlockSize = OriginalMdlFile.Strings.Length,
            };

            string[] str = Encoding.ASCII.GetString(OriginalMdlFile.Strings.ToArray()).Split("\0");
            int counter = 0;

            List<string>[] l = new List<string>[5];
            l[0] = new();
            l[1] = new();
            l[2] = new();
            l[3] = new();
            l[4] = new();

            // TODO: Better implementation to properly assign and sort path stings?
            for (int i = 0; i < str.Length; i++)
            {
                // counter = 0
                // AttributePaths

                if (i == OriginalMdlFile.AttributeNameOffsets.Length)
                {
                    // Bone Paths
                    counter = 1;
                }
                if (i == OriginalMdlFile.AttributeNameOffsets.Length + OriginalMdlFile.BoneNameOffsets.Length)
                {
                    // Material Paths
                    counter = 2;
                }
                if (i == OriginalMdlFile.AttributeNameOffsets.Length + OriginalMdlFile.BoneNameOffsets.Length + OriginalMdlFile.MaterialNameOffsets.Length)
                {
                    //Shape List
                    counter = 3;
                }
                if (i == OriginalMdlFile.AttributeNameOffsets.Length + OriginalMdlFile.BoneNameOffsets.Length + OriginalMdlFile.MaterialNameOffsets.Length + OriginalMdlFile.ModelHeader.ShapeCount)
                {
                    // Extra Paths
                    counter = 4;
                }

                if (str[i] != "")
                {
                    l[counter].Add(str[i]);
                }
            }

            mdlPathData.AttributeList = l[0].ToList();
            mdlPathData.BoneList = l[1].ToList();
            mdlPathData.MaterialList = l[2].ToList();
            mdlPathData.ShapeList = l[3].ToList();
            mdlPathData.ExtraPathList = l[4].ToList();

            List<int> offsets = new List<int>();
            offsets.AddRange(Array.ConvertAll(OriginalMdlFile.AttributeNameOffsets, x => (int)x));
            offsets.AddRange(Array.ConvertAll(OriginalMdlFile.BoneNameOffsets, x => (int)x));
            offsets.AddRange(Array.ConvertAll(OriginalMdlFile.MaterialNameOffsets, x => (int)x));

            SortedSet<int> shapeOffsets = new SortedSet<int>();

            foreach (var s in OriginalMdlFile.Shapes)
            {
                if (!shapeOffsets.Contains((int)s.StringOffset))
                {
                    shapeOffsets.Add((int)s.StringOffset);
                }
            }
            offsets.AddRange(shapeOffsets);

            List<string> names = new List<string>();
            names.AddRange(l[0]);
            names.AddRange(l[1]);
            names.AddRange(l[2]);
            names.AddRange(l[3]);
            names.AddRange(l[4]);

            for (int i = 0; i < offsets.Count; i++)
            {
                OffsetToName.Add(offsets[i], names[i]);
            }

            ImportedXivMdl.PathData = mdlPathData;
        }

        private void SetUnknownData0()
        {
            var e = OriginalMdlFile.ElementIds;
            List<byte> l = new List<byte>();

            foreach (var id in e)
            {
                l.AddRange(BitConverter.GetBytes(id.ElementId));
                l.AddRange(BitConverter.GetBytes(id.ParentBoneName));
                l.AddRange(BitConverter.GetBytes(id.Translate[0]));
                l.AddRange(BitConverter.GetBytes(id.Translate[1]));
                l.AddRange(BitConverter.GetBytes(id.Translate[2]));
                l.AddRange(BitConverter.GetBytes(id.Rotate[0]));
                l.AddRange(BitConverter.GetBytes(id.Rotate[1]));
                l.AddRange(BitConverter.GetBytes(id.Rotate[2]));
            }
            ImportedXivMdl.UnkData0 = new UnknownData0 { Unknown = l.ToArray() };
        }

        private bool SetLoDList()
        {
            ImportedXivMdl.LoDList = new List<LevelOfDetail>();

            int totalLoDMeshes = 0;
            bool getShapeData = true;

            for (int i = 0; i < 3; i++)
            {
                var mdlFileLod = OriginalMdlFile.Lods[i];
                var lod = new LevelOfDetail
                {
                    MeshOffset = mdlFileLod.MeshIndex,
                    MeshCount = (short)mdlFileLod.MeshCount,
                    Unknown0 = BitConverter.ToInt32(BitConverter.GetBytes(mdlFileLod.ModelLodRange), 0),
                    Unknown1 = BitConverter.ToInt32(BitConverter.GetBytes(mdlFileLod.TextureLodRange), 0),
                    MeshEnd = (short)mdlFileLod.WaterMeshIndex,   //Maybe? 
                    ExtraMeshCount = (short)mdlFileLod.WaterMeshCount,
                    MeshSum = (short)mdlFileLod.ShadowMeshIndex,     // idk?
                    Unknown2 = (short)mdlFileLod.ShadowMeshCount,
                    Unknown3 = BitConverter.ToInt32(BitConverter.GetBytes(mdlFileLod.TerrainShadowMeshIndex).Concat(BitConverter.GetBytes(mdlFileLod.TerrainShadowMeshCount)).ToArray()), // Probably?
                    Unknown4 = BitConverter.ToInt32(BitConverter.GetBytes(mdlFileLod.VerticalFogMeshIndex).Concat(BitConverter.GetBytes(mdlFileLod.VerticalFogMeshCount)).ToArray()),   // Probably?
                    Unknown5 = (int)mdlFileLod.EdgeGeometrySize,
                    IndexDataStart = (int)mdlFileLod.EdgeGeometryDataOffset,
                    Unknown6 = (int)mdlFileLod.PolygonCount,
                    Unknown7 = (int)mdlFileLod.Unknown1,
                    VertexDataSize = (int)mdlFileLod.VertexBufferSize,
                    IndexDataSize = (int)mdlFileLod.IndexBufferSize,
                    VertexDataOffset = (int)mdlFileLod.VertexDataOffset,
                    IndexDataOffset = (int)mdlFileLod.IndexDataOffset,
                    MeshDataList = new List<MeshData>()
                };

                totalLoDMeshes += lod.MeshCount;

                // if LoD0 shows no mesh, add one (This is rare, but happens on company chest for example)
                if (i == 0 && lod.MeshCount == 0)
                {
                    lod.MeshCount = 1;
                }

                // This is a simple check to identify old mods that may have broken shape data.
                // Old mods still have LoD 1+ data.
                if (_modded && i > 0 && lod.MeshCount > 0)
                {
                    getShapeData = false;
                }

                ImportedXivMdl.LoDList.Add(lod);
            }

            return getShapeData;
        }

        private void SetExtraLoDList()
        {
            if (OriginalMdlFile.ModelHeader.ExtraLodEnabled)
            {
                ImportedXivMdl.ExtraLoDList = new List<LevelOfDetail>();
                for (int i = 0; i < ImportedXivMdl.ModelData.Unknown10a; i++)
                {
                    var extraLod = OriginalMdlFile.ExtraLods[i];
                    var lod = new LevelOfDetail
                    {
                        MeshOffset = extraLod.LightShaftMeshIndex,
                        MeshCount = (short)extraLod.LightShaftMeshCount,
                        Unknown0 = extraLod.GlassMeshIndex,
                        Unknown1 = extraLod.GlassMeshCount,
                        MeshEnd = (short)extraLod.MaterialChangeMeshIndex,
                        ExtraMeshCount = (short)extraLod.CrestChangeMeshIndex,
                        MeshSum = (short)extraLod.CrestChangeMeshCount,
                        Unknown2 = (short)extraLod.Unknown1,
                        Unknown3 = extraLod.Unknown2,
                        Unknown4 = extraLod.Unknown3,
                        Unknown5 = extraLod.Unknown4,
                        IndexDataStart = extraLod.Unknown5,
                        Unknown6 = extraLod.Unknown6,
                        Unknown7 = extraLod.Unknown7,
                        VertexDataSize = extraLod.Unknown8,
                        IndexDataSize = extraLod.Unknown9,
                        VertexDataOffset = extraLod.Unknown10,
                        IndexDataOffset = extraLod.Unknown11,
                        MeshDataList = new List<MeshData>()
                    };

                    ImportedXivMdl.ExtraLoDList.Add(lod);
                }
            }
        }

        private void SetMeshDataInformation()
        {
            var meshNum = 0;    // Unnecessary?
            var i = 0;
            for (var lodIdx = 0; lodIdx < ImportedXivMdl.LoDList.Count; lodIdx++)
            {
                var lod = ImportedXivMdl.LoDList[lodIdx];
                var totalMeshCount = lod.MeshCount + lod.ExtraMeshCount;
                for (var meshIdx = 0; meshIdx < totalMeshCount; meshIdx++)
                {
                    var mesh = OriginalMdlFile.Meshes[i];
                    var meshDataInfo = new MeshDataInfo
                    {
                        VertexCount = mesh.VertexCount,
                        IndexCount = (int)mesh.IndexCount,
                        MaterialIndex = (short)mesh.MaterialIndex,
                        MeshPartIndex = (short)mesh.SubMeshIndex,
                        MeshPartCount = (short)mesh.SubMeshCount,
                        BoneSetIndex = (short)mesh.BoneTableIndex,
                        IndexDataOffset = (int)mesh.StartIndex,
                        VertexDataOffset0 = (int)mesh.VertexBufferOffset[0],
                        VertexDataOffset1 = (int)mesh.VertexBufferOffset[1],
                        VertexDataOffset2 = (int)mesh.VertexBufferOffset[2],
                        VertexDataEntrySize0 = mesh.VertexBufferStride[0],
                        VertexDataEntrySize1 = mesh.VertexBufferStride[1],
                        VertexDataEntrySize2 = mesh.VertexBufferStride[2],
                        VertexDataBlockCount = mesh.VertexStreamCount
                    };
                    lod.MeshDataList[meshIdx].MeshInfo = meshDataInfo;

                    // In the event we have a null material reference, set it to material 0 to be safe.
                    if (meshDataInfo.MaterialIndex >= ImportedXivMdl.PathData.MaterialList.Count)
                    {
                        meshDataInfo.MaterialIndex = 0;
                    }
                    var materialString = ImportedXivMdl.PathData.MaterialList[meshDataInfo.MaterialIndex];

                    // Try block to cover odd cases like Au Ra Male Face #92 where for some reason the
                    // Last LoD points to using a shp for a material for some reason.
                    try
                    {
                        var typeChar = materialString[4].ToString() + materialString[9].ToString();

                        if (typeChar.Equals("cb"))
                        {
                            lod.MeshDataList[meshIdx].IsBody = true;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    meshNum++;
                    i++;
                }
            }
        }

        private void SetVertexDataStruct()
        {
            var idx = 0;
            for (int i = 0; i < ImportedXivMdl.LoDList.Count; i++)
            {
                var currLoD = ImportedXivMdl.LoDList[i];
                var totalMeshCount = currLoD.MeshCount + currLoD.ExtraMeshCount;
                for (var j = 0; j < totalMeshCount; j++)
                {
                    currLoD.MeshDataList.Add(new MeshData());
                    currLoD.MeshDataList[j].VertexDataStructList = new List<VertexDataStruct>();
                    foreach (var ve in OriginalMdlFile.VertexDeclarations[idx].VertexElements)
                    {
                        var vertexDataStruct = new VertexDataStruct
                        {
                            DataBlock = ve.Stream,
                            DataOffset = ve.Offset,
                            DataType = VertexTypeDictionary[ve.Type],
                            DataUsage = VertexUsageDictionary[ve.Usage]
                        };
                        currLoD.MeshDataList[j].VertexDataStructList.Add(vertexDataStruct);

                        // TODO: Make dict here?
                        
                        if (VertexDict.ContainsKey(vertexDataStruct.DataUsage))
                        {
                        }
                        else
                        {
                            VertexDict.Add(vertexDataStruct.DataUsage, vertexDataStruct.DataType);
                        }
                        
                    }
                    idx++;
                }
            }
        }


        private void SetAttributeDataBlock()
        {
            var attr = new AttributeDataBlock();
            int attrCount = OriginalMdlFile.ModelHeader.AttributeCount;
            attr.AttributePathOffsetList = new List<int>(attrCount);

            for (int i = 0; i < attrCount; i++)
            {
                attr.AttributePathOffsetList.Add((int)OriginalMdlFile.AttributeNameOffsets[i]);
            }

            ImportedXivMdl.AttrDataBlock = attr;
        }

        private void SetUnknownData1()
        {
            // UnkData1.Unknown is uninitialized in the framework

            List<byte> l = new List<byte>();
            foreach (var submesh in OriginalMdlFile.TerrainShadowMeshes)
            {
                l.AddRange(BitConverter.GetBytes(submesh.IndexCount));
                l.AddRange(BitConverter.GetBytes(submesh.StartIndex));
                l.AddRange(BitConverter.GetBytes(submesh.VertexBufferOffset));
                l.AddRange(BitConverter.GetBytes(submesh.VertexCount));
                l.AddRange(BitConverter.GetBytes(submesh.SubMeshIndex));
                l.AddRange(BitConverter.GetBytes(submesh.SubMeshCount));
                l.Add(submesh.VertexBufferStride);
                l.Add(new byte());  // submesh.Padding (which is private)
            }
            ImportedXivMdl.UnkData1 = new UnknownData1 { Unknown = l.ToArray() };
        }

        private void SetMeshParts()
        {
            var mdlFileIdx = 0;
            foreach (var lod in ImportedXivMdl.LoDList)
            {
                foreach (var meshData in lod.MeshDataList)
                {
                    meshData.MeshPartList = new List<MeshPart>();
                    for (int i = 0; i < meshData.MeshInfo.MeshPartCount; i++)
                    {
                        var submesh = OriginalMdlFile.Submeshes[mdlFileIdx];
                        var meshPart = new MeshPart
                        {
                            IndexOffset = (int)submesh.IndexOffset,
                            IndexCount = (int)submesh.IndexCount,
                            AttributeBitmask = submesh.AttributeIndexMask,
                            BoneStartOffset = (short)submesh.BoneStartIndex,
                            BoneCount = (short)submesh.BoneCount
                        };

                        meshData.MeshPartList.Add(meshPart);
                        mdlFileIdx++;
                    }
                }
            }
        }

        private void SetUnknownData2()
        {
            List<byte> l = new List<byte>();
            foreach (var submesh in OriginalMdlFile.TerrainShadowSubmeshes)
            {
                l.AddRange(BitConverter.GetBytes(submesh.IndexOffset));
                l.AddRange(BitConverter.GetBytes(submesh.IndexCount));
                l.AddRange(BitConverter.GetBytes(submesh.Unknown1));
                l.AddRange(BitConverter.GetBytes(submesh.Unknown2));
            }
            ImportedXivMdl.UnkData2 = new UnknownData2 { Unknown = l.ToArray() };
        }

        private void SetMaterialNameOffsets()
        {
            var matDataBlock = new MaterialDataBlock
            {
                MaterialPathOffsetList = new List<int>(ImportedXivMdl.ModelData.MaterialCount)
            };

            foreach (var offset in OriginalMdlFile.MaterialNameOffsets)
            {
                matDataBlock.MaterialPathOffsetList.Add((int)offset);
            }

            ImportedXivMdl.MatDataBlock = matDataBlock;
        }

        private void SetBoneNameOffsets()
        {
            var boneDataBlock = new BoneDataBlock
            {
                BonePathOffsetList = new List<int>(ImportedXivMdl.ModelData.BoneCount)
            };

            foreach (var offset in OriginalMdlFile.BoneNameOffsets)
            {
                boneDataBlock.BonePathOffsetList.Add((int)offset);
            }

            ImportedXivMdl.BoneDataBlock = boneDataBlock;
        }

        private void SetBoneLists()
        {
            ImportedXivMdl.MeshBoneSets = new List<BoneSet>();

            foreach (var bone in OriginalMdlFile.BoneTables)
            {
                var boneIndexMesh = new BoneSet
                {
                    BoneIndices = Array.ConvertAll(bone.BoneIndex, s => (short)s).ToList(),
                    BoneIndexCount = bone.BoneCount
                };

                ImportedXivMdl.MeshBoneSets.Add(boneIndexMesh);
            }
        }

        private void SetMeshShapeData()
        {
            var shapeDataLists = new ShapeData
            {
                ShapeInfoList = new List<ShapeData.ShapeInfo>(),
                ShapeParts = new List<ShapeData.ShapePart>(),
                ShapeDataList = new List<ShapeData.ShapeDataEntry>()
            };

            var totalPartCount = 0;

            foreach (var s in OriginalMdlFile.Shapes)
            {
                var shapeInfo = new ShapeData.ShapeInfo
                {
                    ShapeNameOffset = (int)s.StringOffset,
                    Name = OffsetToName[(int)s.StringOffset],
                    ShapeLods = new List<ShapeData.ShapeLodInfo>()
                };

                var dataInfoIndexList = new List<ushort>();
                dataInfoIndexList.AddRange(s.ShapeMeshStartIndex);

                var infoPartCountList = new List<short>();
                infoPartCountList.AddRange(Array.ConvertAll(s.ShapeMeshCount, x => (short)x));

                for (var j = 0; j < ImportedXivMdl.LoDList.Count; j++)
                {
                    var shapeIndexPart = new ShapeData.ShapeLodInfo
                    {
                        PartOffset = dataInfoIndexList[j],
                        PartCount = infoPartCountList[j]
                    };
                    shapeInfo.ShapeLods.Add(shapeIndexPart);
                    totalPartCount += shapeIndexPart.PartCount;
                }

                shapeDataLists.ShapeInfoList.Add(shapeInfo);
            }

            // Shape Index Info
            foreach (var s in OriginalMdlFile.ShapeMeshes)
            {
                var shapeIndexInfo = new ShapeData.ShapePart
                {
                    MeshIndexOffset = (int)s.StartIndex,
                    IndexCount = (int)s.ShapeValueCount,
                    ShapeDataOffset = (int)s.ShapeValueOffset
                };
                shapeDataLists.ShapeParts.Add(shapeIndexInfo);
            }


            // Shape data
            foreach (var v in OriginalMdlFile.ShapeValues)
            {
                var shapeData = new ShapeData.ShapeDataEntry
                {
                    BaseIndex = v.Offset,  // Base Triangle Index we're replacing
                    ShapeVertex = v.Value  // The Vertex that Triangle Index should now point to instead.
                };
                shapeDataLists.ShapeDataList.Add(shapeData);
            }

            ImportedXivMdl.MeshShapeData = shapeDataLists;
        }

        private void AssignMeshAndLoDNumber()
        {
            // Build the list of offsets so we can match it for shape data.
            var indexOffsets = new List<List<int>>();
            for (int l = 0; l < ImportedXivMdl.LoDList.Count; l++)
            {
                indexOffsets.Add(new List<int>());
                for (int m = 0; m < ImportedXivMdl.LoDList[l].MeshDataList.Count; m++)
                {
                    indexOffsets[l].Add(ImportedXivMdl.LoDList[l].MeshDataList[m].MeshInfo.IndexDataOffset);
                }
            }
            ImportedXivMdl.MeshShapeData.AssignMeshAndLodNumbers(indexOffsets);
        }

        private void SetBoneIndices()
        {
            var partBoneSet = new BoneSet();
            partBoneSet.BoneIndices = new List<short>();
            partBoneSet.BoneIndices = Array.ConvertAll(OriginalMdlFile.SubmeshBoneMap, x => (short)x).ToList();
            partBoneSet.BoneIndexCount = partBoneSet.BoneIndices.Count * 2; // I think?

            ImportedXivMdl.PartBoneSets = partBoneSet;
        }

        private void SetBoundingBoxes()
        {
            var boundingBox = new xivModdingFramework.Models.DataContainers.BoundingBox
            {
                PointList = new List<Vector4>()
            };

            var boxes = new List<Vector4>();
            boxes.AddRange(GetVector(OriginalMdlFile.BoundingBoxes));
            boxes.AddRange(GetVector(OriginalMdlFile.ModelBoundingBoxes));
            boxes.AddRange(GetVector(OriginalMdlFile.WaterBoundingBoxes));
            boxes.AddRange(GetVector(OriginalMdlFile.VerticalFogBoundingBoxes));

            boundingBox.PointList = boxes;

            ImportedXivMdl.BoundBox = boundingBox;
        }

        private List<Vector4> GetVector(BoundingBoxStruct s)
        {
            var ret = new List<Vector4>();
            ret.Add(new Vector4(s.Min));
            ret.Add(new Vector4(s.Max));

            return ret;
        }

        private void SetBoneTransformData()
        {
            ImportedXivMdl.BoneTransformDataList = new List<BoneTransformData>();

            var transformCount = ImportedXivMdl.ModelData.BoneCount;

            if (transformCount == 0)
            {
                transformCount = ImportedXivMdl.ModelData.Unknown8;
            }

            foreach (var box in OriginalMdlFile.BoneBoundingBoxes)
            {
                var boneTransformData = new BoneTransformData
                {
                    Transform0 = new Vector4(box.Min),
                    Transform1 = new Vector4(box.Max)
                };

                ImportedXivMdl.BoneTransformDataList.Add(boneTransformData);
            }
        }

        private void SetVertexData(bool getShapeData)
        {
            var lodNum = 0;
            var totalMeshNum = 0;

            ModelLod[] lodArr = { ModelLod.High, ModelLod.Med, ModelLod.Low };

            for (int lodIdx = 0; lodIdx < ImportedXivMdl.LoDList.Count; lodIdx++)
            {
                var lod = ImportedXivMdl.LoDList[lodIdx];

                if (lod.MeshCount == 0) continue;

                var meshDataList = lod.MeshDataList;

                if (lod.MeshOffset != totalMeshNum)
                {
                    meshDataList = ImportedXivMdl.LoDList[lodNum + 1].MeshDataList;
                }

                Model model;
                switch (lodIdx)
                {
                    case 0:
                        model = new Model(OriginalMdlFile, ModelLod.High);
                        break;
                    case 1:
                        model = new Model(OriginalMdlFile, ModelLod.Med);
                        break;
                    case 2:
                        model = new Model(OriginalMdlFile, ModelLod.Low);
                        break;
                    default:
                        throw new Exception("Unknown LoD: " + lodIdx);
                }

                var meshes = model.Meshes;

                for (int meshIdx = 0; meshIdx < meshDataList.Count; meshIdx++)
                {
                    var meshData = meshDataList[meshIdx];

                    if (ImportedXivMdl.HasShapeData && getShapeData)
                    {
                        if (meshData.ShapePathList == null)
                        {
                            meshData.ShapePathList = new List<string>();
                        }

                        foreach (var key in model.Shapes.Keys)
                        {
                            if (!meshData.ShapePathList.Contains(key))
                            {
                                meshData.ShapePathList.Add(key);
                            }
                        }
                    }

                    if (meshIdx >= meshes.Length)
                    {
                        totalMeshNum++;
                        continue;
                    }

                    if (meshIdx < meshes.Length)
                    {
                        var mesh = meshes[meshIdx];

                        meshData.VertexData = GetVertexData(mesh);
                        totalMeshNum++;
                    }
                }
                lodNum++;
            }
        }

        private VertexData GetVertexData(Mesh mesh)
        {
            var vertexData = new VertexData
            {
                Positions = GetPositions(mesh),
                BoneWeights = GetBoneWeights(mesh),
                BoneIndices = GetBoneIndicies(mesh),
                Normals = GetNormals(mesh),
                //BiNormals = new Vector3Collection(),
                //BiNormalHandedness = new List<byte>(),
                Tangents = GetTangents(mesh),
                //Colors = new List<SharpDX.Color>(),
                //Colors4 = new Color4Collection(),
                //TextureCoordinates0 = new Vector2Collection(),
                //TextureCoordinates1 = new Vector2Collection(),
                Indices = new IntCollection(Array.ConvertAll(mesh.Indices, x => (int)x))
            };

            (vertexData.BiNormals, vertexData.BiNormalHandedness) = GetBiNormals(mesh);
            (vertexData.Colors, vertexData.Colors4) = GetColors(mesh);
            (vertexData.TextureCoordinates0, vertexData.TextureCoordinates1) = GetTextureCoordinates(mesh);

            return vertexData;
        }

        private Vector3Collection GetPositions(Mesh mesh)
        {
            Vector3Collection ret = new Vector3Collection();
            /*
            foreach (var vector in mesh.Vertices)
            {
                var position = vector.Position;
                if (position != null)
                {
                    var value = position.Value;
                    var x = value.X;
                    var y = value.Y;
                    var z = value.Z;

                    var positionVector = new Vector3(x, y, z);
                    ret.Add(positionVector);
                }
            }
            */
            
            VertexDataType vType;
            if (VertexDict.TryGetValue(VertexUsageType.Position, out vType))
            {
                Vector3 positionVector;
                foreach (var vector in mesh.Vertices)
                {
                    var position = vector.Position;
                    if (position != null)
                    {
                        var value = position.Value;

                        if (vType == VertexDataType.Half4)
                        {
                            var x = new SharpDX.Half(value.X);
                            var y = new SharpDX.Half(value.Y);
                            var z = new SharpDX.Half(value.Z);
                            var w = new SharpDX.Half(value.W);

                            positionVector = new Vector3(x, y, z);
                        }
                        else
                        {
                            var x = value.X;
                            var y = value.Y;
                            var z = value.Z;

                            positionVector = new Vector3(x, y, z);
                        }
                        ret.Add(positionVector);
                    }
                }
            }
            

            return ret;
        }

        private List<float[]> GetBoneWeights(Mesh mesh)
        {
            var ret = new List<float[]>();

            foreach (var vector in mesh.Vertices)
            {
                var boneWeights = vector.BlendWeights;
                if (boneWeights != null)
                {
                    var value = boneWeights.Value;
                    var x = value.X;
                    var y = value.Y;
                    var z = value.Z;
                    var w = value.W;

                    float[] weight = { x, y, z, w };

                    ret.Add(weight);
                }
            }

            return ret;
        }

        private List<byte[]> GetBoneIndicies(Mesh mesh)
        {
            var ret = new List<byte[]>();

            foreach (var vertex in mesh.Vertices)
            {
                var indicies = vertex.BlendIndices;
                if (indicies != null)
                {
                    ret.Add(indicies);
                }
            }

            return ret;
        }

        private Vector3Collection GetNormals(Mesh mesh)
        {
            var ret = new Vector3Collection();

            foreach (var vertex in mesh.Vertices)
            {
                var vector = vertex.Normal;
                if (vector != null)
                {
                    var value = vector.Value;
                    var x = value.X;
                    var y = value.Y;
                    var z = value.Z;

                    ret.Add(new Vector3(x, y, z));
                }
            }

            return ret;
        }

        private (Vector3Collection, List<byte>) GetBiNormals(Mesh mesh)
        {
            var retVector = new Vector3Collection();
            var retByte = new List<byte>();

            foreach (var vertex in mesh.Vertices)
            {
                var vector = vertex.Tangent1;
                if (vector != null)
                {
                    var value = vector.Value;

                    /*
                     * Simplification of var x = (value.X * 255f) * 2 / 255f - f;
                     * 
                     * Apparently, the equivalent br.ReadByte = value.X * 255f
                     */

            var x = value.X * 2 - 1f;
                    var y = value.Y * 2 - 1f;
                    var z = value.Z * 2 - 1f;
                    var w = (byte)(value.W * 255f);

                    retVector.Add(new Vector3(x, y, z));
                    retByte.Add(w);
                }
            }

            return (retVector, retByte);
        }

        private Vector3Collection GetTangents(Mesh mesh)
        {
            var ret = new Vector3Collection();
            foreach (var v in mesh.Vertices)
            {
                var vector = v.Tangent2;
                if (vector != null)
                {
                    var value = vector.Value;
                    var x = value.X;
                    var y = value.Y;
                    var z = value.Z;

                    ret.Add(new Vector3(x, y, z));
                }
            }
            return ret;
        }

        private (List<Color>, Color4Collection) GetColors(Mesh mesh)
        {
            var retColor = new List<Color>();
            var retCollection = new Color4Collection();

            foreach (var vertex in mesh.Vertices)
            {
                var vector = vertex.Color;
                if (vector != null)
                {
                    var value = vector.Value;
                    var r = value.X * 255f;
                    var g = value.Y * 255f;
                    var b = value.Z * 255f;
                    var a = value.W * 255f;

                    retColor.Add(new Color(r, g, b, a));
                    retCollection.Add(new Color4(r / 255f, g / 255f, b / 255f, a / 255f));
                }
            }

            return (retColor, retCollection);
        }

        private (Vector2Collection, Vector2Collection) GetTextureCoordinates(Mesh mesh)
        {
            
            var ret0 = new Vector2Collection();
            var ret1 = new Vector2Collection();

            /*
            foreach (var vertex in mesh.Vertices)
            {
                var vector = vertex.UV;
                if (vector != null)
                {
                    var value = vector.Value;

                    var x0 = value.X;
                    var y0 = value.Y;
                    var x1 = value.Z;
                    var y1 = value.W;

                    ret0.Add(new Vector2(x0, y0));
                    ret1.Add(new Vector2(x1, y1));

                }
            }
            */
            
            VertexDataType vType;
            if (VertexDict.TryGetValue(VertexUsageType.Color, out vType))
            {
                foreach (var vertex in mesh.Vertices)
                {
                    var vector = vertex.UV;
                    if (vector != null)
                    {
                        var value = vector.Value;

                        if (vType == VertexDataType.Half4)
                        {
                            var x0 = new SharpDX.Half(value.X);
                            var y0 = new SharpDX.Half(value.Y);
                            var x1 = new SharpDX.Half(value.Z);
                            var y1 = new SharpDX.Half(value.W);
                            ret0.Add(new Vector2(x0, y0));
                            ret1.Add(new Vector2(x1, y1));
                        }
                        else if (vType == VertexDataType.Half2)
                        {
                            var x = new SharpDX.Half(value.X);
                            var y = new SharpDX.Half(value.Y);
                            ret0.Add(new Vector2(x, y));
                        }
                        else if (vType == VertexDataType.Float2)
                        {
                            var x = value.X;
                            var y = value.Y;
                            ret0.Add(new Vector2(x, y));
                        }
                        else if (vType == VertexDataType.Float4)
                        {
                            var x0 = value.X;
                            var y0 = value.Y;
                            var x1 = value.Z;
                            var y1 = value.W;
                            ret0.Add(new Vector2(x0, y0));
                            ret1.Add(new Vector2(x1, y1));
                        }
                        else
                        {
                            var x0 = value.X;
                            var y0 = value.Y;
                            var x1 = value.Z;
                            var y1 = value.W;
                            ret0.Add(new Vector2(x0, y0));
                            ret1.Add(new Vector2(x1, y1));
                        }
                        
                    }
                }
            }

            return (ret0, ret1);
        }

        private static readonly Dictionary<byte, VertexDataType> VertexTypeDictionary =
    new Dictionary<byte, VertexDataType>
    {
                {0x0, VertexDataType.Float1},
                {0x1, VertexDataType.Float2},
                {0x2, VertexDataType.Float3},
                {0x3, VertexDataType.Float4},
                {0x5, VertexDataType.Ubyte4},
                {0x6, VertexDataType.Short2},
                {0x7, VertexDataType.Short4},
                {0x8, VertexDataType.Ubyte4n},
                {0x9, VertexDataType.Short2n},
                {0xA, VertexDataType.Short4n},
                {0xD, VertexDataType.Half2},
                {0xE, VertexDataType.Half4},
                {0xF, VertexDataType.Compress}
    };

        private static readonly Dictionary<byte, VertexUsageType> VertexUsageDictionary =
            new Dictionary<byte, VertexUsageType>
            {
                {0x0, VertexUsageType.Position },
                {0x1, VertexUsageType.BoneWeight },
                {0x2, VertexUsageType.BoneIndex },
                {0x3, VertexUsageType.Normal },
                {0x4, VertexUsageType.TextureCoordinate },
                {0x5, VertexUsageType.Tangent },
                {0x6, VertexUsageType.Binormal },
                {0x7, VertexUsageType.Color }
            };
    }
}
