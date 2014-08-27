using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;
using UnityEngine;

namespace VesselViewRPM.menus
{
    public class VViewCustomMenusMenu : IVViewMenu
    {
        private IVViewMenu rootMenu;
        private int activeItemPos = 0;
        public string name;

        internal List<IVViewMenu> menuList = new List<IVViewMenu>();
        private static List<VViewCustomMenusMenu> instanceList = new List<VViewCustomMenusMenu>();
        private static List<Func<IVViewMenu>> menuMakerList = new List<Func<IVViewMenu>>();

        private VesselViewer instance;

        public VViewCustomMenusMenu(string name, VesselViewer shackledInstance)
        {
            this.rootMenu = null;
            this.name = name;
            instanceList.Add(this);
            instance = shackledInstance;
        }

        public static void registerMenu(Func<IVViewMenu> createInstance) 
        {
            foreach (VViewCustomMenusMenu instance in instanceList) 
            {
                IVViewMenu customMenu = createInstance();
                instance.menuList.Add(customMenu);
                customMenu.setRoot(instance);
            }
            menuMakerList.Add(createInstance);
        }
        
        public void printMenu(ref StringBuilder builder, int width, int height)
        {
            builder.Append("-|");
            builder.Append(name);
            builder.AppendLine("|-");
            int i = 0;
            foreach (IVViewMenu item in menuList)
            {
                if (activeItemPos == i)
                {
                    builder.AppendLine(item.getName() + " <");
                }
                else
                {
                    builder.AppendLine(item.getName());
                }
                i++;
            }
            //i is now line counter
            i += 2;
            while (i < height - 1)
            {
                builder.AppendLine();
                i++;
            }

            builder.AppendLine("VV version " + ViewerConstants.VERSION);
        }

        public void up()
        {
            if (menuList.Count == 0) return;
            activeItemPos--;
            if (activeItemPos < 0) activeItemPos = menuList.Count - 1;
        }

        public void down()
        {
            if (menuList.Count == 0) return;
            activeItemPos++;
            if (activeItemPos >= menuList.Count) activeItemPos = 0;
        }

        public IVViewMenu click()
        {
            if (menuList.Count == 0) return null;
            else
            {
                IVViewMenu menu = menuList.ElementAt(activeItemPos);
                MonoBehaviour.print("Switching VV to custom mode " + menu.getCustomSettings().name);
                instance.setCustomMode(menu.getCustomSettings());
                return menu;
            }
             
        }

        public IVViewMenu back()
        {
            return rootMenu;
        }

        public void activate()
        {
            //nothing to do
        }

        public void deactivate()
        {
            //nothing to do
        }

        public void setRoot(IVViewMenu root)
        {
            rootMenu = root;
        }

        public IVViewMenu getRoot()
        {
            return rootMenu;
        }

        public IVViewMenu update(Vessel ship)
        {
            instance.setCustomMode(null);
            if (menuMakerList.Count != menuList.Count)
            {
                foreach (IVViewMenu menu in menuList)
                {
                    menu.setRoot(null);
                }
                menuList.Clear();
                foreach (Func<IVViewMenu> createInstance in menuMakerList)
                {
                    IVViewMenu newMenu = createInstance();
                    menuList.Add(newMenu);
                    newMenu.setRoot(this);
                }
                return this;
            }
            else return null;
        }

        public string getName()
        {
            return name;
        }


        public CustomModeSettings getCustomSettings()
        {
            return null;
        }


        public void setCustomSettings(CustomModeSettings settings)
        {
            MonoBehaviour.print("just tried to set a custom mode on the custom mode menu. THIS SHOULD NEVER HAPPEN.");
        }
    }
}
