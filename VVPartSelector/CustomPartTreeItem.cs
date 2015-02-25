using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVPartSelector
{
    class CustomPartTreeItem
    {

        public CustomPartTreeItem root;
        public CustomPartTreeItem[] children;
        public int actionCount = 0;

        public Part associatedPart;

        public bool childrnExpanded=true;
        public bool actionsExpanded=false;

        public bool hasChildrn = false;
        public bool hasActions = false;

        public int selectedLine = 0;
        public int selectionMode = (int)VViewMenuPartSelectorTree.SELECTIONMODES.PARTS;

        public CustomPartTreeItem(Part part, CustomPartTreeItem root) {
            this.associatedPart = part;
            this.root = root;
        }

        /// <summary>
        /// Im betting on the part tree not changing all that often.
        /// Otherwise a list might be better.
        /// </summary>
        /// <param name="index"></param>
        internal void removeBranch(int index)
        {
            CustomPartTreeItem[] newChildren = new CustomPartTreeItem[children.Count() - 1];
            for (int i = 0; i < index; i++) {
                newChildren[i] = children[i];
            } 
            for (int i = index+1; i < children.Count(); i++)
            {
                newChildren[i-1] = children[i];
            }
            children = newChildren;
        }

        internal void addBranch(CustomPartTreeItem customPartTreeItem)
        {
            CustomPartTreeItem[] newChildren = new CustomPartTreeItem[children.Count() + 1];
            for (int i = 0; i < children.Count(); i++)
            {
                newChildren[i] = children[i];
            }
            newChildren[children.Count()] = customPartTreeItem;
            children = newChildren;
        }


        public BaseEvent[] getActivableEvents()
        {
            int counter=0;
            if (associatedPart != null)
            {
                foreach (PartModule pm in associatedPart.GetComponents<PartModule>())
                {
                    foreach (BaseEvent mEvent in pm.Events)
                    {
                        if (mEvent.guiActive & mEvent.active) counter++;
                    }
                }
                BaseEvent[] events = new BaseEvent[counter];
                counter = 0;
                foreach (PartModule pm in associatedPart.GetComponents<PartModule>())
                {
                    foreach (BaseEvent mEvent in pm.Events)
                    {
                        if (mEvent.guiActive & mEvent.active)
                        {
                            events[counter] = mEvent;
                            counter++;
                        }
                    }
                }
                return events;
            }
            else return new BaseEvent[0];
        }

    }
}
