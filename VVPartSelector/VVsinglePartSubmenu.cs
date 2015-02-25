using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselView;
using VesselViewRPM.menus;

namespace VVPartSelector
{
    class VVsinglePartSubmenu : IVViewMenu
    {

        private IVViewMenu root;
        private String name;

        private List<Part> partList = new List<Part>();
        private Part selection;

        private int scrollOffset = 0;

        private SelectorDataContainer master;

        internal VVsinglePartSubmenu(SelectorDataContainer master, String action) 
        {
            this.master = master;
            this.name = action;
        }

        public void printMenu(ref StringBuilder builder, int width, int height)
        {
            //MonoBehaviour.print("scroll:"+scrollOffset);
            int linesPrinted = 1;
            int pointerPosition = -1;
            foreach (Part part in partList)
            {
                //MonoBehaviour.print("Adding "+action+" to list");
                if (linesPrinted >= scrollOffset)
                    builder.Append(part.name);
                linesPrinted++;
                if (part == selection)
                {
                    if (linesPrinted >= scrollOffset) builder.AppendLine(" <");
                    pointerPosition = linesPrinted;
                }
                else if (linesPrinted >= scrollOffset) builder.AppendLine("");
            }
            if (pointerPosition == -1)
            {
                pointerPosition = 1;
                if (partList.Count>0) selection = partList.First();
            }
            if (pointerPosition + 1 >= height + scrollOffset) scrollOffset = pointerPosition - height + 2;
            if (pointerPosition - 1 <= scrollOffset) scrollOffset = pointerPosition - 2;
        }

        public void up()
        {
            for (int i = 0; i < partList.Count; i++)
            {
                if (partList.ElementAt(i) == selection)
                {
                    if (i == 0) i = partList.Count;
                    selection = partList.ElementAt(i - 1);
                    return;
                }
            }
        }

        public void down()
        {
            for (int i = 0; i < partList.Count; i++)
            {
                if (partList.ElementAt(i) == selection)
                {
                    if (i == partList.Count - 1) i = -1;
                    selection = partList.ElementAt(i + 1);
                    return;
                }
            }
        }

        public IVViewMenu click()
        {
            if (master.getSymm())
            {
                foreach (Part symPart in selection.symmetryCounterparts)
                {
                    if (symPart == null) continue;
                    foreach (PartModule pm in symPart.GetComponents<PartModule>())
                    {
                        foreach (BaseEvent mEvent in pm.Events)
                        {
                            if (mEvent.guiActive & mEvent.active)
                            {
                                if (mEvent.guiName.Equals(name))
                                {
                                    mEvent.Invoke();
                                }
                            }
                        }
                    }
                }
            }
            foreach (PartModule pm in selection.GetComponents<PartModule>())
            {
                foreach (BaseEvent mEvent in pm.Events)
                {
                    if (mEvent.guiActive & mEvent.active)
                    {
                        if (mEvent.guiName.Equals(name))
                        {
                            mEvent.Invoke();
                        }
                    }
                }
            }
            return update(selection.vessel);
        }

        public IVViewMenu back()
        {
            return root;
        }

        public void activate()
        {
            master.CustomSettings.focusSubset.Clear();
            if (master.getZoom())
            {
                List<Part> focusSubset = new List<Part>();
                focusSubset.Add(selection);
                if (master.getSymm())
                {
                    focusSubset.AddRange(selection.symmetryCounterparts);
                }
            }
            master.selectorSubmenu = this;
        }

        public void deactivate()
        {
            master.CustomSettings.focusSubset.Clear();
            master.selectorSubmenu = null;
        }

        public void setRoot(IVViewMenu root)
        {
            this.root = root;
        }

        public IVViewMenu getRoot()
        {
            return root;
        }

        public IVViewMenu update(Vessel ship)
        {
            partList.Clear();
            foreach (Part part in ship.parts) 
            {
                checkPart(part);
            }
            if (partList.Count == 0) return root;
            if (selection == null) selection = partList.First();
            else if (!partList.Contains(selection)) selection = partList.First();
            if (master.getZoom())
                master.CustomSettings.focusSubset = getSelectedParts();
            return null;
        }

        private void checkPart(Part part)
        {
            foreach (PartModule pm in part.GetComponents<PartModule>())
            {
                foreach (BaseEvent mEvent in pm.Events)
                {
                    if (mEvent.guiActive & mEvent.active)
                    {
                        if (mEvent.guiName.CompareTo(name) == 0) 
                        {
                            if (master.getSymm()) 
                            {
                                foreach (Part oldPart in partList)
                                {
                                    if (oldPart.symmetryCounterparts.Contains(part)) return;
                                }
                            }
                            partList.Add(part);
                            return;
                        }
                    }
                }
            }
        }

        public string getName()
        {
            return name;
        }

        public CustomModeSettings getCustomSettings()
        {
            return master.CustomSettings;
        }

        public void setCustomSettings(CustomModeSettings settings)
        {
            master.CustomSettings = settings;
        }

        internal List<Part> getSelectedParts()
        {
            List<Part> selected = new List<Part>();
            selected.Add(selection);
            if (master.getSymm()) 
            {
                foreach (Part subpart in selection.symmetryCounterparts) 
                {
                    if (partHasAction(subpart)) selected.Add(subpart);
                }
            }
            return selected;
        }

        internal bool partHasAction(Part part) 
        {
            foreach (PartModule pm in part.GetComponents<PartModule>())
            {
                foreach (BaseEvent mEvent in pm.Events)
                {
                    if (mEvent.guiActive & mEvent.active)
                    {
                        if (mEvent.guiName.CompareTo(name) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
