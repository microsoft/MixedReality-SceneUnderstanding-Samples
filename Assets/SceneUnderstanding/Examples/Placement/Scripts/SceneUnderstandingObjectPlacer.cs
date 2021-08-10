// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    /// <summary>
    /// This component contains the logic for generating holograms that interact
    /// with a Scene Understanding scene. it uses the built in physics system
    /// in unity (RigidBody Component)
    /// <summary>
    public class SceneUnderstandingObjectPlacer : MonoBehaviour
    {
        public GameObject objToPlaceRef;
        public Material material;
        public bool UseSUSDKPlacementAPI;
        public SceneUnderstandingManager manager;
        private GameObject objToPlace = null;
        private bool isPlacing = false;
        private readonly float placementScaleForSUSDKAPI = 0.125f;

        // Container for all instantiated objects/holograms
        private List<GameObject> holoObjects =  new List<GameObject>();

        private void StartPlacing()
        {
            objToPlace = Instantiate<GameObject>(objToPlaceRef, Vector3.zero, Quaternion.identity);

            // Add object to the list
            holoObjects.Add(objToPlace);

            // Disable colliders for base object and children if it has any
            RecursiveToggleColliders(objToPlace, false);
        }

        private void FinishPlacing()
        {
            // Enable colliders for base object and children if it has any
            RecursiveToggleColliders(objToPlace, true);

            objToPlace = null;
        }

        private void UpdateObjPos()
        {
            if(objToPlace == null)
            {
                return;
            }

            Vector3 newObjPosition = GetDesiredObjPos(UseSUSDKPlacementAPI);
            objToPlace.transform.position = newObjPosition;
        }

        private Vector3 GetDesiredObjPos(bool useCenterMostPlacementAPI = false)
        {
            RaycastHit hit;
            bool hasTarget = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit);

            Vector3 newObjPos = Vector3.zero;
            if(hasTarget)
            {
                if(useCenterMostPlacementAPI)
                {
                    // Use the SU SDK placement API to get a valid position
                    Vector3? result = GetPositionFromSUSDKAPI(hit.transform);

                    if(result != null)
                    {
                        Vector3 selectedObjFacingTowards = -hit.transform.forward.normalized;
                        newObjPos = Vector3.Dot(Camera.main.transform.TransformDirection(Vector3.forward), selectedObjFacingTowards) < 0 ? result.Value + (selectedObjFacingTowards * 0.3f) : result.Value - (selectedObjFacingTowards * 0.3f);
                    }
                    else
                    {
                        // If no object is being gazed at, then place the object infront of the camera.
                        newObjPos = Camera.main.transform.position + (Camera.main.transform.forward * 2.0f);
                    }
                }
                else
                {
                    // Use Unity to get a valid position
                    // Get a Position Slightly above of the object that the main camera is gazing at
                    Vector3 selectedObjFacingTowards = -hit.transform.forward.normalized;
                    newObjPos = Vector3.Dot(Camera.main.transform.TransformDirection(Vector3.forward), selectedObjFacingTowards) < 0 ? hit.point + (selectedObjFacingTowards * 0.3f) : hit.point - (selectedObjFacingTowards * 0.3f);
                }
            }
            else
            {
                // If no object is being gazed at, then place the object infront of the camera.
                newObjPos = Camera.main.transform.position + (Camera.main.transform.forward * 2.0f);
            }

            return newObjPos;
        }

        IEnumerator SprayCoroutine()
        {
            yield return null;

            for(int i=0; i<10; i++)
            {
                PrimitiveType pt = i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;

                // Init
                GameObject tempgbj = GameObject.CreatePrimitive(pt);
                tempgbj.transform.localScale = new Vector3(0.2f,0.2f,0.2f);
                tempgbj.GetComponent<MeshRenderer>().material = material;
                tempgbj.AddComponent<Rigidbody>();

                // Set Pos and add force
                tempgbj.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 1.0f);
                tempgbj.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 3.0f, ForceMode.Impulse);

                // Add primitive to list
                holoObjects.Add(tempgbj);

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void RecursiveToggleColliders(GameObject root, bool enableColliders = false)
        {
            if(root != null)
            {
                foreach(Collider col in root.GetComponents<Collider>())
                {
                    col.enabled = enableColliders;
                }

                foreach(Transform child in root.transform)
                {
                    RecursiveToggleColliders(child.gameObject, enableColliders);
                }
            }
        }

        private Vector3? GetPositionFromSUSDKAPI(Transform obj)
        {
            Scene suScene = manager.GetLatestDeserializedScene();
            if(suScene == null)
            {
                return null;
            }

            SceneUnderstandingProperties properties = obj.transform.parent.GetComponent<SceneUnderstandingProperties>();
            if(properties == null)
            {
                return null;
            }

            SceneObject suObject = (SceneObject) suScene.FindComponent(properties.suObjectGUID);
            if(suObject == null)
            {
                return null;
            }

            // Before starting to find the proper location, calculate
            // the volume of the object we want to place
            Mesh mesh = objToPlace.GetComponent<MeshFilter>().mesh;
            System.Numerics.Vector2 objectExtent = new System.Numerics.Vector2(mesh.bounds.size.x, mesh.bounds.size.z) * placementScaleForSUSDKAPI;

            //Grab the largest quad, it's very likely that only one quad exists
            //but let's be thorough
            SceneQuad largestQuad = null;
            float fLargestArea = float.NegativeInfinity;
            foreach (SceneQuad quad in suObject.Quads)
            {
                System.Numerics.Vector2 extents = quad.Extents;
                float area = extents.X * extents.Y;

                if (area > fLargestArea)
                {
                    fLargestArea = area;
                    largestQuad = quad;
                }
            }

            if (largestQuad == null)
            {
                //No largest Quad could be calculated
                return null;
            }

            //Use the 'FindCenterMostPlacement' API from the SU SDK to calculate a location on the surface
            System.Numerics.Vector2 result;
            if (largestQuad.FindCentermostPlacement(objectExtent, out result))
            {
                //Now we have the CenterMostPlacement for this Quad Object, but the Coordinates from the 'FindCenterMostPlacement'
                //API align to 'Quad Space' where the origin is in the top left of the Quad. 

                //Create a pivot gameobject, we need to start moving in local space relative to the coordinates of the surface
                //it's easier to anchor a child gameobject to the origin and then move this gameobject to the desired location
                //since it already starts in the local origin of the surface
                GameObject location = new GameObject("CenterOfPlatform");
                location.transform.parent = obj.transform;
                location.transform.localPosition = Vector3.zero;
                location.transform.localRotation = Quaternion.identity;

                //Our pivot gameobject starts in the middle of the platform, that's the local origin in Unity, but we need to move it to the top left 
                //corner of the object. 'FindCenterMostPlacement' coordinates work in 'Quad Space'
                //they start at the top left of the Quad, and go down and right correspondingly in X and Y axis

                //Move to the starting position aka. Top Left Corner
                location.transform.Translate(-(largestQuad.Extents.X / 2.0f), largestQuad.Extents.Y / 2.0f, 0.0f, Space.Self);

                //Move to the CenterMost Place -- Y is negate so that we move right and down, according to the API 'FindCenterMostPlacement'
                //https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/scene-understanding-SDK#quad
                location.transform.Translate(result.X, -result.Y, 0.0f, Space.Self);

                //Store the postion for plaement
                Vector3 resultPos = location.transform.position;

                //We can delete the pivot now, we don't need it anymore
                Destroy(location);

                return resultPos;
            }
            else
            {
                //Object is to small to place this object
                return null;
            }
        }

        public void FreezeHolograms()
        {
            // When the scene starts loading, freeze all holograms in place to avoid them falling on an empty scene
            foreach(GameObject obj in holoObjects)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();

                if(rb == null)
                {
                    continue;
                }

                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        public void UnfreezeHolograms()
        {
            //When the scene finishes loading, unfreeze all holograms
            foreach(GameObject obj in holoObjects)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();

                if(rb == null)
                {
                    continue;
                }

                rb.constraints = RigidbodyConstraints.None;
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            UpdateObjPos();
        }

        //This function is defined as an Input Action, in the Scene Understanding Menu
        // Input Manager component
        public void Place()
        {
            if (!isPlacing)
            {
                StartPlacing();
            }
            else
            {
                FinishPlacing();
            }
            isPlacing = !isPlacing;
        }

        //This function is defined as an Input Action, in the Scene Understanding Menu
        // Input Manager component
        public void Spray()
        {
            StartCoroutine(SprayCoroutine());
        }

        public void TogglePlacementType()
        {
            UseSUSDKPlacementAPI = !UseSUSDKPlacementAPI;
        }
    }
}
