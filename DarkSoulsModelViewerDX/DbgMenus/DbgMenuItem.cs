using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public enum DbgMenuOpenState
    {
        Closed,
        Visible,
        Open
    }

    public class DbgMenuItem
    {
        public static void Init()
        {
            CurrentMenu.Text = "Main Menu";
            CurrentMenu.Items = new List<DbgMenuItem>()
            {
                //new DbgMenuItem()
                //{
                //    Text = "<-- TESTING -->",
                //    Items = new List<DbgMenuItem>
                //    {
                //        new DbgMenuItem()
                //        {
                //            Text = "Write TextureFetchRequest.DEBUG_AllKnownDS1Formats to Console",
                //            ClickAction = () =>
                //            {
                //                foreach (var f in TextureFetchRequest.DEBUG_AllKnownDS1Formats)
                //                {
                //                    Console.WriteLine(f.ToString());
                //                }
                //            }
                //        }
                //    }
                //},
                new DbgMenuItemSceneList()
                {
                    Text = "SCENE"
                },
                new DbgMenuItem()
                {
                    Text = "Game Data",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItemTextLabel(() => $"Data Root: \"{InterrootLoader.Interroot}\"\n     [Click to Browse...]")
                        {
                            ClickAction = (m) => InterrootLoader.Browse()
                        },
                        new DbgMenuItem()
                        {
                            Text = "Scan All Map Textures\n     (Needed for most objects)",
                            ClickAction = (m) =>
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                InterrootLoader.TexPool.AddMapTexUdsfm();
                                sw.Stop();
                                m.Text = $"Scan All Map Textures (Done. Took {sw.Elapsed.TotalSeconds} seconds)\n     (Needed for most objects)";
                            }
                        },
                        new DbgMenuItemSpawnChr(),
                        new DbgMenuItemSpawnObj(),
                        new DbgMenuItem()
                        {
                            Text = "Premade Test Scenes",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem()
                                {
                                    Text = "Test All CHR Lineup (Can take 30+ sec to load)",
                                    ClickAction = (m) =>
                                    {
                                        Stopwatch sw = Stopwatch.StartNew();
                                        GFX.ModelDrawer.ModelInstanceList.Clear();
                                        GFX.ModelDrawer.TestAddAllChr();
                                        sw.Stop();
                                        m.Text = $"Test All CHR Lineup (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)";
                                    }
                                },
                                new DbgMenuItem()
                                {
                                    Text = "Test All OBJ Lineup (Can take 30+ sec to load)",
                                    ClickAction = (m) =>
                                    {
                                        Stopwatch sw = Stopwatch.StartNew();
                                        GFX.ModelDrawer.ModelInstanceList.Clear();
                                        GFX.ModelDrawer.TestAddAllObj();
                                        sw.Stop();
                                        m.Text = $"Test All OBJ Lineup (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)";
                                    }
                                },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "Load Map",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() {Text = "m10_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(10, 00, false); m.Text = $"m10_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m10_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(10, 01, false); m.Text = $"m10_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m10_02_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(10, 02, false); m.Text = $"m10_02_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m11_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(11, 00, false); m.Text = $"m11_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m12_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(12, 00, false); m.Text = $"m12_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m12_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(12, 01, false); m.Text = $"m12_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m13_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(13, 00, false); m.Text = $"m13_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m13_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(13, 01, false); m.Text = $"m13_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m13_02_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(13, 02, false); m.Text = $"m13_02_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m14_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(14, 00, false); m.Text = $"m14_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m14_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(14, 01, false); m.Text = $"m14_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m15_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(15, 00, false); m.Text = $"m15_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m15_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(15, 01, false); m.Text = $"m15_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m16_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(16, 00, false); m.Text = $"m16_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m17_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(17, 00, false); m.Text = $"m17_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m18_00_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(18, 00, false); m.Text = $"m18_00_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                                new DbgMenuItem() {Text = "m18_01_00_00", ClickAction = m => { var sw = Stopwatch.StartNew();  GFX.ModelDrawer.AddMap(18, 01, false); m.Text = $"m18_01_00_00 (Loaded. Took {sw.Elapsed.TotalSeconds} seconds.)"; } },
                            }
                        },
                    }
                },
                //new DbgMenuItem()
                //{
                //    Text = "[DIAGNOSTICS]",
                //    Items = new List<DbgMenuItem>
                //    {
                //        new DbgMenuItem()
                //        {
                //            Text = $"Log {nameof(InterrootLoader)}.{nameof(InterrootLoader.DDS_INFO)}",
                //            ClickAction = () =>
                //            {
                //                foreach (var x in InterrootLoader.DDS_INFO)
                //                {
                //                    Console.WriteLine($"{x.Name} - {x.DDSFormat}");
                //                }
                //            }
                //        }
                //    }
                //},
                new DbgMenuItem()
                {
                    Text = "GFX",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItemGfxFlverShaderAdjust(),
                        //new DbgMenuItemGfxBlendStateAdjust(),
                        //new DbgMenuItemGfxDepthStencilStateAdjust(),
                        new DbgMenuItemNumber("Force LOD", -1, 2, 1,
                            (f) => GFX.ForceLOD = ((int)(Math.Round(f))), () => GFX.ForceLOD,
                            (f) => $"{((int)(Math.Round(f)))}"),
                        new DbgMenuItemNumber("LOD1 Distance", 0, 10000, 1,
                            (f) => 
                            {
                                GFX.LOD1Distance = f;
                                if (GFX.LOD1Distance > GFX.LOD2Distance)
                                    GFX.LOD2Distance = GFX.LOD1Distance;
                            }, () => GFX.LOD1Distance),
                        new DbgMenuItemNumber("LOD2 Distance", 0, 10000, 1,
                            (f) => 
                            {
                                GFX.LOD2Distance = f;
                                if (GFX.LOD2Distance < GFX.LOD1Distance)
                                    GFX.LOD1Distance = GFX.LOD2Distance;
                            }, () => GFX.LOD2Distance),
                        new DbgMenuItemBool("Show Model Names", "YES", "NO",
                            (b) => DBG.ShowModelNames = b, () => DBG.ShowModelNames),
                        new DbgMenuItemBool("Show Model Bounding Boxes", "YES", "NO",
                            (b) => DBG.ShowModelBoundingBoxes = b, () => DBG.ShowModelBoundingBoxes),
                        new DbgMenuItemBool("Show Model Submesh Bounding Boxes", "YES", "NO",
                            (b) => DBG.ShowModelSubmeshBoundingBoxes = b, () => DBG.ShowModelSubmeshBoundingBoxes),
                        new DbgMenuItemBool("Show Grid", "YES", "NO",
                            (b) => DBG.ShowGrid = b, () => DBG.ShowGrid),
                        new DbgMenuItemBool("Textures", "ON", "OFF",
                            (b) => GFX.EnableTextures = b, () => GFX.EnableTextures),
                        new DbgMenuItemBool("View Frustum Culling (Experimental)", "ON", "OFF",
                            (b) => GFX.EnableFrustumCulling = b, () => GFX.EnableFrustumCulling),
                        new DbgMenuItemNumber("Vertical Field of View (Degrees)", 20, 150, 1,
                            (f) => GFX.World.FieldOfView = f, () => GFX.World.FieldOfView,
                            (f) => $"{((int)(Math.Round(f)))}"),
                        new DbgMenuItemNumber("Camera Turn Speed (Gamepad)", 0.01f, 10f, 0.01f,
                            (f) => GFX.World.CameraTurnSpeedGamepad = f, () => GFX.World.CameraTurnSpeedGamepad),
                        new DbgMenuItemNumber("Camera Turn Speed (Mouse)", 0.001f, 10f, 0.001f,
                            (f) => GFX.World.CameraTurnSpeedMouse = f, () => GFX.World.CameraTurnSpeedMouse),
                        new DbgMenuItemNumber("Camera Move Speed", 0.1f, 100f, 0.1f,
                            (f) => GFX.World.CameraMoveSpeed = f, () => GFX.World.CameraMoveSpeed),
                        new DbgMenuItemNumber("Near Clip Distance", 0.0001f, 5, 0.0001f,
                            (f) => GFX.World.NearClipDistance = f, () => GFX.World.NearClipDistance),
                        new DbgMenuItemNumber("Far Clip Distance", 100, 1000000, 100,
                            (f) => GFX.World.FarClipDistance = f, () => GFX.World.FarClipDistance),

                    }
                },
                new DbgMenuItem()
                {
                    Text = "Help",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItem()
                        {
                            Text = "Menu Overlay Controls (Gamepad)",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Back: Toggle Menu (Active/Visible/Hidden)" },
                                new DbgMenuItem() { Text = "D-Pad Up/Down: Move Cursor Up/Down" },
                                new DbgMenuItem() { Text = "A: Enter/Activate (when applicable)" },
                                new DbgMenuItem() { Text = "B: Go Back (when applicable)" },
                                new DbgMenuItem() { Text = "D-Pad Left/Right: Decrease/Increase" },
                                new DbgMenuItem() { Text = "Start: Reset Value to Default" },
                                new DbgMenuItem() { Text = "Hold LB: Increase/Decrease 10x Faster" },
                                new DbgMenuItem() { Text = "Hold X: Increase/Decrease 100x Faster" },
                                new DbgMenuItem() { Text = "Hold RB + Move LS: Move Menu" },
                                new DbgMenuItem() { Text = "Hold RB + Move RS: Resize Menu" },
                                new DbgMenuItem() { Text = "Hold LB + Move or Resize Menu: Move or Resize Menu Faster" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "General 3D Controls (Gamepad)",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "LS: Move Camera Laterally" },
                                new DbgMenuItem() { Text = "LT: Move Camera Directly Downward" },
                                new DbgMenuItem() { Text = "RT: Move Camera Directly Upward" },
                                new DbgMenuItem() { Text = "RS: Turn Camera" },
                                new DbgMenuItem() { Text = "Hold LB: Move Camera More Slowly" },
                                new DbgMenuItem() { Text = "Hold RB: Move Camera More Quickly" },
                                new DbgMenuItem() { Text = "Click LS and Hold: Turn Light With RS Instead of Camera" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "Menu Overlay Controls (Mouse & Keyboard)",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Tilde (~): Toggle Menu (Active/Visible/Hidden)" },

                                new DbgMenuItem() { Text = "Move Mouse Cursor: Move Cursor" },
                                new DbgMenuItem() { Text = "Hold Spacebar + Scroll Mouse Wheel: Change Values" },
                                new DbgMenuItem() { Text = "Mouse Wheel: Scroll Menu" },
                                new DbgMenuItem() { Text = "Enter/Left Click: Enter/Activate (when applicable)" },
                                new DbgMenuItem() { Text = "Backspace/Right Click: Go Back (when applicable)" },
                                new DbgMenuItem() { Text = "Up/Down: Move Cursor Up/Down" },
                                new DbgMenuItem() { Text = "Left/Right: Decrease/Increase" },
                                new DbgMenuItem() { Text = "Home/Middle Click: Reset Value to Default" },
                                new DbgMenuItem() { Text = "Hold Shift: Increase/Decrease 10x Faster" },
                                new DbgMenuItem() { Text = "Hold Ctrl: Increase/Decrease 100x Faster" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "General 3D Controls (Mouse & Keyboard)",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "WASD: Move Camera Laterally" },
                                new DbgMenuItem() { Text = "Q: Move Camera Directly Downward" },
                                new DbgMenuItem() { Text = "E: Move Camera Directly Upward" },
                                new DbgMenuItem() { Text = "Right Click + Move Mouse: Turn Camera" },
                                new DbgMenuItem() { Text = "Hold Shift: Move Camera More Slowly" },
                                new DbgMenuItem() { Text = "Hold Ctrl: Move Camera More Quickly" },
                                new DbgMenuItem() { Text = "Hold Spacebar: Turn Light With Mouse Instead of Camera" },
                            }
                        },
                    }
                },
                new DbgMenuItem()
                {
                    Text = "Exit",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItem() { Text = "Are you sure you want to exit?" },
                        new DbgMenuItem()
                        {
                            Text = "No",
                            ClickAction = (m) => REQUEST_GO_BACK = true
                        },
                        new DbgMenuItem()
                        {
                            Text = "Yes",
                            ClickAction = (m) => MODEL_VIEWER_MAIN.REQUEST_EXIT = true
                        }
                    }
                },

            };
        }

        public const int ITEM_PADDING_REDUCE = 0;

        public const float MENU_MIN_SIZE_X = 256;
        public const float MENU_MIN_SIZE_Y = 128;

        public static SpriteFont FONT => DBG.DEBUG_FONT_UI;
        public static DbgMenuOpenState MenuOpenState = DbgMenuOpenState.Open;
        public static DbgMenuItem CurrentMenu = new DbgMenuItem();
        public static Stack<DbgMenuItem> DbgMenuStack = new Stack<DbgMenuItem>();
        public static Vector2 MenuPosition = Vector2.One * 8;
        public static Vector2 MenuSize = new Vector2(1280, 720);
        public static Rectangle MenuRect => new Rectangle(
            (int)MenuPosition.X, (int)MenuPosition.Y, (int)MenuSize.X, (int)MenuSize.Y);
        public static Rectangle SubMenuRect => new Rectangle(MenuRect.Left, 
            MenuRect.Top + 40, MenuRect.Width, MenuRect.Height - 40);

        public const float UICursorBlinkTimerMax = 0.5f;
        public static float UICursorBlinkTimer = 0;
        public static bool UICursorBlinkState = false;
        public static string UICursorBlinkString => UICursorBlinkState ? "◆" : "◇";

        public static void EnterNewSubMenu(DbgMenuItem menu)
        {
            DbgMenuStack.Push(CurrentMenu);
            CurrentMenu = menu;
        }

        public static void GoBack()
        {
            if (DbgMenuStack.Count > 0)
                CurrentMenu = DbgMenuStack.Pop();
        }

        public static bool REQUEST_GO_BACK = false;

        public string Text = " ";
        public int SelectedIndex = 0;
        public float Scroll;
        public float MaxScroll;
        public List<DbgMenuItem> Items = new List<DbgMenuItem>();
        public DbgMenuItem SelectedItem => SelectedIndex == -1 ? null : Items[SelectedIndex];
        public Action<DbgMenuItem> ClickAction = null;
        public virtual void OnClick()
        {
            ClickAction?.Invoke(this);
        }

        public virtual void OnResetDefault()
        {

        }

        public virtual void OnIncrease(bool isRepeat, int incrementAmount)
        {

        }

        public virtual void OnDecrease(bool isRepeat, int incrementAmount)
        {

        }

        public void GoDown(bool isRepeat, int incrementAmount)
        {
            int prevIndex = SelectedIndex;
            SelectedIndex += incrementAmount;

            //If upper bound reached
            if (SelectedIndex >= Items.Count)
            {
                //If already at end and just tapped button
                if (prevIndex == Items.Count - 1 && !isRepeat)
                    SelectedIndex = 0; //Wrap Around
                else
                    SelectedIndex = Items.Count - 1; //Stop
            }
        }

        public void GoUp(bool isRepeat, int incrementAmount)
        {
            int prevIndex = SelectedIndex;
            SelectedIndex -= incrementAmount;

            //If upper bound reached
            if (SelectedIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    SelectedIndex = Items.Count - 1; //Wrap Around
                else
                    SelectedIndex = 0; //Stop
            }
        }

        public static void UpdateInput(float elapsedSeconds)
        {
            DbgMenuPad.Update(elapsedSeconds);

            if (MenuOpenState == DbgMenuOpenState.Open)
            {
                int incrementAmount = 1;
                if (DbgMenuPad.MoveFastHeld)
                    incrementAmount *= 10;
                if (DbgMenuPad.MoveFasterHeld)
                    incrementAmount *= 100;

                if (DbgMenuPad.Up.State)
                    CurrentMenu.GoUp(!DbgMenuPad.Up.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Down.State)
                    CurrentMenu.GoDown(!DbgMenuPad.Down.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Left.State)
                    CurrentMenu.SelectedItem.OnDecrease(!DbgMenuPad.Left.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Right.State)
                    CurrentMenu.SelectedItem.OnIncrease(!DbgMenuPad.Right.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Enter.State)
                {
                    CurrentMenu.SelectedItem.OnClick();
                    if (CurrentMenu.SelectedItem.Items.Count > 0)
                    {
                        EnterNewSubMenu(CurrentMenu.SelectedItem);
                    }
                }

                if (DbgMenuPad.Cancel.State)
                    GoBack();

                if (REQUEST_GO_BACK)
                {
                    REQUEST_GO_BACK = false;
                    GoBack();
                }

                if (DbgMenuPad.ResetDefault.State)
                    CurrentMenu.SelectedItem.OnResetDefault();

                if (DbgMenuPad.MenuRectMove != Vector2.Zero)
                {
                    MenuPosition += DbgMenuPad.MenuRectMove;
                    MenuPosition.X = MathHelper.Clamp(MenuPosition.X, 0, GFX.Device.Viewport.Width - MenuSize.X);
                    MenuPosition.Y = MathHelper.Clamp(MenuPosition.Y, 0, GFX.Device.Viewport.Height - MenuSize.Y);
                }

                if (DbgMenuPad.MenuRectResize != Vector2.Zero)
                {
                    MenuSize += DbgMenuPad.MenuRectResize;
                    MenuSize.X = MathHelper.Clamp(MenuSize.X, MENU_MIN_SIZE_X, GFX.Device.Viewport.Width - MenuPosition.X);
                    MenuSize.Y = MathHelper.Clamp(MenuSize.Y, MENU_MIN_SIZE_Y, GFX.Device.Viewport.Height - MenuPosition.Y);
                }

                if (DbgMenuPad.IsMouseMovedThisFrame || DbgMenuPad.MouseWheelDelta != 0)
                {
                    for (int i = 0; i < CurrentMenu.Items.Count; i++)
                    {
                        var rect = CurrentMenu.GetItemDisplayRect(i, SubMenuRect);
                        if (rect.Contains(DbgMenuPad.MousePos))
                        {
                            CurrentMenu.SelectedIndex = i;
                            break;
                        }
                    }

                }

                var currentItemDisplayRect = CurrentMenu.GetItemDisplayRect(CurrentMenu.SelectedIndex, SubMenuRect);
                if (currentItemDisplayRect.Contains(DbgMenuPad.MousePos))
                {
                    if (DbgMenuPad.ClickMouse.State)
                    {
                        CurrentMenu.SelectedItem.OnClick();
                        if (CurrentMenu.SelectedItem.Items.Count > 0)
                        {
                            EnterNewSubMenu(CurrentMenu.SelectedItem);
                        }
                    }
                    else if (DbgMenuPad.MiddleClickMouse.State)
                    {
                        CurrentMenu.SelectedItem.OnResetDefault();
                    }
                }

                

                if (DbgMenuPad.MouseWheelDelta != 0)
                {
                    if (DbgMenuPad.IsSpacebarHeld)
                    {
                        bool isIncrease = DbgMenuPad.MouseWheelDelta > 0;
                        int incr = (int)Math.Abs(Math.Round(DbgMenuPad.MouseWheelDelta / 150));
                        for (int i = 0; i < incr; i++)
                        {
                            if (isIncrease)
                                CurrentMenu.SelectedItem.OnIncrease(false, incrementAmount);
                            else
                                CurrentMenu.SelectedItem.OnDecrease(false, incrementAmount);
                        }
                    }
                    else
                    {
                        CurrentMenu.Scroll -= DbgMenuPad.MouseWheelDelta;
                    }

                    
                }
            }
        }

        public virtual void UpdateUI()
        {

        }

        private Rectangle GetItemDisplayRect(int index, Rectangle menuRect)
        {
            float top = 0;
            for (int i = 0; i < index; i++)
            {
                top += GetItemSize(i).Y;
            }
            var thisItemSize = GetItemSize(index);
            return new Rectangle(menuRect.Left, (int)(menuRect.Top + top - Scroll), (int)(thisItemSize.X), (int)(thisItemSize.Y));
        }

        public string GetActualItemDisplayText(int i)
        {
            return $"{(SelectedIndex == i ? $"  {UICursorBlinkString} " : "     ")}{Items[i].Text}" +
                            $"{(Items[i].Items.Count > 0 ? $" ({Items[i].Items.Count})" : "")}";
        }

        private Vector2 GetItemSize(int i)
        {
            return FONT.MeasureString(GetActualItemDisplayText(i));
        }

        private float GetEntireMenuHeight()
        {
            float height = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                height += GetItemSize(i).Y;
            }
            return height;
        }

        public static void UICursorBlinkUpdate(float elapsedSeconds)
        {
            if (MenuOpenState == DbgMenuOpenState.Open)
            {
                UICursorBlinkTimer -= elapsedSeconds;
                if (UICursorBlinkTimer <= 0)
                {
                    UICursorBlinkState = !UICursorBlinkState;
                    UICursorBlinkTimer = UICursorBlinkTimerMax;
                }
            }
            else
            {
                UICursorBlinkTimer = UICursorBlinkTimerMax;
                // If menu is closed, have the cursor visible, ready for when its reopened
                // If menu is visible but not closed, make cursor not visible
                UICursorBlinkState = MenuOpenState == DbgMenuOpenState.Closed;
            }
        }

        public void Draw(float elapsedSeconds)
        {
            if (MenuOpenState != DbgMenuOpenState.Closed)
            {
                UpdateUI();

                float menuBackgroundOpacityMult = MenuOpenState == DbgMenuOpenState.Open ? 1.0f : 0.5f;

                // Draw menu background rect
                GFX.SpriteBatch.Begin();
                //---- Full Background
                GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE, MenuRect, Color.Black * 0.25f * menuBackgroundOpacityMult);
                //---- Slightly Darker Part On Top
                GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE, 
                    new Rectangle(MenuRect.X, MenuRect.Y, MenuRect.Width, 40), Color.Black * 0.25f * menuBackgroundOpacityMult);
                GFX.SpriteBatch.End();
                
                // Draw name on top
                var sb = new StringBuilder();
                //---- If in submenu, append the stack of menues preceding this one
                if (DbgMenuStack.Count > 0)
                {
                    bool first = true;
                    foreach (var chain in DbgMenuStack.Reverse())
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(" > ");
                        sb.Append($"{chain.Text}{(chain.Items.Count > 0 ? $" ({chain.Items.Count})" : "")}");
                    }
                    sb.Append(" > ");
                }
                //---- Append the current menu name.
                sb.Append($"{Text}{(Items.Count > 0 ? $" ({Items.Count})" : "")}");

                //---- Draw full menu name
                DBG.DrawOutlinedText(sb.ToString(), MenuRect.TopLeftCorner() + new Vector2(8, 4), Color.White, DBG.DEBUG_FONT_BIG);

                var selectedItemRect = GetItemDisplayRect(SelectedIndex, SubMenuRect);

                float menuHeight = GetEntireMenuHeight();

                // Only need to calculate scroll stuff if there's text that reaches past the bottom.
                if (menuHeight > SubMenuRect.Height)
                {
                    // Scroll selected into view.

                    //---- If item is ABOVE view
                    if (selectedItemRect.Top < SubMenuRect.Top)
                    {
                        int distanceNeededToScroll = SubMenuRect.Top - selectedItemRect.Top;
                        Scroll -= distanceNeededToScroll;
                    }
                    //---- If item is BELOW view
                    if (selectedItemRect.Bottom > SubMenuRect.Bottom)
                    {
                        int distanceNeededToScroll = selectedItemRect.Bottom - SubMenuRect.Bottom;
                        Scroll += distanceNeededToScroll;
                    }
                }

                

                // Clamp scroll

                MaxScroll = Math.Max(GetEntireMenuHeight() - SubMenuRect.Height, 0);
                if (Scroll > MaxScroll)
                    Scroll = MaxScroll;
                else if (Scroll < 0)
                    Scroll = 0;

                // Debug display of menu item rectangles:
                //for (int i = 0; i < Items.Count; i++)
                //{
                //    var TEST_DebugDrawItemRect = GetItemDisplayRect(i, SubMenuRect);

                //    GFX.SpriteBatch.Begin();
                //    GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE, TEST_DebugDrawItemRect, Color.Yellow);
                //    GFX.SpriteBatch.End();
                //}

                // Store current viewport, then switch viewport to JUST the menu rect
                var oldViewport = GFX.Device.Viewport;
                GFX.Device.Viewport = new Viewport(
                    oldViewport.X + SubMenuRect.X,
                    oldViewport.Y + SubMenuRect.Y, 
                    SubMenuRect.Width,
                    SubMenuRect.Height);
                // ---- These braces manually force a smaller scope so we 
                //      don't forget to return to the old viewport immediately afterward.
                {
                    // Draw Items

                    for (int i = 0; i < Items.Count; i++)
                    {
                        Items[i].UpdateUI();
                        var entryText = GetActualItemDisplayText(i);

                        var itemRect = GetItemDisplayRect(i, SubMenuRect);

                        // Check if this item is inside the actual menu rectangle.
                        if (SubMenuRect.Intersects(itemRect))
                        {
                            // We have to SUBTRACT the menu top/left coord because the string 
                            // drawing is relative to the VIEWPORT, which takes up just the actual menu rect
                            DBG.DrawOutlinedText(entryText, 
                                new Vector2(itemRect.X - SubMenuRect.X, itemRect.Y - SubMenuRect.Y),
                                (SelectedIndex == i && MenuOpenState == DbgMenuOpenState.Open) 
                                    ? Color.LightGreen : Color.White, FONT, disableSmoothing: true);
                        }

                    }

                    // Draw Scrollbar
                    // Only if there's stuff that passes the bottom of the menu.
                    if (menuHeight > SubMenuRect.Height)
                    {
                        GFX.SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront);

                        //---- Draw Scrollbar Background
                        GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE, 
                            new Rectangle(0, 0, 8, SubMenuRect.Height), Color.White * 0.5f * menuBackgroundOpacityMult);

                        float curScrollRectTop = (Scroll / menuHeight) * SubMenuRect.Height;
                        float curScrollRectHeight = (SubMenuRect.Height / menuHeight) * SubMenuRect.Height;

                        //---- Scroll Scrollbar current scroll
                        GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE,
                            new Rectangle(0, (int)curScrollRectTop, 8, (int)curScrollRectHeight),
                            Color.White * 0.75f * menuBackgroundOpacityMult);

                        GFX.SpriteBatch.End();
                    }

                    
                }

                //---- Return to old viewport
                GFX.Device.Viewport = oldViewport;
            }
            
        }
    }
}
