using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public class DbgMenuItemSpawnMap : DbgMenuItem
    {
        public static List<string > IDList = new List<string>();
        private static bool NeedsTextUpdate = false;
        public bool IsRegionSpawner { get; private set; } = false;

        public int IDIndex = 0;

        public static void UpdateSpawnIDs()
        {
            var msbFiles = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"\map\MapStudio\"), @"*.msb")
                .Select(Path.GetFileNameWithoutExtension);
            IDList = new List<string>();
            var IDSet = new HashSet<string>();
            foreach (var cf in msbFiles)
            {
                var dotIndex = cf.IndexOf('.');
                if (dotIndex >= 0)
                {
                    IDList.Add(cf.Substring(0, dotIndex));
                    IDSet.Add(cf.Substring(0, dotIndex));
                }
                else
                {
                    IDList.Add(cf);
                    IDSet.Add(cf);
                }
            }

            var msbFilesDCX = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"\map\MapStudio\"), @"*.msb.dcx")
                .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension);
            foreach (var cf in msbFilesDCX)
            {
                var dotIndex = cf.IndexOf('.');
                if (dotIndex >= 0)
                {
                    if (!IDSet.Contains(cf.Substring(0, dotIndex)))
                        IDList.Add(cf.Substring(0, dotIndex));
                }
                else
                {
                    if (!IDSet.Contains(cf))
                        IDList.Add(cf);
                }
            }
            NeedsTextUpdate = true;
        }

        public DbgMenuItemSpawnMap(bool isRegionSpawner)
        {
            IsRegionSpawner = isRegionSpawner;
            UpdateSpawnIDs();
            UpdateText();
        }

        private void UpdateText()
        {
            string actionText = IsRegionSpawner ? "Click to Spawn MAP - Event Regions" : "Click to Spawn MAP - Models";

            if (!IsRegionSpawner)
            {
                CustomColorFunction = () => (
                    LoadingTaskMan.IsTaskRunning($"{nameof(InterrootLoader.LoadMapInBackground)}_Textures[{IDList[IDIndex]}]")
                    || LoadingTaskMan.IsTaskRunning($"{nameof(InterrootLoader.LoadMapInBackground)}_Models[{IDList[IDIndex]}]"))
                    ? Color.Cyan * 0.5f : Color.Cyan;
            }

            if (IDList.Count == 0)
            {
                IDIndex = 0;
                Text = $"{actionText} [Invalid Data Root Selected]";
            }
            else
            {
                if (IDIndex >= IDList.Count)
                    IDIndex = IDList.Count - 1;

                Text = $"{actionText} [ID: <{IDList[IDIndex]}>]";
            }
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = IDIndex;
            IDIndex += incrementAmount;

            //If upper bound reached
            if (IDIndex >= IDList.Count)
            {
                //If already at end and just tapped button
                if (prevIndex == IDList.Count - 1 && !isRepeat)
                    IDIndex = 0; //Wrap Around
                else
                    IDIndex = IDList.Count - 1; //Stop
            }

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = IDIndex;
            IDIndex -= incrementAmount;

            //If upper bound reached
            if (IDIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    IDIndex = IDList.Count - 1; //Wrap Around
                else
                    IDIndex = 0; //Stop
            }

            UpdateText();
        }

        public override void OnResetDefault()
        {
            IDIndex = 0;
            UpdateText();
        }

        public override void OnClick()
        {
            // THIS IS ACTUALLY HOW FROMSOFT DOES IT I SHIT YOU NOT
            var area = int.Parse(IDList[IDIndex].Substring(1, 2));
            var block = int.Parse(IDList[IDIndex].Substring(4, 2));

            if (IsRegionSpawner)
                DBG.LoadMsbRegions(area, block);
            else
               GFX.ModelDrawer.AddMap(area, block, false);
        }

        public override void UpdateUI()
        {
            if (NeedsTextUpdate)
            {
                UpdateText();
                NeedsTextUpdate = false;
            }
        }

        public override void OnRequestTextRefresh()
        {
            UpdateSpawnIDs();
            UpdateText();
        }
    }
}
