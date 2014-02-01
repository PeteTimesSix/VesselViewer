using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;

namespace VesselViewRPM
{
    class VViewMenuItem
    {
        public string label;
        private ViewerSettings settings;
        private int propertyToChangeID = (int)ViewerSettings.IDs.NULL;
        private int propertyToPrintID = (int)ViewerSettings.IDs.NULL;
        //if true, just change directly
        //if false, change by the value of the property ID'd
        private bool changeValueDirect = true;
        private int changeValue = 0;
        private int changeMode = (int)ViewerSettings.CHANGEMODES.NULL;
        private VViewMenu menuToChangeTo = null;

        public VViewMenuItem(string label) {
            this.label = label;
        }

        public VViewMenuItem(string label, VViewMenu menuTarget){
            this.label = label;
            this.menuToChangeTo = menuTarget;
        }

        public VViewMenuItem(string label, VesselView.ViewerSettings settings, int propertyToChange, int propertyToPrint, bool valueDirect, int value, int changeMode)
        {
            this.label = label;
            this.settings = settings;
            this.propertyToChangeID = propertyToChange;
            this.propertyToPrintID = propertyToPrint;
            this.changeValueDirect = valueDirect;       
            this.changeValue = value;
            this.changeMode = changeMode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if menu should hide</returns>
        internal VViewMenu click()
        {
            if (settings != null) {
                if (changeValueDirect)
                {
                    settings.setPropertyByID(propertyToChangeID, changeMode, changeValue);
                }
                else
                {
                    int value;
                    int.TryParse(settings.getPropertyByID(changeValue), out value);
                    settings.setPropertyByID(propertyToChangeID, changeMode, value);
                }
            }          
            return menuToChangeTo;
        }

        public override string ToString() {
            if (settings == null)
            {
                return label;
            }
            else 
            {
                return label + settings.getSmartPropertyByID(propertyToPrintID);
            }
        }
    }
}
