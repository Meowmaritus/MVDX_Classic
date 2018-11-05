using MeowDSIO;
using SoulsFormats;
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
using System.Text.RegularExpressions;

namespace DarkSoulsModelViewerDX
{
    public class InterrootLoader
    {
        private static object _lock_IO = new object();

        public enum InterrootType
        {
            InterrootDS1,
            InterrootDS3,
            // InterrootDS2,
            InterrootBloodborne,
        };

        public static InterrootType Type = InterrootType.InterrootDS1;

        static InterrootLoader()
        {
            LoadInterrootPathAndInterrootType();

            TexturePool.OnLoadError += TexPool_OnLoadError;
        }

        public static void LoadInterrootPathAndInterrootType()
        {
            lock (_lock_IO)
            {
                if (File.Exists("DSMVDX_InterrootPath.txt"))
                {
                    string[] cfg = File.ReadLines("DSMVDX_InterrootPath.txt").ToArray();
                    if (cfg.Length > 0)
                        Interroot = cfg[0].Trim('\n');

                    if (cfg.Length > 1 && Enum.TryParse(cfg[1].Trim('\n'), out InterrootType cfgInterrootType))
                    {
                        Type = cfgInterrootType;
                    }
                    else
                    {
                        // If its not in the file for some reason, try to guess
                        if (Interroot.Contains("Dark Souls Prepare to Die Edition"))
                            Type = InterrootType.InterrootDS1;
                        else if (Interroot.Contains("DARK SOULS REMASTERED"))
                            Type = InterrootType.InterrootDS1;
                        // the check for DARK SOULS III comes before DARK SOULS II 
                        // because "DARK SOULS III" contains "DARK SOULS II" in it.
                        else if (Interroot.Contains("DARK SOULS III"))
                            Type = InterrootType.InterrootDS3;
                        //else if (Interroot.Contains("DARK SOULS II"))
                        //    Type = InterrootType.InterrootDS2;
                        //else if (Interroot.Contains("Dark Souls II Scholar of the First Sin"))
                        //    Type = InterrootType.InterrootDS2;

                        else if (Interroot.Contains("CUSA00900") || Interroot.ToUpper().Contains("BLOODBORNE"))
                            Type = InterrootType.InterrootBloodborne;

                        // Resave to save the new interroot type.
                        SaveInterrootPathAndInterrootType();
                    }
                }


            }
        }

        public static void SaveInterrootPathAndInterrootType()
        {
            lock (_lock_IO)
                File.WriteAllText("DSMVDX_InterrootPath.txt",
                    $"{Interroot}\n{Type.ToString()}");
        }

