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
        SerializedProperty serializedMeshMaterial;
        SerializedProperty serializedQuadMaterial;
        SerializedProperty serializedWireFrameMaterial;
        SerializedProperty serializedInvisibleMaterial;
        SerializedProperty serializedRenderSceneObjects;
        SerializedProperty serializedRenderPlatformsObjects;
        SerializedProperty serializedRenderBackgroundObjects;
        SerializedProperty serializedRenderUnknownObjects;
        SerializedProperty serializedRenderWorldMesh;
        SerializedProperty serializedRequestInferredRegions;
        SerializedProperty serializedRenderCompletelyInferredSceneObjects;
        SerializedProperty serializedMeshQuality;
        SerializedProperty serializedRenderColorBackGrounds;
        SerializedProperty serializedRenderColorWall;
        SerializedProperty serializedRenderColorFloor;
        SerializedProperty serializedRenderColorCeiling;
        SerializedProperty serializedRenderColorPlatform;
        SerializedProperty serializedRenderColorUnknown;
        SerializedProperty serializedRenderColorCompletelyInferred;
        SerializedProperty serializedRenderColorWorld;
        SerializedProperty serializedisInGhostMode;
        SerializedProperty serializedAddColliders;
        SerializedProperty serializedOnLoadStartedCallback;
        SerializedProperty serializedOnLoadFinishedCallback;

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
            serializedSUScene        = serializedObject.FindProperty("SUSerializedScenePaths");
            serializedRootGameObject = serializedObject.FindProperty("SceneRoot");

            // On Device Request Settings
            serializedBoudingSphereRadiousInMeters = serializedObject.FindProperty("BoundingSphereRadiusInMeters");
            serializedAutoRefreshData              = serializedObject.FindProperty("AutoRefresh");
            serializedAutoRefreshIntervalInSeconds = serializedObject.FindProperty("AutoRefreshIntervalInSeconds");

            // Request Settings
            serializedRequestMode             = serializedObject.FindProperty("SceneObjectRequestMode");
            serializedMeshQuality             = serializedObject.FindProperty("MeshQuality");
            serializedRequestInferredRegions  = serializedObject.FindProperty("RequestInferredRegions");

            // Reference to all materials used.
            serializedMeshMaterial      = serializedObject.FindProperty("SceneObjectMeshMaterial");
            serializedQuadMaterial      = serializedObject.FindProperty("SceneObjectQuadMaterial");
            serializedWireFrameMaterial = serializedObject.FindProperty("SceneObjectWireframeMaterial");
            serializedInvisibleMaterial = serializedObject.FindProperty("TransparentOcclussion");

            // Reference to all toggles and filters for visualization
            serializedRenderSceneObjects                   = serializedObject.FindProperty("RenderSceneObjects");
            serializedRenderPlatformsObjects               = serializedObject.FindProperty("RenderPlatformSceneObjects");
            serializedRenderBackgroundObjects              = serializedObject.FindProperty("RenderBackgroundSceneObjects");
            serializedRenderUnknownObjects                 = serializedObject.FindProperty("RenderUnknownSceneObjects");
            serializedRenderWorldMesh                      = serializedObject.FindProperty("RenderWorldMesh");
            serializedRenderCompletelyInferredSceneObjects = serializedObject.FindProperty("RenderCompletelyInferredSceneObjects");

            // Reference for all colors used
            serializedRenderColorBackGrounds        = serializedObject.FindProperty("ColorForBackgroundObjects");
            serializedRenderColorWall               = serializedObject.FindProperty("ColorForWallObjects");
            serializedRenderColorFloor              = serializedObject.FindProperty("ColorForFloorObjects");
            serializedRenderColorCeiling            = serializedObject.FindProperty("ColorForCeilingObjects");
            serializedRenderColorPlatform           = serializedObject.FindProperty("ColorForPlatformsObjects");
            serializedRenderColorUnknown            = serializedObject.FindProperty("ColorForUnknownObjects");
            serializedRenderColorCompletelyInferred = serializedObject.FindProperty("ColorForInferredObjects");
            serializedRenderColorWorld              = serializedObject.FindProperty("ColorForWorldObjects");

            // Toggle for Occlusion Mode / Ghost Mode
            serializedisInGhostMode = serializedObject.FindProperty("IsInGhostMode");

            // Toggle for Colliders 
            serializedAddColliders = serializedObject.FindProperty("AddColliders");

            // Reference for Callbacks
            serializedOnLoadStartedCallback  = serializedObject.FindProperty("OnLoadStarted");
            serializedOnLoadFinishedCallback = serializedObject.FindProperty("OnLoadFinished");
            
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
            if(!SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("Scene Fragments: ", EditorStyles.boldLabel);
                if(GUILayout.Button("Add Item", GUILayout.Width(buttonWidth)))
                {
                    SUManager.SUSerializedScenePaths.Add(null);
                }

                if(GUILayout.Button("Remove Item", GUILayout.Width(buttonWidth)))
                {
                    if(SUManager.SUSerializedScenePaths.Count >= 1)
                    {
                        SUManager.SUSerializedScenePaths.RemoveAt(SUManager.SUSerializedScenePaths.Count -1);
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
            if(SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("On Device Request Settings", EditorStyles.boldLabel);
                GUIContent BoundingSphereRadiousInMetersContent = new GUIContent("Bounding Sphere Radious In Meters", "Radius of the sphere around the camera, which is used to query the environment.");
                serializedBoudingSphereRadiousInMeters.floatValue = EditorGUILayout.Slider(BoundingSphereRadiousInMetersContent, serializedBoudingSphereRadiousInMeters.floatValue, 5.0f, 100.0f);

                EditorGUILayout.PropertyField(serializedAutoRefreshData);
                
                if(SUManager.AutoRefresh)
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
            EditorGUILayout.PropertyField(serializedRenderColorBackGrounds);
            EditorGUILayout.PropertyField(serializedRenderColorWall);
            EditorGUILayout.PropertyField(serializedRenderColorFloor);
            EditorGUILayout.PropertyField(serializedRenderColorCeiling);
            EditorGUILayout.PropertyField(serializedRenderColorPlatform);
            EditorGUILayout.PropertyField(serializedRenderColorUnknown);
            EditorGUILayout.PropertyField(serializedRenderColorCompletelyInferred);
            EditorGUILayout.PropertyField(serializedRenderColorWorld);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Materials
            EditorGUILayout.PropertyField(serializedMeshMaterial);
            EditorGUILayout.PropertyField(serializedQuadMaterial);
            EditorGUILayout.PropertyField(serializedWireFrameMaterial);
            EditorGUILayout.PropertyField(serializedInvisibleMaterial);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Render Filters
            EditorGUILayout.PropertyField(serializedRenderSceneObjects);
            EditorGUILayout.PropertyField(serializedRenderPlatformsObjects);
            EditorGUILayout.PropertyField(serializedRenderBackgroundObjects);
            EditorGUILayout.PropertyField(serializedRenderUnknownObjects);
            EditorGUILayout.PropertyField(serializedRenderWorldMesh);
            EditorGUILayout.PropertyField(serializedRenderCompletelyInferredSceneObjects);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Ghost Mode
            EditorGUILayout.PropertyField(serializedisInGhostMode);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            EditorGUILayout.PropertyField(serializedAddColliders);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // Callbacks
            EditorGUILayout.PropertyField(serializedOnLoadStartedCallback);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            EditorGUILayout.PropertyField(serializedOnLoadFinishedCallback);
            GUILayout.Space(verticalSpaceBetweenHeaders);

            // On Editor only
            if(!SUManager.QuerySceneFromDevice)
            {
                GUILayout.Label("Actions", EditorStyles.boldLabel);
                if(GUILayout.Button("Bake Scene", GUILayout.Width(buttonWidth)))
                {
                    SUManager.BakeScene();
                }
            }

            // Apply all Changes
            serializedObject.ApplyModifiedProperties();
        }
    }

}