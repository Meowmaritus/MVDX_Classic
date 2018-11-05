using MeowDSIO;
using MeowDSIO.DataFiles;
using SoulsFormats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public static class TexturePool
    {
        private static object _lock_IO = new object();
        //This might be weird because it doesn't follow convention :fatcat:
        public delegate void TextureLoadErrorDelegate(string texName, string error);
        public static event TextureLoadErrorDelegate OnLoadError;
        private static void RaiseLoadError(string texName, string error)
        {
            OnLoadError?.Invoke(texName, error);
        }

        //private Dictionary<string, string> OnDemandTexturePaths = new Dictionary<string, string>();
        private static Dictionary<string, TextureFetchRequest> Fetches = new Dictionary<string, TextureFetchRequest>();

        public static void Flush()
        {
            foreach (var fetch in Fetches)
            {
                fetch.Value.Dispose();
            }
            Fetches.Clear();
        }

        public static void AddFetch(SoulsFormats.TPF tpf, string texName)
        {
            string shortName = Path.GetFileNameWithoutExtension(texName);
            if (!Fetches.ContainsKey(shortName))
            {
                var newFetch = new TextureFetchRequest(tpf, texName);
                Fetches.Add(shortName, newFetch);
            }
                
        }

        public static void AddTpf(SoulsFormats.TPF tpf)
        {
            foreach (var tex in tpf.Textures)
            {
                AddFetch(tpf, tex.Name);
            }
        }

        public static void AddTextureBnd(BND chrbnd, IProgress<double> prog)
        {
            var tpfFiles = chrbnd.Where(x => x.Name.EndsWith(".tpf")).ToList();
            int i = 0;
            foreach (var t in tpfFiles)
            {
                var tpf = SoulsFormats.TPF.Read(t.GetBytes());
                AddTpf(tpf);
                prog?.Report(1.0 * (++i) / tpfFiles.Count);
            }
        }

        public static void AddTpfFromPath(string path)
        {
            SoulsFormats.TPF tpf = InterrootLoader.DirectLoadTpf(path);
            AddTpf(tpf);
        }

        public static void AddAllExternalDS1TexturesInBackground()
        {
            LoadingTaskMan.DoLoadingTask($"AddAllExternalDS1TexturesInBackground_UDSFM_MAP", $"Loading external map textures for DS1...", prog =>
            {
                //UDSFM MAP TEX
                var dir = InterrootLoader.GetInterrootPath(@"map\tx");
                if (Directory.Exists(dir))
                {
                    var mapTpfFileNames = Directory.GetFiles(dir);
                    int i = 0;
                    foreach (var t in mapTpfFileNames)
                    {
                        AddTpfFromPath(t);
                        prog?.Report(1.0 * (++i) / mapTpfFileNames.Length);
                    }
                }
                GFX.ModelDrawer.RequestTextureLoad();
            });

            LoadingTaskMan.DoLoadingTask($"AddAllExternalDS1TexturesInBackground_UDSFM_CHR", $"Loading external boss character textures for DS1...", prog =>
            {
                // UDSFM CHR TEX
                var udsfmTexFolderPath = InterrootLoader.GetInterrootPath($@"chr");
                if (Directory.Exists(udsfmTexFolderPath))
                {
                    var subDirectories = Directory.GetDirectories(udsfmTexFolderPath);
                    int i = 0;
                    foreach (var subDir in subDirectories)
                    {
                        var chrTpfFileNames = Directory.GetFiles(subDir, "*.tpf");
                        foreach (var t in chrTpfFileNames)
                        {
                            AddTpfFromPath(t);
                        }
                        prog?.Report(1.0 * (++i) / subDirectories.Length);
                    }
                }

                GFX.ModelDrawer.RequestTextureLoad();
            });

            LoadingTaskMan.DoLoadingTask($"AddAllExternalDS1TexturesInBackground_CHRBND_9", $"Loading external character textures for DS1...", prog =>
            {
                // CHRBND-9
                var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"chr"), "*9.chrbnd");
                foreach (var ctew9 in chrbndsThatEndWith9)
                {
                    BND entityBnd = null;
                    lock (_lock_IO)
                    {
                        entityBnd = DataFile.LoadFromFile<BND>(ctew9);
                    }
                    AddTextureBnd(entityBnd, prog);
                }

                GFX.ModelDrawer.RequestTextureLoad();
            });

            LoadingTaskMan.DoLoadingTask($"AddAllExternalDS1TexturesInBackground_OBJBND_9", $"Loading external object textures for DS1...", prog =>
            {
                // CHRBND-9
                var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"obj"), "*9.objbnd");
                foreach (var ctew9 in chrbndsThatEndWith9)
                {
                    BND entityBnd = null;
                    lock (_lock_IO)
                    {
                        entityBnd = DataFile.LoadFromFile<BND>(ctew9);
                    }
                    AddTextureBnd(entityBnd, prog);
                }

                GFX.ModelDrawer.RequestTextureLoad();
            });
        }

        public static void AddMapTexBhds(int area, IProgress<double> prog)
        {
            var dir = InterrootLoader.GetInterrootPath($"map\\m{area:D2}");
            if (!Directory.Exists(dir))
                return;
            var mapTpfFileNames = Directory.GetFiles(dir, "*.tpfbhd");
            int fileIndex = 0;
            foreach (var t in mapTpfFileNames)
            {
                BXF4 bxf = null;
                lock (_lock_IO)
                {
                    bxf = BXF4.Read(t, t.Substring(0, t.Length - 7) + ".tpfbdt");
                }

                for (int i = 0; i < bxf.Files.Count; i++)
                {
                    if (bxf.Files[i].Name.Contains(".tpf"))
                    {
                        var tpf = SoulsFormats.TPF.Read(bxf.Files[i].Bytes);

                        foreach (var tn in tpf.Textures)
                        {
                            AddFetch(tpf, tn.Name);
                        }

                        tpf = null;
                    }
                    GFX.ModelDrawer.RequestTextureLoad();
                    // Report each subfile as a tiny part of the bar
                    prog?.Report((1.0 * fileIndex / mapTpfFileNames.Length) + ((1.0 / mapTpfFileNames.Length) * ((i + 1.0) / bxf.Files.Count)));
                }
                bxf = null;

                fileIndex++;
                prog?.Report((1.0 * fileIndex / mapTpfFileNames.Length));
            }

            GFX.ModelDrawer.RequestTextureLoad();
        }

        public static Texture2D FetchTexture(string name)
        {
            if (name == null)
                return null;
            var shortName = Path.GetFileNameWithoutExtension(name);
            if (Fetches.ContainsKey(shortName))
            {
                return Fetches[shortName].Fetch();
            }
            else
            {
                return null;
            }
        }
    }
}
