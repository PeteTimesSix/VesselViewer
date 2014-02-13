using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VesselViewRPM
{
    class VViewMenuPartSelector : IVViewMenu
    {
        private CustomPartTree tree;
        private IVViewMenu root;

        private enum SELECTIONMODES {
            NONE, EXPAND_PARTS, PARTS, EXPAND_ACTIONS, ACTIONS
        }

        private int selectedLine = 0;
        private int selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
        
        public VViewMenuPartSelector(Vessel vessel) {
            tree = new CustomPartTree(vessel);
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
                        if (selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
                        {
                            builder.AppendLine(" <");
                        }
                        else {
                            builder.AppendLine("");
                        }
                        int counter = 0;
                        foreach (CustomPartTreeItem partItem in tree.selectedItem.children) {
                            builder.Append(partItem.associatedPart.name);                           
                            if (selectionMode == (int)SELECTIONMODES.PARTS & counter == selectedLine)
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
                        if (selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
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
                        if (selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
                        {
                            builder.AppendLine(" <");
                        }
                        else {
                            builder.AppendLine("");
                        }
                        int counter = 0;
                        foreach (PartModule pm in tree.selectedItem.associatedPart.GetComponents<PartModule>())
                        {
                            foreach (BaseAction action in pm.Actions) {
                                builder.Append(action.name);
                                if (selectionMode == (int)SELECTIONMODES.ACTIONS & counter == selectedLine)
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
                    }
                    else
                    {
                        builder.Append("[+]ACTIONS");
                        if (selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
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

        private void sanityCheck() {
            if (tree.selectedItem == null)
            {
                selectionMode = (int)SELECTIONMODES.NONE;
            }
            else {
                if (!tree.selectedItem.hasActions & !tree.selectedItem.hasChildrn)
                {
                    selectionMode = (int)SELECTIONMODES.NONE;
                    selectedLine = 0;
                }
                else if (selectionMode == (int)SELECTIONMODES.NONE)
                {
                    if (tree.selectedItem.hasChildrn)
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                    else
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
            }
            
        }

        /// <summary>
        /// Well isnt this a delightful mess of hardcoding?
        /// </summary>
        public void up()
        {
            sanityCheck();
            if (selectionMode == (int)SELECTIONMODES.EXPAND_PARTS) {
                if (tree.selectedItem.hasActions) {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        selectionMode = (int)SELECTIONMODES.ACTIONS;
                        selectedLine = tree.selectedItem.actionCount - 1;
                    }
                    else
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
                else if (tree.selectedItem.childrnExpanded)
                {
                    selectionMode = (int)SELECTIONMODES.PARTS;
                    selectedLine = tree.selectedItem.children.Count() - 1;
                }
                else {
                    selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                }
            }
            else if (selectionMode == (int)SELECTIONMODES.PARTS) {
                selectedLine--;
                if (selectedLine < 0) {
                    selectedLine = 0;
                    selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                }
            }
            else if (selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS) {
                if (tree.selectedItem.hasChildrn)
                {
                    if (tree.selectedItem.childrnExpanded)
                    {
                        selectionMode = (int)SELECTIONMODES.PARTS;
                        selectedLine = tree.selectedItem.children.Count() - 1;
                    }
                    else
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                }
                else {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        selectionMode = (int)SELECTIONMODES.ACTIONS;
                        selectedLine = tree.selectedItem.actionCount - 1;
                    }
                    else
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                }
            }
            else if (selectionMode == (int)SELECTIONMODES.ACTIONS)
            {
                selectedLine--;
                if (selectedLine < 0)
                {
                    selectedLine = 0;
                    selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                }
            }
        }

        public void down()
        {
            sanityCheck();
            if (selectionMode == (int)SELECTIONMODES.EXPAND_PARTS)
                {
                    if (tree.selectedItem.childrnExpanded)
                    {
                        selectionMode = (int)SELECTIONMODES.PARTS;
                        selectedLine = 0;
                    }
                    else if (tree.selectedItem.hasActions)
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                    }
                    else
                    {
                        selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                    }
                }
                else if (selectionMode == (int)SELECTIONMODES.PARTS)
                {
                    selectedLine++;
                    if (selectedLine >= tree.selectedItem.children.Count())
                    {
                        selectedLine = 0;
                        if (tree.selectedItem.hasActions)
                        {
                            selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                        else {
                            selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                    }
                }
                else if (selectionMode == (int)SELECTIONMODES.EXPAND_ACTIONS)
                {
                    if (tree.selectedItem.actionsExpanded)
                    {
                        selectionMode = (int)SELECTIONMODES.ACTIONS;
                        selectedLine = 0;
                    }
                    else {
                        if (tree.selectedItem.hasChildrn)
                        {
                            selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                        else {
                            selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                    }
                }
                else if (selectionMode == (int)SELECTIONMODES.ACTIONS)
                {
                    selectedLine++;
                    if (selectedLine >= tree.selectedItem.actionCount)
                    {
                        selectedLine = 0;
                        if (tree.selectedItem.hasChildrn)
                        {
                            selectionMode = (int)SELECTIONMODES.EXPAND_PARTS;
                        }
                        else
                        {
                            selectionMode = (int)SELECTIONMODES.EXPAND_ACTIONS;
                        }
                    }
                }
        }

        public IVViewMenu click()
        {
            sanityCheck();
            switch (selectionMode) {
                case (int)SELECTIONMODES.NONE:
                    break;
                case (int)SELECTIONMODES.EXPAND_PARTS:
                    tree.selectedItem.childrnExpanded = !tree.selectedItem.childrnExpanded;
                    break;
                case (int)SELECTIONMODES.EXPAND_ACTIONS:
                    tree.selectedItem.actionsExpanded = !tree.selectedItem.actionsExpanded;
                    break;
                case (int)SELECTIONMODES.PARTS:
                    tree.selectedItem = tree.selectedItem.children[selectedLine];
                    selectionMode = (int)SELECTIONMODES.NONE;
                    selectedLine = 0;
                    break;
                case (int)SELECTIONMODES.ACTIONS:
                    int counter = 0;
                    foreach (PartModule pm in tree.selectedItem.associatedPart.GetComponents<PartModule>()) {
                        foreach (BaseAction action in pm.Actions) {
                            if (counter == selectedLine) {
                                //action.Invoke(null);
                            }
                            else{
                                counter++;
                            }
                        }
                    }
                    break;
            }
            
            return null;
        }

        public void setRoot(IVViewMenu root)
        {
            this.root = root;
        }

        public IVViewMenu getRoot()
        {
            return root;
        }

        public void update(Vessel ship)
        {
            sanityCheck();
            tree.updateTree(ship);
        }
    }
}
