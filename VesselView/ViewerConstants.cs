using System;
using UnityEngine;

namespace VesselView
{
    public class ViewerConstants
    {
        public static string VERSION = "0.5"; 
        
        //predefined values for the settings
        public static readonly float[] SCALE_FACTS = { 5, 10, 15, 20, 30, 40, 75, 100, 150 };
        public static readonly int[] OFFSET_MODS = { 128, 128, 128, 128, 128, 128, 128, 128 };
        public static readonly int[] SCREEN_SIZES = { 64, 128, 256, 512, 768, 1024 };

        public static readonly string[] RESCALEMODES = { "Off", "Incremental", "Close", "Best match"};
        public enum RESCALEMODE
        {
            OFF, INCR, CLOSE, BEST
        }

        public static readonly string[] PLANES = { "XY", "XZ", "YZ","Isometric", "Relative", "Real" };
        public enum PLANE
        {
            XY, XZ, YZ, ISO, GRND, REAL
        }

        public static readonly string[] AXES = { "X", "Y", "Z"};
        public enum AXIS
        {
            X, Y, Z
        }
        public static readonly string[] SPIN_SPEEDS = {"None", "Slow", "Medium", "Fast" };
        public static readonly float[] SPIN_SPEED_VAL = {0, 4, 12, 32};

        public static readonly string[] GROUND_DISPMODES = { "Off", "Rocket", "Plane" };
        public enum GROUND_DISPMODE 
        {
            OFF, ROCKET, PLANE 
        }

        public static readonly string[] COLORMODES = { "White", "State", "Stage", "Heat", "Resources","Drag", "Lift", "Stall", "Hide" };
        public enum COLORMODE
        {
            NONE, STATE, STAGE, HEAT, FUEL, DRAG, LIFT, STALL, HIDE
        }

        public enum ICONS
        {
            SQUARE, DIAMOND, SQUARE_DIAMOND,
            ENGINE_NOFUEL,
            ENGINE_NOPOWER,
            ENGINE_NOAIR,
            ENGINE_ACTIVE,
            ENGINE_READY,
            ENGINE_INACTIVE
        }
        

        /// <summary>
        /// My guess is this setups a shader handy for drawing lines?
        /// Yoinked from RPM (JSIOrbitDisplay specificaly).
        /// </summary>
        /// <returns>line material</returns>
        public static Material DrawLineMaterial()
        {
            var lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                               "SubShader { Pass {" +
                               "   BindChannels { Bind \"Color\",color }" +
                               "   Blend SrcAlpha OneMinusSrcAlpha" +
                               "   Cull Off Fog { Mode Off }" +
                               "} } }");
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            return lineMaterial;
        }

        /// <summary>
        /// Since Id like the rectangles I save to come with a color.
        /// </summary>
        public struct RectColor
        {
            public Rect rect;
            public Color color;

            public RectColor(Rect rect, Color color)
            {
                this.rect = rect;
                this.color = color;
            }
        }

        public static float MAX_ALTITUDE = 250;
    }

        
}
