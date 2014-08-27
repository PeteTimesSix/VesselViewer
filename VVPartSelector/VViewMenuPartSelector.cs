using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VesselViewRPM.menus;
using VesselView;

namespace VVPartSelector
{
    public class VViewMenuPartSelector : IVViewMenu
    {
        private CustomPartTree tree;
        private IVViewMenu rootMenu;
        //private ViewerSettings settings;
        private CustomModeSettings customSettings;
        private string name = "Part selector";

        internal bool zoomOnSelection = false;
        internal bool symmetryMode = false;


        public enum SELECTIONMODES {
            NONE, EXPAND_PARTS, PARTS, EXPAND_ACTIONS, ACTIONS
        }

        //private int selectedLine = 0;
        //private int selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
        
        public VViewMenuPartSelector() {
            tree = new CustomPartTree(FlightGlobals.ActiveVessel);
        }

        public Part getSubselection() 
        {
            if (tree == null) return null;
            if (tree.selectedItem == null) return null;
            if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS)
            {
                return tree.selectedItem.children[tree.selectedItem.selectedLine].associatedPart;
            }
            return null;
        }

        public Part getSelection()
        {
            if (tree == null) return null;
            if (tree.selectedItem == null) return null;
            return tree.selectedItem.associatedPart;
        }

