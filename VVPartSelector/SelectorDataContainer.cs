using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVPartSelector
{
    class SelectorDataContainer
    {

        internal VViewMenuPartSelector selector = new VViewMenuPartSelector();

        internal bool getZoom() { return selector.zoomOnSelection; }
        internal void setZoom(bool val) { selector.zoomOnSelection = val; }
        internal bool getSymm() { return selector.symmetryMode; }
        internal void setSymm(bool val) { selector.symmetryMode = val; }

    }
}
