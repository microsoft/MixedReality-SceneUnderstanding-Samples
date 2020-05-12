// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// When running on device (HoloLens 2), this class interacts with the Scene Understanding runtime and keeps on retrieving the latest scene data.
    /// When running on PC, this class reads a passed in serialized scene and serves it as the latest scene.
    /// </summary>
    public class SceneUnderstandingDataProvider : MonoBehaviour
    {
        /// <summary>
        /// When enabled, the device path will get exercised. Otherwise, previously saved, serialized scenes will be loaded and served.
        /// </summary>
        [Tooltip("When enabled, the device path will get exercised. Otherwise, previously saved, serialized scenes will be loaded and served.")]
        public bool RunOnDevice = true;

        /// <summary>
        /// The scene to load when not running on the device.
        /// </summary>
        [Tooltip("The scene to load when not running on the device.")]
        public TextAsset SUSerializedScenePath = null;

        /// <summary>
        /// Radius of the sphere around the camera, which is used to query the environment.
        /// </summary>
        [Tooltip("Radius of the sphere around the camera, which is used to query the environment.")]
        public float BoundingSphereRadiusInMeters = 10.0f;

        /// <summary>
        /// When enabled, requests observed and inferred regions for scene objects.
        /// When disabled, requests only the observed regions for scene objects.
        /// </summary>
        [Tooltip("When enabled, requests observed and inferred regions for scene objects. When disabled, requests only the observed regions for scene objects.")]
        public bool RequestInferredRegions = true;

        /// <summary>
        /// Mesh LOD to request.
        /// </summary>
        internal SceneUnderstanding.SceneMeshLevelOfDetail WorldMeshLOD = SceneUnderstanding.SceneMeshLevelOfDetail.Coarse;

        internal readonly float _minBoundingSphereRadiusInMeters = 5f;
        internal readonly float _maxBoundingSphereRadiusInMeters = 100f;

        private byte[] _latestSerializedScene = null;
        private readonly object _latestSerializedSceneLock = new object();
        private Guid _latestSceneGuid;
        
        /// <summary>
        /// Gets the latest scene from the Scene Understanding runtime when running on device. In the PC case, returns the serialized scene as a byte array.
        /// </summary>
        /// <returns>A tuple composed of a guid and the serialized scene buffer.</returns>
        public Tuple<Guid, byte[]> GetLatestSerializedScene()
        {
            byte[] sceneToReturn = null;
            Guid sceneGuidToReturn;

            lock (_latestSerializedSceneLock)
            {
                if (_latestSerializedScene != null)
                {
                    sceneToReturn = new byte[_latestSerializedScene.Length];
                    Array.Copy(_latestSerializedScene, sceneToReturn, _latestSerializedScene.Length);

                    sceneGuidToReturn = _latestSceneGuid;
                }
            }

            return Tuple.Create<Guid, byte[]>(sceneGuidToReturn, sceneToReturn);
        }

        /// <summary>
        /// Gets the guid of the latest scene.
        /// </summary>
        /// <returns>Guid of the latest scene.</returns>
        public Guid GetLatestSceneGuid()
        {
            Guid sceneGuidToReturn;
            lock (_latestSerializedSceneLock)
            {
                sceneGuidToReturn = _latestSceneGuid;
            }
            return sceneGuidToReturn;
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        private async void Start()
        {
            if (RunOnDevice)
            {
                if (Application.isEditor)
                {
                    Logger.LogWarning("SceneUnderstandingDataProvider.Start: Running in editor with the RunOnDevice mode set is not supported.");
                }

                if (SceneUnderstanding.SceneObserver.IsSupported())
                {
                    var access = await SceneUnderstanding.SceneObserver.RequestAccessAsync();
                    if (access != SceneObserverAccessStatus.Allowed)
                    {
                        Logger.LogError("SceneUnderstandingDataProvider.Start: Access to Scene Understanding has been denied. Reason: " + access);
                        return;
                    }

                    // Then, spin off a background thread to continually retrieve SU data.
                    Task.Run(() => RetrieveDataContinuously());
                }
                else
                {
                    Logger.LogError("SceneUnderstandingDataProvider.Start: Scene Understanding not supported.");
                    return;
                }
            }
            else
            {
                if (SUSerializedScenePath != null)
                {
                    lock (_latestSerializedSceneLock)
                    {
                        _latestSerializedScene = SUSerializedScenePath.bytes;
                        _latestSceneGuid = Guid.NewGuid();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves Scene Understanding data continuously from the runtime.
        /// </summary>
        private void RetrieveDataContinuously()
        {
            // At the beginning, retrieve only the observed scene object meshes.
            RetrieveData(BoundingSphereRadiusInMeters, false, true, false, false, SceneUnderstanding.SceneMeshLevelOfDetail.Coarse);

            while (true)
            {
                // Always request quads, meshes and the world mesh. SceneUnderstandingDisplayManager will take care of rendering only what the user has asked for.
                RetrieveData(BoundingSphereRadiusInMeters, true, true, RequestInferredRegions, true, WorldMeshLOD);
            }
        }

        /// <summary>
        /// Calls into the Scene Understanding APIs, to retrieve the latest scene as a byte array.
        /// </summary>
        /// <param name="enableQuads">When enabled, quad representation of scene objects is retrieved.</param>
        /// <param name="enableMeshes">When enabled, mesh representation of scene objects is retrieved.</param>
        /// <param name="enableInference">When enabled, both observed and inferred scene objects are retrieved. Otherwise, only observed scene objects are retrieved.</param>
        /// <param name="enableWorldMesh">When enabled, retrieves the world mesh.</param>
        /// <param name="lod">If world mesh is enabled, lod controls the resolution of the mesh returned.</param>
        private void RetrieveData(float boundingSphereRadiusInMeters, bool enableQuads, bool enableMeshes, bool enableInference, bool enableWorldMesh, SceneUnderstanding.SceneMeshLevelOfDetail lod)
        {
            Logger.Log("SceneUnderstandingDataProvider.RetrieveData: Started.");
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                SceneUnderstanding.SceneQuerySettings querySettings;
                querySettings.EnableSceneObjectQuads = enableQuads;
                querySettings.EnableSceneObjectMeshes = enableMeshes;
                querySettings.EnableOnlyObservedSceneObjects = !enableInference;
                querySettings.EnableWorldMesh = enableWorldMesh;
                querySettings.RequestedMeshLevelOfDetail = lod;

                // Ensure that the bounding radius is within the min/max range.
                boundingSphereRadiusInMeters = Mathf.Clamp(boundingSphereRadiusInMeters, _minBoundingSphereRadiusInMeters, _maxBoundingSphereRadiusInMeters);
                
                var serializedScene = SceneUnderstanding.SceneObserver.ComputeSerializedAsync(querySettings, boundingSphereRadiusInMeters).GetAwaiter().GetResult();
                lock(_latestSerializedSceneLock)
                {
                    _latestSerializedScene = new byte[serializedScene.Size];
                    serializedScene.GetData(_latestSerializedScene);
                    _latestSceneGuid = Guid.NewGuid();
                }
            }
            catch(Exception e)
            {
                Logger.LogException(e);
            }

            stopwatch.Stop();
            Logger.Log(string.Format("SceneUnderstandingDataProvider.RetrieveData: Completed. Radius: {0}; Quads: {1}; Meshes: {2}; Inference: {3}; WorldMesh: {4}; LOD: {5}; Bytes: {6}; Time (secs): {7};",
                            boundingSphereRadiusInMeters,
                            enableQuads,
                            enableMeshes,
                            enableInference,
                            enableWorldMesh,
                            lod,
                            (_latestSerializedScene == null ? 0 : _latestSerializedScene.Length),
                            stopwatch.Elapsed.TotalSeconds));
        }

    }
}