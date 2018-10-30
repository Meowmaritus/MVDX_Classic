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
    public static class GFX
    {
        private static FrameCounter FpsCounter = new FrameCounter();

        public static float FPS => FpsCounter.CurrentFramesPerSecond;
        public static float AverageFPS => FpsCounter.AverageFramesPerSecond;

        public static bool EnableFrustumCulling = false;
        public static bool EnableTextures = true;

        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOff;
        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOn;

        public static WorldView World = new WorldView();

        public static ModelDrawer ModelDrawer = new ModelDrawer();

        public static GraphicsDevice Device;
        public static FlverShader FlverShader;
        public static DbgPrimShader DbgPrimShader;
        public static SpriteBatch SpriteBatch;
        const string FlverShader__Name = @"Content\NormalMapShader";

        public static bool BackfaceCulling
        {
            set => Device.RasterizerState = value ? HotSwapRasterizerState_BackfaceCullingOn : HotSwapRasterizerState_BackfaceCullingOff;
        }

        private static void CompletelyChangeRasterizerState(RasterizerState rs)
        {
            HotSwapRasterizerState_BackfaceCullingOff = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff.CullMode = CullMode.None;

            HotSwapRasterizerState_BackfaceCullingOn = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn.CullMode = CullMode.CullClockwiseFace;
        }

        public static void Init(ContentManager c)
        {
            FlverShader = new FlverShader(c.Load<Effect>(FlverShader__Name));
            DbgPrimShader = new DbgPrimShader(Device);

            SpriteBatch = new SpriteBatch(Device);

            HotSwapRasterizerState_BackfaceCullingOff = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOff.CullMode = CullMode.None;

            HotSwapRasterizerState_BackfaceCullingOn = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOn.CullMode = CullMode.CullClockwiseFace;

            
        }

        public static void UpdateFPS(float elapsedSeconds)
        {
            FpsCounter.Update(elapsedSeconds);
        }
    }
}
