# Microsoft.MixedReality.SceneUnderstanding.Samples - UnitySample

A Unity-based sample application that showcases Scene Understanding on HoloLens 2.  
When this sample is deployed on a HoloLens, it will show the virtual representation of your real environment.  
When this sample is deployed on a PC, it will load a serialized scene (included under Assets\\SceneUnderstanding\\StandardAssets\\SUScenes) and display it.  
A help menu is presented on launch, which provides information about all the input commands available in the application.  
To learn more about Scene Understanding, please visit this link: [https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding](https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding). To learn more about Scene Understanding SDK, please visit this link: [https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding-sdk](https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding-sdk).

# Prerequisites

Unity 2019.4.11f1 or greater.  
Visual Studio 2017 or 2019 with Universal Windows Platform components.  
Windows SDK version 10.0.18362.0 or greater. Up to date version of Unity Hub.

# Opening the project, verifying UPM packages, Setting up Unity Scenes for build.

After cloning the project or extracting the project from a zip. Open Unity Hub, select 'Add' and add your project folder from where you extracted it or cloned it. Wait for the project to load, after the project loads:

Navigate to:

Window -\> Package Manager

You should see two packages under Microsoft:

1. <u>**Mixed Reality Scene Understanding**</u>
2. <u>**Mixed Reality WinRT Projections**</u>

These two UPM packages are neccessary for the sample to work correctly

If for whatever reason you do not have these packages please download them through the Mixed Reality Feature Tool

[Welcome to the Mixed Reality Feature Tool - Mixed Reality | Microsoft Docs](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool#:~:text=%20Welcome%20to%20the%20Mixed%20Reality%20Feature%20Tool,project%20changes.%20For%20more%20information,%20see...%20More)

# Running Prerequisites

Before trying to build the project, verify the all the relevant unity scenes are loaded in the Unity Build Settings, go to File-\>Build Settings. All the samples scenes are under SceneUnderstanding/Examples folder inside the project. 

<mark>The 'Home-Examples' scene is not a SceneUnderstanding Scene per se, but rather a Menu Scene from which you can load the other example scenes. do so by using voice commands</mark>

# Running on HoloLens 2

To run this sample on the HoloLens 2, please follow the instructions below:

1. Open the SceneUnderstanding Sample Scenes under Assets\\SceneUnderstanding\\Examples (Scenes are Under Placement, NavMesh and Understanding Folder)
2. Select the SceneUnderstandingManager game object and make sure that 'Query Scene From Device' checkbox is checked on the SceneUnderstandingManager Component in all Scenes
3. In the Menu under File-\>Build Settings, click on Build --\> UWP. Once the build completes successfully, a log indicating this will show up in the output console.
4. Navigate to the UWP folder under root and open the 'Scene Understanding.sln' in Visual Studio.
5. Right-click on the 'Scene Understanding (Universal Windows)' project and click on 'Publish' --\> 'Create App Packages'.
6. Run through the wizard and wait for building and packaging to complete.
7. The built app package should be at 'UWP\\AppPackages\\Scene Understanding\\Scene Understanding\__\\Scene Understanding\__.\[appx|msix|appxbundle|msixbundle\]'
8. [Deploy](https://docs.microsoft.com/en-us/hololens/holographic-custom-apps) the package to a HoloLens 2. Ensure you build your application using ARM64, see the topic [Unity 2019.3 and HoloLens](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/BuildAndDeploy.html#unity-20193-and-hololens) for further details.
9. Launch the 'Scene Understanding' app from the 'All Apps' list on the HoloLens 2.

# Running on PC

To run this sample on the PC, please follow the instructions below:

1. Open any of the SceneUnderstanding Sample Scenes under Assets\\SceneUnderstanding\\Examples (Scenes are Under Placement, NavMesh and Understanding Folder)
2. Select the SceneUnderstandingManager game object and uncheck the 'Query Scene From Device' checkbox on the SceneUnderstandingManager Component
3. Ensure SU Serialized Scene Paths on the Scene Understanding component is referring to a serialized Scene Understanding scene, examples scenes are provided under the examples folder
4. Click 'Play' in the Editor.

# Additional Notes

- We have stopped using NugetForUnity and we will no longer support that path. if you still want our legacy build with NugetForUnity please checkout our legacy branch: [microsoft/MixedReality-SceneUnderstanding-Samples at LegacyNugetBuild (github.com)](https://github.com/microsoft/MixedReality-SceneUnderstanding-Samples/tree/LegacyNugetBuild) 
    
    - **note however that this branch is deprecated and we will not support updates on it.**
- <mark>Note that when running on your Hololens all the interactive commands are voice commands, you are require to speak to interact with the scene when running on Hololens. Say 'Scene Objects Wireframe' , 'Load NavMesh' , 'Toggle Auto Refresh' etc.</mark>
