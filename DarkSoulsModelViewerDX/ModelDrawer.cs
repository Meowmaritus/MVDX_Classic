using DarkSoulsModelViewerDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class ModelDrawer
    {
        public List<ModelInstance> ModelInstanceList = new List<ModelInstance>();

        public long Debug_VertexCount = 0;
        public long Debug_SubmeshCount = 0;

        

        public void AddModelInstance(ModelInstance ins)
        {
            ModelInstanceList.Add(ins);

            foreach (var submesh in ins.Model.Submeshes)
            {
                Debug_VertexCount += submesh.VertexCount;
                Debug_SubmeshCount++;
            }
        }

        public void TestAddAllChr()
        {
            float currentX = 0;

            InterrootLoader.TexPoolChr.AddChrBndsThatEndIn9();

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
        }

        public void TestAddAllObj()
        {
            float currentX = 0;

            InterrootLoader.TexPoolObj.AddMapTexUdsfm();
            InterrootLoader.TexPoolChr.AddObjBndsThatEndIn9();

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

        public List<ModelInstance> AddMap(int area, int block, bool excludeScenery)
        {
            var mapModelInstances = InterrootLoader.LoadMap(area, block, excludeScenery);

            foreach (var ins in mapModelInstances)
            {
                AddModelInstance(ins);
            }

            return mapModelInstances;
        }

        private void StartDraw()
        {
            var ds = new DepthStencilState();
            ds.DepthBufferEnable = true;
            ds.DepthBufferWriteEnable = true;
            ds.StencilEnable = true;
            GFX.Device.DepthStencilState = ds;

            GFX.Device.SamplerStates[0] = SamplerState.LinearWrap;

            GFX.World.ApplyViewToShader(GFX.FlverShader);

            GFX.FlverShader.AmbientColor = Vector4.One;

            GFX.FlverShader.AmbientIntensity = 0.75f;

            GFX.FlverShader.DiffuseColor = Vector4.One;
            GFX.FlverShader.DiffuseIntensity = 1f;

            GFX.FlverShader.SpecularColor = Vector4.One;
            GFX.FlverShader.SpecularPower = 15f;

            GFX.FlverShader.LightDirection = GFX.World.LightDirectionVector;

            GFX.FlverShader.EyePosition = GFX.World.CameraTransform.Position;

            GFX.FlverShader.NormalMapCustomZ = 1.0f;

            GFX.FlverShader.ColorMap = MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE;
            GFX.FlverShader.NormalMap = MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_NORMAL;
            GFX.FlverShader.SpecularMap = MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_SPECULAR;
        }

        private void DrawFlverAt(Model flver, Transform transform, bool forceRender)
        {
            GFX.World.ApplyViewToShader(GFX.FlverShader, transform);
            flver.Draw(transform, forceRender);
        }

        public void DrawSpecific(int index)
        {
            StartDraw();
            DrawFlverAt(ModelInstanceList[index].Model, ModelInstanceList[index].Transform, forceRender: true);
        }

        public void Draw()
        {
            StartDraw();
            foreach (var ins in ModelInstanceList)
            {
                DrawFlverAt(ins.Model, ins.Transform, forceRender: false);
            }
        }

        public void DebugDrawAll()
        {
            foreach (var ins in ModelInstanceList)
            {
                ins.DrawDebugInfo();
            }
        }


    }
}
