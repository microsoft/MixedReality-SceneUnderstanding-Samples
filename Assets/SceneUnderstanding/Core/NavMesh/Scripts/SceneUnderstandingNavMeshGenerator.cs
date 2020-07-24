// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEngine;
    using UnityEngine.AI;

    enum AreaType
    {
        Walkable,
        NotWalkable
    }

    /// <summary>
    /// This Script will generate a NavMesh in a Scene Understanding Scene already generated inside unity
    /// using the unity built in NavMesh engine
    /// </summary>
    public class SceneUnderstandingNavMeshGenerator : MonoBehaviour
    {
        // Nav Mesh Surface is a Unity Built in Type, from 
        // the unity NavMesh Assets
        public NavMeshSurface navMeshSurf;
        public GameObject sceneRoot;
        public GameObject navMeshAgentRef;
        private GameObject navMeshAgentInstance;

        // This function runs as a callback for the OnLoadFinished event
        // In the SceneUnderstandingManager Component
        public void BakeMesh()
        {
            UpdateNavMeshSettingsForObjsUnderRoot();
            navMeshSurf.BuildNavMesh();
            CreateNavMeshAgent();
        }

        void CreateNavMeshAgent()
        {
            if(navMeshAgentRef == null)
            {
                return;
            }

            if(navMeshAgentInstance == null)
            {
                navMeshAgentInstance = Instantiate<GameObject>(navMeshAgentRef, new Vector3(0.0f,-0.81f,-3.0f), Quaternion.identity);
            }
        }

        void UpdateNavMeshSettingsForObjsUnderRoot ()
        {
            // Iterate all the Scene Objects
            foreach(Transform sceneObjContainer in sceneRoot.transform)
            {
                foreach(Transform sceneObj in sceneObjContainer.transform)
                {
                    NavMeshModifier nvm = sceneObj.gameObject.AddComponent<NavMeshModifier>();

                    // Walkable = 0, Not Walkable = 1
                    // This area types are unity predefined, in the unity inspector in the navigation tab go to areas
                    // to see them
                    nvm.overrideArea = true;
                    nvm.area         = sceneObj.parent.GetComponent<SceneUnderstandingProperties>().suKind == SceneUnderstanding.SceneObjectKind.Floor ? 
                    (int) AreaType.Walkable : (int) AreaType.NotWalkable;
                }
            }
        }
    }
}
