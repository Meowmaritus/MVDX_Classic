using DarkSoulsModelViewerDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class ModelDrawer
    {
        private static object _lock_ModelLoad_Draw = new object();
        public List<ModelInstance> ModelInstanceList { get; private set; } = new List<ModelInstance>();

        public long Debug_VertexCount = 0;
        public long Debug_SubmeshCount = 0;

        public void ClearScene()
        {
            TexturePool.Flush();
            ModelInstanceList.Clear();
            GC.Collect();
        }

        public void AddModelInstance(ModelInstance ins)
        {
            lock (_lock_ModelLoad_Draw)
            {
                ModelInstanceList.Add(ins);

                foreach (var submesh in ins.Model.Submeshes)
                {
                    Debug_VertexCount += submesh.VertexCount;
                    Debug_SubmeshCount++;
                }
            }
            
        }

        public void TestAddAllChr()
        {
            var thread = new Thread(() =>
            {
                float currentX = 0;

                TexturePool.AddChrBndsThatEndIn9();

                for (int i = 0; i <= 9999; i++)
                {
                    var newChr = AddChr(i, 0, new Transform(currentX, 0, 0, 0, 0, 0));
                    if (newChr != null)
                    {
                        float thisChrWidth = new Vector3(newChr.Model.Bounds.Max.X, 0, newChr.Model.Bounds.Max.Z).Length()
                            + new Vector3(newChr.Model.Bounds.Min.X, 0, newChr.Model.Bounds.Min.Z).Length();
                        newChr.Transform.Position.X += thisChrWidth / 2;
                        currentX += thisChrWidth;
                    }
                }
            });

            thread.IsBackground = true;

            thread.Start();
        }

        public void TestAddAllObj()
        {
            var thread = new Thread(() =>
            {
                float currentX = 0;

                TexturePool.AddMapTexUdsfm();
                TexturePool.AddObjBndsThatEndIn9();

                for (int i = 0; i <= 9999; i++)
                {
                    var newChr = AddObj(i, 0, new Transform(currentX, 0, 0, 0, 0, 0));
                    if (newChr != null)
                    {
                        float thisChrWidth = new Vector3(newChr.Model.Bounds.Max.X, 0, newChr.Model.Bounds.Max.Z).Length()
                            + new Vector3(newChr.Model.Bounds.Min.X, 0, newChr.Model.Bounds.Min.Z).Length();
                        newChr.Transform.Position.X += thisChrWidth / 2;
                        currentX += thisChrWidth;
                    }
                }
            });

            thread.IsBackground = true;

            thread.Start();
            
        }

        public ModelInstance AddChr(int id, int idx, Transform location)
        {
            var model = InterrootLoader.LoadModelChrOptimized(id, idx);
            if (model == null)
                return null;
            var modelInstance = new ModelInstance($"c{id:D4}", model, location);

            AddModelInstance(modelInstance);

            model = null;
            return modelInstance;
        }

        public ModelInstance AddObj(int id, int idx, Transform location)
        {
            var model = InterrootLoader.LoadModelObjOptimized(id, idx);
            if (model == null)
                return null;
            var modelInstance = new ModelInstance($"o{id:D4}", model, location);

            AddModelInstance(modelInstance);

            model = null;
            return modelInstance;
        }

        public void AddMap(int area, int block, bool excludeScenery)
        {
            var thread = new Thread(() =>
            {
                TexturePool.AddMapTexUdsfm();
                InterrootLoader.LoadMapInBackground(area, block, excludeScenery, AddModelInstance);
            });

            thread.IsBackground = true;

            thread.Start();
        }

        private void DrawFlverAt(Model flver, Transform transform)
        {
            GFX.World.ApplyViewToShader(GFX.FlverShader, transform);
            flver.Draw(transform);
        }

        public void DrawSpecific(int index)
        {
            DrawFlverAt(ModelInstanceList[index].Model, ModelInstanceList[index].Transform);
        }

        public void Draw()
        {
            List<ModelInstance> thisDrawModelInstances;
            lock (_lock_ModelLoad_Draw)
            {
                thisDrawModelInstances = ModelInstanceList;
            }

            var drawOrderSortedModelInstances = thisDrawModelInstances
                .Where(x => x.Model.IsVisible && GFX.World.IsInFrustum(x.Model.Bounds, x.Transform))
                .OrderByDescending(m => GFX.World.GetDistanceSquaredFromCamera(m.Transform));

            foreach (var ins in drawOrderSortedModelInstances)
            {
                DrawFlverAt(ins.Model, ins.Transform);
            }
        }

        public void DebugDrawAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var ins in ModelInstanceList)
                {
                    ins.DrawDebugInfo();
                }
            }
        }


    }
}
