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

        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;

        public Rectangle ClientBounds => Window.ClientBounds;

        //MCG MCGTEST_MCG;

        

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            MaxElapsedTime = TimeSpan.FromTicks(166667);

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

            //////// Here is where you should load things for testing. ////////



            ///////////////////////////////////////////////////////////////////

            DBG.CreateDebugPrimitives();

            GFX.World.CameraTransform.Position = new Vector3(0, -1.5f, -13);
            GFX.World.CameraTransform.EulerRotation.X = MathHelper.PiOver4 / 8;

            DbgMenuItem.Init();
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        protected override void Update(GameTime gameTime)
        {
            IsFixedTimeStep = FIXED_TIME_STEP;

            Active = IsActive;

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

            DBG.DbgPrim_Grid.Transform = GFX.World.CameraPositionDefault;

            if (REQUEST_EXIT)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GFX.DrawScene(gameTime);
        }
    }
}
