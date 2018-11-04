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
                Fetches.Add(shortName, new TextureFetchRequest(tpf, texName));
        }

        public static void AddTpf(SoulsFormats.TPF tpf)
        {
            foreach (var tex in tpf.Textures)
            {
                AddFetch(tpf, tex.Name);
            }
        }

        public static void AddTextureBnd(BND chrbnd)
        {
            var tpfFiles = chrbnd.Where(x => x.Name.EndsWith(".tpf"));
            foreach (var t in tpfFiles)
            {
                var tpf = SoulsFormats.TPF.Read(t.GetBytes());
                AddTpf(tpf);
            }
        }

        public static void AddTpfFromPath(string path)
        {
            SoulsFormats.TPF tpf = InterrootLoader.DirectLoadTpf(path);
            AddTpf(tpf);
        }

        public static void AddMapTexUdsfm()
        {
            var thread = new Thread(() =>
            {
                var dir = InterrootLoader.GetInterrootPath(@"map\tx");
                if (!Directory.Exists(dir))
                    return;
                var mapTpfFileNames = Directory.GetFiles(dir);
                foreach (var t in mapTpfFileNames)
                {
                    AddTpfFromPath(t);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void AddChrTexUdsfm()
        {
            var udsfmTexFolderPath = InterrootLoader.GetInterrootPath($@"chr");
            if (Directory.Exists(udsfmTexFolderPath))
            {
                var subDirectories = Directory.GetDirectories(udsfmTexFolderPath);

                foreach (var subDir in subDirectories)
                {
                    var chrTpfFileNames = Directory.GetFiles(subDir, "*.tpf");
                    foreach (var t in chrTpfFileNames)
                    {
                        AddTpfFromPath(t);
                    }
                }
            }
        }

        public static void AddChrBndsThatEndIn9()
        {
            var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"chr"), "*9.chrbnd");
            foreach (var ctew9 in chrbndsThatEndWith9)
            {
                BND entityBnd = null;
                lock (_lock_IO)
                {
                    entityBnd = DataFile.LoadFromFile<BND>(ctew9);
                }
                AddTextureBnd(entityBnd);
            }
        }

        public static void AddObjBndsThatEndIn9()
        {
            var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"obj"), "*9.objbnd");
            foreach (var ctew9 in chrbndsThatEndWith9)
            {
                BND entityBnd = null;
                lock (_lock_IO)
                {
                    entityBnd = DataFile.LoadFromFile<BND>(ctew9);
                }
                AddTextureBnd(entityBnd);
            }
        }

        public static void AddChrBnd(int id, int idx)
        {
            var path = InterrootLoader.GetInterrootPath($@"chr\c{id:D4}.texbnd.dcx");
            if (!File.Exists(path))
            {
                // Bloodborne being a special snowflake :fatcat:
                path = InterrootLoader.GetInterrootPath($@"chr\c{id:D4}.chrbnd.dcx");
            }

            lock (_lock_IO)
            {
                var bnd = BND4.Read(path);
                int i;
                for (i = 0; i < bnd.Files.Count(); i++)
                {
                    if (bnd.Files[i].Name.Contains(".tpf"))
                        break;
                }
                if (i == bnd.Files.Count())
                    return; // No textures I guess
                var tpf = SoulsFormats.TPF.Read(bnd.Files[i].Bytes);

                foreach (var tn in tpf.Textures)
                {
                    AddFetch(tpf, tn.Name);
                }
            }
        }

        /*public static void AddObjBnd(int id, int idx)
        {
            /*var path = InterrootLoader.GetInterrootPath($@"obj\o{id:D4}.objbnd");

            var texNames = FLVEROptimized.ReadTextureNamesFromBnd(path, idx);

            foreach (var tn in texNames)
            {
                AddFetch(TextureFetchRequestType.EntityBnd, path, tn);
            }
                var tpf = t.ReadDataAs<TPF>();
                AddTpf(tpf);
            
        }*/



        public static void AddMapTexBhds(int area)
        {
            var dir = InterrootLoader.GetInterrootPath($"map\\m{area:D2}");
            if (!Directory.Exists(dir))
                return;
            var mapTpfFileNames = Directory.GetFiles(dir);
            foreach (var t in mapTpfFileNames)
            {
                if (t.EndsWith(".tpfbhd"))
                {
                    lock (_lock_IO)
                    {
                        BXF4 bxf = BXF4.Read(t, t.Substring(0, t.Length - 7) + ".tpfbdt");
                        int i;
                        for (i = 0; i < bxf.Files.Count(); i++)
                        {
                            if (bxf.Files[i].Name.Contains(".tpf"))
                            {
                                var tpf = SoulsFormats.TPF.Read(bxf.Files[i].Bytes);

                                foreach (var tn in tpf.Textures)
                                {
                                    AddFetch(tpf, tn.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Texture2D FetchTexture(string name)
        {
            lock (_lock_IO)
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
}
