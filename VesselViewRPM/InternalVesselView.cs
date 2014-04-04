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
        //RPM-related fields
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

        //optional configuration fields
        //(you hear that? thats the sound of every fiber of programmer
        //knowledge in me screaming YOURE WASTING MEMORY YOU LUNATIC)
        [KSPField]
        private int colorModeMesh = (int)ViewerConstants.COLORMODE.NONE;
        [KSPField]
        private int colorModeBox = (int)ViewerConstants.COLORMODE.HIDE;
        [KSPField]
        private bool colorModeMeshDull = true;
        [KSPField]
        private bool colorModeBoxDull = false;
        [KSPField]
        private bool centerOnRootH = true;
        [KSPField]
        private bool centerOnRootV = false;
        [KSPField]
        private bool autoCenterMode = true;
        [KSPField]
        private bool centeringModeRescale = true;
        [KSPField]
        private bool latencyMode = true;
        [KSPField]
        private float scaleFactor = 5;
        [KSPField]
        public float scrOffX = 0;
        [KSPField]
        public float scrOffY = 0;
        [KSPField]
        public bool partSelectMode = false;
        [KSPField]
        public bool selectionSymmetry = true;
        [KSPField]
        public bool centerSelection = false;

        private bool ready = false;
        
        private VesselViewer viewer;
        private ViewerSettings settings;

        private IVViewMenu activeMenu;

        private bool forceRedraw = false;
        private bool textChanged = false;

        public string ShowMenu(int width, int height)
        {
            //MonoBehaviour.print("text draw call");           
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(pageTitle);

            if (activeMenu != null) { 
                activeMenu.update(settings.ship);
                activeMenu.printMenu(ref builder, width, height);
            }
            //MonoBehaviour.print("text draw call done");
            textChanged = true;
            return builder.ToString();
        }


        public bool RenderViewer(RenderTexture screen, float cameraAspect)
        {
            if (forceRedraw & textChanged) {
                forceRedraw = false;
                textChanged = false;
                viewer.forceRedraw();
            }
            //MonoBehaviour.print("screen draw call");
            viewer.drawCall(screen);
            //MonoBehaviour.print("screen draw call done");
            return true;
        }

        public void ButtonProcessor(int buttonID)
        {
            
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
                    activeMenu.deactivate();
                    activeMenu = returnMenu;
                    activeMenu.activate();
                }
                    
            }
            if (buttonID == buttonEsc)
            {
                IVViewMenu returnMenu = activeMenu.back();
                if (returnMenu != null)
                {
                    activeMenu.deactivate();
                    activeMenu = returnMenu;
                    activeMenu.activate();
                }
            }
            
            
            
            if (buttonID == buttonHome)
            {
               
            }
            forceRedraw = true;
        }

        public void PageActive(bool active, int pageNumber)
        {
            if (!ready) Start();
            settings.screenVisible = active;
        }

        public void Start()
        {
            viewer = new VesselViewer();
            settings = viewer.settings;
            setupConfig();
            setupMenus();
            ready = true;
        }

        public void setupConfig() {
            //Im not entirely sure why I still insist on keeping 
            //the RPM version and the standalone separate
            //but as long as I do, I have to keep the configuration plugin-side
            settings.colorModeMesh = colorModeMesh;
            settings.colorModeBox = colorModeBox;
            settings.colorModeMeshDull = colorModeMeshDull;
            settings.colorModeBoxDull = colorModeBoxDull;
            settings.centerOnRootH = centerOnRootH;
            settings.centerOnRootV = centerOnRootV;
            settings.autoCenter = autoCenterMode;
            settings.centerRescale = centeringModeRescale;
            settings.latency = latencyMode;
            settings.scaleFact = scaleFactor;
            settings.scrOffX = scrOffX;
            settings.scrOffY = scrOffY;
            settings.partSelectMode = partSelectMode;
            settings.selectionSymmetry = selectionSymmetry;
            settings.selectionCenter = centerSelection;
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
                new VViewSimpleMenuItem("Bounds color code:",settings,(int)ViewerSettings.IDs.CMB,(int)ViewerSettings.IDs.CMB,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Mesh color mode:",settings,(int)ViewerSettings.IDs.CMM,(int)ViewerSettings.IDs.CMM,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Dull bounds:",settings,(int)ViewerSettings.IDs.BDULL,(int)ViewerSettings.IDs.BDULL,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Dull mesh:",settings,(int)ViewerSettings.IDs.MDULL,(int)ViewerSettings.IDs.MDULL,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                                      };
            VViewSimpleMenu dispModeMenu = new VViewSimpleMenu(DMMItems, "Display modes");

            VViewMenuPartSelector partSelectMenu = new VViewMenuPartSelector(settings);
            VViewSimpleMenuItem[] PCMItems = {
                new VViewSimpleMenuItem("Latency mode:",settings,(int)ViewerSettings.IDs.LAT,(int)ViewerSettings.IDs.LAT,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Autocentering:",settings,(int)ViewerSettings.IDs.AUTC,(int)ViewerSettings.IDs.AUTC,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("A.c. scaling:",settings,(int)ViewerSettings.IDs.CRE,(int)ViewerSettings.IDs.CRE,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Hor. pod center:",settings,(int)ViewerSettings.IDs.CORH,(int)ViewerSettings.IDs.CORH,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Ver. pod center:",settings,(int)ViewerSettings.IDs.CORV,(int)ViewerSettings.IDs.CORV,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Zoom on selection:",settings,(int)ViewerSettings.IDs.PSCEN,(int)ViewerSettings.IDs.PSCEN,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                new VViewSimpleMenuItem("Affect symmetry:",settings,(int)ViewerSettings.IDs.PSSYM,(int)ViewerSettings.IDs.PSSYM,true,0,(int)ViewerSettings.CHANGEMODES.BINV),
                                      };
            VViewSimpleMenu partSelectConfigMenu = new VViewSimpleMenu(PCMItems, "P. sel. config");
            VViewSimpleMenuItem[] MAMItems = {
                new VViewSimpleMenuItem("Plane:",settings,(int)ViewerSettings.IDs.DRP,(int)ViewerSettings.IDs.DRP,true,0,(int)ViewerSettings.CHANGEMODES.SINC),
                new VViewSimpleMenuItem("Display modes",dispModeMenu),
                new VViewSimpleMenuItem("Part selector",partSelectMenu),
                new VViewSimpleMenuItem("Config",partSelectConfigMenu),
                                      };
            VViewSimpleMenu mainMenu = new VViewSimpleMenu(MAMItems, "Main menu");
            VViewSimpleMenuItem[] HIDItems = {
                new VViewSimpleMenuItem("Show",mainMenu)
                                      };
            VViewSimpleMenu hideMenu = new VViewSimpleMenu(HIDItems, "Hidden");
            dispModeMenu.setRoot((IVViewMenu)mainMenu);
            partSelectMenu.setRoot((IVViewMenu)mainMenu);
            partSelectConfigMenu.setRoot((IVViewMenu)mainMenu);
            mainMenu.setRoot((IVViewMenu)hideMenu);
            activeMenu = hideMenu;

        }
    }
}
