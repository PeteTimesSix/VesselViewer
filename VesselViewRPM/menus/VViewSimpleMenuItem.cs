using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VesselView;
using UnityEngine;

namespace VesselViewRPM.menus
{
    public class VViewSimpleMenuItem : IVVSimpleMenuItem
    {
        public string label;
        private ViewerSettings settings;

        private string targetProperty = "";
        private string displayProperty = "";

        //if true, set direct value, else cycle through options
        private bool setValue = false;
        private int value;
        private bool addValue = false;

        //private int propertyToChangeID = (int)ViewerSettings.IDs.NULL;
        //private int propertyToPrintID = (int)ViewerSettings.IDs.NULL;
        //if true, just change directly
        //if false, change by the value of the property ID'd
        //private bool changeValueDirect = true;
        //private int changeValue = 0;
        //private int changeMode = (int)ViewerSettings.CHANGEMODES.NULL;
        private IVViewMenu menuToChangeTo = null;

        public VViewSimpleMenuItem(string label) {
            this.label = label;
        }

        public VViewSimpleMenuItem(string label, IVViewMenu menuTarget){
            this.label = label;
            this.menuToChangeTo = menuTarget;
        }

        public VViewSimpleMenuItem(string label, VesselView.ViewerSettings settings, string propertyToChange, string propertyToPrint, int value, bool addition)
        {
            this.label = label;
            this.settings = settings;
            this.targetProperty = propertyToChange;
            this.displayProperty = propertyToPrint;
            this.setValue = true;
            this.value = value;
            this.addValue = addition;
            //this.propertyToChangeID = propertyToChange;
            //this.propertyToPrintID = propertyToPrint;
            //this.changeValueDirect = valueDirect;       
            //this.changeValue = value;
            //this.changeMode = changeMode;
        }

        public VViewSimpleMenuItem(string label, VesselView.ViewerSettings settings, string propertyToChange, string propertyToPrint/*, bool valueDirect, int value*/)
        {
            this.label = label;
            this.settings = settings;
            this.targetProperty = propertyToChange;
            this.displayProperty = propertyToPrint;
            //this.propertyToChangeID = propertyToChange;
            //this.propertyToPrintID = propertyToPrint;
            //this.changeValueDirect = valueDirect;       
            //this.changeValue = value;
            //this.changeMode = changeMode;
        }

        public override string ToString() {
            if (settings == null)
            {
                return label;
            }
            else 
            {
                if (displayProperty.Equals("")) 
                    return label;
                else 
                    return label + settings.getPropertyDesc(displayProperty);
                //return label + settings.getSmartPropertyByID(propertyToPrintID);
            }
        }

        public IVViewMenu click()
        {
            if (settings != null & !targetProperty.Equals(""))
            {
                MonoBehaviour.print("targetProperty> " + targetProperty);
                FieldInfo fieldInfo = settings.GetType().GetField(targetProperty);
                MonoBehaviour.print("fieldInfo> " + fieldInfo);
                object value = fieldInfo.GetValue(settings);
                MonoBehaviour.print("value> " + value);
                if (value is bool)
                {
                    fieldInfo.SetValue(settings, !(bool)(value));
                }
                else if (value is int)
                {
                    if (setValue)
                    {
                        if (!addValue)
                            fieldInfo.SetValue(settings, this.value);
                        else
                        {
                            int curVal = (int)fieldInfo.GetValue(settings);
                            fieldInfo.SetValue(settings, curVal + this.value);
                        }
                    }
                    else
                    {
                        FieldInfo fieldInfoMax = typeof(ViewerConstants).GetField(targetProperty + "MAX");
                        int valNum = (int)(value) + 1;
                        if (valNum >= (int)(fieldInfoMax.GetValue(settings))) valNum = 0;
                        fieldInfo.SetValue(settings, valNum);
                    }

                }
                /*if (changeValueDirect)
                {
                    //settings.setPropertyByID(propertyToChangeID, changeMode, changeValue);
                }
                else
                {
                    //int value;
                    //int.TryParse(settings.getPropertyByID(changeValue), out value);
                    //settings.setPropertyByID(propertyToChangeID, changeMode, value);
                }*/
            }
            return menuToChangeTo;
        }
    }
}
