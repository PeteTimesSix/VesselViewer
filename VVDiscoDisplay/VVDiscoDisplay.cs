using System;
using UnityEngine;
using VesselView;
using VesselViewRPM.menus;

namespace VVDiscoDisplay
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VVDiscoDisplay : MonoBehaviour
    {

        private Color[] gradient;
        
        /// <summary>
        /// Use for constructor-like stuff.
        /// </summary>
        void Start()
        {
            //this registers a custom menu with the RPM version of VesselView
            VViewCustomMenusMenu.registerMenu(createMenu);
            //this registers a display mode with the plugin version
            VesselViewPlugin.registerCustomMode(setup());
            //this is also the place to do any other init you might want
            gradient = VesselViewer.genColorGradient(256);
        }

        //we set the custom mode up in a separate function so it can
        //be used for both the plugin and the RPM version
        private CustomModeSettings setup() 
        {
            CustomModeSettings settings = new CustomModeSettings();

            //we store our data in a object in the custom settings
            //this is not strictly necessary but it does mean you can have
            //different states active on different screens
            DiscoData dataObject = new DiscoData();
            settings.dataInstance = dataObject;

            settings.name = "DISCO MODE";

            //since we want to change the color of each part, we must override that
            settings.ColorModeOverride = (int)CustomModeSettings.OVERRIDE_TYPES.FUNCTION;
            //orientation and centering can remain user-specified
            settings.OrientationOverride = (int)CustomModeSettings.OVERRIDE_TYPES.AS_BASIC;
            settings.CenteringOverride = (int)CustomModeSettings.OVERRIDE_TYPES.AS_BASIC;
            //we need to override the settings for minimodes to disable them
            settings.MinimodesOverride = (int)CustomModeSettings.OVERRIDE_TYPES.STATIC;

            //all minimodes off
            settings.staticSettings.displayCOM = false;
            settings.staticSettings.displayEngines = false;
            settings.staticSettings.displayAxes = false;
            settings.staticSettings.displayGround = (int)ViewerConstants.GROUND_DISPMODE.OFF;

            //all functions in a given category must be overrided if any are
            settings.fillColorDelegate = returnsRandomColor;
            settings.wireColorDelegate = returnsRandomColor;
            settings.boxColorDelegate = returnsBlack;

            settings.fillColorDullDelegate = returnsFalse;
            settings.wireColorDullDelegate = returnsFalse;
            settings.boxColorDullDelegate = returnsFalse;

            return settings;
        }

        //even though we dont use it, we still have to take a Part as argument to match the delegate
        private Color returnsRandomColor(CustomModeSettings customMode, Part part)
        {
            //grab the data object from the custom mode settings
            DiscoData data = (DiscoData)customMode.dataInstance;
            if (data.strobe) 
            {
                float red = data.rand.Next(256) / 256f;
                float grn = data.rand.Next(256) / 256f;
                float blu = data.rand.Next(256) / 256f;
                Color newColor = new Color(red, grn, blu);
                return newColor;
            }
            else 
            {
                int time = Time.frameCount % 512;
                if (time >= 256) time = 511 - time;
                return gradient[time];
            }
        }

        //note that setting bounding boxes to black makes them not render
        private Color returnsBlack(CustomModeSettings customMode,Part part) 
        {
            return Color.black;
        }

        private bool returnsFalse(CustomModeSettings customMode) 
        {
            return false;
        }

        IVViewMenu createMenu()
        {
            CustomModeSettings settings = setup();
            DiscoData data = (DiscoData)settings.dataInstance;
            //we dont need to, but if we want interaction in RPM we can create a menu
            //two options: either use a combination of VViewSimpleMenus, VViewSimpleMenuItems
            //and VViewSimpleCustomMenuItem, or...
            //Implement the IVViewMenu interface.

            IVVSimpleMenuItem[] INTItems = {
                 new VViewSimpleCustomMenuItem("STROBE:",data.getStrobe,data.setStrobe)
                                       };
            VViewSimpleMenu rootMenu = new VViewSimpleMenu(INTItems, "DISCO MODE");
            
            //either way, all menus associated with a custom display mode must have that mode set
            rootMenu.setCustomSettings(settings);

            //finally, return the menu so that it can be hooked up to the list
            return rootMenu;
        }
    }
}
