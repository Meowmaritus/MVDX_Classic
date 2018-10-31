using DarkSoulsModelViewerDX.DbgMenus;
using DarkSoulsModelViewerDX.DebugPrimitives;
using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DarkSoulsModelViewerDX
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MODEL_VIEWER_MAIN : Game
    {
        public static bool REQUEST_EXIT = false;

        Stopwatch FpsStopwatch = new Stopwatch();

        GraphicsDeviceManager graphics;
        //public ContentManager Content;
        //public bool IsActive = true;

        

        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;

        public Rectangle ClientBounds => Window.ClientBounds;

        //MCG MCGTEST_MCG;

        public DbgPrimGrid DbgPrim_Grid;

        public MODEL_VIEWER_MAIN()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            MaxElapsedTime = TimeSpan.FromTicks(166667);

            //IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;

            graphics.PreferMultiSampling = true;

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;
        }

        private void Graphics_DeviceCreated(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        private void Graphics_DeviceReset(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            DEFAULT_TEXTURE_DIFFUSE = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_DIFFUSE.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

            DEFAULT_TEXTURE_SPECULAR = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_SPECULAR.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

            DEFAULT_TEXTURE_NORMAL = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_NORMAL.SetData(new Color[] { new Color(0.5f, 0.5f, 1.0f) });

            GFX.Device = GraphicsDevice;

            base.Initialize();
        }

        private void TestLoadAllMaps()
        {

        }

        protected override void LoadContent()
        {
            GFX.Init(Content);
            DBG.LoadContent(Content);
            InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            //////// Here is where you should load things for testing. ////////
            
            
            
            ///////////////////////////////////////////////////////////////////

            DbgPrim_Grid = new DbgPrimGrid(Color.Green, Color.Lime * 0.5f, 10, 1);

            if (GFX.ModelDrawer.ModelInstanceList.Count > 0)
                GFX.World.CameraTransform.Position = GFX.ModelDrawer.ModelInstanceList[0].Transform.Position + new Vector3(0, -1.5f, -13);
            else
                GFX.World.CameraTransform.Position = new Vector3(0, -1.5f, -13);

            GFX.World.CameraTransform.EulerRotation.X = MathHelper.PiOver4 / 8;

            DbgMenuItem.Init();
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        //public void Update(GameTime gameTime)
        //{

        //}

        // Unused
        private void DebugDraw()
        {
            var dbgList = new List<string>();
            dbgList.Add($"Dark Souls Model Viewer DX ({(IntPtr.Size * 8)}-Bit Version)");
            dbgList.Add(" ");
            dbgList.Add($"FPS: {(Math.Round(GFX.FPS))}");
            dbgList.Add($"FPS: {(Math.Round(GFX.AverageFPS))}");
            dbgList.Add(" ");
            dbgList.Add($"Total Model Count: {GFX.ModelDrawer.ModelInstanceList.Count}");
            dbgList.Add($"Total Submesh Count: {GFX.ModelDrawer.Debug_SubmeshCount}");
            dbgList.Add($"Total Vertex Count: {GFX.ModelDrawer.Debug_VertexCount}");

            for (int i = 0; i < dbgList.Count; i++)
                DBG.DrawOutlinedText(dbgList[i], new Vector2(8, 8 + (16 * i)), Color.Yellow);


            //FatcatDebug.DrawTextOn3DLocation(World, World.CameraOrigin.Position, "World.CameraOrigin", Color.Fuchsia);
            //FatcatDebug.DrawTextOn3DLocation(World, World.CameraPositionDefault.Position, "World.CameraPositionDefault", Color.Fuchsia);

            //DBG.DrawTextOn3DLocation(Vector3.Transform(GFX.World.CameraTransform.Position, Matrix.Invert(GFX.World.MatrixProjection)), "[CAMERA PHYSICAL LOCATION]", Color.PaleVioletRed);
        }

        //private void DrawMcgBranch(int i)
        //{
        //    if (i < 0 || i > MCGTEST_MCG.McgUnkStructA_List.Count)
        //        return;

        //    Vector3 thisPos = MCGTEST_MCG.McgUnkStructA_List[i].Position;
        //    foreach (var otherPosIndex in MCGTEST_MCG.McgUnkStructA_List[i].IndicesA)
        //    {
        //        var otherPos = MCGTEST_MCG.McgUnkStructA_List[otherPosIndex].Position;
        //        FatcatDebug.DrawLine(thisPos, otherPos, Color.Yellow, Color.Yellow, "" + i);
        //    }
        //}

        //private void DrawMcgTest()
        //{
        //    for (int i = 0; i < MCGTEST_MCG.McgUnkStructA_List.Count; i++)
        //    {
        //        DrawMcgBranch(i);
        //    }
        //    //DrawMcgBranch(MCGTEST_INDEX);
        //}

        protected override void Update(GameTime gameTime)
        {
            DbgMenuItem.UpdateInput((float)gameTime.ElapsedGameTime.TotalSeconds);
            DbgMenuItem.UICursorBlinkUpdate((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
            {
                // Only update input if debug menu isnt fully open.
                GFX.World.UpdateInput(this, gameTime);
            }

            GFX.World.UpdateMatrices(GraphicsDevice);

            GFX.World.CameraPositionDefault.Position = Vector3.Zero;

            GFX.World.CameraOrigin.Position = new Vector3(GFX.World.CameraPositionDefault.Position.X, 
                GFX.World.CameraOrigin.Position.Y, GFX.World.CameraPositionDefault.Position.Z);

            DbgPrim_Grid.Transform = GFX.World.CameraPositionDefault;

            if (REQUEST_EXIT)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Actual Draw Below
            GraphicsDevice.Clear(Color.Gray);

            if (DBG.ShowGrid)
                DbgPrim_Grid.Draw();

            GFX.ModelDrawer.Draw();
            GFX.ModelDrawer.DebugDrawAll();

            DbgMenuItem.CurrentMenu.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            GFX.UpdateFPS((float)FpsStopwatch.Elapsed.TotalSeconds);
            DBG.DrawOutlinedText($"FPS: {(Math.Round(GFX.AverageFPS))}", new Vector2(0, GFX.Device.Viewport.Height - 24), Color.Yellow);
            FpsStopwatch.Restart();
        }
    }
}
