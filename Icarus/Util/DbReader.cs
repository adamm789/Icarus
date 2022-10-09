using System;
using System.Data.SQLite;
using xivModdingFramework.Cache;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Helpers;
using static xivModdingFramework.Cache.XivCache;

namespace Icarus
{
    [Obsolete]
    internal static partial class DbReader
    {
        /// <summary>
        /// Loads a TTModel file from a given SQLite3 DB filepath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        // https://github.com/TexTools/xivModdingFramework/blob/81c234e7b767d56665185e07aabeeae21d895f0b/xivModdingFramework/Models/DataContainers/TTModel.cs#L974
        public static TTModel LoadFromFile(string filePath)
        {
            // TODO: Read Materials from db?
            // Maybe not, because they could be literally anything?
            // Perhaps save them, having an option to import them?
            var connectionString = "Data Source=" + filePath + ";Pooling=False;";
            var model = new TTModel();
            model.Source = filePath;

            // Spawn a DB connection to do the raw queries.
            using (var db = new SQLiteConnection(connectionString))
            {
                db.Open();
                // Using statements help ensure we don't accidentally leave any connections open and lock the file handle.

                // Load Mesh Groups
                LoadMeshGroups(model, db);

                // Load Mesh Parts
                LoadMeshParts(model, db);

                // Load Bones
                LoadBones(model, db);

                // Loop for each part, to populate their internal data structures.
                PopulateInternalDataStructures(connectionString, model);

                // Load Shape Verts
                LoadShapeVerts(model, db);
            }

            ModelModifiers.MakeImportReady(model);

            return model;
        }

        private static void LoadMeshGroups(TTModel model, SQLiteConnection db)
        {
            var query = "select * from meshes order by mesh asc;";
            using (var cmd = new SQLiteCommand(query, db))
            {
                using (var reader = new CacheReader(cmd.ExecuteReader()))
                {
                    while (reader.NextRow())
                    {
                        var meshNum = reader.GetInt32("mesh");

                        // Spawn mesh groups as needed.
                        while (model.MeshGroups.Count <= meshNum)
                        {
                            model.MeshGroups.Add(new TTMeshGroup());
                        }

                        model.MeshGroups[meshNum].Name = reader.GetString("name");
                    }
                }
            }
        }

        private static void LoadMeshParts(TTModel model, SQLiteConnection db)
        {
            string query = "select * from parts order by mesh asc, part asc;";
            using (var cmd = new SQLiteCommand(query, db))
            {
                using (var reader = new CacheReader(cmd.ExecuteReader()))
                {
                    while (reader.NextRow())
                    {
                        var meshNum = reader.GetInt32("mesh");
                        var partNum = reader.GetInt32("part");

                        // Spawn mesh groups if needed.
                        while (model.MeshGroups.Count <= meshNum)
                        {
                            model.MeshGroups.Add(new TTMeshGroup());
                        }

                        // Spawn parts as needed.
                        while (model.MeshGroups[meshNum].Parts.Count <= partNum)
                        {
                            model.MeshGroups[meshNum].Parts.Add(new TTMeshPart());

                        }

                        model.MeshGroups[meshNum].Parts[partNum].Name = reader.GetString("name");
                    }
                }
            }
        }

        private static void LoadBones(TTModel model, SQLiteConnection db)
        {
            string query = "select * from bones where mesh >= 0 order by mesh asc, bone_id asc;";
            using (var cmd = new SQLiteCommand(query, db))
            {
                using (var reader = new CacheReader(cmd.ExecuteReader()))
                {
                    while (reader.NextRow())
                    {
                        var meshId = reader.GetInt32("mesh");
                        model.MeshGroups[meshId].Bones.Add(reader.GetString("name"));
                    }
                }
            }
        }

