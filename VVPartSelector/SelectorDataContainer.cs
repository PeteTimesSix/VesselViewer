using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;

namespace VVPartSelector
{
    class SelectorDataContainer
    {

        internal VViewMenuPartSelector selectorTree;
        internal VViewMenuPartSelector selectorGlobal;

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
            selectorTree = new VViewMenuPartSelector(VViewMenuPartSelector.SELECTORTYPE.TREE, this);
            selectorGlobal = new VViewMenuPartSelector(VViewMenuPartSelector.SELECTORTYPE.GLOBAL, this);
        }
    }
}
