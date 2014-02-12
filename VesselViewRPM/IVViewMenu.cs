using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VesselViewRPM
{
    interface IVViewMenu
    {
        void printMenu(ref StringBuilder builder, int width, int height);

        void up();
        void down();
        IVViewMenu click();

        void setRoot(IVViewMenu root);
        IVViewMenu getRoot();

    }
}
