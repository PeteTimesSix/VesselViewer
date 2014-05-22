using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VesselView
{
    public class VesselViewer
    {

        //centerised... center
        //bounding box for the whole vessel, essentially
        private Vector3 minVecG = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 maxVecG = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        //used for temp transform because matrices are scary
        private GameObject transformTemp = new GameObject();

        //time of last update
        private float lastUpdate = 0.0f;

        //queue of parts yet to be drawn this draw
        private Queue<Part> partQueue = new Queue<Part>();
        private Queue<ViewerConstants.RectColor> rectQueue = new Queue<ViewerConstants.RectColor>();


        //gradient of colors for stage display
        private Color[] stageGradient;
        //line material
        private readonly Material lineMaterial = ViewerConstants.DrawLineMaterial();

        public ViewerSettings settings=new ViewerSettings();

        //stage counters
        private int stagesLastTime = 0;
        private int stagesThisTimeMax = 0;

        private int lastFrameDrawn = 0;


        private static Mesh bakedMesh = new Mesh();

        public void nilOffset(int width, int height) {
            settings.scrOffX = width / 2;
            settings.scrOffY = height / 2;
        }

        public void manuallyOffset(int offsetX, int offsetY) {
            settings.scrOffX += offsetX;
            settings.scrOffY += offsetY;
        }

        public void forceRedraw() {
            lastUpdate = Time.time - 1f;
        }

        public void drawCall(RenderTexture screen, bool internalScreen) {
            //MonoBehaviour.print("VV draw call");
            //Latency mode to limit to one frame per second if FPS is affected
            //also because it happens to look exactly like those NASA screens :3
            int frameDiff = Time.frameCount - lastFrameDrawn;
            bool redraw = false;
            if (settings.latency == (int)ViewerConstants.LATENCY.OFF) redraw = true;
            else if (settings.latency == (int)ViewerConstants.LATENCY.LOW) 
            {
                if (frameDiff >= 3) redraw = true;
            }
            else if (settings.latency == (int)ViewerConstants.LATENCY.MEDIUM)
            {
                if (frameDiff >= 10) redraw = true;
            }
            else if (settings.latency == (int)ViewerConstants.LATENCY.HIGH)
            {
                if (frameDiff >= 30) redraw = true;
            }
            else if (settings.latency == (int)ViewerConstants.LATENCY.TOOHIGH)
            {
                if (frameDiff >= 75) redraw = true;
            }
            if (redraw) 
            {
                lastFrameDrawn = Time.frameCount;
                //MonoBehaviour.print("VV restarting draw, screen internal:"+internalScreen);
                restartDraw(screen);
                if (settings.autoCenter)
                {
                    centerise(screen.width, screen.height);
                }
            }
            //MonoBehaviour.print("VV draw call done");
        }

        public VesselViewer() {
        
        }

        /// <summary>
        /// Start a new draw cycle.
        /// </summary>
        private void restartDraw(RenderTexture screen)
        {
            //reset the vessel bounding box
            minVecG = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxVecG = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            lastUpdate = Time.time;
            partQueue.Clear();
            settings.ship = FlightGlobals.ActiveVessel;
            if (!settings.ship.isEVA)
            {
                partQueue.Enqueue(settings.ship.rootPart);
            }
            try
            {
                renderToTexture(screen);
            }
            catch (Exception e) 
            {
                MonoBehaviour.print("Exception " + e + " during drawing");
            }
            GL.wireframe = false;
            partQueue.Clear();
        }

        /// <summary>
        /// render the vessel diagram to a texture.
        /// </summary>
        /// <param name="renderTexture">Texture to render to.</param>
        void renderToTexture(RenderTexture renderTexture)
        {
            //render not when invisible, grasshopper.
            if (settings.screenVisible)
            {

                //switch rendering to the texture
                RenderTexture backupRenderTexture = RenderTexture.active;
                if (!renderTexture.IsCreated())
                    renderTexture.Create();
                renderTexture.DiscardContents();
                RenderTexture.active = renderTexture;

                //setup viewport and such
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, renderTexture.width, 0, renderTexture.height);
                GL.Viewport(new Rect(0, 0, renderTexture.width, renderTexture.height));

                //clear the texture
                GL.Clear(true, true, Color.black);
                
                //set up the screen position and scaling matrix
                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(settings.scrOffX, settings.scrOffY, 0), Quaternion.identity, new Vector3(settings.scaleFact, settings.scaleFact, 1));
                //dunno what this does, but I trust in the stolen codes
                lineMaterial.SetPass(0);

                if (!settings.partSelectMode) 
                {
                    while (partQueue.Count > 0)
                    {
                        Part next = partQueue.Dequeue();
                        if (next != null)
                        {
                            renderPart(next, matrix, true);
                        }
                    }
                }
                GL.Clear(true, false, Color.black);
                if (partQueue.Count == 0)
                {
                    if (!settings.ship.isEVA)
                    {
                        partQueue.Enqueue(settings.ship.rootPart);
                    }
                }
                //lineMaterial.SetPass(1);
                //turn on wireframe, since triangles would get filled othershipwise
                GL.wireframe = true;
                //now render each part (assumes root part is in the queue)
                while (partQueue.Count > 0)
                {
                    Part next = partQueue.Dequeue();
                    if (next != null)
                    {
                        renderPart(next, matrix, false);
                    }
                }
                //now render engine exhaust indicators
                if (settings.displayEngines)
                {
                    renderEngineThrusts(matrix);
                }
                //now render the bounding boxes (so theyre on top)
                if (settings.colorModeBox != (int)ViewerConstants.COLORMODE.HIDE)
                {
                    renderRects(matrix);
                }
                //now render center of mass
                if (settings.displayCOM) 
                {
                    renderCOM(matrix);
                }
                if (settings.displayGround != (int)ViewerConstants.GROUND_DISPMODE.OFF)
                {
                    //first, render the ground
                    renderGround(matrix);
                }
                if (settings.displayAxes)
                {
                    //first, render the ground
                    renderAxes(matrix);
                }
                /*if (settings.displayCOP)
                {
                    renderCOP(matrix);
                }*/
                //then set the max stages (for the stage coloring)
                stagesLastTime = stagesThisTimeMax;
                //undo stuff
                GL.wireframe = false;
                GL.PopMatrix();
                RenderTexture.active = backupRenderTexture;
            }
        }

        private void renderEngineThrusts(Matrix4x4 screenMatrix)
        {
            foreach (Part part in settings.ship.parts) 
            {
                string transformName = null;
                List<Propellant> propellants = null;
                float maxThrust = 0;
                float finalThrust = 0;
                bool operational = false;
                if (part.Modules.Contains("ModuleEngines"))
                {
                    ModuleEngines engineModule = (ModuleEngines)part.Modules["ModuleEngines"];
                    transformName = engineModule.thrustVectorTransformName;
                    propellants = engineModule.propellants;
                    maxThrust = engineModule.maxThrust;
                    finalThrust = engineModule.finalThrust;
                    operational = engineModule.isOperational;
                }
                else if (part.Modules.Contains("ModuleEnginesFX"))
                {
                    ModuleEnginesFX engineModule = (ModuleEnginesFX)part.Modules["ModuleEnginesFX"];
                    transformName = engineModule.thrustVectorTransformName;
                    propellants = engineModule.propellants;
                    maxThrust = engineModule.maxThrust;
                    finalThrust = engineModule.finalThrust;
                    operational = engineModule.isOperational;
                }
                
                if (transformName!=null) 
                {
                    //MonoBehaviour.print("Found an engine with a transform");
                    
                    float scale = 0;
                    scale = finalThrust / maxThrust;
                    bool Found_LiquidFuel = false;
                    bool Found_ElectricCharge = false;
                    bool Found_IntakeAir = false;
                    bool Found_XenonGas = false;
                    bool Found_Oxidizer = false;
                    bool Found_MonoPropellant = false;
                    bool Found_SolidFuel = false;
                    bool Deprived_LiquidFuel = false;
                    bool Deprived_ElectricCharge = false;
                    bool Deprived_IntakeAir = false;
                    bool Deprived_XenonGas = false;
                    bool Deprived_Oxidizer = false;
                    bool Deprived_MonoPropellant = false;
                    bool Deprived_SolidFuel = false;
                    //MonoBehaviour.print("Propellants for " + part.name);
                    foreach (Propellant propellant in propellants)
                    {
                        //MonoBehaviour.print(propellant.name);
                        if (propellant.name.Equals("LiquidFuel"))
                        {
                            Found_LiquidFuel = true;
                            if (propellant.isDeprived) Deprived_LiquidFuel = true;
                        }
                        else if (propellant.name.Equals("Oxidizer"))
                        {
                            Found_Oxidizer = true;
                            if (propellant.isDeprived) Deprived_Oxidizer = true;
                        }
                        else if (propellant.name.Equals("SolidFuel"))
                        {
                            Found_SolidFuel = true;
                            if (propellant.isDeprived) Deprived_SolidFuel = true;
                        }
                        else if (propellant.name.Equals("IntakeAir"))
                        {
                            Found_IntakeAir = true;
                            if (propellant.isDeprived) Deprived_IntakeAir = true;
                        }
                        else if (propellant.name.Equals("MonoPropellant"))
                        {
                            Found_MonoPropellant = true;
                            if (propellant.isDeprived) Deprived_MonoPropellant = true;
                        }
                        else if (propellant.name.Equals("XenonGas"))
                        {
                            Found_XenonGas = true;
                            if (propellant.isDeprived) Deprived_XenonGas = true;
                        }
                        else if (propellant.name.Equals("ElectricCharge"))
                        {
                            Found_ElectricCharge = true;
                            if (propellant.isDeprived) Deprived_ElectricCharge = true;
                        }
                    }

                    Matrix4x4 transMatrix = genTransMatrix(part.partTransform, settings.ship, true);
                    //if online, render exhaust
                    if (scale > 0.01f) 
                    {
                        if (!transformName.Equals(""))
                        {
                            Transform thrustTransform = part.FindModelTransform(transformName);
                            transMatrix = genTransMatrix(thrustTransform, settings.ship, true);
                            //default to magenta
                            Color color = Color.magenta;
                            //liquid fuel engines
                            if (Found_LiquidFuel & Found_Oxidizer) color = new Color(1, 0.5f, 0);
                            //SRBs
                            else if (Found_SolidFuel) color = new Color(1f, 0.1f, 0.1f);
                            //air breathing engines
                            else if (Found_LiquidFuel & Found_IntakeAir) color = new Color(0.9f, 0.7f, 0.8f);
                            //ion engines
                            else if (Found_XenonGas & Found_ElectricCharge) color = new Color(0f, 0.5f, 1f);
                            //monoprop engines
                            else if (Found_MonoPropellant) color = new Color(0.9f, 0.9f, 0.9f);
                            float massSqrt = (float)Math.Sqrt(part.mass);
                            scale *= massSqrt;
                            renderCone(thrustTransform, scale, massSqrt, screenMatrix, color);

                            Vector3 v = new Vector3(0, 0, scale + part.mass);
                            v = transMatrix.MultiplyPoint3x4(v);
                            if (v.x < minVecG.x) minVecG.x = v.x;
                            if (v.y < minVecG.y) minVecG.y = v.y;
                            if (v.z < minVecG.z) minVecG.z = v.z;
                            if (v.x > maxVecG.x) maxVecG.x = v.x;
                            if (v.y > maxVecG.y) maxVecG.y = v.y;
                            if (v.z > maxVecG.z) maxVecG.z = v.z;
                        }
                        
                    }
                    //render icon
                    float div = 6 / settings.scaleFact;
                    Vector3 posStr = new Vector3();
                    posStr = transMatrix.MultiplyPoint3x4(posStr);
                    //out of fuel
                    if ((Found_LiquidFuel & Deprived_LiquidFuel) | (Found_SolidFuel & Deprived_SolidFuel) | (Found_MonoPropellant & Deprived_MonoPropellant) | (Found_XenonGas & Deprived_XenonGas) | (Found_Oxidizer & Deprived_Oxidizer))
                        renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, Color.red, (int)ViewerConstants.ICONS.ENGINE_NOFUEL);
                    else if ((Found_ElectricCharge & Deprived_ElectricCharge))
                        renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, Color.cyan, (int)ViewerConstants.ICONS.ENGINE_NOPOWER);
                    else if ((Found_IntakeAir & Deprived_IntakeAir))
                        renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, Color.cyan, (int)ViewerConstants.ICONS.ENGINE_NOAIR);
                    else if (scale >= 0.01f)
                        renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, new Color(1,0.5f,0), (int)ViewerConstants.ICONS.ENGINE_ACTIVE);
                    else 
                        {
                            if (!operational)
                                renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, Color.yellow, (int)ViewerConstants.ICONS.ENGINE_INACTIVE);
                            else
                                renderIcon(new Rect(-div + posStr.x, -div + posStr.y, 2 * div, 2 * div), screenMatrix, Color.green, (int)ViewerConstants.ICONS.ENGINE_READY);
                        }

                    
                    //renderIcon(new Rect(-div + posEnd.x, -div + posEnd.y, 2 * div, 2 * div), screenMatrix, Color.yellow, (int)ViewerConstants.ICONS.SQUARE_DIAMOND);
                }
            }
        }



        /// <summary>
        /// Renders the part bounding boxes
        /// </summary>
        /// <param name="screenMatrix">Screen transformation matrix</param>
        void renderRects(Matrix4x4 screenMatrix)
        {
            //render them! render them all upon m-wait.
            while (rectQueue.Count > 0)
            {
                ViewerConstants.RectColor next = rectQueue.Dequeue();
                renderRect(next.rect, screenMatrix, next.color);
            }

        }

        private void renderGround(Matrix4x4 screenMatrix)
        {
            
            //Vector3 groundN = settings.ship.mainBody.GetRelSurfaceNVector(settings.ship.latitude, settings.ship.longitude);
            Vector3d position = settings.ship.vesselTransform.position;
            //unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
            //Vector3d eastUnit = settings.ship.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
            Vector3d upUnit = (position - settings.ship.mainBody.position).normalized;
            Vector3 groundDir = position + upUnit;
            //Quaternion lookAt = Quaternion.LookRotation(upUnit).Inverse();
            //MonoBehaviour.print("upUnit "+upUnit);
            Matrix4x4 worldToLocal = settings.ship.vesselTransform.worldToLocalMatrix;
            Vector3 localSpaceNormal = worldToLocal.MultiplyPoint3x4(groundDir);
            Vector3 perp1;
            if (localSpaceNormal.y > 0.9 | localSpaceNormal.y < -0.9)
                perp1 = Vector3.Cross(Vector3.right, localSpaceNormal);
            else
                perp1 = Vector3.Cross(Vector3.up, localSpaceNormal);
            perp1 = perp1.normalized;
            Vector3 perp2 = Vector3.Cross(localSpaceNormal+perp1, localSpaceNormal);
            perp2 = perp2.normalized;
            //MonoBehaviour.print("localSpaceNormal " + localSpaceNormal);
            //MonoBehaviour.print("perp1 " + perp1);
            //MonoBehaviour.print("perp2 " + perp2);
            //Vector3 worldSpaceNormal = settings.ship.vesselTransform.localToWorldMatrix.MultiplyPoint3x4(groundDir);
            double altitude = settings.ship.altitude-settings.ship.terrainAltitude;
            if (altitude > ViewerConstants.MAX_ALTITUDE) return;
            float biggestCrossSection = maxVecG.x - minVecG.x;
            if (maxVecG.y - minVecG.y > biggestCrossSection) biggestCrossSection = maxVecG.y - minVecG.y;
            if (maxVecG.z - minVecG.z > biggestCrossSection) biggestCrossSection = maxVecG.z - minVecG.z;
            //smallestCrossSection = smallestCrossSection / settings.scaleFact;
            //MonoBehaviour.print("biggestCrossSection " + biggestCrossSection);
            //biggestCrossSection = 20;
            Vector3 groundBelow = localSpaceNormal * -(float)altitude;
            Vector3 groundBelow1 = groundBelow + (perp1 * biggestCrossSection);
            Vector3 groundBelow2 = groundBelow - (perp1 * biggestCrossSection);
            Vector3 groundBelow3 = groundBelow + (perp2 * biggestCrossSection);
            Vector3 groundBelow4 = groundBelow - (perp2 * biggestCrossSection);
            /*Vector3 groundBelow = localSpaceNormal * 10;
            MonoBehaviour.print("localSpaceNormal " + localSpaceNormal);
            groundBelow = worldSpaceNormal * 10;
            MonoBehaviour.print("worldSpaceNormal " + localSpaceNormal);
            groundBelow = localSpaceNormal * 10;*/
            //Vector3 groundBelow = new Vector3(0, -(float)altitude, 0);
            /*Vector3 groundBelow1 = new Vector3(biggestCrossSection, -(float)altitude, biggestCrossSection);
            Vector3 groundBelow2 = new Vector3(biggestCrossSection, -(float)altitude, -biggestCrossSection);
            Vector3 groundBelow3 = new Vector3(-biggestCrossSection, -(float)altitude, -biggestCrossSection);
            Vector3 groundBelow4 = new Vector3(-biggestCrossSection, -(float)altitude, biggestCrossSection);*/
            //Vector3 direction = groundBelow + groundN;
            //MonoBehaviour.print("COM>"+COM);
            Matrix4x4 transMatrix = genTransMatrix(settings.ship.rootPart.transform, settings.ship, true);

            groundBelow = transMatrix.MultiplyPoint3x4(groundBelow);
            groundBelow1 = transMatrix.MultiplyPoint3x4(groundBelow1);
            groundBelow2 = transMatrix.MultiplyPoint3x4(groundBelow2);
            groundBelow3 = transMatrix.MultiplyPoint3x4(groundBelow3);
            groundBelow4 = transMatrix.MultiplyPoint3x4(groundBelow4);

            /*Quaternion rot = Quaternion.FromToRotation(groundN, Vector3.up);
            Quaternion rotInv = Quaternion.FromToRotation(Vector3.up, groundN);*/
            float angle = Vector3.Angle(Vector3.up, localSpaceNormal);
            if (settings.displayGround == (int)ViewerConstants.GROUND_DISPMODE.PLANE) 
            {
                angle = Vector3.Angle(Vector3.back, localSpaceNormal);
            }
            if (angle > 40) angle = 40;
            //MonoBehaviour.print("angle> " + angle);
            Color color = genFractColor(1-(angle / 40f));
            //transMatrix = screenMatrix * transMatrix;
            //now render it

            //direction = transMatrix.MultiplyPoint3x4(direction);


            //groundBelow = settings.ship.vesselTransform.rotation.Inverse() * groundBelow;
            /*groundBelow1 = settings.ship.vesselTransform.rotation.Inverse() * groundBelow1;
            groundBelow2 = settings.ship.vesselTransform.rotation.Inverse() * groundBelow2;
            groundBelow3 = settings.ship.vesselTransform.rotation.Inverse() * groundBelow3;
            groundBelow4 = settings.ship.vesselTransform.rotation.Inverse() * groundBelow4;*/

            /*groundBelow = rot * groundBelow;
            groundBelow1 = rot * groundBelow1;
            groundBelow2 = rot * groundBelow2;
            groundBelow3 = rotInv * groundBelow3;
            groundBelow4 = rot * groundBelow4;*/

            

            /*MonoBehaviour.print("after>" + groundBelow);
            MonoBehaviour.print("after>" + groundBelow1);
            MonoBehaviour.print("after>" + groundBelow2);
            MonoBehaviour.print("after>" + groundBelow3);
            MonoBehaviour.print("after>" + groundBelow4);*/

            //MonoBehaviour.print("COM modified>" + COM);
            float div = 6 / settings.scaleFact;
            renderIcon(new Rect(-div + groundBelow.x, -div + groundBelow.y, 2 * div, 2 * div), screenMatrix, Color.green, (int)ViewerConstants.ICONS.TRIANGLE_DOWN);
            //renderIcon(new Rect(-div + direction.x, -div + direction.y, 2 * div, 2 * div), screenMatrix, Color.magenta, (int)ViewerConstants.ICONS.DIAMOND);

            GL.Begin(GL.LINES);
            GL.Color(color);
            renderLine(groundBelow1.x, groundBelow1.y, groundBelow2.x, groundBelow2.y, screenMatrix);
            renderLine(groundBelow2.x, groundBelow2.y, groundBelow3.x, groundBelow3.y, screenMatrix);
            renderLine(groundBelow3.x, groundBelow3.y, groundBelow4.x, groundBelow4.y, screenMatrix);
            renderLine(groundBelow4.x, groundBelow4.y, groundBelow1.x, groundBelow1.y, screenMatrix);

            renderLine(groundBelow3.x, groundBelow3.y, groundBelow1.x, groundBelow1.y, screenMatrix);
            renderLine(groundBelow4.x, groundBelow4.y, groundBelow2.x, groundBelow2.y, screenMatrix);
            GL.End();

            if (groundBelow.x < minVecG.x) minVecG.x = groundBelow.x;
            if (groundBelow.y < minVecG.y) minVecG.y = groundBelow.y;
            if (groundBelow.z < minVecG.z) minVecG.z = groundBelow.z;
            if (groundBelow.x > maxVecG.x) maxVecG.x = groundBelow.x;
            if (groundBelow.y > maxVecG.y) maxVecG.y = groundBelow.y;
            if (groundBelow.z > maxVecG.z) maxVecG.z = groundBelow.z;
        }

        private void renderAxes(Matrix4x4 screenMatrix)
        {

            Matrix4x4 transMatrix = genTransMatrix(settings.ship.rootPart.transform, settings.ship, true);

            Vector3 up = transMatrix.MultiplyPoint3x4(Vector3.up * 10000);
            Vector3 down = transMatrix.MultiplyPoint3x4(Vector3.down * 10000);
            Vector3 left = transMatrix.MultiplyPoint3x4(Vector3.left * 10000);
            Vector3 right = transMatrix.MultiplyPoint3x4(Vector3.right * 10000);
            Vector3 front = transMatrix.MultiplyPoint3x4(Vector3.forward * 10000);
            Vector3 back = transMatrix.MultiplyPoint3x4(Vector3.back * 10000);

            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            renderLine(left.x, left.y, right.x, right.y, screenMatrix);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            renderLine(up.x, up.y, down.x, down.y, screenMatrix);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            renderLine(front.x, front.y, back.x, back.y, screenMatrix);
            GL.End();

        }

        private void renderCOM(Matrix4x4 screenMatrix)
        {
            
            Vector3 COM = settings.ship.findLocalCenterOfMass();
            //MonoBehaviour.print("COM>"+COM);
            Matrix4x4 transMatrix = genTransMatrix(settings.ship.rootPart.transform, settings.ship, true);
            //transMatrix = screenMatrix * transMatrix;
            //now render it
            COM = transMatrix.MultiplyPoint3x4(COM);
            //MonoBehaviour.print("COM modified>" + COM);
            float div = 6 / settings.scaleFact;
            renderIcon(new Rect(-div + COM.x, -div + COM.y, 2 * div, 2 * div), screenMatrix, Color.magenta, (int)ViewerConstants.ICONS.SQUARE_DIAMOND);
        }

        private void renderCOP(Matrix4x4 screenMatrix)
        {
            Vector3 COP = settings.ship.findLocalCenterOfPressure();
            //MonoBehaviour.print("COM>"+COM);
            Matrix4x4 transMatrix = genTransMatrix(settings.ship.rootPart.transform, settings.ship, true);
            //transMatrix = screenMatrix * transMatrix;
            //now render it
            COP = transMatrix.MultiplyPoint3x4(COP);
            //MonoBehaviour.print("COM modified>" + COM);
            float div = 6 / settings.scaleFact;
            renderIcon(new Rect(-div + COP.x, -div + COP.y, 2 * div, 2 * div), screenMatrix, Color.cyan, (int)ViewerConstants.ICONS.SQUARE_DIAMOND);
        }

        private void renderMOI(Matrix4x4 screenMatrix)
        {
            Vector3 MOI = settings.ship.findLocalMOI();
            //MonoBehaviour.print("COM>"+COM);
            Matrix4x4 transMatrix = genTransMatrix(settings.ship.rootPart.transform, settings.ship, true);
            //transMatrix = screenMatrix * transMatrix;
            //now render it
            MOI = transMatrix.MultiplyPoint3x4(MOI);
            //MonoBehaviour.print("COM modified>" + COM);
            float div = 6 / settings.scaleFact;
            renderIcon(new Rect(-div + MOI.x, -div + MOI.y, 2 * div, 2 * div), screenMatrix, Color.yellow, (int)ViewerConstants.ICONS.SQUARE_DIAMOND);
        }

        /// <summary>
        /// Renders a single part and adds all its children to the draw queue.
        /// Also adds its bounding box to the bounding box queue.
        /// </summary>
        /// <param name="part">Part to render</param>
        /// <param name="scrnMatrix">Screen transform</param>
        private void renderPart(Part part, Matrix4x4 scrnMatrix, bool fill)
        {
            //first off, add all the parts children to the queue
            foreach (Part child in part.children)
            {
                if (!child.Equals(part.parent))
                {
                    partQueue.Enqueue(child);
                }
            }
            
            //get the appropriate colors
            Color partColor;
            Color boxColor;

            if (!settings.partSelectMode)
            {
                if (!fill)  partColor = getPartColor(part, settings.colorModeMesh);
                else        partColor = getPartColor(part, settings.colorModeFill);
                boxColor = getPartColor(part, settings.colorModeBox);
            }
            else {
                partColor = getPartColorSelectMode(part, settings);
                boxColor = getPartColorSelectMode(part, settings);
            }
            if (settings.colorModeBoxDull) {
                boxColor.r = boxColor.r / 2;
                boxColor.g = boxColor.g / 2;
                boxColor.b = boxColor.b / 2;
            }
            if (fill) 
            {
                if (settings.colorModeFillDull)
                {
                    partColor.r = partColor.r / 2;
                    partColor.g = partColor.g / 2;
                    partColor.b = partColor.b / 2;
                }
            }
            else 
            {
                if (settings.colorModeMeshDull)
                {
                    partColor.r = partColor.r / 2;
                    partColor.g = partColor.g / 2;
                    partColor.b = partColor.b / 2;
                }
            }
            
            //now we need to get all meshes in the part
            List<MeshFilter> meshFList = new List<MeshFilter>();
            foreach (MeshFilter mf in part.transform.GetComponentsInChildren<MeshFilter>())
            {
                meshFList.Add(mf);
            }
            
            //MeshFilter[] meshFilters = (MeshFilter[])part.FindModelComponents<MeshFilter>();
            MeshFilter[] meshFilters = meshFList.ToArray();
            //used to determine the part bounding box
            Vector3 minVec = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (MeshFilter meshF in meshFilters)
            {
                if (!(meshF.renderer == null))
                {
                    //only render those meshes that are active
                    //examples of inactive meshes seem to include
                    //parachute canopies, engine fairings...
                    
                    if (meshF.renderer.gameObject.activeInHierarchy)
                    {
                        Mesh mesh = meshF.mesh;
                        //create the trans. matrix for this mesh (also update the bounds)
                        Matrix4x4 transMatrix = genTransMatrix(meshF.transform, settings.ship,false);
                        updateMinMax(mesh.bounds, transMatrix, ref minVec, ref maxVec);
                        transMatrix = scrnMatrix * transMatrix;
                        //now render it
                        if(!partColor.Equals(Color.black))
                            //renderMesh(mesh, transMatrix, partColor);
                            renderMesh(mesh.triangles, mesh.vertices, transMatrix, partColor);
                    }
                }
            }

            SkinnedMeshRenderer[] skinnedMeshes = (SkinnedMeshRenderer[])part.FindModelComponents<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smesh in skinnedMeshes)
            {
                if (smesh.gameObject.activeInHierarchy)
                {
                    //skinned meshes seem to be not nearly as conveniently simple
                    //luckily, I can apparently ask them to do all the work for me
                    smesh.BakeMesh(bakedMesh);
                    //create the trans. matrix for this mesh (also update the bounds)
                    Matrix4x4 transMatrix = genTransMatrix(part.transform, settings.ship,false);
                    updateMinMax(bakedMesh.bounds, transMatrix, ref minVec, ref maxVec);
                    transMatrix = scrnMatrix * transMatrix;
                    //now render it
                    if (!partColor.Equals(Color.black))
                        //renderMesh(bakedMesh, transMatrix, partColor);
                        renderMesh(bakedMesh.triangles, bakedMesh.vertices, transMatrix, partColor);
                }
                
            }
            if (settings.partSelectMode & settings.selectionCenter) {
                if (settings.selectedPart != null) {
                    if (part == settings.selectedPart)
                    {
                        //finally, update the vessel "bounding box"
                        if (minVecG.x > minVec.x) minVecG.x = minVec.x;
                        if (minVecG.y > minVec.y) minVecG.y = minVec.y;
                        if (minVecG.z > minVec.z) minVecG.z = minVec.z;
                        if (maxVecG.x < maxVec.x) maxVecG.x = maxVec.x;
                        if (maxVecG.y < maxVec.y) maxVecG.y = maxVec.y;
                        if (maxVecG.z < maxVec.z) maxVecG.z = maxVec.z;
                    }
                    else if (settings.selectionSymmetry)
                    {
                        foreach (Part symPart in settings.selectedPart.symmetryCounterparts)
                        {
                            if (part == symPart)
                            {
                                //finally, update the vessel "bounding box"
                                if (minVecG.x > minVec.x) minVecG.x = minVec.x;
                                if (minVecG.y > minVec.y) minVecG.y = minVec.y;
                                if (minVecG.z > minVec.z) minVecG.z = minVec.z;
                                if (maxVecG.x < maxVec.x) maxVecG.x = maxVec.x;
                                if (maxVecG.y < maxVec.y) maxVecG.y = maxVec.y;
                                if (maxVecG.z < maxVec.z) maxVecG.z = maxVec.z;
                                break;
                            }
                        }
                    }
                }
                
            }
            else
            {
                //finally, update the vessel "bounding box"
                if (minVecG.x > minVec.x) minVecG.x = minVec.x;
                if (minVecG.y > minVec.y) minVecG.y = minVec.y;
                if (minVecG.z > minVec.z) minVecG.z = minVec.z;
                if (maxVecG.x < maxVec.x) maxVecG.x = maxVec.x;
                if (maxVecG.y < maxVec.y) maxVecG.y = maxVec.y;
                if (maxVecG.z < maxVec.z) maxVecG.z = maxVec.z;
            }
            if (!fill) 
            {
                //and draw a box around the part (later)
                rectQueue.Enqueue(new ViewerConstants.RectColor(new Rect((minVec.x), (minVec.y), (maxVec.x - minVec.x), (maxVec.y - minVec.y)), boxColor));
            }
            
            
        }

        /// <summary>
        /// Renders a mesh. Doesnt work?
        /// </summary>
        /// <param name="transMatrix">Mesh transform.</param>
        /// <param name="color">Color.</param>
        private void renderMesh(Mesh mesh, Matrix4x4 transMatrix, Color color)
        {
            //setup GL
            GL.PushMatrix();
            GL.MultMatrix(transMatrix);
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            //and draw the triangles
            //TODO: Maybe it doesnt have to be done in immediate mode?
            //Unity GL doesnt seem to expose much, though.
            Graphics.DrawMeshNow(mesh, transMatrix);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Renders a mesh.
        /// </summary>
        /// <param name="triangles">Mesh triangles.</param>
        /// <param name="vertices">Mesh vertices.</param>
        /// <param name="transMatrix">Mesh transform.</param>
        /// <param name="color">Color.</param>
        private void renderMesh(int[] triangles, Vector3[] vertices, Matrix4x4 transMatrix, Color color)
        {
            //setup GL
            GL.PushMatrix();
            GL.MultMatrix(transMatrix);
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            //and draw the triangles
            //TODO: Maybe it doesnt have to be done in immediate mode?
            //Unity GL doesnt seem to expose much, though.
            for (int i = 0; i < triangles.Length; i += 3)
            {
                GL.Vertex(vertices[triangles[i]]);
                GL.Vertex(vertices[triangles[i + 1]]);
                GL.Vertex(vertices[triangles[i + 2]]);
            }
            GL.End();
            GL.PopMatrix();
        }

        private void renderCone(Transform thrustTransform, float scale, float offset, Matrix4x4 screenMatrix, Color color)
        {
            float timeAdd = (Time.frameCount % 40);
            if(timeAdd < 20)
            {
                scale += (scale / 100) * timeAdd;
            }
            else
            {
                scale += (scale / 100) * (40-timeAdd);
            }
            float sideScale = scale / 4f;
            Matrix4x4 transMatrix = genTransMatrix(thrustTransform, settings.ship, true);
            Vector3 posStr = new Vector3(0, 0, offset);
            posStr = transMatrix.MultiplyPoint3x4(posStr);
            Vector3 posStr1 = new Vector3(-sideScale, 0, offset+sideScale);
            posStr1 = transMatrix.MultiplyPoint3x4(posStr1);
            Vector3 posStr2 = new Vector3(0, -sideScale, offset+sideScale);
            posStr2 = transMatrix.MultiplyPoint3x4(posStr2);
            Vector3 posStr3 = new Vector3(sideScale, 0, offset+sideScale);
            posStr3 = transMatrix.MultiplyPoint3x4(posStr3);
            Vector3 posStr4 = new Vector3(0, sideScale, offset+sideScale);
            posStr4 = transMatrix.MultiplyPoint3x4(posStr4);
            Vector3 posEnd = new Vector3(0, 0, offset+scale);
            posEnd = transMatrix.MultiplyPoint3x4(posEnd);
            //setup GL, then render the lines
            GL.Begin(GL.LINES);
            GL.Color(color);
            renderLine(posStr1.x, posStr1.y, posStr2.x, posStr2.y, screenMatrix);
            renderLine(posStr2.x, posStr2.y, posStr3.x, posStr3.y, screenMatrix);
            renderLine(posStr3.x, posStr3.y, posStr4.x, posStr4.y, screenMatrix);
            renderLine(posStr4.x, posStr4.y, posStr1.x, posStr1.y, screenMatrix);
            renderLine(posStr1.x, posStr1.y, posEnd.x, posEnd.y, screenMatrix);
            renderLine(posStr2.x, posStr2.y, posEnd.x, posEnd.y, screenMatrix);
            renderLine(posStr3.x, posStr3.y, posEnd.x, posEnd.y, screenMatrix);
            renderLine(posStr4.x, posStr4.y, posEnd.x, posEnd.y, screenMatrix);
            renderLine(posStr1.x, posStr1.y, posStr.x, posStr.y, screenMatrix);
            renderLine(posStr2.x, posStr2.y, posStr.x, posStr.y, screenMatrix);
            renderLine(posStr3.x, posStr3.y, posStr.x, posStr.y, screenMatrix);
            renderLine(posStr4.x, posStr4.y, posStr.x, posStr.y, screenMatrix);
            GL.End();
        }

        /// <summary>
        /// Renders a gui icon.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <param name="screenMatrix">Transformation matrix.</param>
        /// <param name="color">Color.</param>
        private void renderIcon(Rect rect, Matrix4x4 screenMatrix, Color color, int type)
        {
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.wireframe = false;
            GL.Vertex(screenMatrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMin, 0.1f)));
            GL.Vertex(screenMatrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMax, 0.1f)));
            GL.Vertex(screenMatrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax, 0.1f)));
            GL.Vertex(screenMatrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMin, 0.1f)));
            GL.End();
            GL.wireframe = true;
            
            //setup GL, then render the lines
            GL.Begin(GL.LINES);
            GL.Color(color);
            float xMid = ((rect.xMax - rect.xMin) / 2) + rect.xMin;
            float yMid = ((rect.yMax - rect.yMin) / 2) + rect.yMin;
            float xOneFourth = ((xMid - rect.xMin) / 2) + rect.xMin;
            float yOneFourth = ((yMid - rect.yMin) / 2) + rect.yMin;
            float xThreeFourth = ((rect.xMax - xMid) / 2) + xMid;
            float yThreeFourth = ((rect.yMax - yMid) / 2) + yMid;
            switch (type) 
            {
                case (int)ViewerConstants.ICONS.SQUARE:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.DIAMOND:
                    renderLine(xMid, rect.yMin, rect.xMax, yMid, screenMatrix);
                    renderLine(rect.xMax, yMid, xMid, rect.yMax, screenMatrix);
                    renderLine(xMid, rect.yMax, rect.xMin, yMid, screenMatrix);
                    renderLine(rect.xMin, yMid, xMid, rect.yMin, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.SQUARE_DIAMOND:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);
                    renderLine(xMid, rect.yMin, rect.xMax, yMid, screenMatrix);
                    renderLine(rect.xMax, yMid, xMid, rect.yMax, screenMatrix);
                    renderLine(xMid, rect.yMax, rect.xMin, yMid, screenMatrix);
                    renderLine(rect.xMin, yMid, xMid, rect.yMin, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.TRIANGLE_UP:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, xMid, rect.yMax, screenMatrix);
                    renderLine(xMid, rect.yMax, rect.xMin, rect.yMin, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.TRIANGLE_DOWN:
                    renderLine(rect.xMin, rect.yMax, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, xMid, rect.yMin, screenMatrix);
                    renderLine(xMid, rect.yMin, rect.xMin, rect.yMax, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_READY:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(rect.xMin, yMid, xMid, rect.yMin, screenMatrix);
                    renderLine(xMid, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_NOPOWER:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(xMid, rect.yMin, xThreeFourth, yMid, screenMatrix);
                    renderLine(xOneFourth, yMid, xThreeFourth, yMid, screenMatrix);
                    renderLine(xOneFourth, yMid, xMid, rect.yMax, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_NOFUEL:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMax, rect.yMin, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_NOAIR:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(xOneFourth, yMid, xThreeFourth, yMid, screenMatrix);
                    renderLine(xMid, yOneFourth, xMid, yThreeFourth, screenMatrix);

                    renderLine(xOneFourth, yOneFourth, xThreeFourth, yThreeFourth, screenMatrix);
                    renderLine(xOneFourth, yThreeFourth, xThreeFourth, yOneFourth, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_ACTIVE:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(xMid, rect.yMin, xOneFourth, yThreeFourth, screenMatrix);
                    renderLine(xMid, rect.yMin, xThreeFourth, yThreeFourth, screenMatrix);
                    renderLine(xMid, rect.yMax, xOneFourth, yThreeFourth, screenMatrix);
                    renderLine(xMid, rect.yMax, xThreeFourth, yThreeFourth, screenMatrix);
                    break;
                case (int)ViewerConstants.ICONS.ENGINE_INACTIVE:
                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
                    renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
                    renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);

                    renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
                    break;
            }
            GL.End();
        }

        /// <summary>
        /// Renders a rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <param name="screenMatrix">Transformation matrix.</param>
        /// <param name="color">Color.</param>
        private void renderRect(Rect rect, Matrix4x4 screenMatrix, Color color)
        {
            //setup GL, then render the lines
            GL.Begin(GL.LINES);
            GL.Color(color);
            renderLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, screenMatrix);
            renderLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, screenMatrix);
            renderLine(rect.xMax, rect.yMax, rect.xMin, rect.yMax, screenMatrix);
            renderLine(rect.xMin, rect.yMax, rect.xMin, rect.yMin, screenMatrix);
            GL.End();
        }

        /// <summary>
        /// Renders a line. Assumes color was set already.
        /// </summary>
        /// <param name="x1">x1</param>
        /// <param name="y1">y1</param>
        /// <param name="x2">x2</param>
        /// <param name="y2">x2</param>
        /// <param name="screenMatrix">Screen transformation matrix</param>
        private void renderLine(float x1, float y1, float x2, float y2, Matrix4x4 screenMatrix)
        {
            Vector3 v1 = screenMatrix.MultiplyPoint3x4(new Vector3(x1, y1, 0.1f));
            Vector3 v2 = screenMatrix.MultiplyPoint3x4(new Vector3(x2, y2, 0.1f));
            GL.Vertex(v1);
            GL.Vertex(v2);
        }

        /// <summary>
        /// Updates the min and max values for the total part bounding box.
        /// Uses the mesh bounding box.
        /// </summary>
        /// <param name="meshBounds">Mesh bounding box.</param>
        /// <param name="transMatrix">Mesh transform</param>
        /// <param name="minVec">Reference to minimums-so-far vector</param>
        /// <param name="maxVec">Reference to maximums-so-far vector</param>
        private void updateMinMax(Bounds meshBounds, Matrix4x4 transMatrix, ref Vector3 minVec, ref Vector3 maxVec)
        {
            //simplest way to do this is to take the corner points of the bound. box
            //multiply them by the transformation matrix, and get the mins/maxes from the results
            //its a bit of math done on the CPU but insignificant compared to the number of vertices
            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.min.z);
            vertices[1] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.min.z);
            vertices[2] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.min.z);
            vertices[3] = new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.max.z);
            vertices[4] = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.min.z);
            vertices[5] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.max.z);
            vertices[6] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z);
            vertices[7] = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z);
            foreach (Vector3 v1 in vertices)
            {
                Vector3 v = transMatrix.MultiplyPoint3x4(v1);
                if (v.x < minVec.x) minVec.x = v.x;
                if (v.y < minVec.y) minVec.y = v.y;
                if (v.z < minVec.z) minVec.z = v.z;
                if (v.x > maxVec.x) maxVec.x = v.x;
                if (v.y > maxVec.y) maxVec.y = v.y;
                if (v.z > maxVec.z) maxVec.z = v.z;
            }
        }

        /// <summary>
        /// Generate a transform matrix from the meshes and vessels matrix
        /// </summary>
        /// <param name="meshTrans">Mesh matrix</param>
        /// <param name="vessel">Active vessel</param>
        /// <returns></returns>
        private Matrix4x4 genTransMatrix(Transform meshTrans, Vessel vessel, bool zeroFlatter)
        {
            //the mesh transform matrix in local space (which is what we want)
            //is essentialy its world transform matrix minus the transformations
            //applied to the whole vessel.
            Matrix4x4 meshTransMatrix = vessel.vesselTransform.localToWorldMatrix.inverse * meshTrans.localToWorldMatrix;
            //might also need some rotation to show a different side
            transformTemp.transform.rotation = Quaternion.identity;
            //NavBall stockNavBall = GameObject.Find("NavBall").GetComponent<NavBall>();
            Vector3 extraRot = new Vector3(0, 0, 0);
            float speed = ViewerConstants.SPIN_SPEED_VAL[settings.spinSpeed];
            switch (settings.spinAxis) 
            {
                case (int)ViewerConstants.AXIS.X:
                    extraRot.x += ((Time.time * speed) % 360);
                    break;
                case (int)ViewerConstants.AXIS.Y:
                    extraRot.y += ((Time.time * speed) % 360);
                    break;
                case (int)ViewerConstants.AXIS.Z:
                    extraRot.z += ((Time.time * speed) % 360);
                    break;
            }

            switch (settings.drawPlane)
            {
                case (int)ViewerConstants.PLANE.XY:
                    transformTemp.transform.Rotate(extraRot);
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.XZ:
                    transformTemp.transform.Rotate(new Vector3(0, 90, 0));
                    transformTemp.transform.Rotate(extraRot);
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.YZ:
                    transformTemp.transform.Rotate(new Vector3(90, 0, 0));
                    transformTemp.transform.Rotate(extraRot);
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.ISO:
                    transformTemp.transform.Rotate(new Vector3(0, -30, 0));
                    transformTemp.transform.Rotate(new Vector3(15, 0, 0));
                    
                    transformTemp.transform.Rotate(extraRot);
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.GRND:
                    transformTemp.transform.rotation = vessel.srfRelRotation;
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    transformTemp.transform.rotation = Quaternion.FromToRotation(vessel.mainBody.GetSurfaceNVector(0, 0), vessel.mainBody.GetSurfaceNVector(vessel.latitude, vessel.longitude));
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix.inverse * meshTransMatrix;
                    transformTemp.transform.rotation = Quaternion.identity;
                    transformTemp.transform.Rotate(new Vector3(0, 0, 90));
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.REAL:
                    transformTemp.transform.rotation = vessel.vesselTransform.rotation;
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
            }
            Matrix4x4 FLATTER;
            if (zeroFlatter)
            {
                FLATTER = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            }
            else {
                FLATTER = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.001f));
            }
            //scale z by zero to flatten and prevent culling
            meshTransMatrix = FLATTER * meshTransMatrix;
            return meshTransMatrix;
        }

        /// <summary>
        /// Calculate the ideal scale/offset.
        /// </summary>
        private void centerise(int screenWidth, int screenHeight)
        {
            //for padding
            int screenWidthM = (int)(screenWidth * 0.9f);
            int screenHeightM = (int)(screenHeight * 0.9f);
            //if we want the root part to stay in the center on an axis, we need the 
            //bounding box to have the same size from both sides of it in that axis
            if (settings.centerOnRootH)
            {
                if (Math.Abs(maxVecG.x) < Math.Abs(minVecG.x)) maxVecG.x = -minVecG.x;
                else minVecG.x = -maxVecG.x;
            }
            if (settings.centerOnRootV)
            {
                if (Math.Abs(maxVecG.y) < Math.Abs(minVecG.y)) maxVecG.y = -minVecG.y;
                else minVecG.y = -maxVecG.y;
            }
            float xDiff = (maxVecG.x - minVecG.x);
            float yDiff = (maxVecG.y - minVecG.y);
            //to rescale, we need to scale up the vessel render to fit the screen bounds
            if (settings.centerRescale != (int)ViewerConstants.RESCALEMODE.OFF)
            {
                float maxDiff = 0;
                if (settings.centerRescale == (int)ViewerConstants.RESCALEMODE.INCR) maxDiff = 0.5f;
                if (settings.centerRescale == (int)ViewerConstants.RESCALEMODE.CLOSE) maxDiff = 0.85f;
                if (settings.centerRescale == (int)ViewerConstants.RESCALEMODE.BEST) maxDiff = 1f;

                float idealScaleX = screenWidthM / xDiff;
                float idealScaleY = screenHeightM / yDiff;
                //round to nearest integer
                float newScale = (int)((idealScaleX < idealScaleY) ? idealScaleX : idealScaleY);
                float diffFact = settings.scaleFact / newScale;
                if (diffFact < maxDiff | diffFact > 1) 
                {
                    settings.scaleFact = newScale;
                    //and clamp it a bit
                    if (settings.scaleFact < 1) settings.scaleFact = 1;
                    if (settings.scaleFact > 1000) settings.scaleFact = 1000;
                }
            }
            //to centerise, we need to move the center point of the vessel render
            //into the center of the screen
            settings.scrOffX = screenWidth / 2 - (int)((minVecG.x + xDiff / 2) * settings.scaleFact);
            settings.scrOffY = screenHeight / 2 - (int)((minVecG.y + yDiff / 2) * settings.scaleFact);
        }

        private bool partIsOnWayToRoot(Part part, Part leaf, Part root) {
            if (part == null | leaf == null | root == null) return false;
            if (leaf == root) return false;
            if (leaf == part) return true;
            return partIsOnWayToRoot(part, leaf.parent, root);
        }

        private Color getPartColorSelectMode(Part part, ViewerSettings settings) {
            Color darkGreen = Color.green;
            darkGreen.g = 0.6f;
            darkGreen.r = 0.3f;
            darkGreen.b = 0.3f;
            Part selectedPart = settings.selectedPart;
            if (selectedPart == null) return Color.red;
            if (part == selectedPart) return Color.green;
            if (settings.selectionSymmetry) {
                if (selectedPart.symmetryCounterparts.Contains(part)) return darkGreen;
            }
            if (partIsOnWayToRoot(part, selectedPart, settings.ship.rootPart)) return Color.yellow;
            if (part == settings.ship.rootPart) return Color.magenta;
            return Color.white;
        }

        /// <summary>
        /// Returns the color appropriate for a given part,
        /// depending on the coloring mode provided.
        /// </summary>
        /// <param name="part">Associated part.</param>
        /// <param name="mode">Coloring mode.</param>
        /// <returns></returns>
        private Color getPartColor(Part part, int mode)
        {
            switch (mode)
            {
                case (int)ViewerConstants.COLORMODE.NONE:
                    return Color.white;
                case (int)ViewerConstants.COLORMODE.STATE:
                    //it seems most of these are unused, but it does at least
                    //make it (semi-)clear what is the root part and which parts belong
                    //to activated stages
                    if (part.parent == null) { return Color.magenta; }
                    switch (part.State)
                    {
                        case PartStates.ACTIVE:
                            return Color.blue;
                        case PartStates.DEACTIVATED:
                            return Color.red;
                        case PartStates.DEAD:
                            return Color.gray;
                        case PartStates.IDLE:
                            return Color.green;
                        default:
                            return Color.red;
                    }
                case (int)ViewerConstants.COLORMODE.STAGE:
                    //colors the parts by their inverse stage.
                    //first we need an appropriate gradient, so check if we have it
                    //and make it if we dont, or if its too small
                    if (stagesThisTimeMax < part.inverseStage) stagesThisTimeMax = part.inverseStage;

                    int neededColors = Math.Max(stagesLastTime, Math.Max(Staging.StageCount, stagesThisTimeMax)) + 1;
                    if (stageGradient == null)
                    {
                        genColorGradient(neededColors);
                    }
                    else if (stageGradient.Length != neededColors)
                    {
                        genColorGradient(neededColors);
                    }
                    //now return the color 
                    //print("part " + part.name + " inv. stage " + part.inverseStage);
                    if (part.inverseStage >= stageGradient.Length) return Color.magenta;
                    return stageGradient[part.inverseStage];
                case (int)ViewerConstants.COLORMODE.HEAT:
                    //colors the part according to how close its to exploding due to overheat
                    Color color = new Color(0.2f, 0.2f, 0.2f);
                    if (part.maxTemp != 0)
                    {
                        float tempDiff = part.temperature / part.maxTemp;
                        //to power of THREE to emphasise overheating parts MORE
                        tempDiff = (float)Math.Pow(tempDiff, 3);
                        //color.g = 0.2f;
                        color.b = 0.2f * (1 - tempDiff);
                        color.r = 0.2f + tempDiff*0.8f;
                        return color;
                    }
                    else
                    {
                        return color;
                    }
                case (int)ViewerConstants.COLORMODE.FUEL:
                    Color color2 = Color.red;
                    List<PartResource> resList = part.Resources.list;
                    int resCount = resList.Count;
                    int emptyRes = 0;
                    double totalResFraction = 0;
                    //go through all the resources in the part, add up their fullness
                    foreach (PartResource resource in resList)
                    {
                        //2 is almost empty anyway for all but the smallest tanks 
                        //and it eliminates things like intakes or power generating engines
                        if (resource.amount <= 2f)
                        {
                            emptyRes++;
                        }
                        else
                        {
                            double resourceFraction = (resource.amount / resource.maxAmount);
                            totalResFraction += resourceFraction / (double)resCount;
                        }
                    }
                    //now set the part color
                    if (resCount == 0 | emptyRes == resCount)
                    {
                        color2 = new Color(0.2f, 0.2f, 0.2f);
                    }
                    else
                    {
                        return genFractColor((float)totalResFraction);
                    }

                    return color2;
                case (int)ViewerConstants.COLORMODE.DRAG:
                    float drag = part.angularDrag;
                    if (part.Modules.Contains("FARControllableSurface"))
                    {
                        //MonoBehaviour.print("cont. surf.");
                        PartModule FARmodule = part.Modules["FARControllableSurface"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("currentDrag"))
                            {
                                drag = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    else if (part.Modules.Contains("FARWingAerodynamicModel"))
                    {
                        //MonoBehaviour.print("wing");
                        PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("currentDrag"))
                            {
                                drag = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }  
                    else if (part.Modules.Contains("FARBasicDragModel"))
                    {
                        //MonoBehaviour.print("basic drag");
                        PartModule FARmodule = part.Modules["FARBasicDragModel"];
                        foreach (BaseField fieldInList in FARmodule.Fields) 
                        {
                            if (fieldInList.name.Equals("currentDrag")) 
                            {
                                drag = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    return genHeatmapColor(drag);
                case (int)ViewerConstants.COLORMODE.LIFT:
                    float lift = 0;
                    if (part.Modules.Contains("FARControllableSurface"))
                    {
                        //MonoBehaviour.print("cont. surf.");
                        PartModule FARmodule = part.Modules["FARControllableSurface"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("currentLift"))
                            {
                                lift = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    else if (part.Modules.Contains("FARWingAerodynamicModel"))
                    {
                        //MonoBehaviour.print("wing");
                        PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("currentLift"))
                            {
                                lift = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    return genHeatmapColor(lift);
                case (int)ViewerConstants.COLORMODE.STALL:
                    float stall = 0;
                    if (part.Modules.Contains("FARControllableSurface"))
                    {
                        //MonoBehaviour.print("cont. surf.");
                        PartModule FARmodule = part.Modules["FARControllableSurface"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("stall"))
                            {
                                stall = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    else if (part.Modules.Contains("FARWingAerodynamicModel"))
                    {
                        //MonoBehaviour.print("wing");
                        PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                        foreach (BaseField fieldInList in FARmodule.Fields)
                        {
                            if (fieldInList.name.Equals("stall"))
                            {
                                stall = (float)fieldInList.GetValue(FARmodule);
                                break;
                            }
                        }
                    }
                    return genFractColor(1f-stall);
                case (int)ViewerConstants.COLORMODE.HIDE:
                    return Color.black;
                default:
                    return Color.white;
            }
        }

        public Color genHeatmapColor(float value)
        {
            //find the appropriate color for this specific part
            Color color = new Color(0.1f,0.1f,0.1f);
            //grey to blue to cyan to green to yellow to red
            //0    to  1   to  4   to   10   to   40   to infinity
            if (value < 1) 
            {
                color.b += value * 0.9f;
            }else if(value < 4) 
            {
                color.b = 1;
                color.g += ((value - 1) / 3) * 0.9f; 
            }else if (value < 10)
            {
                color.g = 1;
                color.b += 0.9f-(((value - 4) / 6) * 0.9f);
            }
            else if (value < 40)
            {
                color.g = 1;
                color.r += ((value - 10) / 30) * 0.9f; 
            }
            else 
            {
                color.r = 1;
                color.g += 0.9f - (1-(1/(value-40+1)))*0.9f;
            }
            return color;
        }

        public Color genFractColor(float fraction) {
            //find the appropriate color for this specific part
            Color color = Color.red;
            //red to yellow to green
            if (fraction <= 0.5f)
            {
                color.g = (float)(fraction * 2);
            }
            else
            {
                color.r = (float)((1 - fraction) * 2);
                color.g = 1f;
            }
            return color;
        }

        /// <summary>
        /// Generates a beautiful rainbow.
        /// Used for the stage color display.
        /// Colors generated have the same saturation and lightness, and an even hue spread.
        /// </summary>
        /// <param name="numberOfColors"></param>
        private void genColorGradient(int numberOfColors)
        {
            stageGradient = new Color[numberOfColors];
            float perStep = 4f / ((float)(numberOfColors - 1));
            //colors are generated in four intervals
            //0-1 (red is maxed, green increases)
            //1-2 (green is maxed, red recedes)
            //2-3 (green is maxed, blue increases)
            //3-4 (green recedes, blue is maxed)
            //this results in a sweet rainbow of colors.
            for (int i = 0; i < numberOfColors; i++)
            {
                Color color = new Color();
                color.a = 1f;
                float pos = ((float)i) * perStep;
                if (pos <= 1)
                {
                    color.r = 1f;
                    color.g = pos;
                    color.b = 0;
                }
                else if (pos <= 2)
                {
                    color.r = 2f - pos;
                    color.g = 1f;
                    color.b = 0;
                }
                else if (pos <= 3)
                {
                    color.r = 0;
                    color.g = 1f;
                    color.b = pos - 2f;
                }
                else if (pos <= 4)
                {
                    color.r = 0;
                    color.g = 4f - pos;
                    color.b = 1f;
                }
                stageGradient[i] = color;
            }
        }



    }
}
