# Microsoft.MixedReality.SceneUnderstanding.Samples - UnitySample
A Unity-based sample application that showcases Scene Understanding on HoloLens 2.  
When this sample is deployed on a HoloLens, it will show the virtual representation of your real environment.  
When this sample is deployed on a PC, it will load a serialized scene (included under Assets\SceneUnderstanding\StandardAssets\SUScenes) and display it.  
A help menu is presented on launch, which provides information about all the input commands available in the application.  
To learn more about Scene Understanding, please visit this link: https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding.  
To learn more about Scene Understanding SDK, please visit this link: https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding-sdk.  

# Prerequisites
Unity 2019.4.11f1 or greater.  
Visual Studio 2017 or 2019 with Universal Windows Platform components.  
Windows SDK version 10.0.18362.0 or greater.
Up to date version of Unity Hub.

# Opening the project, verifying Nuget packages, Setting up Unity Scenes for build.
After cloning the project or extracting the project from a zip. Open Unity Hub, select 'Add' and add your project folder from where you extracted it or cloned it.
Wait for the project to load, after the project loaded you should see the following messages in the Unity Console

'Added DLL directory C:\<YOUR-PATH>\MixedReality-SceneUnderstanding-Samples\Assets\Packages\Microsoft.VCRTForwarders.140.1.0.6\Unity\<ARCHITECTURE> to the user search path.'

'Added DLL directory C:\<YOUR-PATH>\MixedReality-SceneUnderstanding-Samples\Assets\Packages\Microsoft.MixedReality.SceneUnderstanding.0.5.2065\Unity\<ARCHITECTURE> to the user search path.'

If for whatever reason this messages don't show on your project, try closing and opening the project again.

To verify your Nuget Packages are correctly installed. Open the Nuget Menu in Unity Nuget-> Manage Nuget Packages -> Installed. Verify Microsoft.MixReality.SceneUnderstanding and Microsoft.VCRTFowarders nugets are both installed. 

# Running Prerequisites
Before trying to build the project, verify the all the relevant unity scenes are loaded in the Unity Build Settings, go to File->Build Settings. All the samples scenes are under SceneUnderstanding/Examples folder inside the project. The 'Home-Examples' scene is not a SceneUnderstanding Scene per se, but rather a Menu Scene from which you can load the other example scenes.

# Running on HoloLens 2
To run this sample on the HoloLens 2, please follow the instructions below:
1. Open the SceneUnderstanding Sample Scenes under Assets\SceneUnderstanding\Examples (Scenes are Under Placement, NavMesh and Understanding Folder)
2. Select the SceneUnderstandingManager game object and make sure that 'Query Scene From Device' checkbox is checked on the SceneUnderstandingManager Component in all Scenes
3. In the Menu under File->Build Settings, click on Build --> UWP. Once the build completes successfully, a log indicating this will show up in the output console.
4. Navigate to the UWP folder under root and open the 'Scene Understanding.sln' in Visual Studio.
5. Right-click on the 'Scene Understanding (Universal Windows)' project and click on 'Publish' --> 'Create App Packages'.
6. Run through the wizard and wait for building and packaging to complete. 
7. The built app package should be at 'UWP\AppPackages\Scene Understanding\Scene Understanding_*\Scene Understanding_*.[appx|msix|appxbundle|msixbundle]'
8. [Deploy](https://docs.microsoft.com/en-us/hololens/holographic-custom-apps) the package to a HoloLens 2. Ensure you build your application using ARM64, see the topic [Unity 2019.3 and HoloLens](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/BuildAndDeploy.html#unity-20193-and-hololens) for further details.
9. Launch the 'Scene Understanding' app from the 'All Apps' list on the HoloLens 2.

# Running on PC
To run this sample on the PC, please follow the instructions below:
1. Open any of the SceneUnderstanding Sample Scenes under Assets\SceneUnderstanding\Examples (Scenes are Under Placement, NavMesh and Understanding Folder)
2. Select the SceneUnderstandingManager game object and uncheck the 'Query Scene From Device' checkbox on the SceneUnderstandingManager Component
3. Ensure SU Serialized Scene Paths on the Scene Understanding component is referring to a serialized Scene Understanding scene, examples scenes are provided under the examples folder
4. Click 'Play' in the Editor.

# Additional Notes
This sample relies on NuGetForUnity package (https://github.com/GlitchEnzo/NuGetForUnity) to bring NuGet support inside Unity.  
When you first launch the sample in Unity, NuGetForUnity will restore the Microsoft.MixedReality.SceneUnderstanding NuGet package and place the contents under Assets\Packages.

Note that when running on your Hololens all the interactive commands are voice commands, you are require to speak to interact with the scene when running on Hololens.

Say 'Scene Objects Wireframe' , 'Load NavMesh' , 'Toggle Auto Refresh' etc. 