        private static void LoadShapeVerts(TTModel model, SQLiteConnection db)
        {
            var query = "select * from shape_vertices order by shape asc, mesh asc, part asc, vertex_id asc;";
            using (var cmd = new SQLiteCommand(query, db))
            {
                using (var reader = new CacheReader(cmd.ExecuteReader()))
                {
                    while (reader.NextRow())
                    {
                        var shapeName = reader.GetString("shape");
                        var meshNum = reader.GetInt32("mesh");
                        var partNum = reader.GetInt32("part");
                        var vertexId = reader.GetInt32("vertex_id");

                        var part = model.MeshGroups[meshNum].Parts[partNum];
                        // Copy the original vertex and update position.
                        TTVertex vertex = (TTVertex)part.Vertices[vertexId].Clone();
                        vertex.Position.X = reader.GetFloat("position_x");
                        vertex.Position.Y = reader.GetFloat("position_y");
                        vertex.Position.Z = reader.GetFloat("position_z");

                        var repVert = part.Vertices[vertexId];
                        if (repVert.Position.Equals(vertex.Position))
                        {
                            // Skip morphology which doesn't actually change anything.
                            continue;
                        }

                        if (!part.ShapeParts.ContainsKey(shapeName))
                        {
                            var shpPt = new TTShapePart();
                            shpPt.Name = shapeName;
                            part.ShapeParts.Add(shapeName, shpPt);
                        }


                        part.ShapeParts[shapeName].VertexReplacements.Add(vertexId, part.ShapeParts[shapeName].Vertices.Count);
                        part.ShapeParts[shapeName].Vertices.Add(vertex);

                    }
                }
            }
        }

        private static void PopulateInternalDataStructures(string connectionString, TTModel model)
        {
            for (var mId = 0; mId < model.MeshGroups.Count; mId++)
            {
                var m = model.MeshGroups[mId];
                for (var pId = 0; pId < m.Parts.Count; pId++)
                {
                    var p = m.Parts[pId];
                    var where = new WhereClause();
                    var mWhere = new WhereClause();
                    mWhere.Column = "mesh";
                    mWhere.Value = mId;
                    var pWhere = new WhereClause();
                    pWhere.Column = "part";
                    pWhere.Value = pId;

                    where.Inner.Add(mWhere);
                    where.Inner.Add(pWhere);

                    // Load Vertices
                    // The reader handles coalescing the null types for us.
                    p.Vertices = XivCache.BuildListFromTable(connectionString, "vertices", where, async (reader) =>
                    {
                        var vertex = new TTVertex();

                        // Positions
                        vertex.Position.X = reader.GetFloat("position_x");
                        vertex.Position.Y = reader.GetFloat("position_y");
                        vertex.Position.Z = reader.GetFloat("position_z");

                        // Normals
                        vertex.Normal.X = reader.GetFloat("normal_x");
                        vertex.Normal.Y = reader.GetFloat("normal_y");
                        vertex.Normal.Z = reader.GetFloat("normal_z");

                        // Vertex Colors - Vertex color is RGBA
                        vertex.VertexColor[0] = (byte)Math.Round(reader.GetFloat("color_r") * 255);
                        vertex.VertexColor[1] = (byte)Math.Round(reader.GetFloat("color_g") * 255);
                        vertex.VertexColor[2] = (byte)Math.Round(reader.GetFloat("color_b") * 255);
                        vertex.VertexColor[3] = (byte)Math.Round(reader.GetFloat("color_a") * 255);

                        // UV Coordinates
                        vertex.UV1.X = reader.GetFloat("uv_1_u");
                        vertex.UV1.Y = reader.GetFloat("uv_1_v");
                        vertex.UV2.X = reader.GetFloat("uv_2_u");
                        vertex.UV2.Y = reader.GetFloat("uv_2_v");

                        // Bone Ids
                        vertex.BoneIds[0] = reader.GetByte("bone_1_id");
                        vertex.BoneIds[1] = reader.GetByte("bone_2_id");
                        vertex.BoneIds[2] = reader.GetByte("bone_3_id");
                        vertex.BoneIds[3] = reader.GetByte("bone_4_id");

                        // Weights
                        vertex.Weights[0] = (byte)Math.Round(reader.GetFloat("bone_1_weight") * 255);
                        vertex.Weights[1] = (byte)Math.Round(reader.GetFloat("bone_2_weight") * 255);
                        vertex.Weights[2] = (byte)Math.Round(reader.GetFloat("bone_3_weight") * 255);
                        vertex.Weights[3] = (byte)Math.Round(reader.GetFloat("bone_4_weight") * 255);

                        return vertex;
                    }).GetAwaiter().GetResult();

                    p.TriangleIndices = XivCache.BuildListFromTable(connectionString, "indices", where, async (reader) =>
                    {
                        try
                        {
                            return reader.GetInt32("vertex_id");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }).GetAwaiter().GetResult();
                }
            }
        }
    }
}
