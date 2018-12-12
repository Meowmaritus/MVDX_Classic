using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkSoulsModelViewerDX
{
    public class Model : IDisposable
    {
        public bool IsVisible { get; set; } = true;
        public BoundingBox Bounds { get; private set; }

        private List<ModelInstance> Instances = new List<ModelInstance>();
        public int InstanceCount => Instances.Count;

        VertexBuffer InstanceBuffer;
        public VertexBufferBinding InstanceBufferBinding { get; private set; }


        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(64, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(72, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        public void AddNewInstance(ModelInstance ins)
        {
            Instances.Add(ins);

            if (InstanceBuffer != null)
                InstanceBuffer.Dispose();

            InstanceBuffer = new VertexBuffer(GFX.Device, instanceVertexDeclaration, Instances.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances.Select(x => x.Data).ToArray());
            InstanceBufferBinding = new VertexBufferBinding(InstanceBuffer, 0, 1);
        }

        private List<FlverSubmeshRenderer> Submeshes = new List<FlverSubmeshRenderer>();

        public Model(FLVER flver)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Meshes)
            {
                var smm = new FlverSubmeshRenderer(this, flver, submesh);
                Submeshes.Add(smm);
                subBoundsPoints.Add(smm.Bounds.Min);
                subBoundsPoints.Add(smm.Bounds.Max);
            }

            //DEBUG//
            //Console.WriteLine($"{flver.Meshes[0].DefaultBoneIndex}");
            //Console.WriteLine();
            //Console.WriteLine();
            //foreach (var mat in flver.Materials)
            //{
            //    Console.WriteLine($"{mat.Name}: {mat.MTD}");
            //}
            /////////

            if (Submeshes.Count == 0)
            {
                Bounds = new BoundingBox();
                IsVisible = false;
            }
            else
            {
                Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
            }
        }

        public Model(FLVERD flver)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Meshes)
            {
                if (submesh.ToTriangleList().Length > 0)
                {
                    var smm = new FlverSubmeshRenderer(this, flver, submesh);
                    Submeshes.Add(smm);
                    subBoundsPoints.Add(smm.Bounds.Min);
                    subBoundsPoints.Add(smm.Bounds.Max);
                }
            }

            //DEBUG//
            //Console.WriteLine($"{flver.Meshes[0].DefaultBoneIndex}");
            //Console.WriteLine();
            //Console.WriteLine();
            //foreach (var mat in flver.Materials)
            //{
            //    Console.WriteLine($"{mat.Name}: {mat.MTD}");
            //}
            /////////

            if (Submeshes.Count == 0)
            {
                Bounds = new BoundingBox();
                IsVisible = false;
            }
            else
            {
                Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
            }
        }

        public Model(HKX hkx)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var col in hkx.DataSection.Objects)
            {
                if (col is HKX.FSNPCustomParamCompressedMeshShape)
                {
                    var smm = new FlverSubmeshRenderer(this, hkx, (HKX.FSNPCustomParamCompressedMeshShape)col);
                    Submeshes.Add(smm);
                    subBoundsPoints.Add(smm.Bounds.Min);
                    subBoundsPoints.Add(smm.Bounds.Max);
                }
            }

            if (Submeshes.Count == 0)
            {
                Bounds = new BoundingBox();
                IsVisible = false;
            }
            else
            {
                Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
            }
        }

        public void DebugDraw()
        {
            foreach (var ins in Instances)
            {
                ins.DrawDebugInfo();
            }
            //TODO
        }

        public void Draw()
        {
            var lod = 0;// GFX.World.GetLOD(modelLocation);
            foreach (var submesh in Submeshes)
            {
                submesh.Draw(lod, GFX.FlverShader);
            }
        }

        public void TryToLoadTextures()
        {
            foreach (var sm in Submeshes)
                sm.TryToLoadTextures();
        }

        public void Dispose()
        {
            if (Submeshes != null)
            {
                for (int i = 0; i < Submeshes.Count; i++)
                {
                    if (Submeshes[i] != null)
                        Submeshes[i].Dispose();
                }

                Submeshes = null;
            }

            InstanceBuffer?.Dispose();
            InstanceBuffer = null;
        }
    }
}
