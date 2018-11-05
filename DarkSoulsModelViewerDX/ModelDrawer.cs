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
        private bool IsTextureLoadRequested = false;
        public void RequestTextureLoad() => IsTextureLoadRequested = true;

        internal static object _lock_ModelLoad_Draw = new object();
        public List<ModelInstance> ModelInstanceList { get; private set; } = new List<ModelInstance>();

        public ModelInstance Selected = null;
        public bool HighlightSelectedPiece = true;
        public bool WireframeSelectedPiece = false;

        public long Debug_VertexCount = 0;
        public long Debug_SubmeshCount = 0;

        public void ClearScene()
        {
            foreach (var mi in ModelInstanceList)
                mi.Dispose();

            TexturePool.Flush();
            ModelInstanceList.Clear();
            GC.Collect();
        }

        public void InvertVisibility()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in ModelInstanceList)
                    m.IsVisible = !m.IsVisible;
            }
        }

        public void HideAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in ModelInstanceList)
                    m.IsVisible = false;
            }
        }

        public void ShowAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in ModelInstanceList)
                    m.IsVisible = true;
            }
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
            LoadingTaskMan.DoLoadingTask("TestAddAllChr", "Loading lineup of all characters...", prog =>
            {
                float currentX = 0;

                //TexturePool.AddChrBndsThatEndIn9();
                int i = 0;
                foreach (int ID in DbgMenus.DbgMenuItemSpawnChr.IDList)
                {
                    var newModels = AddChr(ID, new Transform(currentX, 0, 0, 0, 0, 0));
                    foreach (var mdl in newModels)
                    {
                        float thisModelWidth = new Vector3(mdl.Model.Bounds.Max.X, 0, mdl.Model.Bounds.Max.Z).Length()
                            + new Vector3(mdl.Model.Bounds.Min.X, 0, mdl.Model.Bounds.Min.Z).Length();
                        mdl.Transform.Position.X += thisModelWidth / 2;
                        currentX += thisModelWidth;
                    }

                    prog?.Report(1.0 * (++i) / DbgMenus.DbgMenuItemSpawnChr.IDList.Count);
                }

            });
        }

        public void TestAddAllObj()
        {
            LoadingTaskMan.DoLoadingTask("TestAddAllObj", "Loading lineup of all objects...", prog =>
            {
                float currentX = 0;

                //TexturePool.AddObjBndsThatEndIn9();
                int i = 0;
                foreach (int ID in DbgMenus.DbgMenuItemSpawnObj.IDList)
                {
                    var newModels = AddObj(ID, new Transform(currentX, 0, 0, 0, 0, 0));
                    foreach (var mdl in newModels)
                    {
                        float thisModelWidth = new Vector3(mdl.Model.Bounds.Max.X, 0, mdl.Model.Bounds.Max.Z).Length()
                            + new Vector3(mdl.Model.Bounds.Min.X, 0, mdl.Model.Bounds.Min.Z).Length();
                        mdl.Transform.Position.X += thisModelWidth / 2;
                        currentX += thisModelWidth;
                    }

                    prog?.Report(1.0 * (++i) / DbgMenus.DbgMenuItemSpawnObj.IDList.Count);
                }

            });
        }

        public List<ModelInstance> AddChr(int id, Transform location)
        {
            var models = InterrootLoader.LoadModelChr(id);

            var returnedModelInstances = new List<ModelInstance>();

            for (int i = 0; i < models.Count; i++)
            {
                var m = new ModelInstance($"c{id:D4}{(i > 0 ? $"[{i + 1}]" : "")}", models[i], location, -1, -1, -1, -1);
                AddModelInstance(m);
                returnedModelInstances.Add(m);
            }

            GFX.ModelDrawer.RequestTextureLoad();

            return returnedModelInstances;
        }

        public List<ModelInstance> AddObj(int id, Transform location)
        {
            var models = InterrootLoader.LoadModelObj(id);

            var returnedModelInstances = new List<ModelInstance>();

            for (int i = 0; i < models.Count; i++)
            {
                var m = new ModelInstance($"o{id:D4}{(i > 0 ? $"[{i + 1}]" : "")}", models[i], location, -1, -1, -1, -1);
                AddModelInstance(m);
                returnedModelInstances.Add(m);
            }

            GFX.ModelDrawer.RequestTextureLoad();

            return returnedModelInstances;
        }

        public void AddMap(int area, int block, bool excludeScenery)
        {
            InterrootLoader.LoadMapInBackground(area, block, excludeScenery, AddModelInstance);
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
            lock (_lock_ModelLoad_Draw)
            {
                if (IsTextureLoadRequested)
                {
                    foreach (var ins in ModelInstanceList)
                        ins.TryToLoadTextures();
                    IsTextureLoadRequested = false;
                }

                var drawOrderSortedModelInstances = ModelInstanceList
                .Where(x => x.IsVisible && GFX.World.IsInFrustum(x.Model.Bounds, x.Transform))
                .OrderByDescending(m => GFX.World.GetDistanceSquaredFromCamera(m.Transform));

                if (Selected != null)
                {
                    foreach (var ins in drawOrderSortedModelInstances)
                    {
                        if (Selected.DrawgroupMatch(ins))
                            DrawFlverAt(ins.Model, ins.Transform);
                    }
                }
                else
                {
                    foreach (var ins in drawOrderSortedModelInstances)
                    {
                        DrawFlverAt(ins.Model, ins.Transform);
                    }
                }

                
            }
        }

        public void DrawSelected()
        {
            if (Selected != null && (HighlightSelectedPiece || WireframeSelectedPiece))
            {
                GFX.World.ApplyViewToShader(GFX.DbgPrimShader, Selected.Transform);

                var lod = GFX.World.GetLOD(Selected.Transform);

                var oldWireframeSetting = GFX.Wireframe;

                var effect = ((BasicEffect)GFX.DbgPrimShader.Effect);

                if (HighlightSelectedPiece)
                {
                    GFX.Wireframe = false;

                    effect.VertexColorEnabled = true;

                    foreach (var submesh in Selected.Model.Submeshes)
                        submesh.Draw(lod, GFX.DbgPrimShader, forceNoBackfaceCulling: true);
                }

                if (WireframeSelectedPiece)
                {
                    GFX.Wireframe = true;
                    effect.VertexColorEnabled = false;

                    foreach (var submesh in Selected.Model.Submeshes)
                        submesh.Draw(lod, GFX.DbgPrimShader, forceNoBackfaceCulling: true);

                    GFX.Wireframe = oldWireframeSetting;
                }

                effect.VertexColorEnabled = true;
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
