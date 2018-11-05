using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class ModelInstance : IDisposable
    {
        public string Name;
        public Model Model;
        public Transform Transform = Transform.Default;
        public bool IsVisible
        {
            get => Model.IsVisible;
            set => Model.IsVisible = value;
        }
        public bool IsDummyMapPart = false;

        public int DrawGroup1 = -1;
        public int DrawGroup2 = -1;
        public int DrawGroup3 = -1;
        public int DrawGroup4 = -1;

        public bool DrawgroupMatch(ModelInstance other)
        {
            return (
                ((DrawGroup1 & other.DrawGroup1) != 0) ||
                ((DrawGroup2 & other.DrawGroup2) != 0) ||
                ((DrawGroup3 & other.DrawGroup3) != 0) ||
                ((DrawGroup4 & other.DrawGroup4) != 0)
                );
        }

        public BoundingBox WorldBounds => new BoundingBox(
                    Vector3.Transform(Model.Bounds.Min, Transform.WorldMatrix),
                    Vector3.Transform(Model.Bounds.Max, Transform.WorldMatrix)
                    );

        public Vector3 GetCenterPoint()
        {
            return WorldBounds.GetCenter();
        }

        public Vector3 GetTopCenterPoint(float verticalOffset = 0)
        {
            var absoluteCenter = GetCenterPoint();
            return new Vector3(absoluteCenter.X, WorldBounds.Max.Y + verticalOffset, absoluteCenter.Z);
        }

        public Vector3 GetBottomCenterPoint(float verticalOffset = 0)
        {
            var absoluteCenter = GetCenterPoint();
            return new Vector3(absoluteCenter.X, WorldBounds.Min.Y + verticalOffset, absoluteCenter.Z);
        }

        public float GetRoughBoundsDiameter()
        {
            return Model.Bounds.Min.Length() + Model.Bounds.Max.Length();
        }

        public ModelInstance(string name, Model model, Transform transform, int drawGroup1, int drawGroup2, int drawGroup3, int drawGroup4)
        {
            Name = name;
            Model = model;
            Transform = transform;
            DrawGroup1 = drawGroup1;
            DrawGroup2 = drawGroup2;
            DrawGroup3 = drawGroup3;
            DrawGroup4 = drawGroup4;
        }

        public void DrawDebugInfo()
        {
            if (DBG.ShowModelBoundingBoxes)
                DBG.DrawBoundingBox(Model.Bounds, Color.Yellow, Transform);

            if (DBG.ShowModelSubmeshBoundingBoxes)
            {
                foreach (var sm in Model.Submeshes)
                {
                    DBG.DrawBoundingBox(sm.Bounds, Color.Orange, Transform);
                }
            }
            
            if (DBG.ShowModelNames)
                DBG.DrawTextOn3DLocation(GetTopCenterPoint(verticalOffset: 0.25f), Name, Color.Yellow, 0.5f);
        }

        public void TryToLoadTextures()
        {
            Model.TryToLoadTextures();
        }

        public void Dispose()
        {
            Model.Dispose();
            Model = null;

            Name = null;
        }
    }
}
