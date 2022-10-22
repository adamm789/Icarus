using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Util.Extensions
{
    public static class TTModelExtensions
    {
        public static float GetModelSize(this TTModel model)
        {
            float minX = 9999.0f, minY = 9999.0f, minZ = 9999.0f;
            float maxX = -9999.0f, maxY = -9999.0f, maxZ = -9999.0f;
            foreach (var m in model.MeshGroups)
            {
                foreach (var p in m.Parts)
                {
                    foreach (var v in p.Vertices)
                    {
                        minX = minX < v.Position.X ? minX : v.Position.X;
                        minY = minY < v.Position.Y ? minY : v.Position.Y;
                        minZ = minZ < v.Position.Z ? minZ : v.Position.Z;

                        maxX = maxX > v.Position.X ? maxX : v.Position.X;
                        maxY = maxY > v.Position.Y ? maxY : v.Position.Y;
                        maxZ = maxZ > v.Position.Z ? maxZ : v.Position.Z;
                    }
                }
            }

            Vector3 min = new Vector3(minX, minY, minZ);
            Vector3 max = new Vector3(maxX, maxY, maxZ);

            return Vector3.Distance(min, max);
        }

        public static TTVertex DeepCopy(this TTVertex vertex)
        {
            var clone = new TTVertex();

            clone.Position = new(vertex.Position.ToArray());
            clone.Normal = new(vertex.Normal.ToArray());
            clone.Binormal = new(vertex.Binormal.ToArray());
            clone.Tangent = new(vertex.Tangent.ToArray());

            clone.Handedness = vertex.Handedness;

            clone.UV1 = new(vertex.UV1.ToArray());
            clone.UV2 = new(vertex.UV2.ToArray());

            clone.VertexColor = new byte[4];
            clone.BoneIds = new byte[4];
            clone.Weights = new byte[4];

            Array.Copy(vertex.BoneIds, 0, clone.BoneIds, 0, 4);
            Array.Copy(vertex.Weights, 0, clone.Weights, 0, 4);
            Array.Copy(vertex.VertexColor, 0, clone.VertexColor, 0, 4);

            return clone;
        }


        // TODO: DeepCopy() here or TTMeshPart.Clone() ?
        public static TTMeshPart DeepCopy(this TTMeshPart part)
        {
            var copy = new TTMeshPart();
            copy.Name = part.Name;

            foreach(var vertex in part.Vertices)
            {
                copy.Vertices.Add(vertex.DeepCopy());
            }

            foreach(var index in part.TriangleIndices)
            {
                copy.TriangleIndices.Add(index);
            }

            foreach (var attr in part.Attributes)
            {
                copy.Attributes.Add(attr);
            }

            foreach (var kvp in part.ShapeParts)
            {
                copy.ShapeParts.Add(kvp.Key, kvp.Value.DeepCopy());
            }

            return copy;
        }

        public static TTShapePart DeepCopy(this TTShapePart part)
        {
            var copy = new TTShapePart();
            copy.Name = part.Name;
            
            foreach (var vertex in part.Vertices)
            {
                copy.Vertices.Add(vertex.DeepCopy());
            }

            foreach(var kvp in part.VertexReplacements)
            {
                copy.VertexReplacements.Add(kvp.Key, kvp.Value);
            }

            return copy;
        }

        public static TTMeshGroup DeepCopy(this TTMeshGroup group)
        {
            var copy = new TTMeshGroup();

            foreach (var part in group.Parts)
            {
                copy.Parts.Add(part.DeepCopy());
            }

            copy.Material = group.Material;
            copy.Name = group.Name;

            foreach(var bone in group.Bones)
            {
                copy.Bones.Add(bone);
            }

            return copy;
        }

        public static TTModel DeepCopy(this TTModel model)
        {
            var copy = new TTModel();
            copy.Source = model.Source;

            foreach (var group in model.MeshGroups)
            {
                copy.MeshGroups.Add(group.DeepCopy());
            }

            foreach(var shape in model.ActiveShapes)
            {
                copy.ActiveShapes.Add(shape);
            }

            return copy;
        }
    }
}
