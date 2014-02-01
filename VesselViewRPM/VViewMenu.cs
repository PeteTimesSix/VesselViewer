using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VesselView;

namespace VesselViewRPM
{
    class VViewMenu
    {
        private VViewMenu rootMenu;
        private VViewMenuItem[] menuItems;
        private int activeItemPos = 0;
        public string name;

        public VViewMenuItem ActiveItem
        {
            get { return menuItems[activeItemPos]; }
        }

        public VViewMenu(VViewMenuItem[] items, string name)
        {
            this.rootMenu = null;
            this.menuItems = items;
            this.name = name;
        }
        
        internal void printMenu(ref StringBuilder builder, int width, int height)
        {
            builder.Append("-|");
            builder.Append(name);
            builder.AppendLine("|-");
            int i = 0;
            foreach (VViewMenuItem item in menuItems) {
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

        internal void up()
        {
            activeItemPos--;
            if (activeItemPos < 0) activeItemPos = menuItems.Length - 1;
        }

        internal void down()
        {
            activeItemPos++;
            if (activeItemPos >= menuItems.Length) activeItemPos = 0;
        }

        internal VViewMenu getRoot()
        {
            return rootMenu;
        }

        internal void setRoot(VViewMenu root) {
            this.rootMenu = root;
        }
    }
}
