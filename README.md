# Microsoft.MixedReality.SceneUnderstanding.Samples - UnitySample
A Unity-based sample application that showcases Scene Understanding on HoloLens 2.  
When this sample is deployed on a HoloLens, it will show the virtual representation of your real environment.  
When this sample is deployed on a PC, it will load a serialized scene (included under Resources\SerializedScenesForPCPath) and display it.  
A help menu is presented on launch, which provides information about all the input commands available in the application.  
To learn more about Scene Understanding, please visit this link: https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding.  
To learn more about Scene Understanding SDK, please visit this link: https://docs.microsoft.com/en-us/windows/mixed-reality/scene-understanding-sdk.  

# Prerequisites
Unity 2018.4.12.* or greater.  
Visual Studio 2017 or 2019 with Universal Windows Platform components.  
Windows SDK version 10.0.18362.0 or greater.  

# Running on HoloLens 2
To run this sample on the HoloLens 2, please follow the instructions below:
1. Open the SceneUnderstandingSample scene in Assets\Scenes, if not already open.
2. Select the SceneUnderstandingManager game object and make sure that 'Run On Device' checkbox is checked on the Scene Understanding Data Provider component.
3. In the Menu, click on Build --> UWP. Once the build completes successfully, a log indicating this will show up in the output console.
4. Navigate to the UWP folder under root and open the 'Scene Understanding.sln' in Visual Studio.
5. Right-click on the 'Scene Understanding (Universal Windows)' project and click on 'Publish' --> 'Create App Packages'.
6. Run through the wizard and wait for building and packaging to complete. 
7. The built app package should be at 'UWP\AppPackages\Scene Understanding\Scene Understanding_*\Scene Understanding_*.[appx|msix|appxbundle|msixbundle]'
8. Deploy the package to a HoloLens 2.
9. Launch the 'Scene Understanding' app from the 'All Apps' list on the HoloLens 2.

# Running on PC
To run this sample on the PC, please follow the instructions below:
1. Open the SceneUnderstandingSample scene in Assets\Scenes, if not already open.
2. Select the SceneUnderstandingManager game object and uncheck the 'Run On Device' checkbox on the Scene Understanding Data Provider component.
3. Ensure SU Serialized Scene Path on the Scene Understanding Data Provider component is referring to a serialized Scene Understanding scene.
4. Click 'Play' in the Editor.

# Additional Notes
This sample relies on NuGetForUnity package (https://github.com/GlitchEnzo/NuGetForUnity) to bring NuGet support inside Unity.  
When you first launch the sample in Unity, NuGetForUnity will restore the Microsoft.MixedReality.SceneUnderstanding NuGet package and place the contents under Assets\Packages.