        public static void Browse()
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                FileName = "DarkSoulsIII.exe",
                Filter = "All Files (*.*)|*.*",
                Title = "Select Game Executable (*.exe for PC, eboot.bin for PS4)",
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.FileName.ToUpper().EndsWith("EBOOT.BIN"))
                {
                    Type = InterrootType.InterrootBloodborne;
                    string possibleInterroot = Path.Combine(new FileInfo(dlg.FileName).DirectoryName, "dvdroot_ps4");
                    if (Directory.Exists(possibleInterroot))
                    {
                        Interroot = possibleInterroot;
                        SaveInterrootPathAndInterrootType();
                    }
                    else
                    {
                        MessageBox.Show("A PS4 executable was selected but no /dvdroot_ps4/ folder was found next to it. Unable to determine data root path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show("Automatically switched to Bloodborne game type since it is the PS4 exclusive one.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                }
                else
                {
                    Interroot = new FileInfo(dlg.FileName).DirectoryName;

                    if (dlg.FileName.Contains("DARKSOULS.exe"))
                    {
                        Type = InterrootType.InterrootDS1;
                        MessageBox.Show("Automatically switched to Dark Souls game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }
                    else if (dlg.FileName.Contains("DarkSoulsRemastered.exe"))
                    {
                        Type = InterrootType.InterrootDS1;
                        MessageBox.Show("Automatically switched to Dark Souls game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }
                    //else if (dlg.FileName.Contains("DarkSoulsII.exe"))
                    //{
                    //    Type = InterrootType.InterrootDS2;
                    //    MessageBox.Show("Automatically switched to Dark Souls II game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    //}
                    else if (dlg.FileName.Contains("DarkSoulsIII.exe"))
                    {
                        Type = InterrootType.InterrootDS3;
                        MessageBox.Show("Automatically switched to Dark Souls III game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }

                    SaveInterrootPathAndInterrootType();
                }
                
                
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

        // Utility function to detect and load a potentially DCX compressed BND
        public static BND LoadDecompressedBND(string path)
        {
            // Search for the decompressed bnd
            BND bnd = null;
            if (File.Exists(path))
            {
                lock (_lock_IO)
                {
                    bnd = DataFile.LoadFromFile<BND>(path);
                }
            }
            // Look for a compressed one if no decompressed one exists
            else if (File.Exists(path + ".dcx"))
            {
                lock (_lock_IO)
                {
                    bnd = DataFile.LoadFromDcxFile<BND>(path + ".dcx");
                }
            }
            return bnd;
        }

        public static List<SoulsFormats.TPF> DirectLoadAllTpfInDir(string relPath)
        {
            lock (_lock_IO)
            {
                var path = GetInterrootPath(relPath);
                if (!Directory.Exists(path))
                    return new List<SoulsFormats.TPF>();

                var tpfNames = (Type == InterrootType.InterrootDS1) ? Directory.GetFiles(path, "*.tpf") : Directory.GetFiles(path, "*.tpf.dcx");
                return tpfNames
                    .Select(x => SoulsFormats.TPF.Read(x))
                    .ToList();
            }
               
        }

        public static List<SoulsFormats.TPF> LoadChrTexUdsfm(int id)
        {
            return DirectLoadAllTpfInDir($@"chr\c{id:D4}");
        }

        public static SoulsFormats.TPF DirectLoadTpf(string path)
        {
            lock (_lock_IO)
                return SoulsFormats.TPF.Read(path);
        }

        public static List<Model> LoadModelsFromBnd(BND bnd)
        {
            var modelEntries = bnd.Where(x => x.Name.ToUpper().EndsWith(".FLVER"));
            if (modelEntries.Any())
                return modelEntries.Select(x => new Model(SoulsFormats.FLVER.Read(x.GetBytes()))).ToList();
            else
                return new List<Model>();
        }

        public static List<Model> LoadModelChr(int id)
        {
            var bndName = GetInterrootPath($@"chr\c{id:D4}.chrbnd");
            var texBndName = GetInterrootPath($@"chr\c{id:D4}.texbnd");

            BND bnd = LoadDecompressedBND(bndName);
            if (bnd != null)
            {
                var models = LoadModelsFromBnd(bnd);
                if (Type == InterrootType.InterrootDS3)
                {
                    // DS3 has separate texbnds for textures
                    BND texbnd = LoadDecompressedBND(texBndName);
                    if (texbnd != null)
                    {
                        TexturePool.AddTextureBnd(texbnd, null);
                    }
                }
                else
                {
                    TexturePool.AddTextureBnd(bnd, null);
                }
                return models;
            }

            return new List<Model>();
        }

        public static List<Model> LoadModelObj(int id)
        {
            var bndName = GetInterrootPath($@"obj\o{id:D4}.objbnd");

            BND bnd = LoadDecompressedBND(bndName);
            if (bnd != null)
            {
                lock (_lock_IO)
                {
                    bnd = DataFile.LoadFromFile<BND>(bndName);
                }
                var models = LoadModelsFromBnd(bnd);
                TexturePool.AddTextureBnd(bnd, null);
                return models;
            }

            return new List<Model>();
        }

        public static SoulsFormats.FLVER LoadMapFlver(string noExtensionPath)
        {
            if (File.Exists(noExtensionPath + ".mapbnd.dcx"))
            {
                // DS3's snowflake packaging of flvers
                return SoulsFormats.FLVER.Read(BND4.Read(noExtensionPath + ".mapbnd.dcx").Files[0].Bytes);
            }
            else if (File.Exists(noExtensionPath + ".flver"))
            {
                // Usual case for DS1 with udsfm
                return SoulsFormats.FLVER.Read(noExtensionPath + ".flver");
            }
            else if (File.Exists(noExtensionPath + ".flver.dcx"))
            {
                // Bloodborne and possibly DS2
                return SoulsFormats.FLVER.Read(noExtensionPath + ".flver.dcx");
            }
            return null;
        }

        public static void LoadMapInBackground(int area, int block, bool excludeScenery, Action<ModelInstance> addMapModel)
        {
            var mapStr = $"m{area:D2}_{block:D2}_00_00";
            
            if (Type == InterrootType.InterrootDS1)
            {
                LoadingTaskMan.DoLoadingTask($"LoadMapInBackground[{mapStr}]", $"Loading {mapStr} models...", prog =>
                {
                    LoadDS1MapInBackground(area, block, excludeScenery, addMapModel, prog);
                });
            }
            else if (Type == InterrootType.InterrootBloodborne || Type == InterrootType.InterrootDS3)
            {
                LoadingTaskMan.DoLoadingTask($"LoadMapInBackground_Models[{mapStr}]", $"Loading {mapStr} models...", prog =>
                {
                    LoadBBMapInBackground(area, block, excludeScenery, addMapModel, prog);
                });

                LoadingTaskMan.DoLoadingTask($"LoadMapInBackground_Textures[{mapStr}]", $"Loading {mapStr} textures...", prog =>
                {
                    TexturePool.AddMapTexBhds(area);
                });   
            }
            
        }

        public static void LoadDS1MapInBackground(int area, int block, bool excludeScenery, 
            Action<ModelInstance> addMapModel, IProgress<double> progress)
        {
            var modelDir = GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00");
            var modelDict = new Dictionary<string, Model>();
            //foreach (var mfn in modelFileNames)
            //{
            //    if (excludeScenery && (mfn.StartsWith("m8") || mfn.StartsWith("m9")))
            //        continue;
            //    modelDict.Add(MiscUtil.GetFileNameWithoutDirectoryOrExtension(mfn), DataFile.LoadFromFile<FLVER>(mfn));
            //}

            Model loadModel(string modelName, PartsParamSubtype partType)
            {
                if (!modelDict.ContainsKey(modelName))
                {
                    SoulsFormats.FLVER flver = null;

                    lock (_lock_IO)
                    {
                        switch (partType)
                        {
                            case PartsParamSubtype.MapPieces:
                                flver = LoadMapFlver(
                                    GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00\{modelName}A{area:D2}"));
                                break;
                            case PartsParamSubtype.NPCs:
                            case PartsParamSubtype.DummyNPCs:
                            case PartsParamSubtype.Objects:
                            case PartsParamSubtype.DummyObjects:
                                string bndRelPath = (partType == PartsParamSubtype.Objects
                                    || partType == PartsParamSubtype.DummyObjects)
                                    ? $@"obj\{modelName}.objbnd" : $@"chr\{modelName}.chrbnd";

                                var bnd = LoadDecompressedBND(GetInterrootPath(bndRelPath));
                                if (bnd != null)
                                {
                                    foreach (var entry in bnd)
                                    {
                                        var compareName = entry.Name.ToUpper();
                                        if (flver == null && compareName.EndsWith(".FLVER"))
                                            flver = SoulsFormats.FLVER.Read(entry.GetBytes());
                                        else if (compareName.EndsWith(".TPF"))
                                            TexturePool.AddTpf(SoulsFormats.TPF.Read(entry.GetBytes()));
                                    }
                                }
                                break;
                        }
                    }

                    if (flver != null)
                        modelDict.Add(modelName, new Model(flver));
                }

                if (modelDict.ContainsKey(modelName))
                    return modelDict[modelName];
                else
                    return null;
            }

            var msb = DataFile.LoadFromFile<MSB>(GetInterrootPath($@"map\MapStudio\m{area:D2}_{block:D2}_00_00.msb"));

            void addMsbPart(MsbPartsBase part)
            {
                var partSubtype = part.GetSubtypeValue();

                var model = loadModel(part.ModelName, partSubtype);

                if (model != null)
                {
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

            // Be sure to update this count if more types of parts are loaded.
            int totalNumberOfParts =
                msb.Parts.MapPieces.Count +
                msb.Parts.NPCs.Count +
                msb.Parts.DummyNPCs.Count +
                msb.Parts.Objects.Count +
                msb.Parts.DummyObjects.Count
                ;

            int i = 0;

            foreach (var part in msb.Parts.MapPieces)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            foreach (var part in msb.Parts.NPCs)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            foreach (var part in msb.Parts.DummyNPCs)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            foreach (var part in msb.Parts.Objects)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            foreach (var part in msb.Parts.DummyObjects)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            modelDict = null;
        }

        public static void LoadBBMapInBackground(int area, int block, bool excludeScenery, 
            Action<ModelInstance> addMapModel, IProgress<double> progress)
        {
            var modelDir = GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00");
            var modelDict = new Dictionary<string, Model>();

            Model loadModel(string modelName)
            {
                if (!modelDict.ContainsKey(modelName))
                {
                    SoulsFormats.FLVER flver = null;

                    lock (_lock_IO)
                    {
                        flver = LoadMapFlver(
                            GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00\m{area:D2}_{block:D2}_00_00_{modelName.Substring(1)}"));
                    }

                    if (flver != null)
                        modelDict.Add(modelName, new Model(flver));
                }

                if (modelDict.ContainsKey(modelName))
                    return modelDict[modelName];
                else
                    return null;
            }

            var msb = MSB64.Read(GetInterrootPath($@"map\MapStudio\m{area:D2}_{block:D2}_00_00.msb.dcx"),
                (Type == InterrootType.InterrootBloodborne ? MSB64.MSBVersion.MSBVersionBB : MSB64.MSBVersion.MSBVersionDS3));

            void addMsbPart(MSB64.Part part)
            {
                var model = loadModel(part.ModelName);

                if (model != null)
                {
                    var partModelInstance = new ModelInstance(part.Name, model, new Transform(part.Position.X, part.Position.Y, part.Position.Z,
                        MathHelper.ToRadians(part.Rotation.X), MathHelper.ToRadians(part.Rotation.Y), MathHelper.ToRadians(part.Rotation.Z),
                        part.Scale.X, part.Scale.Y, part.Scale.Z), (int)part.DrawGroup1, (int)part.DrawGroup2, (int)part.DrawGroup3, (int)part.DrawGroup4);

                    addMapModel.Invoke(partModelInstance);
                }
            }

            // Be sure to update this count if more types of parts are loaded.
            int totalNumberOfParts = 
                msb.Parts.MapPieces.Count
                ;

            int i = 0;

            foreach (var part in msb.Parts.MapPieces)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            modelDict = null;
        }

        public static void LoadDragDroppedFiles(string[] fileNames)
        {
            LoadingTaskMan.DoLoadingTask("LoadDragDroppedFiles_" + DateTime.Now.Ticks, "Loading dropped models...", prog =>
            {
                var spawnTransform = GFX.World.GetSpawnPointFromMouseCursor(10.0f, false, true, true);
                int i = 0;
                foreach (var fn in fileNames)
                {
                    var shortName = Path.GetFileNameWithoutExtension(fn);
                    var upper = fn.ToUpper();
                    if (upper.EndsWith(".CHRBND") || upper.EndsWith(".OBJBND") || upper.EndsWith(".PARTSBND"))
                    {
                        BND bnd = null;
                        lock (_lock_IO)
                        {
                            bnd = DataFile.LoadFromFile<BND>(fn);
                        }
                        TexturePool.AddTextureBnd(bnd, null);
                        var models = LoadModelsFromBnd(bnd);
                        foreach (var m in models)
                        {
                            GFX.ModelDrawer.AddModelInstance(
                                new ModelInstance(shortName, m, spawnTransform, -1, -1, -1, -1));
                        }
                    }
                    else if (upper.EndsWith(".FLVER"))
                    {
                        var flver = SoulsFormats.FLVER.Read(File.ReadAllBytes(fn));
                        var model = new Model(flver);
                        var modelInstance = new ModelInstance(shortName, model, spawnTransform, -1, -1, -1, -1);
                        GFX.ModelDrawer.AddModelInstance(modelInstance);
                    }
                    prog?.Report(1.0 * (++i) / fileNames.Length);
                }
            });

            
        }
    }
}
