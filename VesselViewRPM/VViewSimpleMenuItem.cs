using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;

namespace VesselViewRPM
{
    class VViewSimpleMenuItem
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
        private IVViewMenu menuToChangeTo = null;

        public VViewSimpleMenuItem(string label) {
            this.label = label;
        }

        public VViewSimpleMenuItem(string label, IVViewMenu menuTarget){
            this.label = label;
            this.menuToChangeTo = menuTarget;
        }

        public VViewSimpleMenuItem(string label, VesselView.ViewerSettings settings, int propertyToChange, int propertyToPrint, bool valueDirect, int value, int changeMode)
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
        /// <returns>Menu to change to, null if stay</returns>
        internal IVViewMenu click()
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
