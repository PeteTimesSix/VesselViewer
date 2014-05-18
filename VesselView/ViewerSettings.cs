

namespace VesselView
{
    public class ViewerSettings
    {
        //settings
        public int colorModeMesh = (int)ViewerConstants.COLORMODE.NONE;
        public int colorModeBox = (int)ViewerConstants.COLORMODE.HIDE;
        public bool colorModeMeshDull = true;
        public bool colorModeBoxDull = false;
        public bool centerOnRootH = true;
        public bool centerOnRootV = false;
        public bool displayCOM = true;
        public bool displayEngines = true;
        //public bool displayCOP = true;

        public int centerRescale = (int)ViewerConstants.RESCALEMODE.INCR;

        public bool autoCenter = true;
        public bool latency = false;
        public int drawPlane = 0;
        public int scalePos = 0;
        public float scaleFact = 5;
        public float scrOffX = 0;
        public float scrOffY = 0;
        public int spinAxis = (int)ViewerConstants.AXIS.Y;
        public int spinSpeed = 0;

        public int centerRescaleMAX = ViewerConstants.RESCALEMODES.Length;
        public int colorModeMeshMAX = ViewerConstants.COLORMODES.Length;
        public int colorModeBoxMAX = ViewerConstants.COLORMODES.Length;
        public int drawPlaneMAX = ViewerConstants.PLANES.Length;
        public int scalePosMAX = ViewerConstants.SCALE_FACTS.Length;
        public int spinAxisMAX = ViewerConstants.AXES.Length;
        public int spinSpeedMAX = ViewerConstants.SPIN_SPEEDS.Length;

        //is window displayed?
        public bool screenVisible = false;
        //is window displayed?
        public bool configScreenVisible = false;

        //hardcoded separate mode because its easy and Im lazy
        public bool partSelectMode = false;
        public Part selectedPart;
        public Part subselectedPart;
        public bool selectionSymmetry = true;
        public bool selectionCenter = false;

        public Vessel ship;

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
         * MDULL - dull mesh
         * BDULL - dull box
         * PSMOD - part selection mode
         * PSSYM - part selection affect symmetry
         * PSCEN - zoom in on selected part
         */

        /*public enum IDs 
        {
            NULL, CMM, CMB, CORH, CORV, CRE, AUTC, LAT, DRP, SCP, SCA, SV, CSV, MDULL, BDULL, PSMOD, PSSYM, PSCEN
        }*/

        /*
         * NULL - no change (ignores changeValue)
         * SET - sets the property
         * ADD - addds to the property
         * SUB - subtracts from the property
         * MULT - multiplies the property
         * BSET - sets a boolean property
         * BINV - inverts a boolean property (ignores changeValue)
         * SINC - smart increment - increases by one and loops to zero on reaching over max value (ignores changeValue)
         * */

        //well this is all absolutely insane.
        //NO NO NO NO NO.
        //NO.

