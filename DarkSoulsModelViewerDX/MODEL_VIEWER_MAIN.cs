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

        //private void LoadEntireFuckingMap(string interroot, int area, int block)
        //{
        //    string map = $"m{area:D2}_{block:D2}_00_00";

        //    var mapPieceFlverNames = Directory.GetFiles($@"{interroot}\map\{map}", "*.flver");
        //    var flvers = new Dictionary<string, FLVER>();

        //    var piecePlacements = new List<KeyValuePair<FLVER, FatcatTransform>>();

        //    //foreach (var name in Directory.GetFiles($@"{interroot}\chr\", "*.chrbnd"))
        //    //{
        //    //    flvers.Add(MiscUtil.GetFileNameWithoutDirectoryOrExtension(name), DataFile.LoadFromFile<EntityBND>(name).Models[0].Mesh);
        //    //}

        //    foreach (var name in mapPieceFlverNames)
        //    {
        //        flvers.Add(MiscUtil.GetFileNameWithoutDirectoryOrExtension(name), DataFile.LoadFromFile<FLVER>(name));
        //        break;
        //    }

        //    var msb = DataFile.LoadFromFile<MSB>($@"{interroot}\map\MapStudio\{map}.msb");


        //    foreach (var part in msb.Parts.MapPieces)
        //    {
        //        piecePlacements.Add(new KeyValuePair<FLVER, FatcatTransform>(flvers[$"{part.ModelName}A{area}"],
        //            new FatcatTransform(part.PosX, part.PosY, part.PosZ, part.RotX, part.RotY, part.RotZ)));
        //        break;
        //    }

        //    //foreach (var part in msb.Parts.NPCs)
        //    //{
        //    //    piecePlacements.Add(new KeyValuePair<FLVER, FatcatTransform>(flvers[$"{part.ModelName}"],
        //    //        new FatcatTransform(part.PosX, part.PosY, part.PosZ, part.RotX, part.RotY, part.RotZ)));
        //    //}

        //    foreach (var kvp in piecePlacements)
        //    {
        //        GFX.ModelDrawer.ModelInstanceList.Add(new FatcatModelInstance(kvp.Key, kvp.Value));
        //    }
        //}

        private void TestLoadAllMaps()
        {

        }

        protected override void LoadContent()
        {
            GFX.Init(Content);

            DBG.LoadContent(Content);

            InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            //DEBUG TESTING//

            //LoadEntireFuckingMap(@"G:\SteamLibrary\steamapps\common\Dark Souls Prepare to Die Edition\DATA", 10, 2);

            //InterrootLoader.TexPoolMap.AddMapTexUdsfm();

            //GFX.ModelDrawer.AddMap(15, 1);

            //var rand = new Random();
            //Vector3 randUnitVector()
            //{
            //    return new Vector3((float)rand.NextDouble() - 0.5f,
            //        (float)rand.NextDouble() - 0.5f,
            //        (float)rand.NextDouble() - 0.5f) * 2;
            //}

            //var smough = GFX.ModelDrawer.AddChr(5250, new Transform(randUnitVector() * 10, randUnitVector() * MathHelper.TwoPi));

            //smough.Name = "Model 1";

            //for (int i = 0; i < 999; i++)
            //{
            //    GFX.ModelDrawer.AddModelInstance(new ModelInstance($"Model {(i + 2)}", smough.Model, new Transform(randUnitVector() * 10, randUnitVector() * MathHelper.PiOver4)));
            //}

            //TestLoadAllObj();

            //GFX.ModelDrawer.AddMap(10, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(10, 01, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(10, 02, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(11, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(12, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(12, 01, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(13, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(13, 01, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(13, 02, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(14, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(14, 01, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(15, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(15, 01, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(16, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(17, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(18, 00, excludeScenery: true);
            //GFX.ModelDrawer.AddMap(18, 01, excludeScenery: true);

            DbgPrim_Grid = new DbgPrimGrid(Color.Green, Color.Lime * 0.5f, 10, 1);

            //MCGTEST_MCG = DataFile.LoadFromFile<MCG>(@"D:\FRPG_MOD\ANALYSIS\MCG_MCP\m10_02_00_00.mcg");

            //foreach (var x in GFX.ModelDrawer.ModelInstanceList.Where(x => x.Name.StartsWith("m8") || x.Name.StartsWith("m9")))
            //{
            //    x.Model.IsVisible = false;
            //}


            /////////////////

            if (GFX.ModelDrawer.ModelInstanceList.Count > 0)
                GFX.World.CameraTransform.Position = GFX.ModelDrawer.ModelInstanceList[0].Transform.Position + new Vector3(0, -1.5f, -13);
            else
                GFX.World.CameraTransform.Position = new Vector3(0, -1.5f, -13);

            DbgMenuItem.Init();
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        //public void Update(GameTime gameTime)
        //{

        //}

        private void DebugDraw()
        {
            var dbgList = new List<string>();
            dbgList.Add($"Dark Souls Model Viewer DX ({(IntPtr.Size * 8)}-Bit Version)");
            dbgList.Add(" ");
            dbgList.Add($"Current FPS: {(Math.Round(GFX.FPS))}");
            dbgList.Add($"Average FPS: {(Math.Round(GFX.AverageFPS))}");
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


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FpsStopwatch.Restart();

            // Actual Draw Below
            GraphicsDevice.Clear(Color.Gray);

            if (DBG.ShowGrid)
                DbgPrim_Grid.Draw();


            GFX.ModelDrawer.Draw();
            GFX.ModelDrawer.DebugDrawAll();

            //DebugDraw();

            DbgMenuItem.CurrentMenu.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            GFX.UpdateFPS((float)FpsStopwatch.Elapsed.TotalSeconds);
            FpsStopwatch.Restart();
        }
    }
}
