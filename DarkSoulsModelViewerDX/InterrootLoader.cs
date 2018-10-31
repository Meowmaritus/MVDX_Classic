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
            }
        }

        //This might be weird because it doesn't follow convention :fatcat:
        public delegate void FatcatContentLoadErrorDelegate(string contentName, string error);
        public static event FatcatContentLoadErrorDelegate OnLoadError;
        private static void RaiseLoadError(string contentName, string error)
        {
            OnLoadError?.Invoke(contentName, error);
        }

        static InterrootLoader()
        {
            TexPoolChr.OnLoadError += TexPool_OnLoadError;
            TexPoolObj.OnLoadError += TexPool_OnLoadError;
            TexPoolMap.OnLoadError += TexPool_OnLoadError;
        }

        private static void TexPool_OnLoadError(string texName, string error)
        {
            RaiseLoadError($"TexPool: \"{texName}\"", error);
        }

        public static TexturePool TexPoolChr = new TexturePool();
        public static TexturePool TexPoolObj = new TexturePool();
        public static TexturePool TexPoolMap = new TexturePool();

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

        public static EntityBND DirectLoadEntityBnd(string relPath)
        {
            var fileName = GetInterrootPath(relPath);
            if (!File.Exists(fileName))
                return null;
            return DataFile.LoadFromFile<EntityBND>(fileName);
        }

        public static EntityBND LoadChr(int id)
        {
            return DirectLoadEntityBnd($@"chr\c{id:D4}.chrbnd");
        }

        public static EntityBND LoadObj(int id)
        {
            return DirectLoadEntityBnd($@"obj\o{id:D4}.objbnd");
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
            TexPoolChr.AddChrBnd(id, idx);
            TexPoolChr.AddChrTexUdsfm(id);
            return new Model(chrbnd.Models[0].Mesh, TexPoolChr);
        }

        public static Model LoadModelChrOptimized(int id, int idx)
        {
            var name = GetInterrootPath($@"chr\c{id:D4}.chrbnd");

            if (!File.Exists(name))
                return null;

            TexPoolChr.AddChrBnd(id, idx);
            TexPoolChr.AddChrTexUdsfm(id);

            return new Model(FLVEROptimized.ReadFromBnd(name, 0), TexPoolChr);
        }

        public static Model LoadModelObjOptimized(int id, int idx)
        {
            var name = GetInterrootPath($@"obj\o{id:D4}.objbnd");

            if (!File.Exists(name))
                return null;

            TexPoolChr.AddObjBnd(id, idx);

            return new Model(FLVEROptimized.ReadFromBnd(name, 0), TexPoolObj);
        }

        public static Model LoadModelObj(int id, int idx)
        {
            var chrbnd = LoadObj(id);
            if (chrbnd == null)
                return null;
            TexPoolChr.AddObjBnd(id, idx);
            return new Model(chrbnd.Models[0].Mesh, TexPoolObj);
        }

        public static List<ModelInstance> LoadMap(int area, int block, bool excludeScenery)
        {
            var result = new List<ModelInstance>();
            //TexPoolMap.LoadMapTexUdsfm();
            var modelFileNames = Directory.GetFiles(GetInterrootPath($@"map\m{area:D2}_{block:D2}_00_00"), "*.flver");
            var modelDict = new Dictionary<string, FLVER>();
            foreach (var mfn in modelFileNames)
            {
                if (excludeScenery && (mfn.StartsWith("m8") || mfn.StartsWith("m9")))
                    continue;
                modelDict.Add(MiscUtil.GetFileNameWithoutDirectoryOrExtension(mfn), DataFile.LoadFromFile<FLVER>(mfn));
            }
            var msb = DataFile.LoadFromFile<MSB>(GetInterrootPath($@"map\MapStudio\m{area:D2}_{block:D2}_00_00.msb"));
            foreach (var part in msb.Parts.MapPieces)
            {
                if (excludeScenery && (part.ModelName.StartsWith("m8") || part.ModelName.StartsWith("m9") || !part.IsShadowDest))
                    continue;
                var model = new Model(modelDict[part.ModelName + $"A{area:D2}"], TexPoolMap);
                result.Add(new ModelInstance(part.Name, model, new Transform(part.PosX, part.PosY, part.PosZ, part.RotX, part.RotY, part.RotZ)));
            }

            modelDict = null;

            return result;
        }
    }
}
