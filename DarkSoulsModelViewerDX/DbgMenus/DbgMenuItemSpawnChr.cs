using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public class DbgMenuItemSpawnChr : DbgMenuItem
    {
        private readonly int[] possibleChrIDs = new int[]
        {
            0000,
            1000,
            1200,
            1201,
            1202,
            1203,
            2060,
            2210,
            2230,
            2231,
            2232,
            2240,
            2250,
            2260,
            2270,
            2280,
            2290,
            2300,
            2310,
            2320,
            2330,
            2360,
            2370,
            2380,
            2390,
            2400,
            2410,
            2430,
            2500,
            2510,
            2511,
            2520,
            2530,
            2540,
            2550,
            2560,
            2570,
            2590,
            2591,
            2600,
            2640,
            2650,
            2660,
            2670,
            2680,
            2690,
            2700,
            2710,
            2711,
            2730,
            2731,
            2750,
            2780,
            2790,
            2791,
            2792,
            2793,
            2794,
            2795,
            2800,
            2810,
            2811,
            2820,
            2830,
            2840,
            2860,
            2861,
            2870,
            2900,
            2910,
            2920,
            2921,
            2930,
            2940,
            2950,
            2960,
            3090,
            3110,
            3200,
            3210,
            3220,
            3230,
            3240,
            3250,
            3270,
            3290,
            3300,
            3320,
            3330,
            3340,
            3341,
            3350,
            3370,
            3380,
            3390,
            3400,
            3410,
            3420,
            3421,
            3422,
            3430,
            3431,
            3440,
            3450,
            3451,
            3460,
            3461,
            3470,
            3471,
            3472,
            3480,
            3490,
            3491,
            3500,
            3501,
            3510,
            3511,
            3520,
            3530,
            3531,
            4090,
            4091,
            4095,
            4100,
            4110,
            4115,
            4120,
            4130,
            4140,
            4145,
            4150,
            4160,
            4170,
            4171,
            4172,
            4180,
            4190,
            4500,
            4510,
            4511,
            4520,
            4530,
            4531,
            5200,
            5201,
            5202,
            5210,
            5220,
            5230,
            5231,
            5240,
            5250,
            5260,
            5261,
            5270,
            5271,
            5280,
            5290,
            5291,
            5300,
            5310,
            5320,
            5330,
            5340,
            5350,
            5351,
            5352,
            5353,
            5360,
            5361,
            5362,
            5370,
            5390,
            5400,
            5401,
        };

        public int ChrIdIndex = 0;


        public DbgMenuItemSpawnChr()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            Text = $"Click to Spawn CHR [ID: <c{possibleChrIDs[ChrIdIndex]:D4}>]";
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = ChrIdIndex;
            ChrIdIndex += incrementAmount;

            //If upper bound reached
            if (ChrIdIndex >= possibleChrIDs.Length)
            {
                //If already at end and just tapped button
                if (prevIndex == possibleChrIDs.Length - 1 && !isRepeat)
                    ChrIdIndex = 0; //Wrap Around
                else
                    ChrIdIndex = possibleChrIDs.Length - 1; //Stop
            }

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = ChrIdIndex;
            ChrIdIndex -= incrementAmount;

            //If upper bound reached
            if (ChrIdIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    ChrIdIndex = possibleChrIDs.Length - 1; //Wrap Around
                else
                    ChrIdIndex = 0; //Stop
            }

            UpdateText();
        }

        public override void OnResetDefault()
        {
            ChrIdIndex = 0;
            UpdateText();
        }

        public override void OnClick()
        {
            GFX.ModelDrawer.AddChr(possibleChrIDs[ChrIdIndex], 0, GFX.World.GetSpawnPointInFrontOfCamera(distance: 5,
                faceBackwards: false, lockPitch: true, alignToFloor: true));
        }
    }
}
