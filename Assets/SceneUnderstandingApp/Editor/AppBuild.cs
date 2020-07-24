// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Linq;
    using UnityEditor;

    public static class AppBuild
    {
        public static void BuildUWP()
        {
            BuildUWPInternal();
            EditorApplication.Exit(0);
        }

        [MenuItem("Build/UWP")]
        private static void BuildUWPInternal()
        {
            // Versions of SDK to build against.
            // If the value of VS/SDK is empty or invalid version, Unity uses the latest VS/SDK installed on the machine.
            EditorUserBuildSettings.wsaUWPVisualStudioVersion = string.Empty;
            EditorUserBuildSettings.wsaUWPSDK = "10.0.18362.0";
            
            // Make sure we're building for UWP.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
            EditorUserBuildSettings.SetPlatformSettings("WindowsStoreApps", "CopyReferences", "true");

            // Configure Player options.
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = "UWP";
            buildPlayerOptions.targetGroup = BuildTargetGroup.WSA;
            buildPlayerOptions.target = BuildTarget.WSAPlayer;
            buildPlayerOptions.options = BuildOptions.None;

            // Set the scene path based on what's being enabled in the UI.
            var enabledScenes = from scene in EditorBuildSettings.scenes
                                where scene.enabled
                                select scene.path;
            buildPlayerOptions.scenes = enabledScenes.ToArray();

            // Set the build player options.
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            //Debug.Log("Build completed.");
        }
    }
}
