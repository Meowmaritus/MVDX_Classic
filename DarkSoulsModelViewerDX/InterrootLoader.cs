using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.MSB;
using Microsoft.Xna.Framework;
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
        private static object _lock_IO = new object();

        static InterrootLoader()
        {
            lock (_lock_IO)
            {
                if (File.Exists("DSMVDX_InterrootPath.txt"))
                    Interroot = File.ReadAllText("DSMVDX_InterrootPath.txt").Trim('\n');
            }
            

            TexturePool.OnLoadError += TexPool_OnLoadError;
        }

        public static void SaveInterrootPath()
        {
            lock (_lock_IO)
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

        public static List<TPF> DirectLoadAllTpfInDir(string relPath)
        {
            lock (_lock_IO)
            {
                var path = GetInterrootPath(relPath);
                if (!Directory.Exists(path))
                    return new List<TPF>();

                var tpfNames = Directory.GetFiles(path, "*.tpf");
                return tpfNames
                    .Select(x => DataFile.LoadFromFile<TPF>(x))
                    .ToList();
            }
               
        }

        public static List<TPF> LoadChrTexUdsfm(int id)
        {
            return DirectLoadAllTpfInDir($@"chr\c{id:D4}");
        }

        public static TPF DirectLoadTpf(string path)
        {
            lock (_lock_IO)
                return DataFile.LoadFromFile<TPF>(path);
        }

        public static List<Model> LoadModelsFromBnd(BND bnd)
        {
            var modelEntries = bnd.Where(x => x.Name.ToUpper().EndsWith(".FLVER"));
            if (modelEntries.Any())
                return modelEntries.Select(x => new Model(x.ReadDataAs<FLVER>())).ToList();
            else
                return new List<Model>();
        }


        public static List<Model> LoadModelChr(int id)
        {
            var bndName = GetInterrootPath($@"chr\c{id:D4}.chrbnd");

            if (File.Exists(bndName))
            {
                BND bnd = null;
                lock (_lock_IO)
                {
                    bnd = DataFile.LoadFromFile<BND>(bndName);
                }
                var models = LoadModelsFromBnd(bnd);
                TexturePool.AddTextureBnd(bnd);
                return models;
            }

            return new List<Model>();
        }

        public static List<Model> LoadModelObj(int id)
        {
            var bndName = GetInterrootPath($@"obj\o{id:D4}.objbnd");

            if (File.Exists(bndName))
            {
                BND bnd = null;
                lock (_lock_IO)
                {
                    bnd = DataFile.LoadFromFile<BND>(bndName);
                }
                var models = LoadModelsFromBnd(bnd);
                TexturePool.AddTextureBnd(bnd);
                return models;
            }

            return new List<Model>();
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

            FLVER loadModel(string modelName, PartsParamSubtype partType)
            {
                if (!modelDict.ContainsKey(modelName))
                {
                    FLVER flver = null;

                    lock (_lock_IO)
                    {
                        switch (partType)
                        {
                            case PartsParamSubtype.MapPieces:
                                flver = DataFile.LoadFromFile<FLVER>(
                                    GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00\{modelName}A{area:D2}.flver"));
                                break;
                            case PartsParamSubtype.NPCs:
                            case PartsParamSubtype.DummyNPCs:
                            case PartsParamSubtype.Objects:
                            case PartsParamSubtype.DummyObjects:
                                string bndRelPath = (partType == PartsParamSubtype.Objects
                                    || partType == PartsParamSubtype.DummyObjects)
                                    ? $@"obj\{modelName}.objbnd" : $@"chr\{modelName}.chrbnd";


                                var bnd = DataFile.LoadFromFile<BND>(GetInterrootPath(bndRelPath));
                                foreach (var entry in bnd)
                                {
                                    var compareName = entry.Name.ToUpper();
                                    if (flver == null && compareName.EndsWith(".FLVER"))
                                        flver = entry.ReadDataAs<FLVER>();
                                    else if (compareName.EndsWith(".TPF"))
                                        TexturePool.AddTpf(entry.ReadDataAs<TPF>());
                                }
                                break;
                        }
                    }

                    modelDict.Add(modelName, flver);
                }

                if (modelDict.ContainsKey(modelName))
                    return modelDict[modelName];
                else
                    return null;
            }

            var msb = DataFile.LoadFromFile<MSB>(GetInterrootPath($@"map\MapStudio\m{area:D2}_{block:D2}_00_00.msb"));
            foreach (var part in msb.Parts.GlobalList)
            {
                //if (excludeScenery && (part.ModelName.StartsWith("m8") || part.ModelName.StartsWith("m9") || !part.IsShadowDest))
                //    continue;

                var partSubtype = part.GetSubtypeValue();

                var flverMesh = loadModel(part.ModelName, partSubtype);

                if (flverMesh != null)
                {
                    var model = new Model(flverMesh);

                    var partModelInstance = new ModelInstance(part.Name, model, new Transform(part.PosX, part.PosY, part.PosZ,
                        MathHelper.ToRadians(part.RotX), MathHelper.ToRadians(part.RotY), MathHelper.ToRadians(part.RotZ),
                        part.ScaleX, part.ScaleY, part.ScaleZ), part.DrawGroup1, part.DrawGroup2, part.DrawGroup3, part.DrawGroup4);

                    if (partSubtype == PartsParamSubtype.DummyNPCs || partSubtype == PartsParamSubtype.DummyObjects)
                    {
                        partModelInstance.IsDummyMapPart = true;
                    }

                    addMapModel.Invoke(partModelInstance);
                }
                
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
                    BND bnd = null;
                    lock (_lock_IO)
                    {
                        bnd = DataFile.LoadFromFile<BND>(fn);
                    }
                    TexturePool.AddTextureBnd(bnd);
                    var models = LoadModelsFromBnd(bnd);
                    foreach (var m in models)
                    {
                        GFX.ModelDrawer.AddModelInstance(new ModelInstance(Path.GetFileNameWithoutExtension(fn), m,
                            GFX.World.GetSpawnPointFromMouseCursor(10.0f, false, true, true), -1, -1, -1, -1));
                    }
                }
            }
        }
    }
}