        /*
        public enum CHANGEMODES
        {
            NULL, SET, ADD, SUB, MULT, BSET, BINV, SINC
        }

        

        /// <summary>
        /// sets a property. silently fails on non-viable properties,
        /// ie: cant add numbers to a bool
        /// </summary>
        /// <param name="propertyID"></param>
        /// <param name="changeMode"></param>
        /// <param name="changeValue"></param>
        public void setPropertyByID(int propertyID, int changeMode, int changeValue){
            if (propertyID == (int)IDs.NULL) return;
            switch (changeMode) {
                case (int)CHANGEMODES.NULL:
                    break;
                case (int)CHANGEMODES.SET:
                    switch (propertyID)
                    {
                        case (int)IDs.CMM:
                            colorModeMesh = changeValue;
                            break;
                        case (int)IDs.CMB:
                            colorModeBox = changeValue;
                            break;
                        case (int)IDs.DRP:
                            drawPlane = changeValue;
                            break;
                        case (int)IDs.SCP:
                            scalePos = changeValue;
                            break;
                        case (int)IDs.SCA:
                            scaleFact = changeValue;
                            break;
                    }
                    break;
                case (int)CHANGEMODES.SUB:
                    changeValue = -changeValue;
                    //why do I have to do this, C#, whyyyy
                    goto case (int)CHANGEMODES.ADD;
                case (int)CHANGEMODES.ADD:
                    switch (propertyID)
                    {
                        case (int)IDs.CMM:
                            colorModeMesh += changeValue;
                            break;
                        case (int)IDs.CMB:
                            colorModeBox += changeValue;
                            break;
                        case (int)IDs.DRP:
                            drawPlane += changeValue;
                            break;
                        case (int)IDs.SCP:
                            scalePos += changeValue;
                            break;
                        case (int)IDs.SCA:
                            scaleFact += changeValue;
                            break;
                    }
                    break;
                case (int)CHANGEMODES.MULT:
                    switch (propertyID)
                    {
                        case (int)IDs.CMM:
                            colorModeMesh *= changeValue;
                            break;
                        case (int)IDs.CMB:
                            colorModeBox *= changeValue;
                            break;
                        case (int)IDs.DRP:
                            drawPlane *= changeValue;
                            break;
                        case (int)IDs.SCP:
                            scalePos *= changeValue;
                            break;
                        case (int)IDs.SCA:
                            scaleFact *= changeValue;
                            break;
                    }
                    break;
                case (int)CHANGEMODES.BSET:
                    bool boolVal = (changeValue==0)?false:true;
                    switch (propertyID)
                    {
                        case (int)IDs.CORH:
                            centerOnRootH = boolVal;
                            break;
                        case (int)IDs.CORV:
                            centerOnRootV = boolVal;
                            break;
                        case (int)IDs.CRE:
                            centerRescale = boolVal;
                            break;
                        case (int)IDs.AUTC:
                            autoCenter = boolVal;
                            break;
                        case (int)IDs.LAT:
                            latency = boolVal;
                            break;
                        case (int)IDs.SV:
                            screenVisible = boolVal;
                            break;
                        case (int)IDs.CSV:
                            configScreenVisible = boolVal;
                            break;
                        case (int)IDs.MDULL:
                            colorModeMeshDull = boolVal;
                            break;
                        case (int)IDs.BDULL:
                            colorModeBoxDull = boolVal;
                            break;
                        case (int)IDs.PSMOD:
                            partSelectMode = boolVal;
                            break;
                        case (int)IDs.PSSYM:
                            selectionSymmetry = boolVal;
                            break;
                        case (int)IDs.PSCEN:
                            selectionCenter = boolVal;
                            break;

                    }
                    break;
                case (int)CHANGEMODES.BINV:
                    switch (propertyID)
                    {
                        case (int)IDs.CORH:
                            centerOnRootH = !centerOnRootH;
                            break;
                        case (int)IDs.CORV:
                            centerOnRootV = !centerOnRootV;
                            break;
                        case (int)IDs.CRE:
                            centerRescale = !centerRescale;
                            break;
                        case (int)IDs.AUTC:
                            autoCenter = !autoCenter;
                            break;
                        case (int)IDs.LAT:
                            latency = !latency;
                            break;
                        case (int)IDs.SV:
                            screenVisible = !screenVisible;
                            break;
                        case (int)IDs.CSV:
                            configScreenVisible = !configScreenVisible;
                            break;
                        case (int)IDs.MDULL:
                            colorModeMeshDull = !colorModeMeshDull;
                            break;
                        case (int)IDs.BDULL:
                            colorModeBoxDull = !colorModeBoxDull;
                            break;
                        case (int)IDs.PSMOD:
                            partSelectMode = !partSelectMode;
                            break;
                        case (int)IDs.PSSYM:
                            selectionSymmetry = !selectionSymmetry;
                            break;
                        case (int)IDs.PSCEN:
                            selectionCenter = !selectionCenter;
                            break;
                    }
                    break;
                case (int)CHANGEMODES.SINC:
                    switch (propertyID)
                    {
                        case (int)IDs.CMM:
                            colorModeMesh++;
                            if (colorModeMesh >= ViewerConstants.COLORMODES.Length) colorModeMesh = 0;
                            break;
                        case (int)IDs.CMB:
                            colorModeBox++;
                            if (colorModeBox >= ViewerConstants.COLORMODES.Length) colorModeBox = 0;
                            break;
                        case (int)IDs.DRP:
                            drawPlane++;
                            if (drawPlane >= ViewerConstants.PLANES.Length) drawPlane = 0;
                            break;
                        case (int)IDs.SCP:
                            scalePos++;
                            if (scalePos >= ViewerConstants.SCALE_FACTS.Length) scalePos = 0;
                            break;
                    }
                    break;
            }
        }

         * */
        
