// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections.Generic;
    using UnityEngine;
    using Microsoft.MixedReality.SceneUnderstanding.Samples.Unity;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// This Script Controls the DollHouse Experience,
    /// DollHouse has two modes, the Minimap Mode and the
    /// Bounding Box mode
    /// 
    /// Minimap Mode: The entire SU Scan will be miniaturized and displayed infront of you
    /// Bounding Box: The app will try to find the immediate walls that surround you 
    /// 
    /// </summary>
    public class SUFloorPlanManager : MonoBehaviour
    {
        //Public, accessible through the Unity Editor
        public SceneUnderstandingManager manager; //Reference to the SceneUnderstandingManager
        public Material SUDefaultMaterial;
        public Material SUWireFrameMaterial;
        public float AcceptedDistanceOutsideBoudingBox; // How far away can a vertex be while still being considered 'inside' the bouding box                 
        public GameObject PlayerToken; //Reference to the Prefab that represents the player token
        public Vector3 PlayerTokenOffset;
        public Vector3 MiniaturizedPlayerTokenScale;

        //Flags for the bouding box mode
        public bool RemoveSUWallsFromBoudingBox;
        public bool RemoveSUInferredObjectsFromBoudingBox;
        public bool RemoveSUCeilingsFromBoudingBox;
        public bool RemoveSUFloorsFromBoudingBox;

        //References
        private GameObject gbjDollHouse; //Mini Twin of your SU enviroment
        private GameObject gbjPlayerPosition; //Unity GameObject in the Real-Size SU enviroment
        private GameObject gbjPlayerDollHousePosition; //Unity GameObject in the miniaturized SU Enviroment (DollHouse)
        private GameObject gbjPlayerBoudingBoxPosition; //Unity GameObject in the miniaturized SU Enviroment (Bounding Box)

        //In reality, when we create a bounding box we actually create two of them
        //One that's invisible and perfectly mapped to your enviroment in size and orientation
        //and a smaller one that's right in front of you for visualization
        private GameObject gbjBoudingBox;
        private GameObject gbjBoudingBoxMini;

        private bool isFloorPlanOn = false;  //flag for when the dollhouse is being displayed
        private bool isBoudingBoxOn = false; //flag for when the bouding box is being displayed

        //Here we store which game objects are the candidates for the walls that
        //surround you, these may change every frame, depending on where you are
        //we use this later to create the well defined bouding box
        private GameObject gbjSUWallFront;
        private GameObject gbjSUWallBack;
        private GameObject gbjSUWallLeft;
        private GameObject gbjSUWallRight;
        private GameObject gbjSUFloorDown;
        private GameObject gbjSUCeilingUp;

        private Color clrDefaultCeiling = new Color(0.474509f, 0.592156f, 0.952941f);
        private Color clrDefaultWall    = new Color(0.952941f, 0.494117f, 0.474509f);
        private Color clrDefaultFloor   = new Color(0.733333f, 0.952941f, 0.474509f);
        private void Update()
        {
            UpdatePlayerPosition();
            UpdatePlayerPositionInDollHouse();
            UpdatePlayerPositionInBoudingBox();
            UpdateBoudingBoxWalls();
        }

        private void FloorPlanOn()
        {
            GenerateMinimap();
            ToggleSceneRootVisibility();
        }

        private void FloorPlanOff()
        {
            Destroy(gbjDollHouse);
            Destroy(gbjPlayerPosition);
            ToggleSceneRootVisibility();
        }

        //This function is registered in the 'SceneUnderstanding Menu' in the 'Input Actions' section
        public void ToggleFloorPlan()
        {
            if (isBoudingBoxOn)
            {
                return;
            }

            isFloorPlanOn = !isFloorPlanOn;
            if (isFloorPlanOn)
            {
                FloorPlanOn();

            }
            else
            {
                FloorPlanOff();
            }
        }

        //Make the scene root invisible, not disabled, but invisible
        private void ToggleSceneRootVisibility()
        {
            foreach (Transform ParentTransform in manager.SceneRoot.transform)
            {
                foreach (Transform suGeometry in ParentTransform)
                {
                    suGeometry.GetComponent<MeshRenderer>().enabled = !suGeometry.GetComponent<MeshRenderer>().enabled;
                }
            }

            if(gbjPlayerPosition != null)
            {
                gbjPlayerPosition.GetComponent<MeshRenderer>().enabled = !gbjPlayerPosition.GetComponent<MeshRenderer>().enabled;
            }
        }

        //The minimap is the miniature SU representation or Dollhouse
        void GenerateMinimap()
        {
            //RayCasts take a bit mask to know what can they collide with, we pass it a '101' binary mask and bit shift it to the 9 and 11 position
            int floorsAndCeilingMask = 5 << Mathf.Min(LayerMask.NameToLayer("SUFloors"), LayerMask.NameToLayer("SUCeilings"));

            //Create the player token in the Scene Root
            if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, floorsAndCeilingMask))
            {
                gbjPlayerPosition = Instantiate<GameObject>(PlayerToken);
                gbjPlayerPosition.name = "PlayerPositionWorld";
                gbjPlayerPosition.transform.position = hit.point + PlayerTokenOffset;
                gbjPlayerPosition.transform.rotation = Quaternion.identity;
                gbjPlayerPosition.transform.parent = manager.SceneRoot.transform;
                gbjPlayerPosition.transform.localScale = MiniaturizedPlayerTokenScale;
            }

            //Create the DollHouse
            gbjDollHouse = Instantiate(manager.SceneRoot);
            gbjDollHouse.name = "DollHouse";
            gbjDollHouse.transform.position = Camera.main.transform.position + Camera.main.transform.forward + (Vector3.down * 0.15f);
            gbjDollHouse.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            //Set all the layers of the Minature to Default to avoid the minature getting in the way of the raycast
            foreach (Transform gbjDollHouseTransform in gbjDollHouse.transform)
            {
                if (gbjDollHouseTransform.name == "Floor")
                {
                    foreach (Transform suGeometry in gbjDollHouseTransform)
                    {
                        suGeometry.gameObject.layer = LayerMask.NameToLayer("Default");
                    }
                }

                if (gbjDollHouseTransform.name == "Wall")
                {
                    foreach (Transform suGeometry in gbjDollHouseTransform)
                    {
                        suGeometry.gameObject.layer = LayerMask.NameToLayer("Default");
                    }
                }

                //Since we replicated the SceneRoot as minature the minature contains a miniaturized player token
                //remember it.
                if (gbjDollHouseTransform.name == "PlayerPositionWorld")
                {
                    gbjPlayerDollHousePosition = gbjDollHouseTransform.gameObject;
                }
            }
        }

        
        private void UpdatePlayerPosition()
        {
            //If the player token exists, update its position every frame
            if (gbjPlayerPosition != null)
            {
                int floorsAndCeilingMask = 5 << Mathf.Min(LayerMask.NameToLayer("SUFloors"), LayerMask.NameToLayer("SUCeilings"));
                if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, floorsAndCeilingMask))
                {
                    gbjPlayerPosition.transform.position = hit.point + (Vector3.up * 0.41f);
                }
            }
        }

        private void UpdatePlayerPositionInDollHouse()
        {
            //If the minaturized player token exists mimic its postion with the full size player token
            if (gbjPlayerDollHousePosition != null && gbjPlayerPosition != null)
            {
                gbjPlayerDollHousePosition.transform.localPosition = gbjPlayerPosition.transform.localPosition;
            }
        }

        private void UpdatePlayerPositionInBoudingBox()
        {
            //If the minaturized player token exists mimic its postion with the full size player token
            if (gbjPlayerBoudingBoxPosition != null && gbjPlayerPosition != null)
            {
                gbjPlayerBoudingBoxPosition.transform.localPosition = gbjPlayerPosition.transform.localPosition;
            }
        }

        //This function is registered in the 'SceneUnderstanding Menu' in the 'Input Actions' section
        public void ToggleBoundingBox()
        {
            if (isFloorPlanOn)
            {
                return;
            }

            isBoudingBoxOn = !isBoudingBoxOn;
            if (isBoudingBoxOn)
            {
                BoudingBoxOn();

            }
            else
            {
                BoundingBoxOff();
            }
        }

        private void BoudingBoxOn()
        {
            //Bouding Walls are updated every frame, if there's any bound missing stop
            if(gbjSUWallFront == null || gbjSUWallBack == null || gbjSUWallLeft == null || 
            gbjSUWallRight == null || gbjSUCeilingUp   == null || gbjSUFloorDown == null)
            {
                isBoudingBoxOn = false;
                Debug.Log("No suitable bouding box detected, move around to a more defined space");
            }
            else
            {
                //Remember The original Parents of the SU walls
                Transform tsfParentSUWallFront = gbjSUWallFront.transform.parent;
                Transform tsfParentSUWallBack  = gbjSUWallBack.transform.parent;
                Transform tsfParentSUWallLeft  = gbjSUWallLeft.transform.parent;
                Transform tsfParentSUWallRight = gbjSUWallRight.transform.parent;
                Transform tsfParentSUCeiling   = gbjSUCeilingUp.transform.parent;
                Transform tsfParentSUFloor     = gbjSUFloorDown.transform.parent;

                //Add SU Walls to a pivot gameobject
                gbjBoudingBox = new GameObject("BoudingBox");
                gbjSUWallFront.transform.parent = gbjBoudingBox.transform;
                gbjSUWallBack.transform.parent  = gbjBoudingBox.transform;
                gbjSUWallLeft.transform.parent  = gbjBoudingBox.transform;
                gbjSUWallRight.transform.parent = gbjBoudingBox.transform;
                gbjSUCeilingUp.transform.parent = gbjBoudingBox.transform;
                gbjSUFloorDown.transform.parent = gbjBoudingBox.transform;

                //Align the SU walls to the world axis, first get the necessary angle to rotate in the 'Y' axis or in the X-Z plane, however you want to see it
                Vector3 vc3RoomFoward = gbjSUWallFront.transform.forward;
                vc3RoomFoward.y = 0.0f;
                vc3RoomFoward.Normalize();
                float Angle = Mathf.Acos(Vector3.Dot(vc3RoomFoward, Vector3.forward)) * Mathf.Rad2Deg;

                //We have the angle now, but how do we know if we need to rotate clockwise or counterclockwise?
                Vector3 Orientation = Vector3.Cross(Vector3.forward, vc3RoomFoward);

                //If the 'Orientation Vector' is pointing 'Upwards' (positive 'Y') rotate counter-clockwise otherwise, (negative 'Y') rotate clockwise
                Angle = Orientation.y > 0.0f ? -Angle : Angle; 

                //Rotate the pivot and align it to the world axis
                gbjBoudingBox.transform.Rotate(Vector3.up, Angle);

                //Use the SU Walls to create a well defined water tight bouding box

                //FrontWall
                Vector3 vc3LowerLeft  = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallFront.transform.position.z);
                Vector3 vc3UpperRight = new Vector3(gbjSUWallRight.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallFront.transform.position.z);
                Vector3 vc3UpperLeft  = new Vector3(vc3LowerLeft.x, vc3UpperRight.y, vc3LowerLeft.z);
                Vector3 vc3LowerRight = new Vector3(vc3UpperRight.x, vc3LowerLeft.y, vc3UpperRight.z);

                GameObject gbjFrontWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, false);
                gbjFrontWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultWall);

                //LeftWall
                vc3LowerLeft  = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperRight = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallFront.transform.position.z);
                vc3UpperLeft  = new Vector3(vc3LowerLeft.x, vc3UpperRight.y, vc3LowerLeft.z);
                vc3LowerRight = new Vector3(vc3UpperRight.x, vc3LowerLeft.y, vc3UpperRight.z);

                GameObject gbjLeftWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, false);
                gbjLeftWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultWall);

                //RightWall
                vc3LowerLeft = new Vector3(gbjSUWallRight.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallFront.transform.position.z);
                vc3UpperRight = new Vector3(gbjSUWallRight.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperLeft = new Vector3(vc3LowerLeft.x, vc3UpperRight.y, vc3LowerLeft.z);
                vc3LowerRight = new Vector3(vc3UpperRight.x, vc3LowerLeft.y, vc3UpperRight.z);

                GameObject gbjRightWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, false);
                gbjRightWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultWall);

                //BackWall
                vc3LowerLeft = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperRight = new Vector3(gbjSUWallRight.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperLeft = new Vector3(vc3LowerLeft.x, vc3UpperRight.y, vc3LowerLeft.z);
                vc3LowerRight = new Vector3(vc3UpperRight.x, vc3LowerLeft.y, vc3UpperRight.z);

                GameObject gbjBackWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, true);
                gbjBackWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultWall);

                //Floor
                vc3LowerLeft = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperRight = new Vector3(gbjSUWallRight.transform.position.x, gbjSUFloorDown.transform.position.y, gbjSUWallFront.transform.position.z);
                vc3UpperLeft = new Vector3(vc3LowerLeft.x, vc3LowerLeft.y, vc3UpperRight.z);
                vc3LowerRight = new Vector3(vc3UpperRight.x, vc3UpperRight.y, vc3LowerRight.z);

                GameObject gbjFloorWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, false);
                gbjFloorWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultFloor);

                //Ceiling
                vc3LowerLeft = new Vector3(gbjSUWallLeft.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallBack.transform.position.z);
                vc3UpperRight = new Vector3(gbjSUWallRight.transform.position.x, gbjSUCeilingUp.transform.position.y, gbjSUWallFront.transform.position.z);
                vc3UpperLeft = new Vector3(vc3LowerLeft.x, vc3LowerLeft.y, vc3UpperRight.z);
                vc3LowerRight = new Vector3(vc3UpperRight.x, vc3UpperRight.y, vc3LowerLeft.z);

                GameObject gbjCeilingWall = CreateBoudingWall(vc3LowerLeft, vc3LowerRight, vc3UpperRight, vc3UpperLeft, true);
                gbjCeilingWall.GetComponent<MeshRenderer>().material.SetColor("_WireColor", clrDefaultCeiling);

                //Add bouding box box to the pivot
                gbjFrontWall.transform.parent   = gbjBoudingBox.transform;
                gbjRightWall.transform.parent   = gbjBoudingBox.transform;
                gbjLeftWall.transform.parent    = gbjBoudingBox.transform;
                gbjBackWall.transform.parent    = gbjBoudingBox.transform;
                gbjFloorWall.transform.parent   = gbjBoudingBox.transform;
                gbjCeilingWall.transform.parent = gbjBoudingBox.transform;

                //Return pivot to original location
                gbjBoudingBox.transform.Rotate(Vector3.up, -Angle);

                //Give Default Color and Name to the BoudingBox
                gbjFrontWall.name   = "Front";
                gbjRightWall.name   = "Right";
                gbjLeftWall.name    = "Left";
                gbjBackWall.name    = "Back";
                gbjFloorWall.name   = "Floor";
                gbjCeilingWall.name = "Ceiling";
                gbjFrontWall.GetComponent<MeshRenderer>().material.color   = new Color(0.952941f, 0.494117f, 0.474509f);
                gbjRightWall.GetComponent<MeshRenderer>().material.color   = new Color(0.952941f, 0.494117f, 0.474509f);
                gbjLeftWall.GetComponent<MeshRenderer>().material.color    = new Color(0.952941f, 0.494117f, 0.474509f);
                gbjBackWall.GetComponent<MeshRenderer>().material.color    = new Color(0.952941f, 0.494117f, 0.474509f);
                gbjFloorWall.GetComponent<MeshRenderer>().material.color   = new Color(0.733333f, 0.952941f, 0.474509f);
                gbjCeilingWall.GetComponent<MeshRenderer>().material.color = new Color(0.474509f, 0.592156f, 0.952931f);

                //Setup the bouding Box for Player Positon detection
                gbjFloorWall.layer = LayerMask.NameToLayer("SUFloors"); //The artificial 'Floor' from the bouding box will work like a regular floor to detect player position
                gbjFloorWall.AddComponent<BoxCollider>(); //Needs box collider to hit raycast
                
                //Create the player token in a suitable location
                if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SUFloors")))
                {
                    gbjPlayerPosition = Instantiate<GameObject>(PlayerToken);
                    gbjPlayerPosition.name = "PlayerPositionWorld";
                    gbjPlayerPosition.transform.position = hit.point + PlayerTokenOffset;
                    gbjPlayerPosition.transform.rotation = Quaternion.identity;
                    gbjPlayerPosition.transform.parent = gbjBoudingBox.transform;
                    gbjPlayerPosition.transform.localScale = MiniaturizedPlayerTokenScale;
                }

                //Find All the SU objects inside the bounding box
                //Create a box collider that encompasses the entire area of the bouding box
                BoxCollider boxCollider = gbjCeilingWall.AddComponent<BoxCollider>();
                float roomHeight = gbjSUCeilingUp.transform.position.y - gbjSUFloorDown.transform.position.y;
                boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y - (roomHeight / 2.0f), boxCollider.center.z);
                boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y + roomHeight, boxCollider.size.z);

                //We are going to find all the objects that are inside this box collider
                List<Tuple<Transform,GameObject>> ObjectsInsideBoudingBoxAndTheirParents = new List<Tuple<Transform,GameObject>>();
                foreach (Transform tsfParentHolder in manager.SceneRoot.transform)
                {
                    //If a flag for removing and object from the bouding box is on, dont add that object, even if its inside the box collider
                    if((RemoveSUWallsFromBoudingBox && tsfParentHolder.name == "Wall") || (RemoveSUInferredObjectsFromBoudingBox && tsfParentHolder.name == "CompletelyInferred") ||
                    (RemoveSUCeilingsFromBoudingBox && tsfParentHolder.name == "Ceiling") || (RemoveSUFloorsFromBoudingBox && tsfParentHolder.name == "Floor"))
                    {
                        continue;
                    }

                    foreach (Transform tsfSUGeometry in tsfParentHolder)
                    {
                        if(tsfSUGeometry.name.Contains("Quad"))
                        {
                            continue;
                        }

                        Mesh mesh = tsfSUGeometry.GetComponent<MeshFilter>().mesh;
                        bool bAllVerticesInside = true;
                        foreach (Vector3 vertex in mesh.vertices)
                        {
                            Vector3 worldPos = tsfSUGeometry.transform.TransformPoint(vertex);

                            if ((boxCollider.ClosestPoint(worldPos) - worldPos).sqrMagnitude > (AcceptedDistanceOutsideBoudingBox * AcceptedDistanceOutsideBoudingBox))
                            {
                                bAllVerticesInside = false;
                            }
                        }

                        //If all the vertices are inside the bounding box then the object is inside the bouding box
                        if (bAllVerticesInside)
                        {
                            //Remember it
                            ObjectsInsideBoudingBoxAndTheirParents.Add(new Tuple<Transform, GameObject>(tsfSUGeometry.parent, tsfSUGeometry.gameObject));

                            //And make it part of the bouding box
                            tsfSUGeometry.transform.parent = gbjBoudingBox.transform;
                        }

                    }
                }

                //Make the SU walls parent of the scene root again (Remove them from the pivot)
                gbjSUWallFront.transform.parent = tsfParentSUWallFront;
                gbjSUWallBack.transform.parent  = tsfParentSUWallBack;
                gbjSUWallLeft.transform.parent  = tsfParentSUWallLeft;
                gbjSUWallRight.transform.parent = tsfParentSUWallRight;
                gbjSUCeilingUp.transform.parent = tsfParentSUCeiling;
                gbjSUFloorDown.transform.parent = tsfParentSUFloor;


                //Now we create the miniature bouding box
                gbjBoudingBoxMini                      = Instantiate(gbjBoudingBox);
                gbjBoudingBoxMini.name                 = "BoudingBoxMini";
                gbjBoudingBoxMini.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                gbjBoudingBoxMini.transform.position   = Camera.main.transform.position + Camera.main.transform.forward + (Vector3.down * 0.15f);

                //Find the miniature player token in the mini bouding box
                foreach (Transform tfsBoudingBoxChild in gbjBoudingBoxMini.transform)
                {
                    if(tfsBoudingBoxChild.name == "PlayerPositionWorld")
                    {
                        gbjPlayerBoudingBoxPosition = tfsBoudingBoxChild.gameObject;
                    }
                }

                //After the mini Bouding Box is done you can anchor the objects inside the large bouding box back to scene root
                foreach(Tuple<Transform,GameObject> item in ObjectsInsideBoudingBoxAndTheirParents)
                {
                    item.Item2.GetComponent<MeshRenderer>().enabled = true;
                    item.Item2.transform.parent = item.Item1;
                }

                //Make the large bouding box invisible
                foreach (Transform tfsBoudingBoxChild in gbjBoudingBox.transform)
                {
                    tfsBoudingBoxChild.GetComponent<MeshRenderer>().enabled = false;
                }

                //Disable the scene root
                manager.SceneRoot.SetActive(false);

                //After all this we have 3 things
                // - A disabled Scene Root Left intact
                // - An Invisible bouding box, perfectly align to your real world
                // - A minaturized visible version of the larger bouding box for you to see
            }
        }

        private void BoundingBoxOff()
        {
            manager.SceneRoot.SetActive(true);
            Destroy(gbjBoudingBox);
            Destroy(gbjBoudingBoxMini);
        }


        //Pass it 4 points to create a wall mesh
        private GameObject CreateBoudingWall(Vector3 vc3LowerLeft, Vector3 vc3LowerRight, Vector3 vc3UpperRight, Vector3 vc3UpperLeft, bool flipWinding)
        {
            GameObject gbjReturn = new GameObject();

            List<Vector3> vertexList = new List<Vector3>() { vc3LowerLeft, vc3LowerRight, vc3UpperLeft, vc3UpperRight };
            List<Vector2> uvList = new List<Vector2>() { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1) };
            List<int> indexList = new List<int>() { 0, 2, 1, 1, 2, 3 };
            if (flipWinding)
            {
                indexList = new List<int>() { 0, 1, 2, 1, 3, 2 };
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertexList);
            mesh.SetIndices(indexList.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uvList);
            mesh.RecalculateNormals();

            gbjReturn.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = gbjReturn.AddComponent<MeshRenderer>();
            if(manager.SceneObjectRequestMode == Microsoft.MixedReality.SceneUnderstanding.Samples.Unity.RenderMode.Mesh)
            {
                mr.material = SUDefaultMaterial;
            }
            else
            {
                mr.material = SUWireFrameMaterial;
            }

            return gbjReturn;
        }


        //Every frame we get 4 walls 1 ceiling and 1 floor from the SU representation that are the candidates to help create the bouding box
        private void UpdateBoudingBoxWalls()
        {
            //Directions
            Vector3 foward = Vector3.Cross(Camera.main.transform.right, Vector3.up).normalized;
            Vector3 right = Camera.main.transform.right;
            Vector3 left = -right;
            Vector3 backwards = -foward;

            //Important information to remember
            float ClosestFrontWallDistance = Mathf.Infinity;
            GameObject gbjLocalClosestFrontWall = null;
            Vector3 vc3HitLocation = Vector3.zero;
            Vector3 vc3WallNormal = Vector3.zero;

            //Throw 5 'whiskers' or Rays foward from the Camera, use that information to locate walls and identify which is the best wall to consider 'Front Wall'
            Quaternion rot1 = Quaternion.AngleAxis(45.0f / 2.0f,  Vector3.up);
            Quaternion rot2 = Quaternion.AngleAxis(-45.0f / 2.0f, Vector3.up);
            EvaluateRay(Camera.main.transform.position,                  foward, ref ClosestFrontWallDistance, ref gbjLocalClosestFrontWall, Color.yellow, ref vc3HitLocation, ref vc3WallNormal);
            EvaluateRay(Camera.main.transform.position + (right * 0.5f), foward, ref ClosestFrontWallDistance, ref gbjLocalClosestFrontWall, Color.yellow, ref vc3HitLocation, ref vc3WallNormal);
            EvaluateRay(Camera.main.transform.position +  (left * 0.5f), foward, ref ClosestFrontWallDistance, ref gbjLocalClosestFrontWall, Color.yellow, ref vc3HitLocation, ref vc3WallNormal);
            EvaluateRay(Camera.main.transform.position,           rot1 * foward, ref ClosestFrontWallDistance, ref gbjLocalClosestFrontWall, Color.yellow, ref vc3HitLocation, ref vc3WallNormal);
            EvaluateRay(Camera.main.transform.position,           rot2 * foward, ref ClosestFrontWallDistance, ref gbjLocalClosestFrontWall, Color.yellow, ref vc3HitLocation, ref vc3WallNormal);

            if(gbjLocalClosestFrontWall != null)
            {
                //Update front wall
                gbjSUWallFront = gbjLocalClosestFrontWall;

                //From the information obtained from the selected front wall, locate it's correspoding left, right and back wall, relative to the front wall's normal and the user position
                //Echo locate right Wall
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Vector3.Cross(vc3WallNormal, Vector3.up), out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SUWalls")))
                {
                    gbjSUWallRight = hit.transform.gameObject;
                }
                else
                {
                    gbjSUWallRight = null;
                }

                //Echo locate left Wall
                if (Physics.Raycast(Camera.main.transform.position, Vector3.Cross(Vector3.up, vc3WallNormal), out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SUWalls")))
                {
                    gbjSUWallLeft = hit.transform.gameObject;
                }
                else
                {
                    gbjSUWallLeft = null;
                }

                //Echo locate Back Wall
                if (Physics.Raycast(Camera.main.transform.position, vc3WallNormal, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SUWalls")))
                {
                    gbjSUWallBack = hit.transform.gameObject;
                }
                else
                {
                    gbjSUWallBack = null;
                }

                //The 4 bouding walls have been located, we are only missing floor and ceiling to have a nice clean bouding box

                //Again we do the '101' bit mask and shift it to the 9 and 11 position
                //Depending on how you scan you may have scans where the ceiling of a room is the floor of another.
                //therefore we have situations in which the 'ceiling' on top of you is actually a floor, or the floor beneath your feet is actually a 'ceiling'
                int floorsAndCeilingMask = 5 << Mathf.Min(LayerMask.NameToLayer("SUFloors"), LayerMask.NameToLayer("SUCeilings"));
                if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out hit, Mathf.Infinity, floorsAndCeilingMask))
                {
                    gbjSUFloorDown = hit.transform.gameObject;
                }
                else
                {
                    gbjSUFloorDown = null;
                }

                if (Physics.Raycast(Camera.main.transform.position, Vector3.up, out hit, Mathf.Infinity, floorsAndCeilingMask))
                {
                    gbjSUCeilingUp = hit.transform.gameObject;
                }
                else
                {
                    gbjSUCeilingUp = null;
                }

            }

        }


        //This is for the foward wiskers in the raycast that locate the front wall
        private void EvaluateRay(in Vector3 vc3StartPos, in Vector3 vc3RayDir, ref float ClosestWallDistance, ref GameObject gbjLocalClosestWall, in Color clrDebugRay, ref Vector3 vc3HitLocation, ref Vector3 vc3WallNormal)
        {
            if (Physics.Raycast(vc3StartPos, vc3RayDir, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SUWalls")))
            {
                Vector3 DistanceToWallAsVector = (hit.point - vc3StartPos);
                Vector3 hitDirection = DistanceToWallAsVector.normalized;
                //Its possible to look at a wall from its front or back, we flip its normal to always look in your direction.
                Vector3 SUSurfaceNormal = Vector3.Dot(-hit.transform.forward, hitDirection) < 0.0f ? -hit.transform.forward : hit.transform.forward;

                //If the Wall is facing towards you (mildly)
                if (Vector3.Dot(hitDirection, SUSurfaceNormal) < -0.5f)
                {
                    if (DistanceToWallAsVector.sqrMagnitude < ClosestWallDistance)
                    {
                        gbjLocalClosestWall = hit.transform.gameObject;
                        ClosestWallDistance = DistanceToWallAsVector.sqrMagnitude;
                        vc3HitLocation = hit.point;
                        vc3WallNormal = SUSurfaceNormal;
                    }
                }
            }
        }

        //Callback for when the scene auto refreshes, The Scene Understanding Manager Component
        //has a callback for when the scene starts loading, this function is registered there.
        public void OnSceneLoadStarted()
        {
            //Ideally neither the DollHouse nor the BoudingBox is being displayed with AutoRefresh On
            //For the purposes of this demo, if the scene auto refeshes while the minimap or the boudingbox are on
            //Delete the minimap and the bouding box, and return to the normal SU view

            Destroy(gbjDollHouse);
            Destroy(gbjPlayerPosition);
            Destroy(gbjBoudingBox);
            Destroy(gbjBoudingBoxMini);

            manager.SceneRoot.SetActive(true);
            foreach (Transform ParentTransform in manager.SceneRoot.transform)
            {
                foreach (Transform suGeometry in ParentTransform)
                {
                    suGeometry.GetComponent<MeshRenderer>().enabled = true;
                }
            }
            isFloorPlanOn = false;
            isBoudingBoxOn = false;
        }

        public void SwitchToWireFrame()
        {
            manager.SceneObjectRequestMode = RenderMode.Wireframe;
            manager.DisplayDataAsync();
        }

        public void SwitchToDefault()
        {
            manager.SceneObjectRequestMode = RenderMode.Mesh;
            manager.DisplayDataAsync();
        }

        public void ToggleAutoRefresh()
        {
            manager.AutoRefresh = !manager.AutoRefresh;
            if (!manager.AutoRefresh)
            {
                manager.TimeElapsedSinceLastAutoRefresh = manager.AutoRefreshIntervalInSeconds;
            }
        }

        public void RefreshScene()
        {
            manager.DisplayDataAsync();
        }

    }

    

}