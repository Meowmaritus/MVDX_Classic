using DarkSoulsModelViewerDX.GFXShaders;
using MeowDSIO;
//using MeowDSIO.DataTypes.FLVER;
using SoulsFormats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string TexNameDiffuse { get; private set; } = null;
        public string TexNameSpecular { get; private set; } = null;
        public string TexNameNormal { get; private set; } = null;

        public Texture2D TexDataDiffuse { get; private set; } = null;
        public Texture2D TexDataSpecular { get; private set; } = null;
        public Texture2D TexDataNormal { get; private set; } = null;

        public GFXDrawStep DrawStep { get; private set; }

        public int VertexCount { get; private set; }

        /*public FlverSubmeshRenderer(FlverSubmesh f)
        {
            var shortMaterialName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(f.Material.MTDName);
            if (shortMaterialName.EndsWith("_Alp") || shortMaterialName.EndsWith("_Edge"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            foreach (var matParam in f.Material.Parameters)
            {
                if (matParam.Name.ToUpper() == "G_DIFFUSE")
                    TexNameDiffuse = matParam.Value;
                else if (matParam.Name.ToUpper() == "G_SPECULAR")
                    TexNameSpecular = matParam.Value;
                else if (matParam.Name.ToUpper() == "G_BUMPMAP")
                    TexNameNormal = matParam.Value;
            }

            var MeshVertices = new VertexPositionColorNormalTangentTexture[f.Vertices.Count];
            for (int i = 0; i < f.Vertices.Count; i++)
            {
                var vert = f.Vertices[i];
                MeshVertices[i] = new VertexPositionColorNormalTangentTexture();

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                if (vert.Normal != null && vert.BiTangent != null)
                {
                    MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));
                    MeshVertices[i].Tangent = Vector3.Normalize(new Vector3(vert.BiTangent.X, vert.BiTangent.Y, vert.BiTangent.Z));
                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), Vector3.Normalize(MeshVertices[i].Tangent)) * vert.BiTangent.W;
                }

                if (vert.UVs.Count > 0)
                {
                    MeshVertices[i].TextureCoordinate = vert.UVs[0];
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                }

                // We set the mesh's vertex color to that of a selected mesh.
                // The shader with lighting ignores this so it will only show
                // up on the primitive shader, which is what is used to draw
                // the currently highlighted map piece
                MeshVertices[i].Color = Main.SELECTED_MESH_COLOR.ToVector4();
            }

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            foreach (var faceset in f.FaceSets)
            {
                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = faceset.CullBackfaces,
                    IsTriangleStrip = faceset.IsTriangleStrip,
                    IndexBuffer = new IndexBuffer(
                                GFX.Device,
                                IndexElementSize.SixteenBits,
                                sizeof(short) * faceset.VertexIndices.Count,
                                BufferUsage.None),
                    IndexCount = faceset.VertexIndices.Count,
                };

                if (faceset.FlagsLOD1)
                {
                    newFaceSet.LOD = (byte)1;
                    HasNoLODs = false;
                }
                else if (faceset.FlagsLOD2)
                {
                    newFaceSet.LOD = (byte)2;
                    HasNoLODs = false;
                }

                newFaceSet.IndexBuffer.SetData(faceset.VertexIndices
                    .Select(x =>
                    {
                        if (x == ushort.MaxValue)
                            return (short)(-1);
                        else
                            return (short)x;
                    })
                    .ToArray());
                MeshFacesets.Add(newFaceSet);
            }

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);
        }*/

        public FlverSubmeshRenderer(FLVER flvr, FLVER.Mesh mesh)
        {
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

            foreach (var matParam in flvr.Materials[mesh.MaterialIndex].Params)
            {
                var paramNameCheck = matParam.Param.ToUpper();
                // DS3/BB
                if (paramNameCheck == "G_DIFFUSETEXTURE")
                    TexNameDiffuse = matParam.Value;
                else if (paramNameCheck == "G_SPECULARTEXTURE")
                    TexNameSpecular = matParam.Value;
                else if (paramNameCheck == "G_BUMPMAPTEXTURE")
                    TexNameNormal = matParam.Value;
                // DS1 params
                else if (paramNameCheck == "G_DIFFUSE")
                    TexNameDiffuse = matParam.Value;
                else if (paramNameCheck == "G_SPECULAR")
                    TexNameSpecular = matParam.Value;
                else if (paramNameCheck == "G_BUMPMAP")
                    TexNameNormal = matParam.Value;
            }

            var MeshVertices = new VertexPositionColorNormalTangentTexture[mesh.VertexGroups[0].Vertices.Count];
            for (int i = 0; i < mesh.VertexGroups[0].Vertices.Count; i++)
            {
                var vert = mesh.VertexGroups[0].Vertices[i];
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
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                }
            }

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            foreach (var faceset in mesh.FaceSets)
            {
                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = faceset.CullBackfaces,
                    IsTriangleStrip = faceset.TriangleStrip,
                    IndexBuffer = new IndexBuffer(
                                GFX.Device,
                                IndexElementSize.SixteenBits,
                                sizeof(short) * faceset.Vertices.Length,
                                BufferUsage.None),
                    IndexCount = faceset.Vertices.Length,
                };

                if (faceset.Flags == FLVER.FaceSet.FSFlags.LodLevel1)
                {
                    newFaceSet.LOD = (byte)1;
                    HasNoLODs = false;
                }
                else if (faceset.Flags == FLVER.FaceSet.FSFlags.LodLevel2)
                {
                    newFaceSet.LOD = (byte)2;
                    HasNoLODs = false;
                }

                newFaceSet.IndexBuffer.SetData(faceset.Vertices
                    .Select(x =>
                    {
                        if (x == ushort.MaxValue)
                            return (short)(-1);
                        else
                            return (short)x;
                    })
                    .ToArray());
                MeshFacesets.Add(newFaceSet);
            }

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);
        }

        public void Draw<T>(int lod, IGFXShader<T> shader, bool forceNoBackfaceCulling = false)
            where T : Effect
        {

            if (GFX.EnableTextures && shader == GFX.FlverShader)
            {
                if (TexDataDiffuse == null && TexNameDiffuse != null)
                    TexDataDiffuse = TexturePool.FetchTexture(TexNameDiffuse);

                if (TexDataSpecular == null && TexNameSpecular != null)
                    TexDataSpecular = TexturePool.FetchTexture(TexNameSpecular);

                if (TexDataNormal == null && TexNameNormal != null)
                    TexDataNormal = TexturePool.FetchTexture(TexNameNormal);

                GFX.FlverShader.Effect.ColorMap = TexDataDiffuse ?? Main.DEFAULT_TEXTURE_DIFFUSE;
                GFX.FlverShader.Effect.SpecularMap = TexDataSpecular ?? Main.DEFAULT_TEXTURE_SPECULAR;
                GFX.FlverShader.Effect.NormalMap = TexDataNormal ?? Main.DEFAULT_TEXTURE_NORMAL;
            }
                

            //foreach (var technique in shader.Effect.Techniques)
            //{
            //    shader.Effect.CurrentTechnique = technique;
                foreach (EffectPass pass in shader.Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GFX.Device.SetVertexBuffer(VertBuffer);

                    foreach (var faceSet in MeshFacesets)
                    {
                        if (!HasNoLODs && faceSet.LOD != lod)
                            continue;

                        GFX.Device.Indices = faceSet.IndexBuffer;

                        GFX.BackfaceCulling = forceNoBackfaceCulling ? false : faceSet.BackfaceCulling;

                        GFX.Device.DrawIndexedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                            faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3));

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
        }
    }
}
