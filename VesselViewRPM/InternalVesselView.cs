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
        private int centeringModeRescaleNew = (int)ViewerConstants.RESCALEMODE.INCR;
        [KSPField]
        private bool latencyMode = false;
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
        [KSPField]
        public int spinAxis = (int)ViewerConstants.AXIS.Y;
        [KSPField]
        public int spinSpeed = 0;
        [KSPField]
        public bool displayEngines = true;
        [KSPField]
        public bool displayCOM = true;
        [KSPField]
        public bool displayGround = true;
        [KSPField]
        public bool displayAxes = false;

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
            settings.centerRescale = centeringModeRescaleNew;
            settings.latency = latencyMode;
            settings.scaleFact = scaleFactor;
            settings.scrOffX = scrOffX;
            settings.scrOffY = scrOffY;
            settings.partSelectMode = partSelectMode;
            settings.selectionSymmetry = selectionSymmetry;
            settings.selectionCenter = centerSelection;
            settings.spinAxis = spinAxis;
            settings.spinSpeed = spinSpeed;
            settings.displayEngines = displayEngines;
            settings.displayCOM = displayCOM;
            settings.displayGround = displayGround;
            settings.displayAxes = displayAxes;
        }

        private void setupMenus() {
            //well I was gonna have to hardcode this SOMEWHERE.
            List<VViewSimpleMenuItem> itemList = new List<VViewSimpleMenuItem>();
            /*

            
            VViewSimpleMenuItem[] PCMItems = {

                                      };
            */
            itemList.Clear();
            itemList.Add(new VViewSimpleMenuItem("Active: ", settings, "", "drawPlane"));
            for (int i = 0; i < ViewerConstants.PLANES.Length; i++) 
            {
                itemList.Add(new VViewSimpleMenuItem(ViewerConstants.PLANES[i], settings, "drawPlane", "", i));
            }
            VViewSimpleMenu orientationMENU = new VViewSimpleMenu(itemList.ToArray(), "Vessel orientation");
            itemList.Clear();
            itemList.Add(new VViewSimpleMenuItem("Active: ", settings, "", "spinAxis"));
            for (int i = 0; i < ViewerConstants.AXES.Length; i++)
            {
                itemList.Add(new VViewSimpleMenuItem(ViewerConstants.AXES[i]+" axis", settings, "spinAxis", "", i));
            }
            itemList.Add(new VViewSimpleMenuItem("Rotation speed:", settings, "spinSpeed", "spinSpeed"));
            VViewSimpleMenu rotationMENU = new VViewSimpleMenu(itemList.ToArray(), "Display autorotation");
            
            VViewSimpleMenuItem[] DCONItems = {
                new VViewSimpleMenuItem("Vessel orientation",orientationMENU),
                new VViewSimpleMenuItem("Display autorotation",rotationMENU),
                new VViewSimpleMenuItem("Autocentering:",settings,"autoCenter","autoCenter"),
                new VViewSimpleMenuItem("A.c. scaling:",settings,"centerRescale","centerRescale"),
                new VViewSimpleMenuItem("Hor. pod center:",settings,"centerOnRootH","centerOnRootH"),
                new VViewSimpleMenuItem("Ver. pod center:",settings,"centerOnRootV","centerOnRootV"),
                new VViewSimpleMenuItem("Dull bounds:",settings,"colorModeBoxDull","colorModeBoxDull"),
                new VViewSimpleMenuItem("Dull mesh:",settings,"colorModeMeshDull","colorModeMeshDull"),
                                      };
            VViewSimpleMenu displayConfigMENU = new VViewSimpleMenu(DCONItems, "Display configuration");

            orientationMENU.setRoot((IVViewMenu)displayConfigMENU);
            rotationMENU.setRoot((IVViewMenu)displayConfigMENU); 

            /***************************************************************************************************/
            
            itemList.Clear();
            itemList.Add(new VViewSimpleMenuItem("Active: ", settings, "", "colorModeMesh"));
            for (int i = 0; i < ViewerConstants.COLORMODES.Length; i++)
            {
                itemList.Add(new VViewSimpleMenuItem(ViewerConstants.COLORMODES[i], settings, "colorModeMesh", "", i));
            }
            VViewSimpleMenu passiveDisplayWireMENU = new VViewSimpleMenu(itemList.ToArray(), "Passive display (wire)");
            itemList.Clear();
            itemList.Add(new VViewSimpleMenuItem("Active: ", settings, "", "colorModeBox"));
            for (int i = 0; i < ViewerConstants.COLORMODES.Length; i++)
            {
                itemList.Add(new VViewSimpleMenuItem(ViewerConstants.COLORMODES[i], settings, "colorModeBox", "", i));
            }
            VViewSimpleMenu passiveDisplayBoundsMENU = new VViewSimpleMenu(itemList.ToArray(), "Passive display (wire)");

            VViewSimpleMenuItem[] PASItems = {
                new VViewSimpleMenuItem("Passive display (wire)",passiveDisplayWireMENU),
                new VViewSimpleMenuItem("Passive display (bounds)",passiveDisplayBoundsMENU),
                new VViewSimpleMenuItem("Display axes:",settings,"displayAxes","displayAxes"),
                new VViewSimpleMenuItem("Display COM:",settings,"displayCOM","displayCOM"),
                new VViewSimpleMenuItem("Display engine status:",settings,"displayEngines","displayEngines"),
                new VViewSimpleMenuItem("Display landing assist:",settings,"displayGround","displayGround"),
                                      };
            VViewSimpleMenu passiveDisplaysMENU = new VViewSimpleMenu(PASItems, "Passive display modes");

            passiveDisplayWireMENU.setRoot((IVViewMenu)passiveDisplaysMENU);
            passiveDisplayBoundsMENU.setRoot((IVViewMenu)passiveDisplaysMENU);

            /***************************************************************************************************/
            VViewMenuPartSelector partSelectMenu = new VViewMenuPartSelector(settings);
            VViewSimpleMenuItem[] INTItems = {
                new VViewSimpleMenuItem("Part selector (tree-traversal)",partSelectMenu),
                new VViewSimpleMenuItem("Zoom on selection:",settings,"selectionCenter","selectionCenter"),
                new VViewSimpleMenuItem("Affect symmetry:",settings,"selectionSymmetry","selectionSymmetry"),
                                      };
            VViewSimpleMenu interactiveDisplaysMENU = new VViewSimpleMenu(INTItems, "Interactive modes");

            partSelectMenu.setRoot((IVViewMenu)interactiveDisplaysMENU);
            /***************************************************************************************************/

            VViewSimpleMenuItem[] OTHItems = {
                new VViewSimpleMenuItem("Latency mode:",settings,"latency","latency"),
                                      };
            VViewSimpleMenu configurationMENU = new VViewSimpleMenu(OTHItems, "Other configuration");

            /***************************************************************************************************/

            VViewSimpleMenuItem[] MAMItems = {
                new VViewSimpleMenuItem("Display configuration",displayConfigMENU),
                new VViewSimpleMenuItem("Passive display modes",passiveDisplaysMENU),
                new VViewSimpleMenuItem("Interactive modes",interactiveDisplaysMENU),
                new VViewSimpleMenuItem("Other configuration",configurationMENU),
                                      };
            VViewSimpleMenu mainMenu = new VViewSimpleMenu(MAMItems, "Main menu");

            displayConfigMENU.setRoot((IVViewMenu)mainMenu);
            passiveDisplaysMENU.setRoot((IVViewMenu)mainMenu);
            interactiveDisplaysMENU.setRoot((IVViewMenu)mainMenu);
            configurationMENU.setRoot((IVViewMenu)mainMenu);

            VViewSimpleMenuItem[] HIDItems = {
                new VViewSimpleMenuItem("Show",mainMenu)
                                      };
            VViewSimpleMenu hideMenu = new VViewSimpleMenu(HIDItems, "Hidden");
            /*dispModeMenu.setRoot((IVViewMenu)mainMenu);
            */
            mainMenu.setRoot((IVViewMenu)hideMenu);
            activeMenu = hideMenu;

        }
    }
}
