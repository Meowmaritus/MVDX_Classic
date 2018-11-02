using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DarkSoulsModelViewerDX
{
    public class InterrootLoader
    {
        static InterrootLoader()
        {
            if (File.Exists("DSMVDX_InterrootPath.txt"))
                Interroot = File.ReadAllText("DSMVDX_InterrootPath.txt").Trim('\n');

            TexturePool.OnLoadError += TexPool_OnLoadError;
        }

        public static void SaveInterrootPath()
        {
            File.WriteAllText("DSMVDX_InterrootPath.txt", Interroot);
        }

        public static void Browse()
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                FileName = "DARKSOULS.exe",
                Filter = "EXEs (*.exe)|*.exe",
                Title = "Select DARKSOULS.exe",
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Interroot = new FileInfo(dlg.FileName).DirectoryName;
                SaveInterrootPath();
            }
        }

        //This might be weird because it doesn't follow convention :fatcat:
        public delegate void FatcatContentLoadErrorDelegate(string contentName, string error);
        public static event FatcatContentLoadErrorDelegate OnLoadError;
        private static void RaiseLoadError(string contentName, string error)
        {
            OnLoadError?.Invoke(contentName, error);
        }

        private static void TexPool_OnLoadError(string texName, string error)
        {
            RaiseLoadError($"TexPool: \"{texName}\"", error);
        }

        private static string Frankenpath(params string[] pathParts)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pathParts.Length; i++)
            {
                sb.Append(pathParts[i].Trim('\\'));
                if (i < pathParts.Length - 1)
                    sb.Append('\\');
            }

            return sb.ToString();
        }

        public static string GetInterrootPath(string relPath)
        {
            return Frankenpath(Interroot, relPath);
        }

        public static string Interroot = @"G:\SteamLibrary\steamapps\common\Dark Souls Prepare to Die Edition\DATA";

        public static EntityBND DirectLoadEntityBnd(string path)
        {
            var fileName = path;
            if (!File.Exists(fileName))
                return null;
            return DataFile.LoadFromFile<EntityBND>(fileName);
        }

        public static EntityBND LoadChr(int id)
        {
            return DirectLoadEntityBnd(GetInterrootPath($@"chr\c{id:D4}.chrbnd"));
        }

        public static EntityBND LoadObj(int id)
        {
            return DirectLoadEntityBnd(GetInterrootPath($@"obj\o{id:D4}.objbnd"));
        }

        public static List<TPF> DirectLoadAllTpfInDir(string relPath)
        {
            var path = GetInterrootPath(relPath);
            if (!Directory.Exists(path))
                return new List<TPF>();

            var tpfNames = Directory.GetFiles(path, "*.tpf");
            return tpfNames
                .Select(x => DataFile.LoadFromFile<TPF>(x))
                .ToList();
        }

        public static List<TPF> LoadChrTexUdsfm(int id)
        {
            return DirectLoadAllTpfInDir($@"chr\c{id:D4}");
        }

        public static TPF DirectLoadTpf(string path)
        {
            return DataFile.LoadFromFile<TPF>(path);
        }

        public static Model LoadModelChr(int id, int idx)
        {
            var chrbnd = LoadChr(id);
            if (chrbnd == null)
                return null;
            TexturePool.AddChrBnd(id, idx);
            TexturePool.AddChrTexUdsfm(id);
            return new Model(chrbnd.Models[0].Mesh);
        }

        public static Model LoadModelChrOptimized(int id, int idx)
        {
            var name = GetInterrootPath($@"chr\c{id:D4}.chrbnd");

            if (!File.Exists(name))
                return null;

            TexturePool.AddChrBnd(id, idx);
            TexturePool.AddChrTexUdsfm(id);

            return new Model(FLVEROptimized.ReadFromBnd(name, 0));
        }

        public static Model LoadModelObjOptimized(int id, int idx)
        {
            var name = GetInterrootPath($@"obj\o{id:D4}.objbnd");

            if (!File.Exists(name))
                return null;

            TexturePool.AddObjBnd(id, idx);

            return new Model(FLVEROptimized.ReadFromBnd(name, 0));
        }

        public static Model LoadModelObj(int id, int idx)
        {
            var chrbnd = LoadObj(id);
            if (chrbnd == null)
                return null;
            TexturePool.AddObjBnd(id, idx);
            return new Model(chrbnd.Models[0].Mesh);
        }

        public static void LoadMapInBackground(int area, int block, bool excludeScenery, Action<ModelInstance> addMapModel)
        {
            var modelDir = GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00");
            var modelDict = new Dictionary<string, FLVER>();
            //foreach (var mfn in modelFileNames)
            //{
            //    if (excludeScenery && (mfn.StartsWith("m8") || mfn.StartsWith("m9")))
            //        continue;
            //    modelDict.Add(MiscUtil.GetFileNameWithoutDirectoryOrExtension(mfn), DataFile.LoadFromFile<FLVER>(mfn));
            //}

            FLVER loadModel(string modelName)
            {
                if (!modelDict.ContainsKey(modelName + $"A{area:D2}"))
                    modelDict.Add(modelName + $"A{area:D2}", DataFile.LoadFromFile<FLVER>(Path.Combine(modelDir, modelName + $"A{area:D2}" + ".flver")));

                return modelDict[modelName + $"A{area:D2}"];
            }

            var msb = DataFile.LoadFromFile<MSB>(GetInterrootPath($@"map\MapStudio\m{area:D2}_{block:D2}_00_00.msb"));
            foreach (var part in msb.Parts.MapPieces)
            {
                if (excludeScenery && (part.ModelName.StartsWith("m8") || part.ModelName.StartsWith("m9") || !part.IsShadowDest))
                    continue;
                var model = new Model(loadModel(part.ModelName));
                addMapModel.Invoke(new ModelInstance(part.Name, model, new Transform(part.PosX, part.PosY, part.PosZ, part.RotX, part.RotY, part.RotZ)));
            }

            modelDict = null;
        }

        public static void LoadDragDroppedFiles(string[] fileNames)
        {
            foreach (var fn in fileNames)
            {
                var upper = fn.ToUpper();
                if (upper.EndsWith(".CHRBND") || upper.EndsWith(".OBJBND") || upper.EndsWith(".PARTSBND"))
                {
                    var entityBnd = InterrootLoader.DirectLoadEntityBnd(fn);
                    var modelShortName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(fn);
                    for (int i = 0; i < entityBnd.Models.Count; i++)
                    {
                        string instanceName = modelShortName + (i > 0 ? $"_{i}" : "");
                        GFX.ModelDrawer.AddModelInstance(
                            new ModelInstance(
                                instanceName, 
                                new Model(entityBnd.Models[i].Mesh), 
                                GFX.World.GetSpawnPointFromMouseCursor(10.0f, false, true, true)
                                ));
                        foreach (var tex in entityBnd.Models[i].Textures)
                        {
                            TexturePool.AddFetch(TextureFetchRequestType.EntityBnd, fn, tex.Key);
                        }
                    }
                }
            }
        }
    }
}
