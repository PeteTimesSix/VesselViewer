using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VesselView;
using VesselViewRPM;

namespace JSI.Handlers
{
    class InternalVesselView : InternalModule
    {
        [KSPField]
        public string pageTitle = "-------------Vessel Viewer--------------";
        [KSPField]
        public int buttonUp = 0;
        [KSPField]
        public int buttonDown = 1;
        [KSPField]
        public int buttonEnter = 2;
        [KSPField]
        public int buttonEsc = 3;
        [KSPField]
        public int buttonHome = 4;
        
        private VesselViewer viewer;
        private ViewerSettings settings;

        private IVViewMenu activeMenu;

        public string ShowMenu(int width, int height)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(pageTitle);
            if (settings.configScreenVisible)
            {
                activeMenu.printMenu(ref builder, width, height);
            }else{
                builder.AppendLine("M <");
            }
            return builder.ToString();
        }


        public bool RenderViewer(RenderTexture screen, float cameraAspect)
        {
            viewer.drawCall(screen);
            return true;
        }

        public void ButtonProcessor(int buttonID)
        {
            if (settings.configScreenVisible)
            {
                //viewer.forceRedraw();
                if (buttonID == buttonUp)
                {
                    activeMenu.up();
                }
                if (buttonID == buttonDown)
                {
                    activeMenu.down();
                }
                if (buttonID == buttonEnter)
                {
                    //returns a menu to change to or null
                    IVViewMenu returnMenu = activeMenu.click();
                    if (returnMenu != null)
                    {
                        activeMenu = returnMenu;
                    }
                    
                }
                if (buttonID == buttonEsc)
                {
                    IVViewMenu returnMenu = activeMenu.getRoot();
                    if (returnMenu != null)
                    {
                        activeMenu = returnMenu;
                    }
                    else 
                    {
                        settings.configScreenVisible = false;
                    }
                }
            }
            else {
                if (buttonID == buttonEnter)
                {
                    settings.configScreenVisible = true;
                }
            }
            
            if (buttonID == buttonHome)
            {
               
            }

            
        }

        // Analysis disable once UnusedParameter
        public void PageActive(bool active, int pageNumber)
        {
            settings.screenVisible = active;
        }

        public void Start()
        {
            viewer = new VesselViewer();
            settings = viewer.settings;
            setupMenus();
        }



        /*
         * NULL - nothing
         * CMM - mesh color mode
         * CMB - bounds color mode
         * CORH - center on root horizontal
         * CORV - center on root vertical
         * CRE - automatic centering rescaling
         * AUTC - automatic centering 
         * LAT - latency mode (one redraw per second)
         * DRP - draw plane (XY, XZ, Real, Relative, and so on)
         * SCP - scale list position
         * SCA - scaling factor
         * SV - is screen visible
         * CSV - is config screen/menu visible
         */

        private void setupMenus() {
            //well I was gonna have to hardcode this SOMEWHERE.
            //int propertyToChange, int propertyToPrint, bool valueDirect, int value, int changeMode
            VViewSimpleMenuItem[] DMMItems = {
                new VViewSimpleMenuItem("Bounds CM:",settings,(int)ViewerSettings.IDs.CMB,(int)ViewerSettings.IDs.CMB,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Mesh CM:",settings,(int)ViewerSettings.IDs.CMM,(int)ViewerSettings.IDs.CMM,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Fill CM: WIP"),
                                      };
            VViewSimpleMenu dispModeMenu = new VViewSimpleMenu(DMMItems, "Display modes");
            VViewSimpleMenuItem[] TMMItems = {
                new VViewSimpleMenuItem("Autocentering:",settings,(int)ViewerSettings.IDs.AUTC,(int)ViewerSettings.IDs.AUTC,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("A.c. scaling:",settings,(int)ViewerSettings.IDs.CRE,(int)ViewerSettings.IDs.CRE,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Hor. pod center:",settings,(int)ViewerSettings.IDs.CORH,(int)ViewerSettings.IDs.CORH,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Ver. pod center:",settings,(int)ViewerSettings.IDs.CORV,(int)ViewerSettings.IDs.CORV,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                                      };
            VViewSimpleMenu transModeMenu = new VViewSimpleMenu(TMMItems, "Transforms");
            VViewSimpleMenuItem[] MOMItems = {
                new VViewSimpleMenuItem("Latency mode:",settings,(int)ViewerSettings.IDs.LAT,(int)ViewerSettings.IDs.LAT,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                                      };
            VViewSimpleMenu miscMenu = new VViewSimpleMenu(MOMItems, "Misc. options");
            VViewSimpleMenuItem[] MAMItems = {
                new VViewSimpleMenuItem("Plane:",settings,(int)ViewerSettings.IDs.DRP,(int)ViewerSettings.IDs.DRP,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Display modes",dispModeMenu),
                new VViewSimpleMenuItem("Transforms",transModeMenu),
                new VViewSimpleMenuItem("Misc.",miscMenu),
                                      };
            VViewSimpleMenu mainMenu = new VViewSimpleMenu(MAMItems, "Main menu");
            dispModeMenu.setRoot((IVViewMenu)mainMenu);
            transModeMenu.setRoot((IVViewMenu)mainMenu);
            miscMenu.setRoot((IVViewMenu)mainMenu);

            activeMenu = mainMenu;

        }
    }
}
