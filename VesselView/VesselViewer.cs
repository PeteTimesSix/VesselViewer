using System;
using System.Collections.Generic;
using UnityEngine;

namespace VesselView
{
    public class VesselViewer
    {

        //centerised... center
        private int scrOffX = 512;
        private int scrOffY = 512;
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

        private static Mesh bakedMesh = new Mesh();

        public void nilOffset(int width, int height) {
            scrOffX = width / 2;
            scrOffY = height / 2;
        }

        public void manuallyOffset(int offsetX, int offsetY) {
            scrOffX += offsetX;
            scrOffY += offsetY;
        }

        public void forceRedraw() {
            // ...what? it works.
            //Im assuming the refresh rate of text in RPM
            //is lower than the background (the config file seems
            //to support this, so we delay by a tiny bit
            lastUpdate = Time.time - 0.8f;
        }

        public void drawCall(RenderTexture screen) {
            //Latency mode to limit to one frame per second if FPS is affected
            //also because it happens to look exactly like those NASA screens :3
            if (settings.latency)
            {
                if ((Time.time - lastUpdate) > 1 & settings.latency)
                {
                    restartDraw(screen);
                    if (settings.autoCenter)
                    {
                        centerise(screen.width, screen.height);
                    }
                }
            }
            else
            {
                restartDraw(screen);
                if (settings.autoCenter)
                {
                    centerise(screen.width, screen.height);
                }
            }
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
            renderToTexture(screen);
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
                //turn on wireframe, since triangles would get filled othershipwise
                GL.wireframe = true;
                //set up the screen position and scaling matrix
                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(scrOffX, scrOffY, 0), Quaternion.identity, new Vector3(settings.scaleFact, settings.scaleFact, settings.scaleFact));
                //dunno what this does, but I trust in the stolen codes
                lineMaterial.SetPass(0);

                //now render each part (assumes root part is in the queue)
                while (partQueue.Count > 0)
                {
                    Part next = partQueue.Dequeue();
                    if (next != null)
                    {
                        renderPart(next, matrix);
                    }
                }
                //now render the bounding boxes (so theyre on top)
                if (settings.colorModeBox != (int)ViewerConstants.COLORMODE.HIDE)
                {
                    renderRects(matrix);
                }
                //then set the max stages (for the stage coloring)
                stagesLastTime = stagesThisTimeMax;
                //undo stuff
                GL.wireframe = false;
                GL.PopMatrix();
                RenderTexture.active = backupRenderTexture;
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

        /// <summary>
        /// Renders a single part and adds all its children to the draw queue.
        /// Also adds its bounding box to the bounding box queue.
        /// </summary>
        /// <param name="part">Part to render</param>
        /// <param name="scrnMatrix">Screen transform</param>
        private void renderPart(Part part, Matrix4x4 scrnMatrix)
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
                partColor = getPartColor(part, settings.colorModeMesh);
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
            if (settings.colorModeMeshDull)
            {
                partColor.r = partColor.r / 2;
                partColor.g = partColor.g / 2;
                partColor.b = partColor.b / 2;
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
                        Matrix4x4 transMatrix = genTransMatrix(meshF.transform, settings.ship);
                        updateMinMax(mesh.bounds, transMatrix, ref minVec, ref maxVec);
                        transMatrix = scrnMatrix * transMatrix;
                        //now render it
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
                    Matrix4x4 transMatrix = genTransMatrix(part.transform, settings.ship);
                    updateMinMax(bakedMesh.bounds, transMatrix, ref minVec, ref maxVec);
                    transMatrix = scrnMatrix * transMatrix;
                    //now render it
                    renderMesh(bakedMesh.triangles, bakedMesh.vertices, transMatrix, partColor);
                }
                
            }
            //if(settings.partSelectMode & settings.partSelectCenterPart)
            //finally, update the vessel "bounding box"
            if (minVecG.x > minVec.x) minVecG.x = minVec.x;
            if (minVecG.y > minVec.y) minVecG.y = minVec.y;
            if (minVecG.z > minVec.z) minVecG.z = minVec.z;
            if (maxVecG.x < maxVec.x) maxVecG.x = maxVec.x;
            if (maxVecG.y < maxVec.y) maxVecG.y = maxVec.y;
            if (maxVecG.z < maxVec.z) maxVecG.z = maxVec.z;
            //and draw a box around the part (later)
            rectQueue.Enqueue(new ViewerConstants.RectColor(new Rect((minVec.x), (minVec.y), (maxVec.x - minVec.x), (maxVec.y - minVec.y)), boxColor));
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
            GL.Color(color);
            GL.PushMatrix();
            GL.MultMatrix(transMatrix);
            GL.Begin(GL.TRIANGLES);
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