        /// <summary>
        /// Returns a meaningful description of a given property
        /// </summary>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        public string getPropertyDesc(string propertyName){

            switch (propertyName) 
            {
                case "colorModeMesh":
                    return ViewerConstants.COLORMODES[colorModeMesh];
                case "colorModeBox":
                    return ViewerConstants.COLORMODES[colorModeBox];
                case "centerOnRootH":
                    return boolAsString(centerOnRootH);
                case "centerOnRootV":
                    return boolAsString(centerOnRootV);
                case "displayCOM":
                    return boolAsString(displayCOM);
                case "displayEngines":
                    return boolAsString(displayEngines);
                /*case "displayCOP":
                    return boolAsString(displayCOP);*/
                case "centerRescale":
                    return ViewerConstants.RESCALEMODES[centerRescale];
                case "autoCenter":
                    return boolAsString(autoCenter);
                case "latency":
                    return boolAsString(latency);
                case "drawPlane":
                    return ViewerConstants.PLANES[drawPlane];
                case "scalePos":
                    return ViewerConstants.SCALE_FACTS[scalePos]+" predefined";
                case "colorModeBoxDull":
                    return boolAsString(colorModeBoxDull);
                case "colorModeMeshDull":
                    return boolAsString(colorModeMeshDull);
                case "partSelectMode":
                    return boolAsString(partSelectMode);
                case "selectionSymmetry":
                    return boolAsString(selectionSymmetry);
                case "selectionCenter":
                    return boolAsString(selectionCenter);
                case "spinAxis":
                    return ViewerConstants.AXES[spinAxis];
                case "spinSpeed":
                    return ViewerConstants.SPIN_SPEEDS[spinSpeed];
            }
            return "ERROR";
        }

        /// <summary>
        /// Because On/Off is nicer and shorter than True/False
        /// </summary>
        /// <returns></returns>
        private string boolAsString(bool boolean) {
            return (boolean) ? "On" : "Off";
        }
        

        /*
        public string getPropertyByID(int propertyID)
        {
            switch (propertyID)
            {
                case (int)IDs.NULL:
                    return "";
                case (int)IDs.CMM:
                    return "" + colorModeMesh;
                case (int)IDs.CMB:
                    return "" + colorModeBox;
                case (int)IDs.CORH:
                    return "" + centerOnRootH;
                case (int)IDs.CORV:
                    return "" + centerOnRootV;
                case (int)IDs.CRE:
                    return "" + centerRescale;
                case (int)IDs.AUTC:
                    return "" + autoCenter;
                case (int)IDs.LAT:
                    return "" + latency;
                case (int)IDs.DRP:
                    return "" + drawPlane;
                case (int)IDs.SCP:
                    return "" + scalePos;
                case (int)IDs.SCA:
                    return "" + scaleFact;
                case (int)IDs.SV:
                    return "" + screenVisible;
                case (int)IDs.CSV:
                    return "" + configScreenVisible;
            }
            return "ERROR";
        }
         * */
        
    }
}