        public void printMenu(ref StringBuilder builder, int width, int height)
        {
            //builder.AppendLine("WIP");
            
            if (tree.selectedItem != null) {
                builder.AppendLine("-" + tree.selectedItem.associatedPart.name + "-");
                if (tree.selectedItem.hasChildrn) {
                    if (tree.selectedItem.childrnExpanded)
                    {
                        builder.Append("[-]PARTS");
                        if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
                        {
                            builder.AppendLine(" <");
                        }
                        else {
                            builder.AppendLine("");
                        }
                        int counter = 0;
                        foreach (CustomPartTreeItem partItem in tree.selectedItem.children) {
                            builder.Append(partItem.associatedPart.name);
                            if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS & counter == tree.selectedItem.selectedLine)
                            {
                                builder.AppendLine(" <");
                            }
                            else
                            {
                                builder.AppendLine("");
                            }
                            counter++;
                        }
                    }
                    else {
                        builder.Append("[+]PARTS");
                        if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
                        {
                            builder.AppendLine(" <");
                        }
                        else {
                            builder.AppendLine("");
                        }
                    }
                }
                if (tree.selectedItem.hasActions)
                {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        builder.Append("[-]ACTIONS");
                        if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
                        {
                            builder.AppendLine(" <");
                        }
                        else {
                            builder.AppendLine("");
                        }
                        int counter = 0;
                        foreach (BaseEvent action in tree.selectedItem.getActivableEvents()) {
                            builder.Append(action.guiName);
                            if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.ACTIONS & counter == tree.selectedItem.selectedLine)
                            {
                                builder.AppendLine(" <");
                            }
                            else
                            {
                                builder.AppendLine("");
                            }
                            counter++;              
                        }
                    }
                    else
                    {
                        builder.Append("[+]ACTIONS");
                        if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
                            {
                                builder.AppendLine(" <");
                            }
                        else {
                            builder.AppendLine("");
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Returns true if safe to proceed
        /// </summary>
        /// <returns></returns>
        private bool sanityCheck() {
            if (tree.selectedItem == null)
            {
                return false;
            }
            else {
                if (!tree.selectedItem.hasActions & tree.selectedItem.selectionMode == (int)SELECTIONMODES.ACTIONS)
                {
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.NONE;
                    tree.selectedItem.selectedLine = 0;
                }
                if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS & !tree.selectedItem.hasChildrn)
                {
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.NONE;
                    tree.selectedItem.selectedLine = 0;
                }
                else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.NONE)
                {
                    if (tree.selectedItem.hasChildrn)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                    else if (tree.selectedItem.hasActions)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
            }
            if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS)
            {
                if (tree.selectedItem.selectedLine < 0) tree.selectedItem.selectedLine = 0;
                if (tree.selectedItem.selectedLine >= tree.selectedItem.children.Count()) tree.selectedItem.selectedLine = tree.selectedItem.children.Count() - 1;
            }
            else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.ACTIONS)
            {
                if (tree.selectedItem.selectedLine < 0) tree.selectedItem.selectedLine = 0;
                if (tree.selectedItem.selectedLine >= tree.selectedItem.getActivableEvents().Count()) tree.selectedItem.selectedLine = tree.selectedItem.getActivableEvents().Count() - 1;
            }
            return true;
        }

        /// <summary>
        /// Well isnt this a delightful mess of hardcoding?
        /// </summary>
        public void up()
        {
            if (sanityCheck()) {
            if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
            {
                if (tree.selectedItem.hasActions) {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.ACTIONS;
                        tree.selectedItem.selectedLine = tree.selectedItem.actionCount - 1;
                    }
                    else
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
                else if (tree.selectedItem.childrnExpanded)
                {
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.PARTS;
                    tree.selectedItem.selectedLine = tree.selectedItem.children.Count() - 1;
                }
                else {
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                }
            }
            else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS)
            {
                tree.selectedItem.selectedLine--;
                if (tree.selectedItem.selectedLine < 0)
                {
                    tree.selectedItem.selectedLine = 0;
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                }
            }
            else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
            {
                if (tree.selectedItem.hasChildrn)
                {
                    if (tree.selectedItem.childrnExpanded)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.PARTS;
                        tree.selectedItem.selectedLine = tree.selectedItem.children.Count() - 1;
                    }
                    else
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                }
                else {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.ACTIONS;
                        tree.selectedItem.selectedLine = tree.selectedItem.actionCount - 1;
                    }
                    else
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
            }
            else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.ACTIONS)
            {
                tree.selectedItem.selectedLine--;
                if (tree.selectedItem.selectedLine < 0)
                {
                    tree.selectedItem.selectedLine = 0;
                    tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                }
            }
            }
            customSettings.focusSubset.Clear();
            if (zoomOnSelection)
            {
                if (tree.selectedItem != null)
                {
                    if (symmetryMode)
                    {
                        foreach (Part symPart in tree.selectedItem.associatedPart.symmetryCounterparts)
                        {
                            customSettings.focusSubset.Add(symPart);
                        }
                    }
                    customSettings.focusSubset.Add(tree.selectedItem.associatedPart);
                }
            }      
        }

        public void down()
        {
            if (sanityCheck())
            {
                if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
                {
                    if (tree.selectedItem.childrnExpanded)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.PARTS;
                        tree.selectedItem.selectedLine = 0;
                    }
                    else if (tree.selectedItem.hasActions)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                    else
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                }
                else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.PARTS)
                {
                    tree.selectedItem.selectedLine++;
                    if (tree.selectedItem.selectedLine >= tree.selectedItem.children.Count())
                    {
                        tree.selectedItem.selectedLine = 0;
                        if (tree.selectedItem.hasActions)
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                        else
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                    }
                }
                else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
                {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.ACTIONS;
                        tree.selectedItem.selectedLine = 0;
                    }
                    else
                    {
                        if (tree.selectedItem.hasChildrn)
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                        else
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                    }
                }
                else if (tree.selectedItem.selectionMode == (int)SELECTIONMODES.ACTIONS)
                {
                    tree.selectedItem.selectedLine++;
                    if (tree.selectedItem.selectedLine >= tree.selectedItem.actionCount)
                    {
                        tree.selectedItem.selectedLine = 0;
                        if (tree.selectedItem.hasChildrn)
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                        else
                        {
                            tree.selectedItem.selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                    }
                }
            }
            customSettings.focusSubset.Clear();
            if (zoomOnSelection) 
            {
                if (tree.selectedItem != null)
                {
                    if (symmetryMode)
                    {
                        foreach (Part symPart in tree.selectedItem.associatedPart.symmetryCounterparts)
                        {
                            customSettings.focusSubset.Add(symPart);
                        }
                    }
                    customSettings.focusSubset.Add(tree.selectedItem.associatedPart);
                }
            }       
        }

        public IVViewMenu click()
        {
            if (sanityCheck())
            {
                switch (tree.selectedItem.selectionMode)
                {
                    case (int)SELECTIONMODES.NONE:
                        break;
                    case (int)SELECTIONMODES.EXPAND_PARTS:
                        tree.selectedItem.childrnExpanded = !tree.selectedItem.childrnExpanded;
                        break;
                    case (int)SELECTIONMODES.EXPAND_ACTIONS:
                        tree.selectedItem.actionsExpanded = !tree.selectedItem.actionsExpanded;
                        break;
                    case (int)SELECTIONMODES.PARTS:
                        tree.selectedItem = tree.selectedItem.children[tree.selectedItem.selectedLine];
                        tree.selectedItem.selectionMode = (int)SELECTIONMODES.NONE;
                        tree.selectedItem.selectedLine = 0;
                        break;
                    case (int)SELECTIONMODES.ACTIONS:

                        //REALLY a lot easier than expected

                        if (symmetryMode){
                            string name = tree.selectedItem.getActivableEvents()[tree.selectedItem.selectedLine].guiName;
                            foreach (Part part in tree.selectedItem.associatedPart.symmetryCounterparts) {
                                if (part == null) continue;
                                foreach (PartModule pm in part.GetComponents<PartModule>())
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
                        //the symmetry ones are done first because some actions affect
                        //the gui name of the event (such as toggling engines)
                        tree.selectedItem.getActivableEvents()[tree.selectedItem.selectedLine].Invoke();

                        break;
                }
            }
            customSettings.focusSubset.Clear();
            if (zoomOnSelection)
            {
                if (tree.selectedItem != null)
                {
                    if (symmetryMode)
                    {
                        foreach (Part symPart in tree.selectedItem.associatedPart.symmetryCounterparts)
                        {
                            customSettings.focusSubset.Add(symPart);
                        }
                    }
                    customSettings.focusSubset.Add(tree.selectedItem.associatedPart);
                }
            }      
            return null;
        }

        public IVViewMenu back()
        {
            if (sanityCheck()) {
                if (tree.rootSelected())
                {
                    return rootMenu;
                }
                else
                {
                    tree.selectedItem = tree.selectedItem.root;
                    customSettings.focusSubset.Clear();
                    if (zoomOnSelection)
                    {
                        if (tree.selectedItem != null)
                        {
                            if (symmetryMode)
                            {
                                foreach (Part symPart in tree.selectedItem.associatedPart.symmetryCounterparts)
                                {
                                    customSettings.focusSubset.Add(symPart);
                                }
                            }
                            customSettings.focusSubset.Add(tree.selectedItem.associatedPart);
                        }
                    }      
                    return null;
                }
            }
            
            return null;
        }

        public void setRoot(IVViewMenu root)
        {
            this.rootMenu = root;
        }

        public IVViewMenu getRoot()
        {
            return rootMenu;
        }

        public IVViewMenu update(Vessel ship)
        {
            sanityCheck();
            tree.updateTree(ship);
            return null;
        }



        public void activate()
        {
            customSettings.focusSubset.Clear();
            if (zoomOnSelection)
            {
                if (tree.selectedItem != null)
                {
                    if (symmetryMode)
                    {
                        foreach (Part symPart in tree.selectedItem.associatedPart.symmetryCounterparts)
                        {
                            customSettings.focusSubset.Add(symPart);
                        }
                    }
                    customSettings.focusSubset.Add(tree.selectedItem.associatedPart);
                }
            }      
        }

        public void deactivate()
        {
            customSettings.focusSubset.Clear();
        }


        public string getName()
        {
            return name;
        }


        public CustomModeSettings getCustomSettings()
        {
            return customSettings;
        }


        public void setCustomSettings(CustomModeSettings settings)
        {
            customSettings = settings;
        }
    }
}
