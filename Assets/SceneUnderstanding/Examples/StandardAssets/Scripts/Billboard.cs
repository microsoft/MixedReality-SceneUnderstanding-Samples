// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEngine;

    public enum PivotAxis
    {
        // Most common option.
        XY,
        // Rotate about an individual axis.
        Y,
        X,
        Z,
        // Rotate about a pair of axes.
        XZ,
        YZ,
        // Rotate about all axes.
        Free,
        //Keep the object static
        None
    }
    


    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        [Tooltip("The axis about which the object will rotate.")]
        public PivotAxis PivotAxis = PivotAxis.XY;

        /// <summary>
        /// The target we will orient to. If no target is specified, the main camera will be used.
        /// </summary>
        [Tooltip("The target we will orient to. If no target is specified, the main camera will be used.")]
        public Transform TargetTransform = null;
       
        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            TargetTransform = TargetTransform == null ? Camera.main.transform : TargetTransform;
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void LateUpdate()
        {
            if(PivotAxis == PivotAxis.None)
            {
                return;
            }

            // Get a Vector that points from the target to the main camera.
            Vector3 directionToTarget = TargetTransform.position - transform.position;

            bool useCameraAsUpVector = true;

            // Adjust for the pivot axis.
            switch (PivotAxis)
            {
                case PivotAxis.X:
                    directionToTarget.x = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Y:
                    directionToTarget.y = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Z:
                    directionToTarget.x = 0.0f;
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.XY:
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.XZ:
                    directionToTarget.x = 0.0f;
                    break;

                case PivotAxis.YZ:
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.Free:
                default:
                    // No changes needed.
                    break;
            }

            // If we are right next to the camera the rotation is undefined. 
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            // Calculate and apply the rotation required to reorient the object.
            if (useCameraAsUpVector)
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget, Camera.main.transform.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget);
            }
        }
    }
}