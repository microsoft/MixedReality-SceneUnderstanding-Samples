---
page_type: sample
name: Scene Understanding samples for Unity
description: A Unity-based sample application that showcases Scene Understanding on HoloLens 2.
languages:
- csharp
products:
- windows-mixed-reality
- hololens
---

# Microsoft.MixedReality.SceneUnderstanding.Samples - UnitySample

![License](https://img.shields.io/badge/license-MIT-green.svg)

Supported Unity versions | Built with XR configuration
:-----------------: | :----------------: | 
Unity 2020.3.12f1 | Windows XR | 

A Unity-based sample application that showcases Scene Understanding on HoloLens 2. When this sample is deployed on a HoloLens, it will show the virtual representation of your real environment. For PC deployment, the sample will load a serialized scene (included under **Assets\\SceneUnderstanding\\StandardAssets\\SUScenes**) and display it. A help menu is presented on launch, which provides information about all the input commands available in the application.  

To learn more about Scene Understanding, visit our [Scene Understanding](https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding) and [Scene Understanding SDK](https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding-sdk) documentation.

## Contents

| File/folder | Description |
|-------------|-------------|
| `Assets` | Unity assets, scenes, prefabs, and scripts. |
| `Packages` | Project manifest and packages list. |
| `ProjectSettings` | Unity asset setting files. |
| `UIElementsSchema` | UIElements schema files from the Unity editor. |
| `.gitignore` | Define what to ignore at commit time. |
| `LICENSE`   | The license for the sample. |
| `README.md` | This README file. |

## Prerequisites

* Unity 2020.3.12f1 or higher
* Up-to-date version of [Unity Hub](https://unity3d.com/get-unity/download) 
* Visual Studio 2017 or 2019 with Universal Windows Platform components 
* Windows SDK version 10.0.18362.0 or higher

## Setup 

1. Clone or download this sample repository. 
2. Open Unity Hub, select 'Add' and choose the project folder where you extracted the cloned sample.
3. After the project loads, navigate to **Windows > Package Manager** and check that you have the required packages installed:
    * Mixed Reality Scene Understanding
    * Mixed Reality WinRT Projections
4. If they're missing, download them using the [Mixed Reality Feature Tool](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool)

## Running the sample

Before trying to build the project, go to **File > Build Settings** and make sure all samples scenes in the **SceneUnderstanding/Examples** folder appear in the list.

> **[!IMPORTANT]
> The **Home-Examples** scene is not a SceneUnderstanding Scene per se, but rather a Menu Scene from which you can load the other example scenes. You can load any of the other example scenes using voice commands.**

### Running on HoloLens 2

To run this sample on a HoloLens 2:

1. Open the SceneUnderstanding Sample Scenes under **Assets\\SceneUnderstanding\\Examples** - Scenes are in the **Placement**, **NavMesh** and **Understanding** folders
2. Select the **SceneUnderstandingManager** game object and make sure that **Query Scene From Device** is selected on the **SceneUnderstandingManager Component** in all Scenes
3. Go to **File > Build Settings** and select **Build > UWP**. Once the build completes successfully, a log indicating this will show up in the output console.
4. Navigate to the **UWP** folder under root and open 'Scene Understanding.sln' in Visual Studio.
5. Right-click on the 'Scene Understanding (Universal Windows)' project and click on 'Publish' --\> 'Create App Packages'.
6. Run through the wizard and wait for building and packaging to complete.
7. The built app package should be at **UWP\\AppPackages\\Scene Understanding\\Scene Understanding\__\\Scene Understanding\__.\[appx|msix|appxbundle|msixbundle\]**
8. [Deploy](https://docs.microsoft.com/en-us/hololens/holographic-custom-apps) the package to a HoloLens 2. Ensure you build your application using **ARM64**, see the topic [Unity 2019.3 and HoloLens](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/BuildAndDeploy.html#unity-20193-and-hololens) for further details.
9. Launch the 'Scene Understanding' app from the 'All Apps' list on the HoloLens 2!

### Running on PC

To run this sample on a PC:

1. Open the SceneUnderstanding Sample Scenes under **Assets\\SceneUnderstanding\\Examples** - Scenes are in the **Placement**, **NavMesh** and **Understanding** folders
2. Select the **SceneUnderstandingManager** game object and uncheck the **Query Scene From Device** checkbox on the **SceneUnderstandingManager Component**
3. Ensure SU Serialized Scene Paths on the Scene Understanding component is referring to a serialized Scene Understanding scene, examples scenes are provided under the examples folder
4. Click **Play** in the Editor!

## Common Issues - Troubleshooting

Problem:
```
Multiple errors occur in SceneUnderstandingManager.cs
Line 571.
System.Numerics.Matrix4x4 converted4x4LocationMatrix = ConvertRightHandedMatrix4x4ToLeftHanded(suObject.GetLocationAsMatrix());
error CS7069: Reference to type 'Matrix4x4' claims it is defined in 'System.Numerics', but it could not be found
```

Solution:

* Go to **Build Settings > Player Settings > Other Settings > Api Compatibility Level** and select **.Net 4.x** 
* This setting might revert when upgrading Unity versions

## Additional Notes

* We have stopped using NugetForUnity and we will no longer support that path. If you still want our legacy build with NugetForUnity please checkout our legacy branch: [microsoft/MixedReality-SceneUnderstanding-Samples at LegacyNugetBuild (github.com)](https://github.com/microsoft/MixedReality-SceneUnderstanding-Samples/tree/LegacyNugetBuild) 
    * **This branch is deprecated and we will not support updates on it.**

> **[!NOTE]
> When running on your Hololens, all the interactive commands are voice commands. You're require to speak to interact with the scene when running on Hololens. Say **Scene Objects Wireframe**, **Load NavMesh**, **Toggle Auto Refresh** and so on.**

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
