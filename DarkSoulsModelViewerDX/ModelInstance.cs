using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class ModelInstance
    {
        public string Name;
        public Model Model;
        public Transform Transform = Transform.Default;
        bool IsVisible
        {
            get => Model.IsVisible;
            set => Model.IsVisible = value;
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

        public ModelInstance(string name, Model model, Transform transform)
        {
            Name = name;
            Model = model;
            Transform = transform;
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
    }
}
