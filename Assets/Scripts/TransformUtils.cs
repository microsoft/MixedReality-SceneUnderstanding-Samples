// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System;
    using UnityEngine;
#if WINDOWS_UWP
    using Windows.Perception.Spatial;
#endif

    /// <summary>
    /// Provides helper methods for transformation related operations.
    /// </summary>
    public static class TransformUtils
    {
        /// <summary>
        /// Retrieves the translation, rotation and scale components from a 4x4 transformation matrix.
        /// </summary>
        /// <param name="transformationMatrix">4x4 transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <param name="rotation">Rotation quaternion.</param>
        /// <param name="scale">Scale vector.</param>
        public static void GetTRSFromMatrix4x4(System.Numerics.Matrix4x4 transformationMatrix, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            System.Numerics.Vector3 t;
            System.Numerics.Vector3 s;
            System.Numerics.Quaternion r;

            System.Numerics.Matrix4x4.Decompose(transformationMatrix, out s, out r, out t);

            translation = new Vector3(t.X, t.Y, t.Z);
            rotation = new Quaternion(r.X, r.Y, r.Z, r.W);
            scale = new Vector3(s.X, s.Y, s.Z);
        }

        /// <summary>
        /// Takes in a transformation matrix and assigns it to a Unity transform.
        /// </summary>
        /// <param name="transformationMatrix">Transformation matrix.</param>
        /// <param name="unityTransform">Unity transform.</param>
        /// <param name="updateLocalTransformOnly">Whether to update local or absolute transform.</param>
        public static void SetUnityTransformFromMatrix4x4(System.Numerics.Matrix4x4 transformationMatrix, Transform unityTransform, bool updateLocalTransformOnly = false)
        {
            if (unityTransform == null)
            {
                Logger.LogWarning("TransformUtils.SetUnityTransformFromMatrix4x4: Unity transform is null.");
                return;
            }

            Vector3 t;
            Quaternion r;
            Vector3 s;

            GetTRSFromMatrix4x4(transformationMatrix, out t, out r, out s);

            // NOTE: Scale is being ignored.
            if (updateLocalTransformOnly)
            {
                unityTransform.localPosition = t;
                unityTransform.localRotation = r;
            }
            else
            {
                unityTransform.SetPositionAndRotation(t, r);
            }
        }

        /// <summary>
        /// Converts a transformation matrix from right handed (+x is right, +y is up, +z is back) to left handed (+x is right, +y is up, +z is front).
        /// </summary>
        /// <param name="transformationMatrix">Right-handed transformation matrix to convert.</param>
        /// <returns>Converted left-handed matrix.</returns>
        public static System.Numerics.Matrix4x4 ConvertRightHandedMatrix4x4ToLeftHanded(System.Numerics.Matrix4x4 transformationMatrix)
        {
            transformationMatrix.M13 = -transformationMatrix.M13;
            transformationMatrix.M23 = -transformationMatrix.M23;
            transformationMatrix.M43 = -transformationMatrix.M43;

            transformationMatrix.M31 = -transformationMatrix.M31;
            transformationMatrix.M32 = -transformationMatrix.M32;
            transformationMatrix.M34 = -transformationMatrix.M34;

            return transformationMatrix;
        }

        /// <summary>
        /// Transforms the list of vertices, using the passed in transformationMatrix.
        /// </summary>
        /// <param name="transformationMatrix">Transformation matrix to apply.</param>
        /// <param name="vertices">List of vertices to transform.</param>
        public static void TransformVertices(System.Numerics.Matrix4x4 transformationMatrix, System.Numerics.Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = System.Numerics.Vector3.Transform(vertices[i], transformationMatrix);
            }
        }



        /// <summary>
        /// Takes in a spatial node id and returns the transformation matrix that specifies the transformation from the spatial node to the Unity world.
        /// </summary>
        /// <param name="nodeId">Id of the spatial node.</param>
        /// <param name="runOnDevice">True if the application is running on a hololens device</param>
        /// <returns>Transformation matrix from the spatial node to the Unity world.</returns>
        public static System.Numerics.Matrix4x4? GetSceneToUnityTransform(Guid nodeId, bool runOnDevice)
        {
            System.Numerics.Matrix4x4? sceneToUnityTransform = System.Numerics.Matrix4x4.Identity;

#if WINDOWS_UWP
            // Only get the spatial coordinate if we are running on device
            if (runOnDevice)
            {
                Logger.Log("TransformUtils.GetSceneToUnityTransform: About to create a coordinate system for node id: " + nodeId);
                SpatialCoordinateSystem sceneSpatialCoordinateSystem = Windows.Perception.Spatial.Preview.SpatialGraphInteropPreview.CreateCoordinateSystemForNode(nodeId);
                SpatialCoordinateSystem unitySpatialCoordinateSystem = (SpatialCoordinateSystem)System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(
                    UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr());

                sceneToUnityTransform = sceneSpatialCoordinateSystem.TryGetTransformTo(unitySpatialCoordinateSystem);

                if (sceneToUnityTransform != null)
                {
                    sceneToUnityTransform = TransformUtils.ConvertRightHandedMatrix4x4ToLeftHanded(sceneToUnityTransform.Value);
                }
                else
                {
                    Logger.LogWarning("TransformUtils.GetSceneToUnityTransform: Scene to Unity transform is null. Not good.");
                }
            }
#endif
            return sceneToUnityTransform;
        }
    }
}
