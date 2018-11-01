using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class Model
    {
        public bool IsVisible { get; set; } = true;
        public BoundingBox Bounds { get; private set; }

        public List<FlverSubmeshRenderer> Submeshes { get; private set; }  = new List<FlverSubmeshRenderer>();
        public Model(FLVER flver)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Submeshes)
            {
                var smm = new FlverSubmeshRenderer(submesh);
                Submeshes.Add(smm);
                subBoundsPoints.Add(smm.Bounds.Min);
                subBoundsPoints.Add(smm.Bounds.Max);
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

        public Model(FLVEROptimized flver)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Submeshes)
            {
                var smm = new FlverSubmeshRenderer(submesh);
                Submeshes.Add(smm);
                subBoundsPoints.Add(smm.Bounds.Min);
                subBoundsPoints.Add(smm.Bounds.Max);
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

        public void Draw(Transform modelLocation, bool forceRender = false)
        {
            var lod = GFX.World.GetLOD(modelLocation);
            foreach (var submesh in Submeshes)
            {
                submesh.Draw(lod);
            }
        }
    }
}
