using System;
using UnityEngine;

namespace VesselView
{
    public class ViewerConstants
    {
        public static string VERSION = "0.2.24"; 
        
        //predefined values for the settings
        public static readonly float[] SCALE_FACTS = { 5, 10, 15, 20, 30, 40, 75, 100, 150 };
        public static readonly int[] OFFSET_MODS = { 128, 128, 128, 128, 128, 128, 128, 128 };
        public static readonly int[] SCREEN_SIZES = { 64, 128, 256, 512, 768, 1024 };
        public static readonly string[] PLANES = { "XY", "XZ", "YZ"/*, "GrndRelYZ"*/, "Real" };
        public enum PLANE
        {
            XY, XZ, YZ, /*GRNDYZ, */REAL
        }

        public static readonly string[] COLORMODES = { "White", "State", "Stage", "Heat", "Resources", "Hide" };
        public enum COLORMODE
        {
            NONE, STATE, STAGE, HEAT, FUEL, HIDE
        }
        //splatrix
        public static readonly Matrix4x4 FLATTER = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0));
        

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
                               "   ZWrite Off Cull Off Fog { Mode Off }" +
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
    }

        
}
