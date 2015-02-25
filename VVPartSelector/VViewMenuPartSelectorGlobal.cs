using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VesselView;
using VesselViewRPM.menus;

namespace VVPartSelector
{
    class VViewMenuPartSelectorGlobal : IVViewMenu
    {
        private IVViewMenu root;
        private SortedDictionary<String, List<Part>> actionsList = new SortedDictionary<String,List<Part>>();
        //private Dictionary<String, bool> unfoldList;

        private string name = "Part selector (list)";
        private int scrollOffset = 0;

        private bool active = false;

        public bool Active
        {
            get { return active; }
        }

        private String selection = "";
        //private Part subselection;

        private SelectorDataContainer master;

        public VViewMenuPartSelectorGlobal(SelectorDataContainer master) 
        {
            this.master = master;
        }
        
        public void printMenu(ref StringBuilder builder, int width, int height)
        {
            //MonoBehaviour.print("scroll:"+scrollOffset);
            int linesPrinted = 1;
            int pointerPosition = -1;
            foreach (String action in actionsList.Keys) 
            {
                //MonoBehaviour.print("Adding "+action+" to list");
                if(linesPrinted>=scrollOffset)
                    builder.Append(action);
                linesPrinted++;
                if (action.CompareTo(selection) == 0) {
                    if (linesPrinted >= scrollOffset)  builder.AppendLine(" <");
                    pointerPosition = linesPrinted;
                }
                else if (linesPrinted >= scrollOffset) builder.AppendLine("");
            }
            if (pointerPosition == -1) 
            {
                pointerPosition = 1;
                if (actionsList.Keys.Count > 0) selection = actionsList.Keys.First();
                else selection = "";
            }
            if (pointerPosition + 1 >= height + scrollOffset) scrollOffset = pointerPosition - height + 2;
            if (pointerPosition - 1 <= scrollOffset) scrollOffset = pointerPosition - 2;
        }

        public void up()
        {
            for (int i = 0; i < actionsList.Keys.Count;  i++)
            {
                if (actionsList.Keys.ElementAt(i).CompareTo(selection) == 0) 
                {
                    if(i==0) i = actionsList.Keys.Count;
                    selection = actionsList.Keys.ElementAt(i - 1);
                    return;
                }
            }
            resetFocus();
        }

        public void down()
        {
            for (int i = 0; i < actionsList.Keys.Count; i++)
            {
                if (actionsList.Keys.ElementAt(i).CompareTo(selection) == 0)
                {
                    if (i == actionsList.Keys.Count-1) i = -1;
                    selection = actionsList.Keys.ElementAt(i + 1);
                    return;
                }
            }
            resetFocus();
        }

        public IVViewMenu click()
        {
            VVsinglePartSubmenu submenu = new VVsinglePartSubmenu(master, selection);
            submenu.setRoot(this);
            return submenu;
        }

        public IVViewMenu back()
        {
            return root;
            //throw new NotImplementedException();
        }

        private void resetFocus() 
        {
            master.CustomSettings.focusSubset.Clear();
            if (master.getZoom())
            {
                List<Part> focusSubset = new List<Part>();
                List<Part> focusSubsetOrig = new List<Part>();
                if (actionsList.ContainsKey(selection))
                {
                    focusSubsetOrig.AddRange(actionsList[selection]);
                    focusSubset.AddRange(focusSubsetOrig);
                }
                if (master.getSymm())
                {
                    foreach (Part directPart in focusSubsetOrig)
                    {
                        foreach (Part counterpart in directPart.symmetryCounterparts)
                        {
                            if (!focusSubset.Contains(counterpart)) focusSubset.Add(counterpart);
                        }
                    }
                }
            }
        }

        public void activate()
        {
            //MonoBehaviour.print("Activate call");
            resetFocus();
            active = true;
        }

        public void deactivate()
        {
            master.CustomSettings.focusSubset.Clear();
            active = false;
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
            actionsList.Clear();
            foreach (Part part in ship.parts) 
            {
                checkPart(part);
            }
            if(master.getZoom())
                master.CustomSettings.focusSubset = getPartsMatchingSelection();
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
                        List<Part> partList;
                        if (!actionsList.ContainsKey(mEvent.guiName))
                        {
                            partList = new List<Part>();
                            actionsList.Add(mEvent.guiName, partList);
                        }
                        else 
                        {
                            partList = actionsList[mEvent.guiName];
                        }
                        if (partList != null) 
                        {
                            partList.Add(part);
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

        internal List<Part> getPartsMatchingSelection()
        {
            if (!actionsList.ContainsKey(selection)) return new List<Part>();
            else return actionsList[selection];
        }
    }
}
