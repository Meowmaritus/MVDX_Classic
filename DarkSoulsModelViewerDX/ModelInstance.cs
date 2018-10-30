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
        public Transform Transform;
        bool IsVisible
        {
            get => Model.IsVisible;
            set => Model.IsVisible = value;
        }

        public BoundingBox WorldBounds => new BoundingBox(
                    Vector3.Transform(Model.Bounds.Min, Transform.ViewMatrix),
                    Vector3.Transform(Model.Bounds.Max, Transform.ViewMatrix)
                    );

        public Vector3 GetCenterPoint()
        {
            return WorldBounds.GetCenter();
        }

        public Vector3 GetBottomCenterPoint()
        {
            var absoluteCenter = GetCenterPoint();
            return new Vector3(absoluteCenter.X, WorldBounds.Min.Y, absoluteCenter.Z);
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
                DBG.DrawTextOn3DLocation(GetCenterPoint(), Name, Color.Yellow, 0.5f);
        }
    }
}
