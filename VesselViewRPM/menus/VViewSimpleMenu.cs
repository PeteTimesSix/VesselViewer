using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VesselView;

namespace VesselViewRPM
{
    class VViewSimpleMenu : IVViewMenu
    {
        private IVViewMenu rootMenu;
        private VViewSimpleMenuItem[] menuItems;
        private int activeItemPos = 0;
        public string name;

        public VViewSimpleMenuItem ActiveItem
        {
            get { return menuItems[activeItemPos]; }
        }

        public VViewSimpleMenu(VViewSimpleMenuItem[] items, string name)
        {
            this.rootMenu = null;
            this.menuItems = items;
            this.name = name;
        }
        
        public void printMenu(ref StringBuilder builder, int width, int height)
        {
            builder.Append("-|");
            builder.Append(name);
            builder.AppendLine("|-");
            int i = 0;
            foreach (VViewSimpleMenuItem item in menuItems) {
                if (activeItemPos == i)
                {
                    builder.AppendLine(item.ToString() + " <");
                }
                else 
                {
                    builder.AppendLine(item.ToString());
                }
                i++;
            }
            //i is now line counter
            i+=2;
            while (i < height - 1) {
                builder.AppendLine();
                i++;
            }
            
            builder.AppendLine("VV version "+ViewerConstants.VERSION);
        }

        public void up()
        {
            activeItemPos--;
            if (activeItemPos < 0) activeItemPos = menuItems.Length - 1;
        }

        public void down()
        {
            activeItemPos++;
            if (activeItemPos >= menuItems.Length) activeItemPos = 0;
        }

        public IVViewMenu click()
        {
            return ActiveItem.click();
        }

        public IVViewMenu back()
        {
            return rootMenu;
        }

        public IVViewMenu getRoot()
        {
            return rootMenu;
        }

        public void setRoot(IVViewMenu root)
        {
            this.rootMenu = root;
        }


        public void update(Vessel ship)
        {
            //nothing.
        }



        public void activate()
        {
            //nothing
        }

        public void deactivate()
        {
            //nothing
        }
    }
}
