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
using DarkSoulsModelViewerDX.DebugPrimitives;

namespace DarkSoulsModelViewerDX
{
    public class InterrootLoader
    {
        private static object _lock_IO = new object();

        public enum InterrootType
        {
            InterrootDS1,
            InterrootDS1R,
            InterrootDS3,
            InterrootDS2,
            InterrootBloodborne,
        };

        public static InterrootType Type = InterrootType.InterrootDS1;

        public static string Interroot = @"C:\Program Files (x86)\steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA";

        static InterrootLoader()
        {
            CFG.Init();

            TexturePool.OnLoadError += TexPool_OnLoadError;
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
                        CFG.Save();
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
                        Type = InterrootType.InterrootDS1R;
                        MessageBox.Show("Automatically switched to Dark Souls Remastered game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }
                    else if (dlg.FileName.Contains("DarkSoulsII.exe"))
                    {
                        Type = InterrootType.InterrootDS2;
                        MessageBox.Show("Automatically switched to Dark Souls II game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }
                    else if (dlg.FileName.Contains("DarkSoulsIII.exe"))
                    {
                        Type = InterrootType.InterrootDS3;
                        MessageBox.Show("Automatically switched to Dark Souls III game type based on selected file.\nIf this is incorrect, be sure to modify the \"Game Type\" option below");
                    }

                    CFG.Save();
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
            while (!Directory.Exists(Interroot))
                Browse();
            return Frankenpath(Interroot, relPath);
        }

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
            var modelEntries = (Type == InterrootType.InterrootDS2) ? bnd.Where(x => x.Name.ToUpper().EndsWith(".FLV")) : bnd.Where(x => x.Name.ToUpper().EndsWith(".FLVER"));
            if (modelEntries.Any())
                return modelEntries.Select(x => new Model(SoulsFormats.FLVER.Read(x.GetBytes()))).ToList();
            else
                return new List<Model>();
        }

        public static List<Model> LoadModelChr(int id)
        {
            string bndName;
            string texBndName;
            if (Type == InterrootType.InterrootDS2)
            {
                bndName = GetInterrootPath($@"model\chr\c{id:D4}.bnd");
                texBndName = GetInterrootPath($@"model\chr\c{id:D4}.texbnd");
            }
            else
            {
                bndName = GetInterrootPath($@"chr\c{id:D4}.chrbnd");
                texBndName = GetInterrootPath($@"chr\c{id:D4}.texbnd");
            }
            // Used in Bloodborne
            var texExtendedTpf = GetInterrootPath($@"chr\c{id:D4}_2.tpf.dcx");

            BND bnd = LoadDecompressedBND(bndName);
            if (bnd != null)
            {
                var models = LoadModelsFromBnd(bnd);
                if (Type == InterrootType.InterrootDS3 || Type == InterrootType.InterrootDS2)
                {
                    // DS2 and DS3 has separate texbnds for textures
                    BND texbnd = LoadDecompressedBND(texBndName);
                    if (texbnd != null)
                    {
                        TexturePool.AddTextureBnd(texbnd, null);
                    }
                }
                else
                {
                    if (File.Exists(texExtendedTpf))
                    {
                        SoulsFormats.TPF tpf = null;
                        lock (_lock_IO)
                        {
                            tpf = SoulsFormats.TPF.Read(texExtendedTpf);
                        }
                        TexturePool.AddTpf(tpf);
                    }
                    TexturePool.AddTextureBnd(bnd, null);
                }
                return models;
            }

            return new List<Model>();
        }

        public static List<Model> LoadModelObj(int id)
        {
            string bndPath = "";
            if (Type == InterrootType.InterrootDS3)
                bndPath = $@"obj\o{id:D6}.objbnd";
            else if (Type == InterrootType.InterrootDS2)
                bndPath = $@"model\obj\o{id/10000:D2}_{id%10000:D4}.bnd";
            else
                bndPath = $@"obj\o{id:D4}.objbnd";

            var bndName = GetInterrootPath(bndPath);

            BND bnd = LoadDecompressedBND(bndName);
            if (bnd != null)
            {
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
                // Bloodborne and DS1R
                return SoulsFormats.FLVER.Read(noExtensionPath + ".flver.dcx");
            }
            return null;
        }

        public static void LoadMapInBackground(string mapName, bool excludeScenery, Action<Model, string, Transform> addMapModel)
        {
            
            if (Type == InterrootType.InterrootDS1)
            {
                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Models[{mapName}]", $"Loading {mapName} models...", prog =>
                {
                    LoadDS1MapInBackground(mapName, excludeScenery, addMapModel, prog);
                });
            }
            else if (Type == InterrootType.InterrootDS1R)
            {
                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Models[{mapName}]", $"Loading {mapName} models...", prog =>
                {
                    LoadDS1MapInBackground(mapName, excludeScenery, addMapModel, prog);
                });

                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Textures[{mapName}]", $"Loading {mapName} textures...", prog =>
                {
                    if (int.TryParse(mapName.Substring(1, 2), out int area))
                        TexturePool.AddMapTexBXF3(area, prog);
                });
            }
            else if (Type == InterrootType.InterrootDS2)
            {
                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Models[{mapName}]", $"Loading {mapName} models...", prog =>
                {
                    LoadDS2MapInBackground(mapName, excludeScenery, addMapModel, prog);
                });

                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Textures[{mapName}]", $"Loading {mapName} textures...", prog =>
                {
                    TexturePool.AddMapTexBXF4DS2(mapName, prog);
                });
            }
            else if (Type == InterrootType.InterrootBloodborne || Type == InterrootType.InterrootDS3)
            {
                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Models[{mapName}]", $"Loading {mapName} models...", prog =>
                {
                    LoadBBMapInBackground(mapName, excludeScenery, addMapModel, prog);
                });

                LoadingTaskMan.DoLoadingTask($"{nameof(LoadMapInBackground)}_Textures[{mapName}]", $"Loading {mapName} textures...", prog =>
                {
                    if (int.TryParse(mapName.Substring(1, 2), out int area))
                        TexturePool.AddMapTexBXF4(area, prog);
                });   
            }
            
        }

        public static void LoadDS1MapInBackground(string mapName, bool excludeScenery,
            Action<Model, string, Transform> addMapModel, IProgress<double> progress)
        {
            var modelDir = GetInterrootPath($@"map\{mapName}");
            var modelDict = new Dictionary<string, Model>();

            int area = int.Parse(mapName.Substring(1, 2));

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
                                    GetInterrootPath($@"map\{mapName}\{modelName}A{area:D2}"));
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

            var msb = DataFile.LoadFromFile<MSB>(GetInterrootPath($@"map\MapStudio\{mapName}.msb"));

            void addMsbPart(MsbPartsBase part)
            {
                var partSubtype = part.GetSubtypeValue();

                var model = loadModel(part.ModelName, partSubtype);

                if (model != null)
                {
                    addMapModel.Invoke(model, part.Name, new Transform(part.PosX, part.PosY, part.PosZ,
                        MathHelper.ToRadians(part.RotX), MathHelper.ToRadians(part.RotY), MathHelper.ToRadians(part.RotZ),
                        part.ScaleX, part.ScaleY, part.ScaleZ));
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

            GFX.ModelDrawer.RequestTextureLoad();
        }

        public static void LoadDS2MapInBackground(string mapName, bool excludeScenery,
            Action<Model, string, Transform> addMapModel, IProgress<double> progress)
        {
            var msbDir = GetInterrootPath($@"map\{mapName}");
            var modelDir = GetInterrootPath($@"model\map\");
            var modelDict = new Dictionary<string, Model>();

            BXF4 models = null;
            lock (_lock_IO)
            {
                models = BXF4.Read(modelDir + $@"\{mapName}.mapbhd", modelDir + $@"\{mapName}.mapbdt");
            }
            Dictionary<string, byte[]> flverLookup = new Dictionary<string, byte[]>();
            foreach (var model in models.Files)
            {
                flverLookup.Add(Path.GetFileName(model.Name), model.Bytes);
            }
            var msb = MSB2.Read(GetInterrootPath($@"map\{mapName}\{mapName}.msb"));

            Model loadModel(string modelName)
            {
                if (!modelDict.ContainsKey(modelName))
                {
                    SoulsFormats.FLVER flver = null;

                    if (flverLookup.ContainsKey(modelName + ".flv.dcx"))
                    {
                        flver = SoulsFormats.FLVER.Read(flverLookup[modelName + ".flv.dcx"]);
                    }

                    if (flver != null)
                        modelDict.Add(modelName, new Model(flver));
                }

                if (modelDict.ContainsKey(modelName))
                    return modelDict[modelName];
                else
                    return null;
            }

            void addMsbPart(MSB2.Part part)
            {
                var model = loadModel(part.ModelName);

                if (model != null)
                {
                    addMapModel.Invoke(model, part.Name, new Transform(part.Position.X, part.Position.Y, part.Position.Z,
                        MathHelper.ToRadians(part.Rotation.X), MathHelper.ToRadians(part.Rotation.Y), MathHelper.ToRadians(part.Rotation.Z),
                        part.Scale.X, part.Scale.Y, part.Scale.Z));
                }
            }

            // Be sure to update this count if more types of parts are loaded.
            int totalNumberOfParts = msb.Parts.MapPieces.Count;

            int i = 0;

            foreach (var part in msb.Parts.MapPieces)
            {
                addMsbPart(part);
                progress?.Report(1.0 * (++i) / totalNumberOfParts);
            }

            modelDict = null;

            GFX.ModelDrawer.RequestTextureLoad();
        }

        public static void LoadBBMapInBackground(string mapName, bool excludeScenery, 
            Action<Model, string, Transform> addMapModel, IProgress<double> progress)
        {
            var modelDir = GetInterrootPath($@"map\{mapName}");
            var modelDict = new Dictionary<string, Model>();

            Model loadModel(string modelName)
            {
                if (!modelDict.ContainsKey(modelName))
                {
                    SoulsFormats.FLVER flver = null;

                    lock (_lock_IO)
                    {
                        flver = LoadMapFlver(
                            GetInterrootPath($@"map\{mapName}\{mapName}_{modelName.Substring(1)}"));
                    }

                    if (flver != null)
                        modelDict.Add(modelName, new Model(flver));
                }

                if (modelDict.ContainsKey(modelName))
                    return modelDict[modelName];
                else
                    return null;
            }

            var msb = MSB64.Read(GetInterrootPath($@"map\MapStudio\{mapName}.msb.dcx"),
                (Type == InterrootType.InterrootBloodborne ? MSB64.MSBVersion.MSBVersionBB : MSB64.MSBVersion.MSBVersionDS3));

            void addMsbPart(MSB64.Part part)
            {
                var model = loadModel(part.ModelName);

                if (model != null)
                {
                    addMapModel.Invoke(model, part.Name, new Transform(part.Position.X, part.Position.Y, part.Position.Z,
                        MathHelper.ToRadians(part.Rotation.X), MathHelper.ToRadians(part.Rotation.Y), MathHelper.ToRadians(part.Rotation.Z),
                        part.Scale.X, part.Scale.Y, part.Scale.Z));
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

            GFX.ModelDrawer.RequestTextureLoad();
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
                            throw new NotImplementedException();
                            //GFX.ModelDrawer.AddModelInstance(
                            //    new ModelInstance(shortName, m, spawnTransform, -1, -1, -1, -1));
                        }
                    }
                    else if (upper.EndsWith(".FLVER") || upper.EndsWith(".FLVER.DCX"))
                    {
                        var flver = SoulsFormats.FLVER.Read(File.ReadAllBytes(fn));
                        var model = new Model(flver);
                        var modelInstance = new ModelInstance(shortName, model, spawnTransform, -1, -1, -1, -1);
                        //GFX.ModelDrawer.AddModelInstance(modelInstance);
                        throw new NotImplementedException();
                    }
                    prog?.Report(1.0 * (++i) / fileNames.Length);
                }
            });

            
        }

        public static void LoadMsbRegions(string mapName)
        {
            if (Type == InterrootType.InterrootDS1 || Type == InterrootType.InterrootDS1R)
            {
                LoadMsbRegionsDS1(mapName);
            }
            else if (Type == InterrootType.InterrootBloodborne)
            {
                LoadMsbRegionsDS3andBB(mapName, MSB64.MSBVersion.MSBVersionBB);
            }
            else if (Type == InterrootType.InterrootDS3)
            {
                LoadMsbRegionsDS3andBB(mapName, MSB64.MSBVersion.MSBVersionDS3);
            }
        }

        public static void LoadMsbRegionsDS3andBB(string mapName, MSB64.MSBVersion version)
        {
            var cylinder = new DbgPrimWireCylinder(
               location: Transform.Default,
               range: 1.0f,
               height: 1,
               numSegments: 12,
               color: Color.Cyan);

            var sphere = new DbgPrimWireSphere(Transform.Default, 1f, 12, 12, Color.Red);

            var circle = new DbgPrimWireSphere(Transform.Default, 1f, 12, 12, Color.Fuchsia);

            var point = new DbgPrimWireSphere(Transform.Default, 0.25f, 4, 4, Color.Lime);

            var box = new DbgPrimWireBox(Transform.Default, Vector3.One, Color.Yellow);

            var msb = MSB64.Read(GetInterrootPath($@"map\MapStudio\{mapName}.msb.dcx"), version);

            foreach (var msbBox in msb.Regions.Boxes)
            {
                var newBox = box.Instantiate(msbBox.Name, new Transform(msbBox.Position.X, msbBox.Position.Y, msbBox.Position.Z,
                    MathHelper.ToRadians(msbBox.Rotation.X), MathHelper.ToRadians(msbBox.Rotation.Y), MathHelper.ToRadians(msbBox.Rotation.Z),
                    msbBox.Length, msbBox.Height, msbBox.Width));
                DBG.AddPrimitive(newBox);
            }

            foreach (var msbSphere in msb.Regions.Spheres)
            {
                var newSphere = sphere.Instantiate(msbSphere.Name, new Transform(msbSphere.Position.X, msbSphere.Position.Y, msbSphere.Position.Z,
                    MathHelper.ToRadians(msbSphere.Rotation.X), MathHelper.ToRadians(msbSphere.Rotation.Y), MathHelper.ToRadians(msbSphere.Rotation.Z),
                    msbSphere.Radius, msbSphere.Radius, msbSphere.Radius));
                DBG.AddPrimitive(newSphere);
            }

            foreach (var msbCylinder in msb.Regions.Cylinders)
            {
                var newCylinder = cylinder.Instantiate(msbCylinder.Name, new Transform(msbCylinder.Position.X, msbCylinder.Position.Y, msbCylinder.Position.Z,
                    MathHelper.ToRadians(msbCylinder.Rotation.X), MathHelper.ToRadians(msbCylinder.Rotation.Y), MathHelper.ToRadians(msbCylinder.Rotation.Z),
                    msbCylinder.Radius, msbCylinder.Height, msbCylinder.Radius));
                DBG.AddPrimitive(newCylinder);
            }

            foreach (var msbPoint in msb.Regions.Points)
            {
                var newPoint = point.Instantiate(msbPoint.Name, new Transform(msbPoint.Position.X, msbPoint.Position.Y, msbPoint.Position.Z,
                    MathHelper.ToRadians(msbPoint.Rotation.X), MathHelper.ToRadians(msbPoint.Rotation.Y), MathHelper.ToRadians(msbPoint.Rotation.Z)));
            }

            // I think circles are probably just beta spheres kek
            foreach (var msbCircle in msb.Regions.Circles)
            {
                var newCircle = circle.Instantiate(msbCircle.Name, new Transform(msbCircle.Position.X, msbCircle.Position.Y, msbCircle.Position.Z,
                    MathHelper.ToRadians(msbCircle.Rotation.X), MathHelper.ToRadians(msbCircle.Rotation.Y), MathHelper.ToRadians(msbCircle.Rotation.Z),
                    msbCircle.Radius, msbCircle.Radius, msbCircle.Radius));
                DBG.AddPrimitive(newCircle);
            }
        }

        public static void LoadMsbRegionsDS1(string mapName)
        {
            var cylinder = new DbgPrimWireCylinder(
               location: Transform.Default,
               range: 1.0f,
               height: 1,
               numSegments: 12,
               color: Color.Cyan);

            var sphere = new DbgPrimWireSphere(Transform.Default, 1f, 12, 12, Color.Red);

            var point = new DbgPrimWireSphere(Transform.Default, 0.25f, 4, 4, Color.Lime);

            var box = new DbgPrimWireBox(Transform.Default, Vector3.One, Color.Yellow);

            var msb = DataFile.LoadFromFile<MSB>(InterrootLoader.GetInterrootPath($@"map\MapStudio\{mapName}.msb"));

            foreach (var msbBox in msb.Regions.Boxes)
            {
                var newBox = box.Instantiate(msbBox.Name, new Transform(msbBox.PosX, msbBox.PosY, msbBox.PosZ,
                    MathHelper.ToRadians(msbBox.RotX), MathHelper.ToRadians(msbBox.RotY), MathHelper.ToRadians(msbBox.RotZ),
                    msbBox.WidthX, msbBox.HeightY, msbBox.DepthZ));
                DBG.AddPrimitive(newBox);
            }

            foreach (var msbSphere in msb.Regions.Spheres)
            {
                var newSphere = sphere.Instantiate(msbSphere.Name, new Transform(msbSphere.PosX, msbSphere.PosY, msbSphere.PosZ,
                    MathHelper.ToRadians(msbSphere.RotX), MathHelper.ToRadians(msbSphere.RotY), MathHelper.ToRadians(msbSphere.RotZ),
                    msbSphere.Radius, msbSphere.Radius, msbSphere.Radius));
                DBG.AddPrimitive(newSphere);
            }

            foreach (var msbCylinder in msb.Regions.Cylinders)
            {
                var newCylinder = cylinder.Instantiate(msbCylinder.Name, new Transform(msbCylinder.PosX, msbCylinder.PosY, msbCylinder.PosZ,
                    MathHelper.ToRadians(msbCylinder.RotX), MathHelper.ToRadians(msbCylinder.RotY), MathHelper.ToRadians(msbCylinder.RotZ),
                    msbCylinder.Radius, msbCylinder.Height, msbCylinder.Radius));
                DBG.AddPrimitive(newCylinder);
            }

            foreach (var msbPoint in msb.Regions.Points)
            {
                var newPoint = point.Instantiate(msbPoint.Name, new Transform(msbPoint.PosX, msbPoint.PosY, msbPoint.PosZ,
                    MathHelper.ToRadians(msbPoint.RotX), MathHelper.ToRadians(msbPoint.RotY), MathHelper.ToRadians(msbPoint.RotZ)));
            }
        }
    }
}
