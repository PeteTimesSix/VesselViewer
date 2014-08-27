using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;

namespace VesselViewRPM.menus
{
    public interface IVViewMenu
    {
        void printMenu(ref StringBuilder builder, int width, int height);

        void up();
        void down();
        IVViewMenu click();
        IVViewMenu back();

        void activate();
        void deactivate();

        void setRoot(IVViewMenu root);
        IVViewMenu getRoot();

        IVViewMenu update(Vessel ship);

        string getName();

        //returns null if not a custom mode
        CustomModeSettings getCustomSettings();
        void setCustomSettings(CustomModeSettings settings);
    }
}
