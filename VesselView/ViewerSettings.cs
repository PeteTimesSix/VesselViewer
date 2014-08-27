using System;

namespace VesselView
{
    public class ViewerSettings
    {
        //---------------------------------------------------------------------------
        //editable settings

        //part-specific color options
        public int colorModeFill = (int)ViewerConstants.COLORMODE.FUEL;
        public int colorModeWire = (int)ViewerConstants.COLORMODE.NONE;
        public int colorModeBox = (int)ViewerConstants.COLORMODE.HIDE;

        //render toggles options
        public bool colorModeFillDull = false;
        public bool colorModeWireDull = true;
        public bool colorModeBoxDull = false;

        //orientation
        public int drawPlane = 0;
        public int spinAxis = (int)ViewerConstants.AXIS.Y;
        public int spinSpeed = 0;

        //minimodes
        public bool displayCOM = true;
        public bool displayEngines = true;
        public bool displayAxes = false;
        public int displayGround = (int)ViewerConstants.GROUND_DISPMODE.OFF;

        //centering
        public bool centerOnRootH = true;
        public bool centerOnRootV = false;
        public int centerRescale = (int)ViewerConstants.RESCALEMODE.INCR;
        public int margin = (int)ViewerConstants.MARGIN.SMALL;
        public bool autoCenter = true;
        


        //---------------------------------------------------------------------------
        //noneditable settings
        //note that these can be changed in the GUI and such
        //but they are not meant to be changed by modes


        public int scalePos = 0;
        public float scaleFact = 5;
        public int scrOffX = 0;
        public int scrOffY = 0;
        public int latency = (int)ViewerConstants.LATENCY.LOW;

        //---------------------------------------------------------------------------
        //internal settings

        //is window displayed? (page visible in RPM)
        public bool screenVisible = false;
        //is config window displayed? (plugin only)
        public bool configScreenVisible = false;

        //hardcoded separate mode because its easy and Im lazy
        /*public bool partSelectMode = false;
        public Part selectedPart;
        public Part subselectedPart;
        public bool selectionSymmetry = true;
        public bool selectionCenter = false;*/

        //public Vessel ship;
        
        /// <summary>
        /// Returns a meaningful description of a given property
        /// </summary>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        public string getPropertyDesc(string propertyName){

            switch (propertyName) 
            {
                case "colorModeFill":
                    return ViewerConstants.COLORMODES[colorModeFill];
                case "colorModeMesh":
                    return ViewerConstants.COLORMODES[colorModeWire];
                case "colorModeBox":
                    return ViewerConstants.COLORMODES[colorModeBox];
                case "centerOnRootH":
                    return ViewerConstants.boolAsString(centerOnRootH);
                case "centerOnRootV":
                    return ViewerConstants.boolAsString(centerOnRootV);
                case "displayCOM":
                    return ViewerConstants.boolAsString(displayCOM);
                case "displayEngines":
                    return ViewerConstants.boolAsString(displayEngines);
                case "displayGround":
                    return ViewerConstants.GROUND_DISPMODES[displayGround];
                case "displayAxes":
                    return ViewerConstants.boolAsString(displayAxes);
                /*case "displayCOP":
                    return boolAsString(displayCOP);*/
                case "centerRescale":
                    return ViewerConstants.RESCALEMODES[centerRescale];
                case "autoCenter":
                    return ViewerConstants.boolAsString(autoCenter);
                case "latency":
                    return ViewerConstants.LATENCIES[latency];
                case "drawPlane":
                    return ViewerConstants.PLANES[drawPlane];
                case "scalePos":
                    return ViewerConstants.SCALE_FACTS[scalePos] + " predefined";
                case "colorModeFillDull":
                    return ViewerConstants.boolAsString(colorModeFillDull);
                case "colorModeBoxDull":
                    return ViewerConstants.boolAsString(colorModeBoxDull);
                case "colorModeMeshDull":
                    return ViewerConstants.boolAsString(colorModeWireDull);
                /*case "partSelectMode":
                    return boolAsString(partSelectMode);
                case "selectionSymmetry":
                    return boolAsString(selectionSymmetry);
                case "selectionCenter":
                    return boolAsString(selectionCenter);*/
                case "spinAxis":
                    return ViewerConstants.AXES[spinAxis];
                case "spinSpeed":
                    return ViewerConstants.SPIN_SPEEDS[spinSpeed];
            }
            return "ERROR";
        }


    }
}
