using System;
using System.Collections.Generic;
using UnityEngine;
using VesselViewRPM.menus;
using VesselView;

namespace VVPartSelector
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VVPartSelector : MonoBehaviour
    {



        /// <summary>
        /// Use for constructor-like stuff.
        /// </summary>
        void Start()
        {
            VViewCustomMenusMenu.registerMenu(createMenu);
            //VesselViewPlugin.registerCustomMode(setup());
        }

        private CustomModeSettings setup() 
        {
            CustomModeSettings settings = new CustomModeSettings();

            SelectorDataContainer dataObject = new SelectorDataContainer();
            settings.dataInstance = dataObject;

            settings.name = "Part Selector";
            settings.ColorModeOverride = (int)CustomModeSettings.OVERRIDE_TYPES.FUNCTION;
            settings.OrientationOverride = (int)CustomModeSettings.OVERRIDE_TYPES.AS_BASIC;
            settings.CenteringOverride = (int)CustomModeSettings.OVERRIDE_TYPES.AS_BASIC;
            settings.MinimodesOverride = (int)CustomModeSettings.OVERRIDE_TYPES.STATIC;

            settings.staticSettings.displayCOM = false;
            settings.staticSettings.displayEngines = false;
            settings.staticSettings.displayAxes = false;
            settings.staticSettings.displayGround = (int)ViewerConstants.GROUND_DISPMODE.OFF;

            settings.fillColorDelegate = returnsBlack;
            settings.wireColorDelegate = getPartColorSelectMode;
            settings.boxColorDelegate = getBoxColorSelectMode;

            settings.fillColorDullDelegate = returnsTrue;
            settings.wireColorDullDelegate = returnsFalse;
            settings.boxColorDullDelegate = returnsFalse;

            return settings;
        }

        IVViewMenu createMenu() 
        {
            CustomModeSettings settings = setup();

            SelectorDataContainer container = (SelectorDataContainer)settings.dataInstance;

            IVVSimpleMenuItem[] INTItems = {
                 new VViewSimpleMenuItem("Part selector (tree-traversal)",container.selectorTree),
                 new VViewSimpleMenuItem("Part selector (global list)",container.selectorGlobal),
                 new VViewSimpleCustomMenuItem("Zoom on selection:",container.getZoom,container.setZoom),
                 new VViewSimpleCustomMenuItem("Affect symmetry:",container.getSymm,container.setSymm)
                                       };
            VViewSimpleMenu rootMenu = new VViewSimpleMenu(INTItems, "Part selector");
            container.selectorTree.setRoot((IVViewMenu)rootMenu);
            container.selectorGlobal.setRoot((IVViewMenu)rootMenu);
            container.CustomSettings = settings;
            rootMenu.setCustomSettings(settings);

            return rootMenu;
        }

        public bool returnsTrue(CustomModeSettings customMode) 
        {
            return true;
        }

        public bool returnsFalse(CustomModeSettings customMode) 
        {
            return false;
        }

        public Color returnsBlack(CustomModeSettings customMode,Part part) 
        {
            return Color.black;
        }

        private bool partIsOnWayToRoot(Part part, Part leaf, Part root)
        {
            if (part == null | leaf == null | root == null) return false;
            if (leaf == root) return false;
            if (leaf == part) return true;
            return partIsOnWayToRoot(part, leaf.parent, root);
        }

        public Color getBoxColorSelectMode(CustomModeSettings customMode,Part part)
        {
            if (((SelectorDataContainer)(customMode.dataInstance)).selectorTree.Active)
            {
                Part subselect = ((SelectorDataContainer)(customMode.dataInstance)).selectorTree.getSubselection();
                if (part == subselect) return Color.cyan;
                else return Color.black;
            }
            else if (((SelectorDataContainer)(customMode.dataInstance)).selectorGlobal.Active)
            {
                List<Part> parts = ((SelectorDataContainer)(customMode.dataInstance)).selectorGlobal.getPartsMatchingSelection();
                //if (parts == null) {MonoBehaviour.print("part list null"); return Color.red;}
                //if (part == null) { MonoBehaviour.print("part itself null"); return Color.red; }
                if (parts.Contains(part)) return Color.cyan;
                else return Color.black;
            }
            else if (((SelectorDataContainer)(customMode.dataInstance)).selectorSubmenu != null)
            {
                List<Part> parts = ((SelectorDataContainer)(customMode.dataInstance)).selectorSubmenu.getSelectedParts();
                if (parts.Contains(part)) return Color.cyan;
                else return Color.black;
            }
            else return Color.black;
        }

        public Color getPartColorSelectMode(CustomModeSettings customMode,Part part)
        {
            if (((SelectorDataContainer)(customMode.dataInstance)).selectorTree.Active)
            {
                Color darkGreen = Color.green;
                darkGreen.g = 0.6f;
                darkGreen.r = 0.3f;
                darkGreen.b = 0.3f;
                Part selectedPart = ((SelectorDataContainer)(customMode.dataInstance)).selectorTree.getSelection();
                if (selectedPart == null) return Color.grey;
                if (part == selectedPart) return Color.green;
                if (((SelectorDataContainer)(customMode.dataInstance)).getSymm())
                {
                    if (selectedPart.symmetryCounterparts.Contains(part)) return darkGreen;
                }
                if (partIsOnWayToRoot(part, selectedPart, FlightGlobals.ActiveVessel.rootPart)) return Color.yellow;
                if (part == FlightGlobals.ActiveVessel.rootPart) return Color.magenta;
                return Color.gray;
            }
            else if (((SelectorDataContainer)(customMode.dataInstance)).selectorGlobal.Active)
            {
                List<Part> parts = ((SelectorDataContainer)(customMode.dataInstance)).selectorGlobal.getPartsMatchingSelection();
                if (parts.Contains(part)) return Color.green;
                else return Color.gray;
            }
            else if (((SelectorDataContainer)(customMode.dataInstance)).selectorSubmenu != null)
            {
                List<Part> parts = ((SelectorDataContainer)(customMode.dataInstance)).selectorSubmenu.getSelectedParts();
                if (parts.Contains(part)) return Color.green;
                else return Color.gray;
            }
            else return Color.gray;
            
        }

        

    }
}
