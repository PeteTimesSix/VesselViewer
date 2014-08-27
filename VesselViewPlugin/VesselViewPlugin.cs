using System;
using System.Collections.Generic;
using UnityEngine;
using Toolbar;
using VesselView;

namespace VesselView
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselViewPlugin : MonoBehaviour
    {

        public int scrnSizeXpos = 3;
        public int scrnSizeYpos = 3;

        //GUI stuff
        //Toolbar button
        private IButton button;
        protected Rect windowPos;
        private IButton buttonC;
        protected Rect windowCPos;

        //our screen
        RenderTexture screen;
        private VesselViewer viewer;
        private ViewerSettings settings;

        private int customMode = -1;
        private static List<CustomModeSettings> customModes = new List<CustomModeSettings>();

        public static void registerCustomMode(CustomModeSettings settings) 
        {
            customModes.Add(settings);
        }

        /// <summary>
        /// Yoinked from a tutorial.
        /// </summary>
        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Vessel View", GUILayout.MinWidth(screen.width), GUILayout.MaxWidth(screen.width), GUILayout.MinHeight(screen.height), GUILayout.MaxHeight(screen.height));
        }

        /// <summary>
        /// Yoinked from a tutorial.
        /// </summary>
        private void drawGUIC()
        {
            GUI.skin = HighLogic.Skin;
            windowCPos = GUILayout.Window(2, windowCPos, WindowGUI, "Vessel View Config", GUILayout.MinWidth(64));
        }

        /// <summary>
        /// Setup the window for the addon.
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUI(int windowID)
        {
            //setup the style
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            GUIStyle myStyL = new GUIStyle(GUI.skin.label);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            if (windowID == 1)
            {
                //draw the texture
                GUILayout.Box(screen);
            }
            if (windowID == 2)
            {
                //and now, buttons. lots of buttons.
                GUILayout.BeginHorizontal();
                GUILayout.Label("Display configuration", myStyL, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Orientation:" + ViewerConstants.PLANES[settings.drawPlane], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.drawPlane++;
                    if (settings.drawPlane == ViewerConstants.PLANES.Length) settings.drawPlane = 0;
                }
                if (GUILayout.Button("Autocentering:" + settings.autoCenter, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.autoCenter = !settings.autoCenter;
                }
                if (GUILayout.Button("Hor. root center:" + settings.centerOnRootH, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.centerOnRootH = !settings.centerOnRootH;
                }
                if (GUILayout.Button("Ver. root center:" + settings.centerOnRootV, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.centerOnRootV = !settings.centerOnRootV;
                }
                if (GUILayout.Button("Autoscaling:" + ViewerConstants.RESCALEMODES[settings.centerRescale], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.centerRescale++;
                    if (settings.centerRescale == ViewerConstants.centerRescaleMAX) settings.centerRescale = 0;
                }

                /*if (GUILayout.Button("Scale:" + settings.scaleFact, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.scalePos++;
                    if (settings.scalePos == ViewerConstants.SCALE_FACTS.Length) settings.scalePos = 0;
                    //scrOffX = OFFX_DEF + (int)((scrOffX - OFFX_DEF) / (settings.scaleFact / SCALE_FACTS[settings.scalePos]));
                    //scrOffY = OFFY_DEF + (int)((scrOffY - OFFY_DEF) / (settings.scaleFact / SCALE_FACTS[settings.scalePos]));
                    settings.scaleFact = ViewerConstants.SCALE_FACTS[settings.scalePos];
                }*/

                GUILayout.EndHorizontal();

                if (!settings.autoCenter)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Manual positioning", myStyL, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Up", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        viewer.manuallyOffset(0, ViewerConstants.OFFSET_MODS[settings.scalePos]);
                    }
                    if (GUILayout.Button("Down", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        viewer.manuallyOffset(0, -ViewerConstants.OFFSET_MODS[settings.scalePos]);
                    }
                    if (GUILayout.Button("Left", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        viewer.manuallyOffset(-ViewerConstants.OFFSET_MODS[settings.scalePos], 0);
                    }
                    if (GUILayout.Button("Right", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        viewer.manuallyOffset(ViewerConstants.OFFSET_MODS[settings.scalePos], 0);
                    }
                    if (GUILayout.Button("Center", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                        viewer.nilOffset(screen.width, screen.height);
                    } 
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Autorotation axis:" + ViewerConstants.AXES[settings.spinAxis], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.spinAxis++;
                    if (settings.spinAxis == ViewerConstants.spinAxisMAX) settings.spinAxis = 0;
                }
                if (GUILayout.Button("Autorotation speed:" + ViewerConstants.SPIN_SPEEDS[settings.spinSpeed], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.spinSpeed++;
                    if (settings.spinSpeed == ViewerConstants.spinSpeedMAX) settings.spinSpeed = 0;
                }
                String customModeName = "Inactive";
                if (customModes.Count > 0 & customMode >= 0) customModeName = (customModes.ToArray())[customMode].name;
                if (GUILayout.Button("Custom mode:" + customModeName, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    customMode++;
                    if (customMode >= customModes.Count | customModes.Count == 0) customMode = -1;
                    if (customMode != -1) viewer.customMode = (customModes.ToArray())[customMode];
                    else viewer.customMode = null;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Draw modes", myStyL, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Mesh:" + ViewerConstants.COLORMODES[settings.colorModeFill], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeFill++;
                    if (settings.colorModeFill == ViewerConstants.COLORMODES.Length) settings.colorModeFill = 0;
                }
                if (GUILayout.Button("Dull:" + settings.colorModeFillDull, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeFillDull = !settings.colorModeFillDull;
                }
                if (GUILayout.Button("Wire:" + ViewerConstants.COLORMODES[settings.colorModeWire], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeWire++;
                    if (settings.colorModeWire == ViewerConstants.COLORMODES.Length) settings.colorModeWire = 0;
                }
                if (GUILayout.Button("Dull:" + settings.colorModeWireDull, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeWireDull = !settings.colorModeWireDull;
                }
                if (GUILayout.Button("Bounds:" + ViewerConstants.COLORMODES[settings.colorModeBox], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeBox++;
                    if (settings.colorModeBox == ViewerConstants.COLORMODES.Length) settings.colorModeBox = 0;
                }
                if (GUILayout.Button("Dull:" + settings.colorModeBoxDull, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.colorModeBoxDull = !settings.colorModeBoxDull;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Axes:" + settings.displayAxes, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.displayAxes = !settings.displayAxes;
                }
                if (GUILayout.Button("Center of mass:" + settings.displayCOM, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.displayCOM = !settings.displayCOM;
                }
                if (GUILayout.Button("Engine status:" + settings.displayEngines, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.displayEngines = !settings.displayEngines;
                }
                if (GUILayout.Button("Landing assist:" + ViewerConstants.GROUND_DISPMODES[settings.displayGround], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.displayGround++;
                    if (settings.displayGround == ViewerConstants.displayGroundMAX) settings.displayGround = 0;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Other configuration", myStyL, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("X size:" + screen.width, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    scrnSizeXpos++;
                    if (scrnSizeXpos == ViewerConstants.SCREEN_SIZES.Length) scrnSizeXpos = 0;
                    setupTexture();
                }
                if (GUILayout.Button("Y size:" + screen.height, mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    scrnSizeYpos++;
                    if (scrnSizeYpos == ViewerConstants.SCREEN_SIZES.Length) scrnSizeYpos = 0;
                    setupTexture();
                }
                if (GUILayout.Button("Margins:" + ViewerConstants.MARGINS[settings.margin], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.margin++;
                    if (settings.margin == ViewerConstants.marginMAX) settings.margin = 0;
                }
                if (GUILayout.Button("Latency Mode:" + ViewerConstants.LATENCIES[settings.latency], mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                    settings.latency++;
                    if (settings.latency == ViewerConstants.latencyMAX) settings.latency = 0;
                }
                GUILayout.EndHorizontal();

            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void setupTexture()
        {
            if (screen != null)
            {
                screen.DiscardContents();
                screen.Release();
            }
            screen = new RenderTexture(ViewerConstants.SCREEN_SIZES[scrnSizeXpos], ViewerConstants.SCREEN_SIZES[scrnSizeYpos], 24, RenderTextureFormat.ARGB32);
        }

        /// <summary>
        /// Constructor. Apparently not meant to be used.
        /// </summary>
        public VesselViewPlugin()
        {

        }

        /// <summary>
        /// Called after the scene is loaded.
        /// </summary>
        void Awake()
        {
            //setup the toolbar buttons
            button = ToolbarManager.Instance.add("VV", "VVbutton");
            button.TexturePath = "VesselView/Textures/icon";
            button.ToolTip = "Vessel View";
            button.OnClick += (e) =>
            {
                //Debug.Log("VW button clicked, mouseButton: " + e.MouseButton);
                if (settings.screenVisible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                }
                else
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                }
                settings.screenVisible = !settings.screenVisible;
            };
            buttonC = ToolbarManager.Instance.add("VVC", "VVbuttonC");
            buttonC.TexturePath = "VesselView/Textures/iconC";
            buttonC.ToolTip = "Vessel View Config";
            buttonC.OnClick += (e) =>
            {
                //Debug.Log("VW button clicked, mouseButton: " + e.MouseButton);
                if (settings.configScreenVisible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUIC)); //close the GUI
                }
                else
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUIC));//start the GUI
                }
                settings.configScreenVisible = !settings.configScreenVisible;
            };
        }

        /// <summary>
        /// Use for constructor-like stuff.
        /// </summary>
        void Start()
        {
            setupTexture();
            viewer = new VesselViewer();
            settings = viewer.basicSettings;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        void Update()
        {
            if (settings.screenVisible) 
            {
                viewer.drawCall(screen, false);
            }        
        }



        /// <summary>
        /// Called at a fixed time interval determined by the physics time step.
        /// </summary>
        void FixedUpdate()
        {

        }

        /// <summary>
        /// Called when the game is leaving the scene (or exiting). Perform any clean up work here.
        /// </summary>
        void OnDestroy()
        {
            //remove button from toolbar
            button.Destroy();
            buttonC.Destroy();
        }


    }
}