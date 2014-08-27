using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VesselView;
using UnityEngine;

namespace VesselViewRPM.menus
{
    public class VViewSimpleCustomMenuItem : IVVSimpleMenuItem
    {
        public string label;

        private IVViewMenu menuToChangeTo = null;
        private Action<bool> setterBool;
        private Func<bool> getterBool;
        private Action<int> setterInt;
        private Func<int> getterInt;
        private Func<int> getterMax;

        public bool displayBoolean = false;
        public bool displayInteger = false;
        public bool displayMax = false;

        public VViewSimpleCustomMenuItem(string label) {
            this.label = label;
        }

        public VViewSimpleCustomMenuItem(string label, IVViewMenu menuTarget){
            this.label = label;
            this.menuToChangeTo = menuTarget;
        }

        public VViewSimpleCustomMenuItem(string label, Func<bool>getter,Action<bool> setter)
        {
            this.label = label;
            this.getterBool = getter;
            this.setterBool = setter;
            displayBoolean = true;
        }

        public VViewSimpleCustomMenuItem(string label, Func<int> getter, Action<int> setter, Func<int> getterMax)
        {
            this.label = label;
            this.getterInt = getter;
            this.setterInt = setter;
            this.getterMax = getterMax;
            displayInteger = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Menu to change to, null if stay</returns>
        public IVViewMenu click()
        {
            if (getterBool != null) 
            {
                setterBool(!getterBool());
            }
            if (getterInt != null)
            {
                int val = getterInt() + 1;
                if (val >= getterMax()) val = 0;
                setterInt(val);
            }  
            return menuToChangeTo;
        }

        public override string ToString() {
            String outStr = "" + label;
            if (getterBool != null & displayBoolean) outStr += ViewerConstants.boolAsString(getterBool());
            if (getterInt != null & displayInteger) outStr += " " + getterInt();
            if (getterMax != null & displayMax) outStr += " (" + getterMax() + ")";
            return outStr;
        }
    }
}
