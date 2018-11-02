using DarkSoulsModelViewerDX.DbgMenus;
using DarkSoulsModelViewerDX.DebugPrimitives;
using DarkSoulsModelViewerDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public enum GFXDrawStep : byte
    {
        Opaque = 1,
        AlphaEdge = 2,
        DbgPrim = 3,
        GUI  = 4,
    }

    public static class GFX
    {
        public static class Display
        {
            public static DisplayMode Mode;
            public static bool Vsync = true;
            public static bool Fullscreen = false;
            public static bool SimpleMSAA = true;
            public static void Apply()
            {
                Main.ApplyPresentationParameters(Mode, Vsync, Fullscreen, SimpleMSAA);
            }
        }

        public static GFXDrawStep CurrentStep = GFXDrawStep.Opaque;

        public static readonly GFXDrawStep[] DRAW_STEP_LIST;
        static GFX()
        {
            DRAW_STEP_LIST = (GFXDrawStep[])Enum.GetValues(typeof(GFXDrawStep));
        }

        public const int LOD_MAX = 2;
        public static int ForceLOD = -1;
        public static float LOD1Distance = 200;
        public static float LOD2Distance = 400;

        public static IGFXShader<FlverShader> FlverShader;
        public static IGFXShader<DbgPrimShader> DbgPrimShader;

        public static Stopwatch FpsStopwatch = new Stopwatch();
        private static FrameCounter FpsCounter = new FrameCounter();

        public static float FPS => FpsCounter.CurrentFramesPerSecond;
        public static float AverageFPS => FpsCounter.AverageFramesPerSecond;

        public static bool EnableFrustumCulling = false;
        public static bool EnableTextures = true;

        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOff;
        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOn;

        private static DepthStencilState DepthStencilState_Normal;
        private static DepthStencilState DepthStencilState_DontWriteDepth;

        public static WorldView World = new WorldView();

        public static ModelDrawer ModelDrawer = new ModelDrawer();

        public static GraphicsDevice Device;
        //public static FlverShader FlverShader;
        //public static DbgPrimShader DbgPrimShader;
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

        public static void InitDepthStencil()
        {
            switch (CurrentStep)
            {
                case GFXDrawStep.Opaque:
                    Device.DepthStencilState = DepthStencilState_Normal;
                    break;
                case GFXDrawStep.AlphaEdge:
                case GFXDrawStep.DbgPrim:
                case GFXDrawStep.GUI:
                    Device.DepthStencilState = DepthStencilState_DontWriteDepth;
                    break;
            }
        }

        public static void Init(ContentManager c)
        {

            DepthStencilState_Normal = new DepthStencilState()
            {
                //CounterClockwiseStencilDepthBufferFail = Device.DepthStencilState.CounterClockwiseStencilDepthBufferFail,
                //CounterClockwiseStencilFail = Device.DepthStencilState.CounterClockwiseStencilFail,
                //CounterClockwiseStencilFunction = Device.DepthStencilState.CounterClockwiseStencilFunction,
                //CounterClockwiseStencilPass = Device.DepthStencilState.CounterClockwiseStencilPass,
                //DepthBufferEnable = Device.DepthStencilState.DepthBufferEnable,
                //DepthBufferFunction = Device.DepthStencilState.DepthBufferFunction,
                DepthBufferWriteEnable = true,
                //ReferenceStencil = Device.DepthStencilState.ReferenceStencil,
                //StencilDepthBufferFail = Device.DepthStencilState.StencilDepthBufferFail,
                //StencilEnable = Device.DepthStencilState.StencilEnable,
                //StencilFail = Device.DepthStencilState.StencilFail,
                //StencilFunction = Device.DepthStencilState.StencilFunction,
                //StencilMask = Device.DepthStencilState.StencilMask,
                //StencilPass = Device.DepthStencilState.StencilPass,
                //StencilWriteMask = Device.DepthStencilState.StencilWriteMask,
                //TwoSidedStencilMode = Device.DepthStencilState.TwoSidedStencilMode,
            };

            DepthStencilState_DontWriteDepth = new DepthStencilState()
            {
                //CounterClockwiseStencilDepthBufferFail = Device.DepthStencilState.CounterClockwiseStencilDepthBufferFail,
                //CounterClockwiseStencilFail = Device.DepthStencilState.CounterClockwiseStencilFail,
                //CounterClockwiseStencilFunction = Device.DepthStencilState.CounterClockwiseStencilFunction,
                //CounterClockwiseStencilPass = Device.DepthStencilState.CounterClockwiseStencilPass,
                //DepthBufferEnable = Device.DepthStencilState.DepthBufferEnable,
                //DepthBufferFunction = Device.DepthStencilState.DepthBufferFunction,
                DepthBufferWriteEnable = false,
                //ReferenceStencil = Device.DepthStencilState.ReferenceStencil,
                //StencilDepthBufferFail = Device.DepthStencilState.StencilDepthBufferFail,
                //StencilEnable = Device.DepthStencilState.StencilEnable,
                //StencilFail = Device.DepthStencilState.StencilFail,
                //StencilFunction = Device.DepthStencilState.StencilFunction,
                //StencilMask = Device.DepthStencilState.StencilMask,
                //StencilPass = Device.DepthStencilState.StencilPass,
                //StencilWriteMask = Device.DepthStencilState.StencilWriteMask,
                //TwoSidedStencilMode = Device.DepthStencilState.TwoSidedStencilMode,
            };

            FlverShader = new FlverShader(c.Load<Effect>(FlverShader__Name));

            FlverShader.Effect.AmbientColor = Vector4.One;
            FlverShader.Effect.AmbientIntensity = 0.5f;
            FlverShader.Effect.DiffuseColor = Vector4.One;
            FlverShader.Effect.DiffuseIntensity = 0.75f;
            FlverShader.Effect.SpecularColor = Vector4.One;
            FlverShader.Effect.SpecularPower = 10f;
            FlverShader.Effect.NormalMapCustomZ = 1.0f;

            DbgPrimShader = new DbgPrimShader(Device);

            SpriteBatch = new SpriteBatch(Device);

            HotSwapRasterizerState_BackfaceCullingOff = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOff.CullMode = CullMode.None;

            HotSwapRasterizerState_BackfaceCullingOn = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOn.CullMode = CullMode.CullClockwiseFace;
        }

        public static void BeginDraw()
        {
            InitDepthStencil();
            //InitBlendState();

            World.ApplyViewToShader(DbgPrimShader);
            World.ApplyViewToShader(FlverShader);

            Device.SamplerStates[0] = SamplerState.LinearWrap;

            FlverShader.Effect.EyePosition = World.CameraTransform.Position;
            FlverShader.Effect.LightDirection = World.LightDirectionVector;
            FlverShader.Effect.ColorMap = Main.DEFAULT_TEXTURE_DIFFUSE;
            FlverShader.Effect.NormalMap = Main.DEFAULT_TEXTURE_NORMAL;
            FlverShader.Effect.SpecularMap = Main.DEFAULT_TEXTURE_SPECULAR;

        }

        private static void DoDrawStep(GameTime gameTime)
        {
            switch (CurrentStep)
            {
                case GFXDrawStep.Opaque:
                case GFXDrawStep.AlphaEdge:
                    ModelDrawer.Draw();
                    ModelDrawer.DebugDrawAll();
                    break;
                case GFXDrawStep.DbgPrim:
                    DBG.DrawPrimitives();
                    break;
                case GFXDrawStep.GUI:
                    DbgMenuItem.CurrentMenu.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);
                    break;
            }
        }

        private static void DoDraw(GameTime gameTime)
        {
            if (Main.DISABLE_DRAW_ERROR_HANDLE)
            {
                DoDrawStep(gameTime);
            }
            else
            {
                try
                {
                    DoDrawStep(gameTime);
                }
                catch
                {
                    var errText = $"Draw Call Failed ({CurrentStep.ToString()})";
                    var errTextSize = DBG.DEBUG_FONT_HQ.MeasureString(errText);
                    DBG.DrawOutlinedText(errText, new Vector2(Device.Viewport.Width / 2, Device.Viewport.Height / 2), Color.Red, DBG.DEBUG_FONT_HQ, 0, 0.25f, errTextSize / 2);
                }
            }

            
            
        }

        public static void DrawScene(GameTime gameTime)
        {
            Device.Clear(Color.Gray);

            for (int i = 0; i < DRAW_STEP_LIST.Length; i++)
            {
                CurrentStep = DRAW_STEP_LIST[i];
                BeginDraw();
                DoDraw(gameTime);
            }

            GFX.UpdateFPS((float)FpsStopwatch.Elapsed.TotalSeconds);
            DBG.DrawOutlinedText($"FPS: {(Math.Round(GFX.AverageFPS))}", new Vector2(0, GFX.Device.Viewport.Height - 24), Color.Yellow);
            FpsStopwatch.Restart();
        }

        public static void UpdateFPS(float elapsedSeconds)
        {
            FpsCounter.Update(elapsedSeconds);
        }
    }
}
