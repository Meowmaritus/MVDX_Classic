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
using System.Windows.Forms;

namespace DarkSoulsModelViewerDX
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        //public static Form WinForm;

        public static bool FIXED_TIME_STEP = true;

        public static bool REQUEST_EXIT = false;

        public static bool Active { get; private set; }

        public static bool DISABLE_DRAW_ERROR_HANDLE = true;

        private static float MemoryUsageCheckTimer = 0;
        private static long MemoryUsage_Unmanaged = 0;
        private static long MemoryUsage_Managed = 0;
        private const float MemoryUsageCheckInterval = 0.5f;

        public static readonly Color SELECTED_MESH_COLOR = Color.Yellow * 0.05f;
        public static readonly Color SELECTED_MESH_WIREFRAME_COLOR = Color.Yellow;

        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;
        public static Texture2D DEFAULT_TEXTURE_MISSING;
        public const string DEFAULT_TEXTURE_MISSING_NAME = "Content\\MissingTexture";

        public Rectangle ClientBounds => Window.ClientBounds;

        private static GraphicsDeviceManager graphics;
        //public ContentManager Content;
        //public bool IsActive = true;

        public static List<DisplayMode> GetAllResolutions()
        {
            List<DisplayMode> result = new List<DisplayMode>();
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                result.Add(mode);
            }
            return result;
        }

        public static void ApplyPresentationParameters(
            DisplayMode displayMode, bool vsync, bool fullscreen, bool simpleMsaa)
        {
            graphics.PreferMultiSampling = simpleMsaa;
            graphics.PreferredBackBufferWidth = displayMode.Width;
            graphics.PreferredBackBufferHeight = displayMode.Height;
            graphics.PreferredBackBufferFormat = displayMode.Format;
            graphics.IsFullScreen = fullscreen;
            FIXED_TIME_STEP = graphics.SynchronizeWithVerticalRetrace = vsync;
            
            graphics.ApplyChanges();
        }

       

        //MCG MCGTEST_MCG;

        

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            // Setting this max higher allows it to skip frames instead of do slow motion.
            MaxElapsedTime = TimeSpan.FromTicks(1000000);

            //IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;

            graphics.PreferMultiSampling = true;

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            GFX.Display.Mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

            //GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
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
            var winForm = (Form)Control.FromHandle(Window.Handle);
            winForm.AllowDrop = true;
            winForm.DragEnter += GameWindowForm_DragEnter;
            winForm.DragDrop += GameWindowForm_DragDrop;

            IsMouseVisible = true;

            DEFAULT_TEXTURE_DIFFUSE = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_DIFFUSE.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

            DEFAULT_TEXTURE_SPECULAR = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_SPECULAR.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

            DEFAULT_TEXTURE_NORMAL = new Texture2D(GraphicsDevice, 1, 1);
            DEFAULT_TEXTURE_NORMAL.SetData(new Color[] { new Color(0.5f, 0.5f, 1.0f) });

            DEFAULT_TEXTURE_MISSING = Content.Load<Texture2D>(DEFAULT_TEXTURE_MISSING_NAME);

            GFX.Device = GraphicsDevice;

            base.Initialize();
        }

        private void GameWindowForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] modelFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            InterrootLoader.LoadDragDroppedFiles(modelFiles);
        }

        private void GameWindowForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        protected override void LoadContent()
        {
            GFX.Init(Content);
            DBG.LoadContent(Content);
            InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            DBG.CreateDebugPrimitives();

            GFX.World.CameraTransform.Position = new Vector3(0, -1.5f, -13);
            GFX.World.CameraTransform.EulerRotation.X = MathHelper.PiOver4 / 8;

            DbgMenuItem.Init();

            UpdateMemoryUsage();
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        private string GetMemoryUseString(string prefix, long MemoryUsage)
        {
            const double MEM_KB = 1024f;
            const double MEM_MB = 1024f * 1024f;
            const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_KB)
                return $"{prefix}{(1.0 * MemoryUsage):0} B";
            else if (MemoryUsage < MEM_MB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_KB):0.00} KB";
            else if (MemoryUsage < MEM_GB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_MB):0.00} MB";
            else
                return $"{prefix}{(1.0 * MemoryUsage / MEM_GB):0.00} GB";
        }

        private void DrawMemoryUsage()
        {
            var str_managed = GetMemoryUseString("CLR Mem:  ", MemoryUsage_Managed);
            var str_unmanaged = GetMemoryUseString("Process Mem:  ", MemoryUsage_Unmanaged);

            var strSize_managed = DBG.DEBUG_FONT_SMALL.MeasureString(str_managed);
            var strSize_unmanaged = DBG.DEBUG_FONT_SMALL.MeasureString(str_unmanaged);

            DBG.DrawOutlinedText(str_managed, new Vector2(GFX.Device.Viewport.Width - 8, 
                GFX.Device.Viewport.Height - 8 - strSize_managed.Y - strSize_unmanaged.Y),
                Color.Yellow, DBG.DEBUG_FONT_SMALL, scaleOrigin: new Vector2(strSize_managed.X, 0));

            DBG.DrawOutlinedText(str_unmanaged, new Vector2(GFX.Device.Viewport.Width - 8, 
                GFX.Device.Viewport.Height - 8 - strSize_unmanaged.Y),
                Color.Yellow, DBG.DEBUG_FONT_SMALL, scaleOrigin: new Vector2(strSize_unmanaged.X, 0));
        }

        private void UpdateMemoryUsage()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                MemoryUsage_Unmanaged = proc.PrivateMemorySize64;
            }
            MemoryUsage_Managed = GC.GetTotalMemory(forceFullCollection: false);
        }

        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IsFixedTimeStep = FIXED_TIME_STEP;

            Active = IsActive;

            DbgMenuItem.UpdateInput(elapsed);
            DbgMenuItem.UICursorBlinkUpdate(elapsed);

            if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
            {
                // Only update input if debug menu isnt fully open.
                GFX.World.UpdateInput(this, gameTime);
            }

            GFX.World.UpdateMatrices(GraphicsDevice);

            GFX.World.CameraPositionDefault.Position = Vector3.Zero;

            GFX.World.CameraOrigin.Position = new Vector3(GFX.World.CameraPositionDefault.Position.X, 
                GFX.World.CameraOrigin.Position.Y, GFX.World.CameraPositionDefault.Position.Z);

            DBG.DbgPrim_Grid.Transform = GFX.World.CameraPositionDefault;

            if (REQUEST_EXIT)
                Exit();

            MemoryUsageCheckTimer += elapsed;
            if (MemoryUsageCheckTimer >= MemoryUsageCheckInterval)
            {
                MemoryUsageCheckTimer = 0;
                UpdateMemoryUsage();
            }

            LoadingTaskMan.Update(elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GFX.DrawScene(gameTime);
            DrawMemoryUsage();
        }
    }
}
