using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DebugPrimitives
{
    public class ComplexDebugPrimitive
    {
        public Transform Transform;
        private int[] Indices = new int[0];
        private VertexPositionColor[] Vertices = new VertexPositionColor[0];
        private VertexBuffer VertBuffer;
        private bool NeedToRecreateBuffer = true;

        private void AddVertex(Vector3 pos, Color color)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1].Position = pos;
            Vertices[Vertices.Length - 1].Color = color;

            NeedToRecreateBuffer = true;
        }

        private void AddVertex(VertexPositionColor vert)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1] = vert;

            NeedToRecreateBuffer = true;
        }

        private void AddIndex(int index)
        {
            Array.Resize(ref Indices, Indices.Length + 1);
            Indices[Indices.Length - 1] = index;
        }

        public void AddLine(Vector3 start, Vector3 end, Color color)
        {
            var startVert = new VertexPositionColor(start, color);
            var endVert = new VertexPositionColor(end, color);
            int startIndex = Array.IndexOf(Vertices, startVert);
            int endIndex = Array.IndexOf(Vertices, endVert);

            //If start vertex can't be recycled from an old one, make a new one.
            if (startIndex == -1)
            {
                AddVertex(startVert);
                startIndex = Vertices.Length - 1;
            }

            //If end vertex can't be recycled from an old one, make a new one.
            if (endIndex == -1)
            {
                AddVertex(endVert);
                endIndex = Vertices.Length - 1;
            }

            AddIndex(startIndex);
            AddIndex(endIndex);

            if (NeedToRecreateBuffer)
            {
                VertBuffer = new VertexBuffer(GFX.Device, 
                    typeof(VertexPositionColor), Vertices.Length, BufferUsage.None);
                NeedToRecreateBuffer = false;
            } 
        }

        public void Draw()
        {
            foreach (var pass in GFX.CurrentDbgPrimRenderEffect.CurrentTechnique.Passes)
            {
                GFX.World.ApplyViewToShader(GFX.CurrentDbgPrimGFXShader, Transform);
                pass.Apply();
                GFX.Device.SetVertexBuffer(VertBuffer);
                GFX.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList,
                    Vertices, 0, Vertices.Length, Indices, 0,
                    primitiveCount: Indices.Length / 2 /* 2 indices for each line */);
            }
        }
    }
}
