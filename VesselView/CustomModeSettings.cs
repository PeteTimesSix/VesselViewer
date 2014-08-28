using System;
using System.Collections.Generic;
using UnityEngine;

namespace VesselView
{
    public class CustomModeSettings
    {
        public String name = "DEFAULT";

        //controls part colors
        public int ColorModeOverride = (int)OVERRIDE_TYPES.STATIC;
        //controls draw plane, spin
        public int OrientationOverride = (int)OVERRIDE_TYPES.STATIC;
        //centering
        public int CenteringOverride = (int)OVERRIDE_TYPES.STATIC;
        //COM, landing assist, axes
        public int MinimodesOverride = (int)OVERRIDE_TYPES.STATIC;

        public enum OVERRIDE_TYPES 
        {
            AS_BASIC, STATIC, FUNCTION
        }

        /*
         * AS_BASIC>            Uses whatever the player-set display mode uses.
         * STATIC>              Uses the values from the settings object below.
         * FUNCTION_GLOBAL>     Uses values returned by the below specified functions for all parts.
         * FUNCTION_PER_PART>   Uses values returned by the below specified functions for each part.
         */

        public ViewerSettings staticSettings = new ViewerSettings();

        //color mode

        public Func<CustomModeSettings, Part, Color> fillColorDelegate;
        public Func<CustomModeSettings, Part, Color> wireColorDelegate;
        public Func<CustomModeSettings, Part, Color> boxColorDelegate;

        public Func<CustomModeSettings, bool> fillColorDullDelegate;
        public Func<CustomModeSettings, bool> wireColorDullDelegate;
        public Func<CustomModeSettings, bool> boxColorDullDelegate;

        //orientation
        public Func<CustomModeSettings, int> drawPlaneDelegate;
        public Func<CustomModeSettings, int> spinAxisDelegate;
        public Func<CustomModeSettings, int> spinSpeedDelegate;

        //minimodes
        public Func<CustomModeSettings, bool> displayCOMDelegate;
        public Func<CustomModeSettings, bool> displayEnginesDelegate;
        public Func<CustomModeSettings, bool> displayAxesDelegate;
        public Func<CustomModeSettings, int> displayGroundDelegate;

        //centering
        public Func<CustomModeSettings, bool> centerOnRootHDelegate;
        public Func<CustomModeSettings, bool> centerOnRootVDelegate;
        public Func<CustomModeSettings, int> centerRescaleDelegate;
        public Func<CustomModeSettings, int> marginDelegate;
        public Func<CustomModeSettings, bool> autoCenterDelegate;

        public List<Part> focusSubset = new List<Part>();

        public object dataInstance;
    }
}
