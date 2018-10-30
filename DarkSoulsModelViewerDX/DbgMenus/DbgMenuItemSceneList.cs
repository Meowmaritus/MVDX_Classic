using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public class DbgMenuItemSceneList : DbgMenuItem
    {
        public DbgMenuItemSceneList()
        {
            BuildSceneItems();
        }

        private List<DbgMenuItem> baseMenuItems = new List<DbgMenuItem>
        {
            new DbgMenuItem()
            {
                Text = "[Click to Clear Scene]",
                ClickAction = () => GFX.ModelDrawer.ModelInstanceList.Clear()
            }
        };

        private void BuildSceneItems()
        {
            Items.Clear();
            foreach (var it in baseMenuItems)
            {
                Items.Add(it);
            }

            foreach (var md in GFX.ModelDrawer.ModelInstanceList)
            {
                Items.Add(new DbgMenuItemBool(md.Name, "SHOW", "HIDE", 
                    (b) => md.Model.IsVisible = b, () => md.Model.IsVisible));
            }
        }

        public override void UpdateUI()
        {
            // If the amount of models changes, just rebuild the whole thing. 
            if (Items.Count != (GFX.ModelDrawer.ModelInstanceList.Count + baseMenuItems.Count))
            {
                BuildSceneItems();
            }

            base.UpdateUI();
        }
    }
}
