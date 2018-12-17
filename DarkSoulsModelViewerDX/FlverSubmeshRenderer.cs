using DarkSoulsModelViewerDX.GFXShaders;
using MeowDSIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using MeowDSIO.DataTypes.FLVER;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkSoulsModelViewerDX
{
    public class FlverSubmeshRenderer : IDisposable
    {
        public BoundingBox Bounds;

        private struct FlverSubmeshRendererFaceSet
        {
            public int IndexCount;
            public IndexBuffer IndexBuffer;
            public bool BackfaceCulling;
            public bool IsTriangleStrip;
            public byte LOD;
        }

        List<FlverSubmeshRendererFaceSet> MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

        private bool HasNoLODs = true;

        VertexBuffer VertBuffer;
        VertexBufferBinding VertBufferBinding;

        public string TexNameDiffuse { get; private set; } = null;
        public string TexNameSpecular { get; private set; } = null;
        public string TexNameNormal { get; private set; } = null;
        public string TexNameDOL1 { get; private set; } = null;
        public string TexNameDOL2 { get; private set; } = null;

        public Texture2D TexDataDiffuse { get; private set; } = null;
        public Texture2D TexDataSpecular { get; private set; } = null;
        public Texture2D TexDataNormal { get; private set; } = null;
        public Texture2D TexDataDOL1 { get; private set; } = null;
        public Texture2D TexDataDOL2 { get; private set; } = null;

        public GFXDrawStep DrawStep { get; private set; }

        public int VertexCount { get; private set; }

        public readonly Model Parent;

        public FlverSubmeshRenderer(Model parent, FLVER flvr, FLVER.Mesh mesh)
        {
            Parent = parent;

            var shortMaterialName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(flvr.Materials[mesh.MaterialIndex].MTD);
            if (shortMaterialName.EndsWith("_Alp") ||
                shortMaterialName.Contains("_Edge") ||
                shortMaterialName.Contains("_Decal") ||
                shortMaterialName.Contains("_Cloth") ||
                shortMaterialName.Contains("_al") ||
                shortMaterialName.Contains("BlendOpacity"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            foreach (var matParam in flvr.Materials[mesh.MaterialIndex].Textures)
            {
                var paramNameCheck = matParam.Type.ToUpper();
                // DS3/BB
                if (paramNameCheck == "G_DIFFUSETEXTURE")
                    TexNameDiffuse = matParam.Path;
                else if (paramNameCheck == "G_SPECULARTEXTURE")
                    TexNameSpecular = matParam.Path;
                else if (paramNameCheck == "G_BUMPMAPTEXTURE")
                    TexNameNormal = matParam.Path;
                else if (paramNameCheck == "G_DOLTEXTURE1")
                    TexNameDOL1 = matParam.Path;
                else if (paramNameCheck == "G_DOLTEXTURE2")
                    TexNameDOL2 = matParam.Path;
                // DS1 params
                else if (paramNameCheck == "G_DIFFUSE")
                    TexNameDiffuse = matParam.Path;
                else if (paramNameCheck == "G_SPECULAR")
                    TexNameSpecular = matParam.Path;
                else if (paramNameCheck == "G_BUMPMAP")
                    TexNameNormal = matParam.Path;
            }

            var MeshVertices = new VertexPositionColorNormalTangentTexture[mesh.Vertices.Count];
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vert = mesh.Vertices[i];
                MeshVertices[i] = new VertexPositionColorNormalTangentTexture();

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                if (vert.Normal != null && vert.Tangents != null && vert.Tangents.Count > 0)
                {
                    MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));
                    MeshVertices[i].Tangent = Vector3.Normalize(new Vector3(vert.Tangents[0].X, vert.Tangents[0].Y, vert.Tangents[0].Z));
                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), Vector3.Normalize(MeshVertices[i].Tangent)) * vert.Tangents[0].W;
                }

                if (vert.UVs.Count > 0)
                {
                    MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[0].X, vert.UVs[0].Y);
                    if (vert.UVs.Count > 1)
                    {
                        // Really stupid heuristic to determine light map UVs without reading mtd files or something
                        if (vert.UVs.Count > 2 && flvr.Materials[mesh.MaterialIndex].Textures.Count > 11)
                        {
                            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[2].X, vert.UVs[2].Y);
                        }
                        else
                        {
                            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                        }
                    }
                    else
                    {
                        MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                    }
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                    MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                }
            }

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            foreach (var faceset in mesh.FaceSets)
            {
                bool is32bit = faceset.IndexSize == 0x20;

                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = faceset.CullBackfaces,
                    IsTriangleStrip = faceset.TriangleStrip,
                    IndexBuffer = new IndexBuffer(
                                GFX.Device,
                                is32bit ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits,
                                faceset.Vertices.Length,
                                BufferUsage.WriteOnly),
                    IndexCount = faceset.Vertices.Length,
                };

                if (faceset.Flags == FLVER.FaceSet.FSFlags.LodLevel1)
                {
                    newFaceSet.LOD = 1;
                    HasNoLODs = false;
                }
                else if (faceset.Flags == FLVER.FaceSet.FSFlags.LodLevel2)
                {
                    newFaceSet.LOD = 2;
                    HasNoLODs = false;
                }

                if (is32bit)
                {
                    newFaceSet.IndexBuffer.SetData(faceset.Vertices);
                }
                else
                {
                    newFaceSet.IndexBuffer.SetData(faceset.Vertices.Select(x => (ushort)x).ToArray());
                }

                MeshFacesets.Add(newFaceSet);
            }

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);

            VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);

            TryToLoadTextures();
        }

        public FlverSubmeshRenderer(Model parent, FLVERD flvr, FLVERD.Mesh mesh)
        {
            Parent = parent;

            var shortMaterialName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(flvr.Materials[mesh.MaterialIndex].MTD);
            if (shortMaterialName.EndsWith("_Alp") ||
                shortMaterialName.Contains("_Edge") ||
                shortMaterialName.Contains("_Decal") ||
                shortMaterialName.Contains("_Cloth") ||
                shortMaterialName.Contains("_al") ||
                shortMaterialName.Contains("BlendOpacity"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            foreach (var matParam in flvr.Materials[mesh.MaterialIndex].Textures)
            {
                if (matParam == null || matParam.Type == null)
                {
                    break;
                }
                var paramNameCheck = matParam.Type.ToUpper();
                // DS3/BB
                if (paramNameCheck == "G_DIFFUSETEXTURE")
                    TexNameDiffuse = matParam.Path;
                else if (paramNameCheck == "G_SPECULARTEXTURE")
                    TexNameSpecular = matParam.Path;
                else if (paramNameCheck == "G_BUMPMAPTEXTURE")
                    TexNameNormal = matParam.Path;
                else if (paramNameCheck == "G_DOLTEXTURE1")
                    TexNameDOL1 = matParam.Path;
                else if (paramNameCheck == "G_DOLTEXTURE2")
                    TexNameDOL2 = matParam.Path;
                // DS1 params
                else if (paramNameCheck == "G_DIFFUSE")
                    TexNameDiffuse = matParam.Path;
                else if (paramNameCheck == "G_SPECULAR")
                    TexNameSpecular = matParam.Path;
                else if (paramNameCheck == "G_BUMPMAP")
                    TexNameNormal = matParam.Path;
                else if (paramNameCheck == "G_LIGHTMAP")
                    TexNameDOL1 = matParam.Path;
            }

            var MeshVertices = new VertexPositionColorNormalTangentTexture[mesh.Vertices.Count];
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vert = mesh.Vertices[i];
                MeshVertices[i] = new VertexPositionColorNormalTangentTexture();

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                if (vert.Normal != null && vert.Tangents != null && vert.Tangents.Count > 0)
                {
                    MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));
                    MeshVertices[i].Tangent = Vector3.Normalize(new Vector3(vert.Tangents[0].X, vert.Tangents[0].Y, vert.Tangents[0].Z));
                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), Vector3.Normalize(MeshVertices[i].Tangent)) * vert.Tangents[0].W;
                }

                if (vert.UVs.Count > 0)
                {
                    MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[0].X, vert.UVs[0].Y);
                    if (vert.UVs.Count > 1)
                    {
                        MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    }
                    else
                    {
                        MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                    }
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                    MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                }
            }

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            var tlist = mesh.ToTriangleList();
            var newFaceSet = new FlverSubmeshRendererFaceSet()
            {
                BackfaceCulling = true,
                IsTriangleStrip = false,
                IndexBuffer = new IndexBuffer(
                            GFX.Device,
                            IndexElementSize.SixteenBits,
                            tlist.Length,
                            BufferUsage.WriteOnly),
                IndexCount = tlist.Length,
            };

            newFaceSet.IndexBuffer.SetData(tlist);

            MeshFacesets.Add(newFaceSet);

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);

            VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);

            TryToLoadTextures();
        }

        // Used for collision rendering
        public FlverSubmeshRenderer(Model parent, HKX colhkx, HKX.FSNPCustomParamCompressedMeshShape meshdata)
        {
            Parent = parent;

            var coldata = meshdata.GetMeshShapeData();
            var vertices = new VertexPositionColorNormalTangentTexture[coldata.SmallVertices.Size + coldata.LargeVertices.Size];
            /*for (int i = 0; i < coldata.SmallVertices.Size; i++)
            {
                var vert = coldata.SmallVertices.GetArrayData().Elements[i].Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                vertices[i] = new VertexPositionColorNormalTangentTexture();
                vertices[i].Position = new Vector3(vert.X, vert.Y, vert.Z);
            }*/

            var largebase = coldata.SmallVertices.Size;
            for (int i = 0; i < coldata.LargeVertices.Size; i++)
            {
                var vert = coldata.LargeVertices.GetArrayData().Elements[i].Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                vertices[i + largebase] = new VertexPositionColorNormalTangentTexture();
                vertices[i + largebase].Position = new Vector3(vert.X, vert.Y, vert.Z);
            }

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();
            int ch = 0;
            foreach (var chunk in coldata.Chunks.GetArrayData().Elements)
            {
                /*if (ch != 1)
                {
                    ch++;
                    continue;
                }
                ch++;*/
                List<ushort> indices = new List<ushort>();
                for (int i = 0; i < chunk.ByteIndicesLength; i++)
                {
                    var tri = coldata.MeshIndices.GetArrayData().Elements[i + chunk.ByteIndicesIndex];
                    if (tri.Idx2 == tri.Idx3 && tri.Idx1 != tri.Idx2)
                    {
                        if (tri.Idx0 < chunk.VertexIndicesLength)
                        {
                            ushort index = (ushort)((uint)tri.Idx0 + chunk.SmallVerticesBase);
                            indices.Add(index);

                            var vert = coldata.SmallVertices.GetArrayData().Elements[index].Decompress(chunk.SmallVertexScale, chunk.SmallVertexOffset);
                            vertices[index] = new VertexPositionColorNormalTangentTexture();
                            vertices[index].Position = new Vector3(vert.X, vert.Y, vert.Z);
                        }
                        else
                        {
                            indices.Add((ushort)(coldata.VertexIndices.GetArrayData().Elements[tri.Idx0 + chunk.VertexIndicesIndex - chunk.VertexIndicesLength].data + largebase));
                        }

                        if (tri.Idx1 < chunk.VertexIndicesLength)
                        {
                            ushort index = (ushort)((uint)tri.Idx1 + chunk.SmallVerticesBase);
                            indices.Add(index);

                            var vert = coldata.SmallVertices.GetArrayData().Elements[index].Decompress(chunk.SmallVertexScale, chunk.SmallVertexOffset);
                            vertices[index] = new VertexPositionColorNormalTangentTexture();
                            vertices[index].Position = new Vector3(vert.X, vert.Y, vert.Z);
                        }
                        else
                        {
                            indices.Add((ushort)(coldata.VertexIndices.GetArrayData().Elements[tri.Idx1 + chunk.VertexIndicesIndex - chunk.VertexIndicesLength].data + largebase));
                        }

                        if (tri.Idx2 < chunk.VertexIndicesLength)
                        {
                            ushort index = (ushort)((uint)tri.Idx2 + chunk.SmallVerticesBase);
                            indices.Add(index);

                            var vert = coldata.SmallVertices.GetArrayData().Elements[index].Decompress(chunk.SmallVertexScale, chunk.SmallVertexOffset);
                            vertices[index] = new VertexPositionColorNormalTangentTexture();
                            vertices[index].Position = new Vector3(vert.X, vert.Y, vert.Z);
                        }
                        else
                        {
                            indices.Add((ushort)(coldata.VertexIndices.GetArrayData().Elements[tri.Idx2 + chunk.VertexIndicesIndex - chunk.VertexIndicesLength].data + largebase));
                        }
                    }
                }

                if (indices.Count > 0)
                {
                    var newFaceSet = new FlverSubmeshRendererFaceSet()
                    {
                        BackfaceCulling = false,
                        IsTriangleStrip = false,
                        IndexBuffer = new IndexBuffer(
                            GFX.Device,
                            IndexElementSize.SixteenBits,
                            indices.Count,
                            BufferUsage.WriteOnly),
                        IndexCount = indices.Count,
                    };

                    newFaceSet.IndexBuffer.SetData(indices.Select(x => (ushort)x).ToArray());

                    MeshFacesets.Add(newFaceSet);
                }
            }

            Bounds = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), vertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(vertices);

            VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);
        }

        public FlverSubmeshRenderer(Model parent, HKX colhkx, HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage meshdata)
        {
            Parent = parent;

            var vertices = new VertexPositionColorNormalTangentTexture[(meshdata.Indices16.Size / 4) * 3];

            //for (int i = 0; i < meshdata.Vertices.Size; i++)
            //{
            //    var vert = meshdata.Vertices.GetArrayData().Elements[i];
            //    vertices[i] = new VertexPositionColorNormalTangentTexture();
            //    vertices[i].Position = new Vector3(vert.Vector.X, vert.Vector.Y, vert.Vector.Z);
            //}

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();
            List<ushort> indices = new List<ushort>();
            int j = 0;
            for (var index = 0; index < meshdata.Indices16.Size / 4; index++)
            {
                var idx = meshdata.Indices16.GetArrayData().Elements;
                var vtxs = meshdata.Vertices.GetArrayData().Elements;

                var vert1 = vtxs[idx[index * 4].data].Vector;
                var vert2 = vtxs[idx[index * 4 + 1].data].Vector;
                var vert3 = vtxs[idx[index * 4 + 2].data].Vector;

                vertices[index * 3].Position = new Vector3(vert1.X, vert1.Y, vert1.Z);
                vertices[index * 3 + 1].Position = new Vector3(vert2.X, vert2.Y, vert2.Z);
                vertices[index * 3 + 2].Position = new Vector3(vert3.X, vert3.Y, vert3.Z);

                Vector3 a = new Vector3(vert2.X - vert1.X, vert2.Y - vert1.Y, vert2.Z - vert1.Z);
                Vector3 b = new Vector3(vert3.X - vert1.X, vert3.Y - vert1.Y, vert3.Z - vert1.Z);

                Vector3 normal = Vector3.Cross(a, b);
                normal.Normalize();

                vertices[index * 3].Normal = normal;
                vertices[index * 3 + 1].Normal = normal;
                vertices[index * 3 + 2].Normal = normal;

                a.Normalize();
                vertices[index * 3].Tangent = a;
                vertices[index * 3 + 1].Tangent = a;
                vertices[index * 3 + 2].Tangent = a;

                vertices[index * 3].Binormal = Vector3.Cross(normal, a);
                vertices[index * 3 + 1].Binormal = Vector3.Cross(normal, a);
                vertices[index * 3 + 2].Binormal = Vector3.Cross(normal, a);

                indices.Add((ushort)(index * 3));
                indices.Add((ushort)(index * 3 + 1));
                indices.Add((ushort)(index * 3 + 2));
            }

            if (indices.Count > 0)
            {
                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = false,
                    IsTriangleStrip = false,
                    IndexBuffer = new IndexBuffer(
                        GFX.Device,
                        IndexElementSize.SixteenBits,
                        indices.Count,
                        BufferUsage.WriteOnly),
                    IndexCount = indices.Count,
                };

                newFaceSet.IndexBuffer.SetData(indices.Select(x => (ushort)x).ToArray());

                MeshFacesets.Add(newFaceSet);

            }
            else
            {
                vertices = new VertexPositionColorNormalTangentTexture[meshdata.Vertices.Size];

                for (int i = 0; i < meshdata.Vertices.Size; i++)
                {
                    var vert = meshdata.Vertices.GetArrayData().Elements[i];
                    vertices[i] = new VertexPositionColorNormalTangentTexture();
                    vertices[i].Position = new Vector3(vert.Vector.X, vert.Vector.Y, vert.Vector.Z);
                }
            }

            Bounds = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), vertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(vertices);

            VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);
        }

        public void TryToLoadTextures()
        {
            if (TexDataDiffuse == null && TexNameDiffuse != null)
                TexDataDiffuse = TexturePool.FetchTexture(TexNameDiffuse);

            if (TexDataSpecular == null && TexNameSpecular != null)
                TexDataSpecular = TexturePool.FetchTexture(TexNameSpecular);

            if (TexDataNormal == null && TexNameNormal != null)
                TexDataNormal = TexturePool.FetchTexture(TexNameNormal);

            if (TexDataDOL1 == null && TexNameDOL1 != null)
            {
                TexDataDOL1 = TexturePool.FetchTexture(TexNameDOL1);
                if (TexDataDOL1 == null)
                {
                    Console.WriteLine("Failed to load lightmap: " + TexNameDOL1);
                }
            }

            if (TexDataDOL2 == null && TexNameDOL2 != null)
                TexDataDOL2 = TexturePool.FetchTexture(TexNameDOL2);
        }

        public void Draw<T>(int lod, IGFXShader<T> shader, bool forceNoBackfaceCulling = false)
            where T : Effect
        {
            if (GFX.EnableTextures && shader == GFX.FlverShader)
            {
                GFX.FlverShader.Effect.ColorMap = TexDataDiffuse ?? Main.DEFAULT_TEXTURE_DIFFUSE;
                GFX.FlverShader.Effect.SpecularMap = TexDataSpecular ?? Main.DEFAULT_TEXTURE_SPECULAR;
                GFX.FlverShader.Effect.NormalMap = TexDataNormal ?? Main.DEFAULT_TEXTURE_NORMAL;
                GFX.FlverShader.Effect.LightMap1 = TexDataDOL1 ?? Main.DEFAULT_TEXTURE_DIFFUSE;
                //GFX.FlverShader.Effect.LightMap2 = TexDataDOL2 ?? Main.DEFAULT_TEXTURE_DIFFUSE;
            }

            GFX.Device.SetVertexBuffers(VertBufferBinding, Parent.InstanceBufferBinding);

            //foreach (var technique in shader.Effect.Techniques)
            //{
            //    shader.Effect.CurrentTechnique = technique;
            foreach (EffectPass pass in shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var faceSet in MeshFacesets)
                {
                    if (!HasNoLODs && faceSet.LOD != lod)
                        continue;

                    GFX.Device.Indices = faceSet.IndexBuffer;

                    GFX.BackfaceCulling = forceNoBackfaceCulling ? false : faceSet.BackfaceCulling;

                    GFX.Device.DrawInstancedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                        faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3), Parent.InstanceCount);

                }
            }
            //}
        }

        public void Dispose()
        {
            for (int i = 0; i < MeshFacesets.Count; i++)
            {
                MeshFacesets[i].IndexBuffer.Dispose();
            }

            MeshFacesets = null;

            VertBuffer.Dispose();

            // Just leave the texture data as-is, since 
            // TexturePool handles memory cleanup


            //TexDataDiffuse?.Dispose();
            TexDataDiffuse = null;
            TexNameDiffuse = null;

            //TexDataNormal?.Dispose();
            TexDataNormal = null;
            TexNameNormal = null;

            //TexDataSpecular?.Dispose();
            TexDataSpecular = null;
            TexNameSpecular = null;

            TexDataDOL1 = null;
            TexNameDOL1 = null;

            TexDataDOL2 = null;
            TexNameDOL2 = null;
        }
    }
}
