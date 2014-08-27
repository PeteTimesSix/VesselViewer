using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVDiscoDisplay
{
    class DiscoData
    {
        internal System.Random rand = new System.Random();

        //used for the RPM menu button
        internal bool strobe = false;
        public bool getStrobe() { return strobe; }
        public void setStrobe(bool strobe) { this.strobe = strobe; }
    }
}
