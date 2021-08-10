// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;

    public class SUMarkerPlacer : MonoBehaviour
    {
        // Nav Mesh Surface is a Unity Built in Type, from 
        // the unity NavMesh Assets
        //There is no way to modify a NavMeshSurface programatically
        //as a work around we setup 3 different surfaces from the inspector
        public NavMeshSurface navMeshSurf;

        public GameObject sceneRootAsMeshes;
        public SceneUnderstandingManager manager;
        public Material SUDefaultMaterial;
        public GameObject floorMarkerPrefab;

        //Important Objects to remember
        private bool hasValidNavMeshData = false;
        private List<GameObject> FloorMarkers = new List<GameObject>();
        private List<GameObject> GroupParentHolderGbj = new List<GameObject>();
        private List<List<GameObject>> TriangleGroups = new List<List<GameObject>>();

        #region LogicForFloorMarker

        public void FindLocationForFloorMarker()
        {
            if(hasValidNavMeshData)
            {
                Debug.Log("SUMarkerPlacer.FindLocationForFloorMarker: NavMesh Data can only be baked once, valid data has already been created");
                return;
            }

            NavMeshTriangulation? UnityNavMeshData = CreateUnityNavMesh();
            if(!UnityNavMeshData.HasValue)
            {
                Debug.Log("SUMarkerPlacer.FindLocationForFloorMarker: No valid location could be found");
                return;
            }

            List<GameObject> Triangles = CreateMeshAsGameObject(UnityNavMeshData.Value.vertices, UnityNavMeshData.Value.indices);
            TriangleGroups = GroupTriangles(Triangles);
            PlaceFloorMarker(TriangleGroups);
            hasValidNavMeshData = true;
        }

        NavMeshTriangulation? CreateUnityNavMesh()
        {
            UpdateNavMeshSettingsForObjsUnderRoot();

            navMeshSurf.BuildNavMesh();
            NavMeshTriangulation triangulationData = NavMesh.CalculateTriangulation();
            if (triangulationData.vertices.Length > 0 && triangulationData.indices.Length > 0)
            {
                return triangulationData;
            }

            Debug.Log("SUMarkerPlacer.CreateUnityNavMesh -> No data, Scene might be to crowded, small or narrow");
            return null;
        }

        void UpdateNavMeshSettingsForObjsUnderRoot()
        {
            // Iterate all the Scene Objects
            foreach (Transform sceneObjContainer in sceneRootAsMeshes.transform)
            {
                foreach (Transform sceneObj in sceneObjContainer.transform)
                {
                    NavMeshModifier nvm = sceneObj.gameObject.AddComponent<NavMeshModifier>();

                    // Walkable = 0, Not Walkable = 1
                    // This area types are unity predefined, in the unity inspector in the navigation tab go to areas
                    // to see them
                    nvm.overrideArea = true;
                    nvm.area = sceneObj.parent.name == "Floor" ? (int)AreaType.Walkable : (int)AreaType.NotWalkable;
                }
            }
        }

        List<GameObject> CreateMeshAsGameObject(Vector3[] vert, int[] indic)
        {
            int AreaCounter = 0;
            List<GameObject> Triangles = new List<GameObject>();
            for (int i = 0; i < indic.Length; i += 3)
            {
                Vector3[] newVerts = new Vector3[3];
                for (int n = 0; n < 3; n++)
                {
                    int index = indic[i + n];
                    newVerts[n] = vert[index];
                }

                int[] newIndic = new int[3] { 0, 1, 2 };

                Mesh NavMeshMesh = GenerateUnityMeshFromVertices(newVerts, newIndic);
                GameObject gbjNavMesh = new GameObject("PlacementTriangleArea " + AreaCounter++);
                gbjNavMesh.AddComponent<MeshFilter>().sharedMesh = NavMeshMesh;
                MeshRenderer mr = gbjNavMesh.AddComponent<MeshRenderer>();
                mr.sharedMaterial = new Material(SUDefaultMaterial);
                mr.sharedMaterial.color = Color.cyan;

                if (manager.IsInGhostMode)
                    mr.enabled = false;

                Triangles.Add(gbjNavMesh);
            }

            return Triangles;
        }

        private Mesh GenerateUnityMeshFromVertices(Vector3[] vert, int[] indic)
        {
            Mesh unityMesh = new Mesh();
            List<Vector3> vecList = new List<Vector3>(vert);
            unityMesh.SetVertices(vecList);
            unityMesh.SetIndices(indic, MeshTopology.Triangles, 0);
            unityMesh.RecalculateNormals();
            return unityMesh;
        }

        private List<List<GameObject>> GroupTriangles(List<GameObject> triangles)
        {
            List<List<GameObject>> groupsToReturn = new List<List<GameObject>>();

            //Breath First Search
            Dictionary<GameObject, List<GameObject>> graph = GetNeighboursGraph(triangles);
            HashSet<GameObject> visited = new HashSet<GameObject>();
            foreach(GameObject triangle in triangles)
            {
                if (visited.Contains(triangle))
                    continue;

                //Setup BFS
                Queue<GameObject> q = new Queue<GameObject>();
                q.Enqueue(triangle);

                //Setup a new Group
                List<GameObject> newGroup = new List<GameObject>();

                //BFS
                while(q.Count > 0)
                {
                    GameObject current = q.Dequeue();
                    visited.Add(current);
                    newGroup.Add(current);

                    List<GameObject> neighbours = graph[current];
                    foreach(GameObject n in neighbours)
                    {
                        if(!q.Contains(n) && !visited.Contains(n))
                        {
                            q.Enqueue(n);
                        }
                    }
                }

                //BFS for this triangle is finished, Add all triangles found in this traversal
                //as one group to the list of groups
                groupsToReturn.Add(newGroup);
            }
            
            //Anchor all groups to a parent GameObject
            int GroupCounter = 0;
            foreach (List<GameObject> group in groupsToReturn)
            {
                GameObject gbjParentHolder = new GameObject("TriangleGroup " + GroupCounter++);
                foreach (GameObject groupTriangle in group)
                {
                    groupTriangle.transform.SetParent(gbjParentHolder.transform);
                }

                GroupParentHolderGbj.Add(gbjParentHolder);
            }

            return groupsToReturn;
        }

        private Dictionary<GameObject, List<GameObject>> GetNeighboursGraph(List<GameObject> triangles)
        {
            Dictionary<GameObject, List<GameObject>> graphToReturn = new Dictionary<GameObject, List<GameObject>>();
            foreach(GameObject current in triangles)
            {
                foreach(GameObject possibleNeigbour in triangles)
                {
                    //You can't be your own neigbour
                    if (current != possibleNeigbour)
                    {
                        if(VerticesMatch(current, possibleNeigbour))
                        {
                            if (graphToReturn.ContainsKey(current))
                            {
                                graphToReturn[current].Add(possibleNeigbour);
                            }
                            else
                            {
                                graphToReturn.Add(current, new List<GameObject>() { possibleNeigbour });
                            }
                        }
                    }
                }
            }

            return graphToReturn;
        }

        private bool VerticesMatch(GameObject t1, GameObject t2)
        {
            Mesh m1 = t1.GetComponent<MeshFilter>().mesh;
            Mesh m2 = t2.GetComponent<MeshFilter>().mesh;

            if (m1.vertices[0] == m2.vertices[0] || m1.vertices[0] == m2.vertices[1] || m1.vertices[0] == m2.vertices[2])
            {
                return true;
            }

            if (m1.vertices[1] == m2.vertices[0] || m1.vertices[1] == m2.vertices[1] || m1.vertices[1] == m2.vertices[2])
            {
                return true;
            }

            if (m1.vertices[2] == m2.vertices[0] || m1.vertices[2] == m2.vertices[1] || m1.vertices[2] == m2.vertices[2])
            {
                return true;
            }

            return false;
        }

        private void PlaceFloorMarker(List<List<GameObject>> trianglesGroups)
        {
            foreach (List<GameObject> group in trianglesGroups)
            {
                //For each group find the largest triangle and the centroid
                //of the group

                //LargestTriangle Data
                GameObject LargestTriangle = null;
                float LargestArea = 0.0f;
                Vector3 LargestTriangleCentroid = Vector3.zero;

                //Centroid of the Group
                Vector3 GroupCentroid = Vector3.zero;

                foreach (GameObject triangle in group)
                {
                    Mesh mesh = triangle.GetComponent<MeshFilter>().mesh;
                    
                    //Get this triangles centroid, and update the group centroid
                    Vector3 currentTriangleCentroid = GetCentroidOfTriangle(mesh);
                    GroupCentroid += currentTriangleCentroid;

                    float Area = GetTriangleArea(mesh.vertices[0], mesh.vertices[1], mesh.vertices[2]);

                    //Update Largest Triangle
                    if (Area > LargestArea)
                    {
                        LargestArea = Area;
                        LargestTriangle = triangle;
                        LargestTriangleCentroid = currentTriangleCentroid;
                    }
                }

                LargestTriangle.GetComponent<MeshRenderer>().material.color = Color.yellow;

                GroupCentroid /= group.Count;

                GameObject TriangleHeuristicMarker = Instantiate<GameObject>(floorMarkerPrefab, new Vector3(LargestTriangleCentroid.x, LargestTriangleCentroid.y, LargestTriangleCentroid.z) + (Vector3.up * 0.05f) , Quaternion.identity);
                TriangleHeuristicMarker.transform.SetParent(group[0].transform.parent);
                TriangleHeuristicMarker.name = "Triangle Heuristic Marker";
                FloorMarkers.Add(TriangleHeuristicMarker);

                GameObject MeshCentroidHeuristicMarker = Instantiate<GameObject>(floorMarkerPrefab, new Vector3(GroupCentroid.x, GroupCentroid.y, GroupCentroid.z) + (Vector3.up * 0.05f), Quaternion.identity);
                MeshCentroidHeuristicMarker.transform.SetParent(group[0].transform.parent);
                MeshCentroidHeuristicMarker.name = "Mesh Centroid Heuristic Marker";
                FloorMarkers.Add(MeshCentroidHeuristicMarker);

            }
        }

        private Vector3 GetCentroidOfTriangle(Mesh TriangleMesh)
        {
            float TriangleCenterX = (TriangleMesh.vertices[0].x + TriangleMesh.vertices[1].x + TriangleMesh.vertices[2].x) / 3.0f;
            float TriangleCenterY = (TriangleMesh.vertices[0].y + TriangleMesh.vertices[1].y + TriangleMesh.vertices[2].y) / 3.0f;
            float TriangleCenterZ = (TriangleMesh.vertices[0].z + TriangleMesh.vertices[1].z + TriangleMesh.vertices[2].z) / 3.0f;

            return new Vector3(TriangleCenterX, TriangleCenterY, TriangleCenterZ);
        }

        private float GetTriangleArea(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            //Ignore Y axis, Triangles exists in the X-Z plane
            return Mathf.Abs((v0.x * (v0.z - v2.z) + v1.x * (v2.z - v0.z) + v2.x * (v0.z - v1.z)) / 2.0f);

        }

        #endregion

        #region VoiceCommands

        public void ToggleGhostMode()
        {
            manager.IsInGhostMode = !manager.IsInGhostMode;
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

        #endregion

    }
}
