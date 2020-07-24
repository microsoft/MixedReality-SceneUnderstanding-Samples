// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// This Script defines the logic a NavMesh agent in a unity navmesh
    /// </summary>
    public class SceneUnderstandingPathFindingController : MonoBehaviour
    {
        //Raycast used to determine where the nav mesh agent will move
        private RaycastHit raycastHit;

        //Reference to the NavMeshAgent
        private GameObject gbjNavMeshAgent;

        // The agent will move wherever the main camera is gazing at.
        public void MoveAgent()
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out raycastHit, Mathf.Infinity))
            {
                gbjNavMeshAgent = GameObject.FindGameObjectWithTag("NavAgent");
                gbjNavMeshAgent.GetComponent<NavMeshAgent>().SetDestination(raycastHit.point);
            }
        }
    }
}