        /// <summary>
        /// Renders a rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <param name="screenMatrix">Transformation matrix.</param>
        /// <param name="color">Color.</param>
        private void renderRect(Rect rect, Matrix4x4 screenMatrix, Color color)
        {
            //setup GL, then render the lines
            GL.Color(color);
            GL.Begin(GL.LINES);
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
            Vector3 v1 = screenMatrix.MultiplyPoint3x4(new Vector3(x1, y1, 0.01f));
            Vector3 v2 = screenMatrix.MultiplyPoint3x4(new Vector3(x2, y2, 0.01f));
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
        private Matrix4x4 genTransMatrix(Transform meshTrans, Vessel vessel)
        {
            //the mesh transform matrix in local space (which is what we want)
            //is essentialy its world transform matrix minus the transformations
            //applied to the whole vessel.
            Matrix4x4 meshTransMatrix = vessel.vesselTransform.localToWorldMatrix.inverse * meshTrans.localToWorldMatrix;
            //might also need some rotation to show a different side
            transformTemp.transform.rotation = Quaternion.identity;
            NavBall stockNavBall = GameObject.Find("NavBall").GetComponent<NavBall>();

            switch (settings.drawPlane)
            {
                case (int)ViewerConstants.PLANE.XY:
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.XZ:
                    transformTemp.transform.Rotate(new Vector3(0, 90, 0));
                    meshTransMatrix = transformTemp.transform.localToWorldMatrix * meshTransMatrix;
                    break;
                case (int)ViewerConstants.PLANE.YZ:
                    transformTemp.transform.Rotate(new Vector3(90, 0, 0));
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
            Matrix4x4 FLATTER = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.00001f));
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
            if (settings.centerRescale)
            {
                float idealScaleX = screenWidthM / xDiff;
                float idealScaleY = screenHeightM / yDiff;
                settings.scaleFact = (idealScaleX < idealScaleY) ? idealScaleX : idealScaleY;
                //round to nearest integer
                settings.scaleFact = (int)settings.scaleFact;
                //and clamp it a bit
                if (settings.scaleFact < 1) settings.scaleFact = 1;
                if (settings.scaleFact > 1000) settings.scaleFact = 1000;
            }
            //to centerise, we need to move the center point of the vessel render
            //into the center of the screen
            scrOffX = screenWidth/2 - (int)((minVecG.x + xDiff / 2) * settings.scaleFact);
            scrOffY = screenHeight/2 - (int)((minVecG.y + yDiff / 2) * settings.scaleFact);
        }

        private bool partIsOnWayToRoot(Part part, Part leaf, Part root) {
            if (part == null | leaf == null | root == null) return false;
            if (leaf == root) return false;
            if (leaf == part) return true;
            return partIsOnWayToRoot(part, leaf.parent, root);
        }

        private Color getPartColorSelectMode(Part part, ViewerSettings settings) {
            Color darkGreen = Color.green;
            darkGreen.g = 0.7f;
            
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
                case (int)ViewerConstants.COLORMODE.HIDE:
                    return Color.black;
                default:
                    return Color.white;
            }
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
