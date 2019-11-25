// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using SceneUnderstanding = Microsoft.MixedReality.SceneUnderstanding;

    /// <summary>
    /// Different rendering modes available for scene objects.
    /// </summary>
    public enum VisualizationMode
    {
        Quad,
        QuadWithMask,
        Mesh,
        Wireframe
    }

    /// <summary>
    /// Controls the displaying of scene objects. The various options are enabled/disabled by the InputManager component.
    /// </summary>
    [RequireComponent(typeof(SceneUnderstandingDataProvider))]
    [RequireComponent(typeof(SceneUnderstandingUtils))]
    public class SceneUnderstandingDisplayManager : MonoBehaviour
    {
        /// <summary>
        /// Scene Understanding data provider component.
        /// </summary>
        [Tooltip("Scene Understanding data provider component.")]
        public SceneUnderstandingDataProvider SUDataProvider = null;

        /// <summary>
        /// Scene Understanding utilities component.
        /// </summary>
        [Tooltip("Scene Understanding utilities component.")]
        public SceneUnderstandingUtils SUUtils = null;

        /// <summary>
        /// GameObject that will be the parent of all Scene Understanding related game objects.
        /// </summary>
        [Tooltip("GameObject that will be the parent of all Scene Understanding related game objects.")]
        public GameObject SceneRoot;

        /// <summary>
        /// Material for scene object meshes.
        /// </summary>
        [Tooltip("Material for scene object meshes.")]
        public Material SceneObjectMeshMaterial;

        /// <summary>
        /// Material for scene object quads.
        /// </summary>
        [Tooltip("Material for scene object quads.")]
        public Material SceneObjectQuadMaterial;

        /// <summary>
        /// Material for scene object mesh wireframes.
        /// </summary>
        [Tooltip("Material for scene object mesh wireframes.")]
        public Material SceneObjectWireframeMaterial;

        /// <summary>
        /// Material for world mesh.
        /// </summary>
        [Tooltip("Material for world mesh.")]
        public Material WorldMeshMaterial;

        /// <summary>
        /// Display text labels for the scene objects.
        /// </summary>
        [Tooltip("Display text labels for the scene objects.")]
        public bool DisplayTextLabels = true;

        /// <summary>
        /// Font to use for text labels.
        /// </summary>
        [Tooltip("Font to use for text labels.")]
        public Font LabelFont;

        /// <summary>
        /// Component that controls the displaying of the status text.
        /// </summary>
        [Tooltip("Component that controls the displaying of the status text.")]
        public UITextDisplay StatusText = null;

        /// <summary>
        /// When enabled, the latest data from Scene Understanding data provider will be displayed periodically (controlled by the AutoRefreshIntervalInSeconds float).
        /// </summary>
        [Tooltip("When enabled, the latest data from Scene Understanding data provider will be displayed periodically (controlled by the AutoRefreshIntervalInSeconds float).")]
        public bool AutoRefresh = true;

        /// <summary>
        /// Interval to use for auto refresh, in seconds.
        /// </summary>
        [Tooltip("Interval to use for auto refresh, in seconds.")]
        public float AutoRefreshIntervalInSeconds = 10f;

        /// <summary>
        /// Toggles display of all scene objects, except for the world mesh.
        /// </summary>
        [Tooltip("Toggles display of all scene objects, except for the world mesh.")]
        public bool RenderSceneObjects = true;

        /// <summary>
        /// Type of visualization to use for scene objects.
        /// </summary>
        [Tooltip("Type of visualization to use for scene objects.")]
        public VisualizationMode SceneObjectVisualizationMode = VisualizationMode.Mesh;

        /// <summary>
        /// Toggles display of large, horizontal scene objects, aka 'Platform'.
        /// </summary>
        [Tooltip("Toggles display of large, horizontal scene objects, aka 'Platform'.")]
        public bool RenderPlatformSceneObjects = true;

        /// <summary>
        /// Toggles the display of background scene objects.
        /// </summary>
        [Tooltip("Toggles the display of background scene objects.")]
        public bool RenderBackgroundSceneObjects = true;

        /// <summary>
        /// Toggles the display of unknown scene objects.
        /// </summary>
        [Tooltip("Toggles the display of unknown scene objects.")]
        public bool RenderUnknownSceneObjects = true;

        /// <summary>
        /// Toggles the display of completely inferred scene objects.
        /// </summary>
        [Tooltip("Toggles the display of completely inferred scene objects.")]
        public bool RenderCompletelyInferredSceneObjects = true;

        /// <summary>
        /// Toggles the display of the world mesh.
        /// </summary>
        [Tooltip("Toggles the display of the world mesh.")]
        public bool RenderWorldMesh = false;

        private bool _displayInProgress = false;
        private float _timeElapsedSinceLastAutoRefresh = 0f;
        private bool _pcDisplayStarted = false;
        private Guid _lastDisplayedSceneGuid;
        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            SUDataProvider = SUDataProvider == null ? gameObject.GetComponent<SceneUnderstandingDataProvider>() : SUDataProvider;
            SUUtils = SUUtils == null ? gameObject.GetComponent<SceneUnderstandingUtils>() : SUUtils;
            SceneRoot = SceneRoot == null ? SUUtils.CreateGameObject("SceneRoot", null) : SceneRoot;
            SceneObjectMeshMaterial = SceneObjectMeshMaterial == null ? Resources.Load<Material>("Materials/SceneObjectMesh") : SceneObjectMeshMaterial;
            SceneObjectQuadMaterial = SceneObjectQuadMaterial == null ? Resources.Load<Material>("Materials/SceneObjectQuad") : SceneObjectQuadMaterial;
            SceneObjectWireframeMaterial = SceneObjectWireframeMaterial == null ? Resources.Load<Material>("Materials/WireframeTransparent") : SceneObjectWireframeMaterial;
            WorldMeshMaterial = WorldMeshMaterial == null ? Resources.Load<Material>("Materials/WireframeTransparent") : WorldMeshMaterial;
            LabelFont = LabelFont == null ? (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") : LabelFont;
            StatusText = StatusText == null ? GameObject.Find("StatusText").GetComponent<UITextDisplay>() : StatusText;

            // To ensure that the first update, as part of auto refresh, happens immediately.
            _timeElapsedSinceLastAutoRefresh = AutoRefreshIntervalInSeconds;
        }

        /// <summary>
        /// Controls auto refresh (if enabled) on the device path. On the PC path, ensures display happens once.
        /// </summary>
        private void Update()
        {
            if (SUDataProvider.RunOnDevice)
            {
                // Autorefresh only applies when running on the device.
                if (AutoRefresh)
                {
                    _timeElapsedSinceLastAutoRefresh += Time.deltaTime;
                    if (_timeElapsedSinceLastAutoRefresh >= AutoRefreshIntervalInSeconds)
                    {
                        // Only trigger the display of the new scene, if the scene has changed.
                        if (SUDataProvider.GetLatestSceneGuid() != _lastDisplayedSceneGuid)
                        {
                            StartDisplay();
                        }
                        _timeElapsedSinceLastAutoRefresh -= AutoRefreshIntervalInSeconds;
                    }
                }
            }
            else
            {
                // For the PC path, just display the data once.
                if (_pcDisplayStarted == false)
                {
                    StartDisplay();
                    _pcDisplayStarted = true;
                }
            }
        }

        /// <summary>
        /// Enables auto refresh.
        /// </summary>
        public void StartAutoRefresh()
        {
            AutoRefresh = true;
        }

        /// <summary>
        /// Disables auto refresh.
        /// </summary>
        public void StopAutoRefresh()
        {
            AutoRefresh = false;
            _timeElapsedSinceLastAutoRefresh = AutoRefreshIntervalInSeconds;
        }

        /// <summary>
        /// Triggers the displaying of the latest set of scene objects.
        /// </summary>
        public void StartDisplay()
        {
            if (_displayInProgress)
            {
                Logger.Log("SceneUnderstandingDisplayManager.StartDisplay: Display is already in progress.");
                return;
            }

            _displayInProgress = true;
            StartCoroutine(DisplayData());
       }

        /// <summary>
        /// Coroutine that spreads the work of displaying scene objects across multiple frames.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        private IEnumerator DisplayData()
        {
            StatusText.Clear();
            StatusText.Append("About to display the latest set of scene objects.");
            StatusText.Append(string.Format("Current Settings:-\n\tRenderSceneObjects: {0}; SceneObjectVisualizationMode: {1}; RenderInferredRegions: {2};\n\tRenderPlatform: {3}; RenderBackground: {4}; RenderUnknown: {5}; RenderCompletelyInferred: {6};\n\tRenderWorldMesh: {7}; WorldMeshLOD: {8};\n\tBoundingSphereRadiusInMeters: {9};",
                                            RenderSceneObjects, SceneObjectVisualizationMode.ToString(), SUDataProvider.RequestInferredRegions, RenderPlatformSceneObjects, RenderBackgroundSceneObjects, RenderUnknownSceneObjects, RenderCompletelyInferredSceneObjects, RenderWorldMesh, SUDataProvider.WorldMeshLOD.ToString(), SUDataProvider.BoundingSphereRadiusInMeters));
            
            // First, get the latest scene from the data provider.
            Tuple<Guid, byte[]> latestSceneData = SUDataProvider.GetLatestSerializedScene();
            byte[] serializedScene = latestSceneData.Item2;

            if (serializedScene != null)
            {
                // Then, deserialize the scene.
                SceneUnderstanding.Scene scene = SceneUnderstanding.Scene.Deserialize(serializedScene);
                if (scene != null)
                {
                    // This will destroy all game objects under root.
                    SUUtils.DestroyAllGameObjectsUnderParent(SceneRoot.transform);

                    // Return to account for the destruction of the game objects at the end of the frame.
                    yield return null;
        
                    // Retrieve the Scene to Unity world transform.
                    System.Numerics.Matrix4x4? sceneToUnityTransform = TransformUtils.GetSceneToUnityTransform(scene.OriginSpatialGraphNodeId, SUDataProvider.RunOnDevice);
                    if (sceneToUnityTransform != null)
                    {
                        // This will place the root object that represents the scene in the right place.
                        TransformUtils.SetUnityTransformFromMatrix4x4(sceneToUnityTransform.Value, SceneRoot.transform);

                        // Retrieve all the scene objects, associated with this scene.
                        IEnumerable<SceneUnderstanding.SceneObject> sceneObjects = scene.SceneObjects;

                        int i = 0;
                        foreach (SceneUnderstanding.SceneObject sceneObject in sceneObjects)
                        {
                            if (DisplaySceneObject(sceneObject))
                            {
                                ++i;
                                if (i % 5 == 0)
                                {
                                    yield return null;
                                }
                            }
                        }
                    }

                    // When running on PC, orient the main camera such that the floor is on the Unity world's X-Z plane.
                    if (SUDataProvider.RunOnDevice == false)
                    {
                        SUUtils.OrientSceneRootForPC(SceneRoot, scene);
                    }
                }
            }

            StatusText.Append("SceneUnderstandingDisplayManager.DisplayData: Display completed.");
            _displayInProgress = false;
            _lastDisplayedSceneGuid = latestSceneData.Item1;
        }

        /// <summary>
        /// Displays one individual scene object. 
        /// </summary>
        /// <param name="sceneObject">Scene Object to display.</param>
        private bool DisplaySceneObject(SceneUnderstanding.SceneObject sceneObject)
        {
            try
            {
                if (sceneObject == null)
                {
                    Logger.LogWarning("SceneUnderstandingDisplayManager.DisplaySceneObject: Scene Object is null.");
                    return false;
                }

                // Skip the object, if the setting to display that object is set to false.
                if (    (RenderSceneObjects == false                       && sceneObject.Kind != SceneUnderstanding.SceneObjectKind.World)
                     || (RenderWorldMesh == false                          && sceneObject.Kind == SceneUnderstanding.SceneObjectKind.World)
                     || (RenderPlatformSceneObjects == false               && sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Platform)
                     || (RenderBackgroundSceneObjects == false             && sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Background)
                     || (RenderUnknownSceneObjects == false                && sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Unknown)
                     || (RenderCompletelyInferredSceneObjects == false     && sceneObject.Kind == SceneUnderstanding.SceneObjectKind.CompletelyInferred))
                {
                    return false;
                }

                // Create a game object for the scene object, parent it to the root and set it's transform.
                GameObject soGO = SUUtils.CreateGameObject(sceneObject.Kind.ToString(), SceneRoot.transform);
                TransformUtils.SetUnityTransformFromMatrix4x4(TransformUtils.ConvertRightHandedMatrix4x4ToLeftHanded(sceneObject.GetLocationAsMatrix()), soGO.transform, true);

                // This is the new child game object that will contain the meshes, quads, etc.
                GameObject soChildGO = null; 

                // Render the world mesh.
                if (sceneObject.Kind == SceneUnderstanding.SceneObjectKind.World)
                {
                    // Get the meshes from the SU API.
                    IEnumerable<SceneUnderstanding.SceneMesh> meshes = sceneObject.Meshes;

                    // Combine all the world meshes into one unity mesh.
                    Mesh unityMesh = SUUtils.GenerateUnityMeshForSceneObjectMeshes(meshes);

                    // Create a game object with the above unity mesh.
                    soChildGO = SUUtils.CreateGameObjectWithMeshComponents(sceneObject.Kind.ToString(), soGO.transform, unityMesh, WorldMeshMaterial, null);
                }
                // Render all other scene objects.
                else
                {
                    Color? color = SceneUnderstandingUtils.GetColorForLabel(sceneObject.Kind);
                    
                    switch (SceneObjectVisualizationMode)
                    {
                        case VisualizationMode.Quad:
                        case VisualizationMode.QuadWithMask:
                            {
                                // Get the quads from the SU API.
                                IEnumerable<SceneUnderstanding.SceneQuad> quads = sceneObject.Quads;

                                // For each quad, generate the unity mesh, create the game object and apply the invalidation mask, if applicable.
                                foreach (SceneUnderstanding.SceneQuad quad in quads)
                                {
                                    // Generate the unity mesh for the quad.
                                    Mesh unityMesh = SUUtils.GenerateUnityMeshForSceneObjectQuad(quad);

                                    // Create a game object with the above unity mesh.
                                    soChildGO = SUUtils.CreateGameObjectWithMeshComponents(
                                        sceneObject.Kind.ToString(), 
                                        soGO.transform, 
                                        unityMesh, 
                                        SceneObjectVisualizationMode == VisualizationMode.QuadWithMask ? SceneObjectQuadMaterial : SceneObjectMeshMaterial, 
                                        color);

                                    // Apply the invalidation mask.
                                    if (SceneObjectVisualizationMode == VisualizationMode.QuadWithMask)
                                    {
                                        SUUtils.ApplyQuadRegionMask(quad, soChildGO, color);
                                    }
                                }
                                
                            }
                            break;
                        case VisualizationMode.Mesh:
                        case VisualizationMode.Wireframe:
                            {
                                // Get the meshes from the SU API.
                                IEnumerable<SceneUnderstanding.SceneMesh> meshes = sceneObject.Meshes;

                                foreach(SceneUnderstanding.SceneMesh mesh in meshes)
                                {
                                    // Generate the unity mesh for the Scene Understanding mesh.
                                    Mesh unityMesh = SUUtils.GenerateUnityMeshForSceneObjectMesh(mesh);

                                    // Create a game object with the above unity mesh.
                                    soChildGO = SUUtils.CreateGameObjectWithMeshComponents(
                                        sceneObject.Kind.ToString(), 
                                        soGO.transform, 
                                        unityMesh, 
                                        SceneObjectVisualizationMode == VisualizationMode.Mesh ? SceneObjectMeshMaterial : SceneObjectWireframeMaterial, 
                                        SceneObjectVisualizationMode == VisualizationMode.Mesh ? color : null);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (DisplayTextLabels)
                {
                    // Only add text for the labels below.
                    if (sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Ceiling
                        || sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Floor
                        || sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Platform
                        || sceneObject.Kind == SceneUnderstanding.SceneObjectKind.Wall)
                    {
                        SUUtils.AddTextLabel(soChildGO, sceneObject.Kind.ToString(), LabelFont);
                    }
                }

                // When running on device, add a worldanchor component to keep the scene object aligned to the real world. 
                // When running on PC, add a boxcollider component, that is used for the 'Focus' functionality (in CameraMovement.cs).
                if (SUDataProvider.RunOnDevice)
                {
                    soGO.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();
                }
                else
                {
                    soGO.AddComponent<BoxCollider>();
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            return true;
        }
    }
}
