using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public static class TexturePool
    {
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
                fetch.Value.Flush();
            }
        }

        public static void AddFetch(TextureFetchRequestType type, string fetchUri, string texName)
        {
            string shortName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(texName);
            if (!Fetches.ContainsKey(shortName))
                Fetches.Add(shortName, new TextureFetchRequest(type, fetchUri, texName));
        }

        public static void AddChrBnd(int id, int idx)
        {
            var path = InterrootLoader.GetInterrootPath($@"chr\c{id:D4}.chrbnd");

            var texNames = FLVEROptimized.ReadTextureNamesFromBnd(path, idx);

            foreach (var tn in texNames)
            {
                AddFetch(TextureFetchRequestType.EntityBnd, path, tn);
            }
        }

        public static void AddObjBnd(int id, int idx)
        {
            var path = InterrootLoader.GetInterrootPath($@"obj\o{id:D4}.objbnd");

            var texNames = FLVEROptimized.ReadTextureNamesFromBnd(path, idx);

            foreach (var tn in texNames)
            {
                AddFetch(TextureFetchRequestType.EntityBnd, path, tn);
            }
        }

        public static void AddTpf(TPF tpf)
        {
            foreach (var thing in tpf)
            {
                AddFetch(TextureFetchRequestType.Tpf, tpf.FilePath, thing.Name);
            }
        }

        public static void AddTpfFromPath(string path)
        {
            var tpf = InterrootLoader.DirectLoadTpf(path);
            AddTpf(tpf);
        }

        public static void AddMapTexUdsfm()
        {
            var dir = InterrootLoader.GetInterrootPath(@"map\tx");
            if (!Directory.Exists(dir))
                return;
            var mapTpfFileNames = Directory.GetFiles(dir);
            foreach (var t in mapTpfFileNames)
            {
                AddTpfFromPath(t);
            }
        }

        public static void AddChrTexUdsfm(int id)
        {
            var udsfmTexFolderPath = InterrootLoader.GetInterrootPath($@"chr\c{id:D4}");
            if (Directory.Exists(udsfmTexFolderPath))
            {
                var chrTpfFileNames = Directory.GetFiles(udsfmTexFolderPath);
                foreach (var t in chrTpfFileNames)
                {
                    AddTpfFromPath(t);
                }
            }
            
        }

        public static void AddChrBndsThatEndIn9()
        {
            var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"chr"), "*9.chrbnd");
            foreach (var ctew9 in chrbndsThatEndWith9)
            {
                var entityBnd = InterrootLoader.DirectLoadEntityBnd($@"chr\{MiscUtil.GetFileNameWithoutDirectoryOrExtension(ctew9)}.chrbnd");
                foreach (var m in entityBnd.Models)
                {
                    foreach (var t in m.Textures)
                    {
                        AddFetch(TextureFetchRequestType.EntityBnd, entityBnd.FilePath, t.Key);
                    }
                }
            }
        }

        public static void AddObjBndsThatEndIn9()
        {
            var chrbndsThatEndWith9 = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"obj"), "*9.objbnd");
            foreach (var ctew9 in chrbndsThatEndWith9)
            {
                var entityBnd = InterrootLoader.DirectLoadEntityBnd(
                    InterrootLoader.GetInterrootPath($@"obj\{MiscUtil.GetFileNameWithoutDirectoryOrExtension(ctew9)}.objbnd"));
                foreach (var m in entityBnd.Models)
                {
                    foreach (var t in m.Textures)
                    {
                        AddFetch(TextureFetchRequestType.EntityBnd, entityBnd.FilePath, t.Key);
                    }
                }
            }
        }

        public static Texture2D FetchTexture(string name)
        {
            if (name == null)
                return null;
            var shortName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(name);
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
