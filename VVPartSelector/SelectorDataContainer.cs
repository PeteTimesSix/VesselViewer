using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;

namespace VVPartSelector
{
    public class SelectorDataContainer
    {

        internal VViewMenuPartSelectorTree selectorTree;
        internal VViewMenuPartSelectorGlobal selectorGlobal;

        private bool zoomOnSelection = false;
        private bool symmetryMode = false;
        private CustomModeSettings customSettings;

        public CustomModeSettings CustomSettings
        {
            get { return customSettings; }
            set { customSettings = value; }
        }

        internal bool getZoom() { return zoomOnSelection; }
        internal void setZoom(bool val) { zoomOnSelection = val; }
        internal bool getSymm() { return symmetryMode; }
        internal void setSymm(bool val) { symmetryMode = val; }

        public SelectorDataContainer()
        {
            selectorTree = new VViewMenuPartSelectorTree(this);
            selectorGlobal = new VViewMenuPartSelectorGlobal(this);
        }

        internal VVsinglePartSubmenu selectorSubmenu { get; set; }
    }
}
