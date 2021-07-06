// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEditor;
    using UnityEngine;

    ///<summary>
    ///  This Script defines and implements a custom editor view for the Scene Understanding Manager
    ///  component, it utilizes the SerializedProperty type from Unity as much as possible to avoid
    ///  issues with prefab overrides. All serialized Properties in a custom editor will automatically
    ///  override settings per scene for prefabs.
    ///</summary>
    [CustomEditor(typeof(SceneUnderstandingManager))]
    public class SceneUnderstandingManagerEditor : Editor
    {
        // Section hiding
        bool showColors = false;
        bool showLayers = false;
        bool showMaterials = false;
        bool showFilters = false;
        bool showPhysics = false;

        // Reference to the target script
        SceneUnderstandingManager SUManager;

        // Serialized Property of every single value we want to show in the editor.
        SerializedProperty serializedQuerySceneFromDevice;
        SerializedProperty serializedSUScene;
        SerializedProperty serializedRootGameObject;
        SerializedProperty serializedBoudingSphereRadiousInMeters;
        SerializedProperty serializedAutoRefreshData;
        SerializedProperty serializedAutoRefreshIntervalInSeconds;
        SerializedProperty serializedRequestMode;
        SerializedProperty serializedBackgroundMeshMaterial;
        SerializedProperty serializedWallMeshMaterial;
        SerializedProperty serializedFloorMeshMaterial;
        SerializedProperty serializedCeilingMeshMaterial;
        SerializedProperty serializedPlatformMeshMaterial;
        SerializedProperty serializedUnknownMeshMaterial;
        SerializedProperty serializedInferredMeshMaterial;
        SerializedProperty serializedBackgroundQuadMaterial;
        SerializedProperty serializedWallQuadMaterial;
        SerializedProperty serializedFloorQuadMaterial;
        SerializedProperty serializedCeilingQuadMaterial;
        SerializedProperty serializedPlatformQuadMaterial;
        SerializedProperty serializedUnknownQuadMaterial;
        SerializedProperty serializedInferredQuadMaterial;
        SerializedProperty serializedWireFrameMaterial;
        SerializedProperty serializedInvisibleMaterial;
        SerializedProperty serializedFilterAllSceneObjects;
        SerializedProperty serializedFilterPlatformsObjects;
        SerializedProperty serializedFilterBackgroundObjects;
        SerializedProperty serializedFilterUnknownObjects;
        SerializedProperty serializedFilterWorldMesh;
        SerializedProperty serializedFilterWallObjects;
        SerializedProperty serializedFilterCeilingObjects;
        SerializedProperty serializedFilterFloorObjects;
        SerializedProperty serializedRequestInferredRegions;
        SerializedProperty serializedFilterCompletelyInferredSceneObjects;
        SerializedProperty serializedMeshQuality;
        SerializedProperty serializedRenderColorBackGrounds;
        SerializedProperty serializedRenderColorWall;
        SerializedProperty serializedRenderColorFloor;
        SerializedProperty serializedRenderColorCeiling;
        SerializedProperty serializedRenderColorPlatform;
        SerializedProperty serializedRenderColorUnknown;
        SerializedProperty serializedRenderColorCompletelyInferred;
        SerializedProperty serializedRenderColorWorld;
        SerializedProperty serializedLayerBackGrounds;
        SerializedProperty serializedLayerWall;
        SerializedProperty serializedLayerFloor;
        SerializedProperty serializedLayerCeiling;
        SerializedProperty serializedLayerPlatform;
        SerializedProperty serializedLayerUnknown;
        SerializedProperty serializedLayerCompletelyInferred;
        SerializedProperty serializedLayerWorld;
        SerializedProperty serializedisInGhostMode;
        SerializedProperty serializedAddCollidersInPlatformSceneObjects;
        SerializedProperty serializedAddCollidersInBackgroundSceneObjects;
        SerializedProperty serializedAddCollidersInUnknownSceneObjects;
        SerializedProperty serializedAddCollidersInWorldMesh;
        SerializedProperty serializedAddCollidersInCompletelyInferredSceneObjects;
        SerializedProperty serializedAddCollidersInWallSceneObjects;
        SerializedProperty serializedAddCollidersInFloorSceneObjects;
        SerializedProperty serializedAddCollidersCeilingSceneObjects;
        SerializedProperty serializedOnLoadStartedCallback;
        SerializedProperty serializedOnLoadFinishedCallback;
        SerializedProperty serializedAlignSUObjectsNormalToUnityYAxis;

        // Const Floats for Layout dimensions
        const float buttonWidth = 90.0f;
        const float verticalSpaceBetweenHeaders = 4.0f;

        private void OnEnable()
        {
            // Initialize all our properties. find the corresponding variable for each
            // serialized property.

            // Target Component
            SUManager = this.target as SceneUnderstandingManager;

            serializedQuerySceneFromDevice = serializedObject.FindProperty("QuerySceneFromDevice");

            // Settings for loading Scenes from file and Root GameObject, (container of the scene)
            serializedSUScene = serializedObject.FindProperty("SUSerializedScenePaths");
            serializedRootGameObject = serializedObject.FindProperty("SceneRoot");

            // On Device Request Settings
            serializedBoudingSphereRadiousInMeters = serializedObject.FindProperty("BoundingSphereRadiusInMeters");
            serializedAutoRefreshData = serializedObject.FindProperty("AutoRefresh");
            serializedAutoRefreshIntervalInSeconds = serializedObject.FindProperty("AutoRefreshIntervalInSeconds");

            // Request Settings
            serializedRequestMode = serializedObject.FindProperty("SceneObjectRequestMode");
            serializedMeshQuality = serializedObject.FindProperty("MeshQuality");
            serializedRequestInferredRegions = serializedObject.FindProperty("RequestInferredRegions");

            // Reference to all materials used.
            serializedBackgroundMeshMaterial = serializedObject.FindProperty("SceneObjectBackgroundMeshMaterial");
            serializedWallMeshMaterial = serializedObject.FindProperty("SceneObjectWallMeshMaterial");
            serializedFloorMeshMaterial = serializedObject.FindProperty("SceneObjectFloorMeshMaterial");
            serializedCeilingMeshMaterial = serializedObject.FindProperty("SceneObjectCeilingMeshMaterial");
            serializedPlatformMeshMaterial = serializedObject.FindProperty("SceneObjectPlatformMeshMaterial");
            serializedUnknownMeshMaterial = serializedObject.FindProperty("SceneObjectUnknownMeshMaterial");
            serializedInferredMeshMaterial = serializedObject.FindProperty("SceneObjectInferredMeshMaterial");

            serializedBackgroundQuadMaterial = serializedObject.FindProperty("SceneObjectBackgroundQuadMaterial");
            serializedWallQuadMaterial = serializedObject.FindProperty("SceneObjectWallQuadMaterial");
            serializedFloorQuadMaterial = serializedObject.FindProperty("SceneObjectFloorQuadMaterial");
            serializedCeilingQuadMaterial = serializedObject.FindProperty("SceneObjectCeilingQuadMaterial");
            serializedPlatformQuadMaterial = serializedObject.FindProperty("SceneObjectPlatformQuadMaterial");
            serializedUnknownQuadMaterial = serializedObject.FindProperty("SceneObjectUnknownQuadMaterial");
            serializedInferredQuadMaterial = serializedObject.FindProperty("SceneObjectInferredQuadMaterial");

            serializedWireFrameMaterial = serializedObject.FindProperty("SceneObjectWireframeMaterial");
            serializedInvisibleMaterial = serializedObject.FindProperty("TransparentOcclussion");

            // Reference to all toggles and filters for visualization
            serializedFilterAllSceneObjects = serializedObject.FindProperty("FilterAllSceneObjects");
            serializedFilterPlatformsObjects = serializedObject.FindProperty("FilterPlatformSceneObjects");
            serializedFilterBackgroundObjects = serializedObject.FindProperty("FilterBackgroundSceneObjects");
            serializedFilterUnknownObjects = serializedObject.FindProperty("FilterUnknownSceneObjects");
            serializedFilterWorldMesh = serializedObject.FindProperty("FilterWorldMesh");
            serializedFilterWallObjects = serializedObject.FindProperty("FilterWallSceneObjects");
            serializedFilterCeilingObjects = serializedObject.FindProperty("FilterCeilingSceneObjects");
            serializedFilterFloorObjects = serializedObject.FindProperty("FilterFloorSceneObjects");
            serializedFilterCompletelyInferredSceneObjects = serializedObject.FindProperty("FilterCompletelyInferredSceneObjects");

            // Reference for all colors used
            serializedRenderColorBackGrounds = serializedObject.FindProperty("ColorForBackgroundObjects");
            serializedRenderColorWall = serializedObject.FindProperty("ColorForWallObjects");
            serializedRenderColorFloor = serializedObject.FindProperty("ColorForFloorObjects");
            serializedRenderColorCeiling = serializedObject.FindProperty("ColorForCeilingObjects");
            serializedRenderColorPlatform = serializedObject.FindProperty("ColorForPlatformsObjects");
            serializedRenderColorUnknown = serializedObject.FindProperty("ColorForUnknownObjects");
            serializedRenderColorCompletelyInferred = serializedObject.FindProperty("ColorForInferredObjects");
            serializedRenderColorWorld = serializedObject.FindProperty("ColorForWorldObjects");

            // Reference for all layers used
            serializedLayerBackGrounds = serializedObject.FindProperty("LayerForBackgroundObjects");
            serializedLayerWall = serializedObject.FindProperty("LayerForWallObjects");
            serializedLayerFloor = serializedObject.FindProperty("LayerForFloorObjects");
            serializedLayerCeiling = serializedObject.FindProperty("LayerForCeilingObjects");
            serializedLayerPlatform = serializedObject.FindProperty("LayerForPlatformsObjects");
            serializedLayerUnknown = serializedObject.FindProperty("LayerForUnknownObjects");
            serializedLayerCompletelyInferred = serializedObject.FindProperty("LayerForInferredObjects");
            serializedLayerWorld = serializedObject.FindProperty("LayerForWorldObjects");

            // Toggle for Occlusion Mode / Ghost Mode
            serializedisInGhostMode = serializedObject.FindProperty("IsInGhostMode");

            // Toggle for Colliders
            serializedAddCollidersInPlatformSceneObjects = serializedObject.FindProperty("AddCollidersInPlatformSceneObjects");
            serializedAddCollidersInBackgroundSceneObjects = serializedObject.FindProperty("AddCollidersInBackgroundSceneObjects");
            serializedAddCollidersInUnknownSceneObjects = serializedObject.FindProperty("AddCollidersInUnknownSceneObjects");
            serializedAddCollidersInWorldMesh = serializedObject.FindProperty("AddCollidersInWorldMesh");
            serializedAddCollidersInCompletelyInferredSceneObjects = serializedObject.FindProperty("AddCollidersInCompletelyInferredSceneObjects");
            serializedAddCollidersInWallSceneObjects = serializedObject.FindProperty("AddCollidersInWallSceneObjects");
            serializedAddCollidersInFloorSceneObjects = serializedObject.FindProperty("AddCollidersInFloorSceneObjects");
            serializedAddCollidersCeilingSceneObjects = serializedObject.FindProperty("AddCollidersCeilingSceneObjects");

            // Reference for Callbacks
            serializedOnLoadStartedCallback = serializedObject.FindProperty("OnLoadStarted");
            serializedOnLoadFinishedCallback = serializedObject.FindProperty("OnLoadFinished");

            // Toggle to Align SU Objects Normal to Unity's Y axis
            serializedAlignSUObjectsNormalToUnityYAxis = serializedObject.FindProperty("AlignSUObjectsNormalToUnityYAxis");
        }

        public override void OnInspectorGUI()
        {
            // This function will draw the actual inspector in unity.
            // the logic for which properties are shown and how is all here.

            // Update all property values, before doing any changes
            serializedObject.Update();

            // Data Loader Mode (Run On device flag)
            EditorGUILayout.PropertyField(serializedQuerySceneFromDevice);

            // Scene from File
            if (!SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("Scene Fragments: ", EditorStyles.boldLabel);
                if (GUILayout.Button("Add Item", GUILayout.Width(buttonWidth)))
                {
                    SUManager.SUSerializedScenePaths.Add(null);
                }

                if (GUILayout.Button("Remove Item", GUILayout.Width(buttonWidth)))
                {
                    if (SUManager.SUSerializedScenePaths.Count >= 1)
                    {
                        SUManager.SUSerializedScenePaths.RemoveAt(SUManager.SUSerializedScenePaths.Count - 1);
                    }
                }

                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(serializedSUScene, false);
                for (int i = 0; i < serializedSUScene.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(serializedSUScene.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel -= 1;
            }
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Scene Root
            EditorGUILayout.PropertyField(serializedRootGameObject);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // On Device Request Settings
            if (SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("On Device Request Settings", EditorStyles.boldLabel);
                GUIContent BoundingSphereRadiousInMetersContent = new GUIContent("Bounding Sphere Radious In Meters", "Radius of the sphere around the camera, which is used to query the environment.");
                serializedBoudingSphereRadiousInMeters.floatValue = EditorGUILayout.Slider(BoundingSphereRadiousInMetersContent, serializedBoudingSphereRadiousInMeters.floatValue, 5.0f, 100.0f);

                EditorGUILayout.PropertyField(serializedAutoRefreshData);

                if (SUManager.AutoRefresh)
                {
                    GUIContent AutoRefreshIntervalInSeconds = new GUIContent("Auto Refresh Interval In Seconds", "Interval to use for auto refresh, in seconds.");
                    serializedAutoRefreshIntervalInSeconds.floatValue = EditorGUILayout.Slider(AutoRefreshIntervalInSeconds, serializedAutoRefreshIntervalInSeconds.floatValue, 1.0f, 60.0f);
                }
                GUILayout.Space(verticalSpaceBetweenHeaders);
            }

            // Request Settings
            EditorGUILayout.PropertyField(serializedRequestMode);
            EditorGUILayout.PropertyField(serializedMeshQuality);
            EditorGUILayout.PropertyField(serializedRequestInferredRegions);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Colors
            showColors = EditorGUILayout.BeginFoldoutHeaderGroup(showColors, "Colors");
            if (showColors)
            {
                EditorGUILayout.PropertyField(serializedRenderColorBackGrounds);
                EditorGUILayout.PropertyField(serializedRenderColorWall);
                EditorGUILayout.PropertyField(serializedRenderColorFloor);
                EditorGUILayout.PropertyField(serializedRenderColorCeiling);
                EditorGUILayout.PropertyField(serializedRenderColorPlatform);
                EditorGUILayout.PropertyField(serializedRenderColorUnknown);
                EditorGUILayout.PropertyField(serializedRenderColorCompletelyInferred);
                EditorGUILayout.PropertyField(serializedRenderColorWorld);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Layers
            showLayers = EditorGUILayout.BeginFoldoutHeaderGroup(showLayers, "Layers");
            if (showLayers)
            {
                serializedLayerBackGrounds.intValue = EditorGUILayout.LayerField(serializedLayerBackGrounds.displayName, serializedLayerBackGrounds.intValue);
                serializedLayerWall.intValue = EditorGUILayout.LayerField(serializedLayerWall.displayName, serializedLayerWall.intValue);
                serializedLayerFloor.intValue = EditorGUILayout.LayerField(serializedLayerFloor.displayName, serializedLayerFloor.intValue);
                serializedLayerCeiling.intValue = EditorGUILayout.LayerField(serializedLayerCeiling.displayName, serializedLayerCeiling.intValue);
                serializedLayerPlatform.intValue = EditorGUILayout.LayerField(serializedLayerPlatform.displayName, serializedLayerPlatform.intValue);
                serializedLayerUnknown.intValue = EditorGUILayout.LayerField(serializedLayerUnknown.displayName, serializedLayerUnknown.intValue);
                serializedLayerCompletelyInferred.intValue = EditorGUILayout.LayerField(serializedLayerCompletelyInferred.displayName, serializedLayerCompletelyInferred.intValue);
                serializedLayerWorld.intValue = EditorGUILayout.LayerField(serializedLayerWorld.displayName, serializedLayerWorld.intValue);

                // Button for spatial layers
                int spatialLayer = LayerMask.NameToLayer("Spatial Awareness");
                if (spatialLayer > -1)
                {
                    // Did they click the button?
                    if (GUILayout.Button("Set to Spatial Layer"))
                    {
                        // Only change ones that are on the "Default" layer
                        if (serializedLayerBackGrounds.intValue == 0) { serializedLayerBackGrounds.intValue = spatialLayer; }
                        if (serializedLayerWall.intValue == 0) { serializedLayerWall.intValue = spatialLayer; }
                        if (serializedLayerFloor.intValue == 0) { serializedLayerFloor.intValue = spatialLayer; }
                        if (serializedLayerCeiling.intValue == 0) { serializedLayerCeiling.intValue = spatialLayer; }
                        if (serializedLayerPlatform.intValue == 0) { serializedLayerPlatform.intValue = spatialLayer; }
                        if (serializedLayerUnknown.intValue == 0) { serializedLayerUnknown.intValue = spatialLayer; }
                        if (serializedLayerCompletelyInferred.intValue == 0) { serializedLayerCompletelyInferred.intValue = spatialLayer; }
                        if (serializedLayerWorld.intValue == 0) { serializedLayerWorld.intValue = spatialLayer; }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Materials
            showMaterials = EditorGUILayout.BeginFoldoutHeaderGroup(showMaterials, "Materials");
            if(showMaterials)
            {
                EditorGUILayout.PropertyField(serializedBackgroundMeshMaterial);
                EditorGUILayout.PropertyField(serializedWallMeshMaterial);
                EditorGUILayout.PropertyField(serializedFloorMeshMaterial);
                EditorGUILayout.PropertyField(serializedCeilingMeshMaterial);
                EditorGUILayout.PropertyField(serializedPlatformMeshMaterial);
                EditorGUILayout.PropertyField(serializedUnknownMeshMaterial);
                EditorGUILayout.PropertyField(serializedInferredMeshMaterial);

                EditorGUILayout.PropertyField(serializedBackgroundQuadMaterial);
                EditorGUILayout.PropertyField(serializedWallQuadMaterial);
                EditorGUILayout.PropertyField(serializedFloorQuadMaterial);
                EditorGUILayout.PropertyField(serializedCeilingQuadMaterial);
                EditorGUILayout.PropertyField(serializedPlatformQuadMaterial);
                EditorGUILayout.PropertyField(serializedUnknownQuadMaterial);
                EditorGUILayout.PropertyField(serializedInferredQuadMaterial);

                EditorGUILayout.PropertyField(serializedWireFrameMaterial);
                EditorGUILayout.PropertyField(serializedInvisibleMaterial);   
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Filters
            showFilters = EditorGUILayout.BeginFoldoutHeaderGroup(showFilters, "Filters");
            if(showFilters)
            {
                EditorGUILayout.PropertyField(serializedFilterAllSceneObjects);
                EditorGUILayout.PropertyField(serializedFilterPlatformsObjects);
                EditorGUILayout.PropertyField(serializedFilterBackgroundObjects);
                EditorGUILayout.PropertyField(serializedFilterWallObjects);
                EditorGUILayout.PropertyField(serializedFilterCeilingObjects);
                EditorGUILayout.PropertyField(serializedFilterFloorObjects);
                EditorGUILayout.PropertyField(serializedFilterUnknownObjects);
                EditorGUILayout.PropertyField(serializedFilterWorldMesh);
                EditorGUILayout.PropertyField(serializedFilterCompletelyInferredSceneObjects);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Physics
            showPhysics = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysics, "Physics");
            if(showPhysics)
            {
                
                EditorGUILayout.PropertyField(serializedAddCollidersInPlatformSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersInBackgroundSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersInUnknownSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersInWorldMesh);
                EditorGUILayout.PropertyField(serializedAddCollidersInCompletelyInferredSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersInWallSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersInFloorSceneObjects);
                EditorGUILayout.PropertyField(serializedAddCollidersCeilingSceneObjects);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Ghost Mode
            EditorGUILayout.PropertyField(serializedisInGhostMode);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Alignment
            EditorGUILayout.PropertyField(serializedAlignSUObjectsNormalToUnityYAxis);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Callbacks
            EditorGUILayout.PropertyField(serializedOnLoadStartedCallback);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            EditorGUILayout.PropertyField(serializedOnLoadFinishedCallback);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // On Editor only
            if (!SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("Actions", EditorStyles.boldLabel);
                if (GUILayout.Button("Bake Scene", GUILayout.Width(buttonWidth)))
                {
                    SUManager.BakeScene();
                }
            }

            // Apply all Changes
            serializedObject.ApplyModifiedProperties();
        }
    }

}
