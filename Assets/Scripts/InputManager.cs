// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Windows.Speech;
    using UnityEngine.XR.WSA.Input;

    /// <summary>
    /// Deals with input in the form of speech or hand gestures, to control the various features and functionalities available within this sample.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Scene Understanding data provider component.
        /// </summary>
        [Tooltip("Scene Understanding data provider component.")]
        public SceneUnderstandingDataProvider SUDataProvider = null;

        /// <summary>
        /// Scene Understanding display manager component.
        /// </summary>
        [Tooltip("Scene Understanding display manager component.")]
        public SceneUnderstandingDisplayManager SUDisplayManager = null;

        /// <summary>
        /// Script that controls the displaying of the status text.
        /// </summary>
        [Tooltip("Script that controls the displaying of the status text.")]
        public UITextDisplay StatusText = null;

        /// <summary>
        /// Script that controls the displaying of the help text.
        /// </summary>
        [Tooltip("Script that controls the displaying of the help text.")]
        public UITextDisplay HelpText = null;

        /// <summary>
        /// Increments or decrements the radius by this amount.
        /// </summary>
        [Tooltip("Increments or decrements the radius by this amount.")]
        public float RadiusStep = 5f;

        /// <summary>
        /// Scale value to use for the minimap.
        /// </summary>
        [Tooltip("Scale value to use for the minimap.")]
        public float MinimapScale = 0.1f;

        // Keywords for speech input.
        [Tooltip("Display the latest set of scene objects from the Scene Understanding runtime.")]
        public string Keyword_Update = "update";

        [Tooltip("Disables auto refresh.")]
        public string Keyword_AutoRefreshOff = "auto refresh off";

        [Tooltip("Enables auto refresh, i.e. periodically displays the latest set of scene objects from the Scene Understanding runtime.")]
        public string Keyword_AutoRefreshOn = "auto refresh on";

        [Tooltip("Increases the bounding sphere radius, used for Scene Understanding environment query.")]
        public string Keyword_IncreaseRadius = "increase radius";

        [Tooltip("Decreases the bounding sphere radius, used for Scene Understanding environment query.")]
        public string Keyword_DecreaseRadius = "decrease radius";

        [Tooltip("Disables the displaying of scene objects (only world mesh is displayed).")]
        public string Keyword_SceneObjectsOff = "scene objects off";

        [Tooltip("Enables the displaying of scene objects.")]
        public string Keyword_SceneObjectsOn = "scene objects on";

        [Tooltip("Changes the visualization mode for scene objects to quad mode.")]
        public string Keyword_SceneObjectsQuad = "scene objects quad";

        [Tooltip("Changes the visualization mode for scene objects to quad mode, with the region mask applied.")]
        public string Keyword_SceneObjectsQuadMask = "scene objects mask";

        [Tooltip("Changes the visualization mode for scene objects to mesh mode.")]
        public string Keyword_SceneObjectsMesh = "scene objects mesh";

        [Tooltip("Changes the visualization mode for scene objects to wireframe mode.")]
        public string Keyword_SceneObjectsWireframe = "scene objects wireframe";

        [Tooltip("Disables scene objects that have inferred regions. Only observed scene objects are shown.")]
        public string Keyword_InferredRegionsOff = "inference off";

        [Tooltip("Enables scene objects with inferred regions. Both observed and inferred scene objects are shown.")]
        public string Keyword_InferredRegionsOn = "inference on";

        [Tooltip("Disables the world mesh.")]
        public string Keyword_WorldMeshOff = "mesh off";

        [Tooltip("Enables the world mesh.")]
        public string Keyword_WorldMeshOn = "mesh on";

        [Tooltip("Sets the Level of Detail for the world mesh to the lowest resolution, resulting in less triangles overall.")]
        public string Keyword_WorldMeshCoarse = "mesh coarse";

        [Tooltip("Sets the Level of Detail for the world mesh to the medium resolution.")]
        public string Keyword_WorldMeshMedium = "mesh medium";

        [Tooltip("Sets the Level of Detail for the world mesh to the highest resolution, resulting in many more triangles overall.")]
        public string Keyword_WorldMeshFine = "mesh fine";

        [Tooltip("Disables the displaying of large, horizontal scene objects, aka Platform.")]
        public string Keyword_PlatformOff = "platform off";

        [Tooltip("Enables the displaying of large, horizontal scene objects, aka Platform.")]
        public string Keyword_PlatformOn = "platform on";

        [Tooltip("Disables the displaying of background scene objects.")]
        public string Keyword_BackgroundOff = "background off";

        [Tooltip("Enables the displaying of background scene objects.")]
        public string Keyword_BackgroundOn = "background on";

        [Tooltip("Disables the displaying of unknown scene objects.")]
        public string Keyword_UnknownOff = "unknown off";

        [Tooltip("Enables the displaying of unknown scene objects.")]
        public string Keyword_UnknownOn = "unknown on";

        [Tooltip("Disables the displaying of completely inferred scene objects.")]
        public string Keyword_CompletelyInferredOff = "inferred off";

        [Tooltip("Enables the displaying of completely inferred scene objects.")]
        public string Keyword_CompletelyInferredOn = "inferred on";

        [Tooltip("Disables the minimap mode and renders the scene objects at real-world scale.")]
        public string Keyword_MinimapOff = "minimap off";

        [Tooltip("Enables the minimap mode. All scene objects are shrunk and rendered in front of the user.")]
        public string Keyword_MinimapOn = "minimap on";

        [Tooltip("Saves the current Scene Understanding scene to disk.")]
        public string Keyword_SaveDataToDisk = "save data";

        [Tooltip("Disables the displaying of Status Text.")]
        public string Keyword_StatusTextOff = "status off";

        [Tooltip("Enables the displaying of Status Text.")]
        public string Keyword_StatusTextOn = "status on";

        [Tooltip("Disables the displaying of Help Text.")]
        public string Keyword_HelpTextOff = "help off";

        [Tooltip("Enables the displaying of Help Text.")]
        public string Keyword_HelpTextOn = "help on";

        /// <summary>
        /// Component that recognizes hand gestures.
        /// </summary>
        private GestureRecognizer _gestureRecognizer;

        /// <summary>
        /// Component that recognizes speech phrases.
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// Root game object for the minimap.
        /// </summary>
        private GameObject _minimapGO;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            if (SUDataProvider == null)
            {
                Logger.LogWarning("InputManger.Start: SceneUnderstandingDataProvider component is not set on the InputManager. Input will not work.");
                return;
            }

            if (SUDisplayManager == null)
            {
                Logger.LogWarning("InputManger.Start: SceneUnderstandingDisplayManager component is not set on the InputManager. Input will not work.");
                return;
            }

            StatusText = StatusText == null ? GameObject.Find("StatusText").GetComponent<UITextDisplay>() : StatusText;
            HelpText = HelpText == null ? GameObject.Find("HelpText").GetComponent<UITextDisplay>() : HelpText;

            // Sets the help text on the UITextDisplay component.
            SetHelpText();
            // Hide the status text
            StatusText.Hide();

            // Place the camera in a particular position and add the camera movement script, if running on PC.
            if (SUDataProvider.RunOnDevice == false)
            {
                Camera.main.transform.position = new Vector3(0, 0, -5.0f);
                Camera.main.gameObject.AddComponent<CameraMovement>();
            }

            // Create, configure and start the gesture recognizer.
            _gestureRecognizer = new GestureRecognizer();
            _gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
            _gestureRecognizer.Tapped += GestureRecognizer_Tapped;
            _gestureRecognizer.StartCapturingGestures();

            string[] keywords = new string[]
            {
                Keyword_Update,
                Keyword_AutoRefreshOff,
                Keyword_AutoRefreshOn,
                Keyword_IncreaseRadius,
                Keyword_DecreaseRadius,
                Keyword_SceneObjectsOff,
                Keyword_SceneObjectsOn,
                Keyword_SceneObjectsQuad,
                Keyword_SceneObjectsQuadMask,
                Keyword_SceneObjectsMesh,
                Keyword_SceneObjectsWireframe,
                Keyword_InferredRegionsOff,
                Keyword_InferredRegionsOn,
                Keyword_WorldMeshOff,
                Keyword_WorldMeshOn,
                Keyword_WorldMeshCoarse,
                Keyword_WorldMeshMedium,
                Keyword_WorldMeshFine,
                Keyword_PlatformOff,
                Keyword_PlatformOn,
                Keyword_BackgroundOff,
                Keyword_BackgroundOn,
                Keyword_UnknownOff,
                Keyword_UnknownOn,
                Keyword_CompletelyInferredOff,
                Keyword_CompletelyInferredOn,
                Keyword_MinimapOff,
                Keyword_MinimapOn,
                Keyword_SaveDataToDisk,
                Keyword_StatusTextOff,
                Keyword_StatusTextOn,
                Keyword_HelpTextOff,
                Keyword_HelpTextOn
            };

            // Create, configure and start the keyword recognizer.
            _keywordRecognizer = new KeywordRecognizer(keywords);
            _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            _keywordRecognizer.Start();
        }

        /// <summary>
        /// Callback for the tap hand gesture.
        /// </summary>
        private void GestureRecognizer_Tapped(TappedEventArgs obj)
        {
            Logger.Log("InputManager.GestureRecognizer_Tapped: Tap recognized.");
            SUDisplayManager.StartDisplay();
        }

        /// <summary>
        /// Callback for the OnPhraseRecognized event, to deal with speech input.
        /// </summary>
        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            string arg = args.text;
            Logger.Log("InputManager.OnPhraseRecognized: Phrase '" + arg + "' recognized.");

            if (arg.Contains(Keyword_Update))
            {
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_AutoRefreshOff))
            {
                SUDisplayManager.StopAutoRefresh();
            }
            else if (arg.Contains(Keyword_AutoRefreshOn))
            {
                SUDisplayManager.StartAutoRefresh();
            }
            else if (arg.Contains(Keyword_DecreaseRadius))
            {
                SUDataProvider.BoundingSphereRadiusInMeters -= RadiusStep;
                SUDataProvider.BoundingSphereRadiusInMeters = Mathf.Clamp(SUDataProvider.BoundingSphereRadiusInMeters, SUDataProvider._minBoundingSphereRadiusInMeters, SUDataProvider._maxBoundingSphereRadiusInMeters);
            }
            else if (arg.Contains(Keyword_IncreaseRadius))
            {
                SUDataProvider.BoundingSphereRadiusInMeters += RadiusStep;
                SUDataProvider.BoundingSphereRadiusInMeters = Mathf.Clamp(SUDataProvider.BoundingSphereRadiusInMeters, SUDataProvider._minBoundingSphereRadiusInMeters, SUDataProvider._maxBoundingSphereRadiusInMeters);
            }
            else if (arg.Contains(Keyword_SceneObjectsOff))
            {
                SUDisplayManager.RenderSceneObjects = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_SceneObjectsOn))
            {
                SUDisplayManager.RenderSceneObjects = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_SceneObjectsQuad))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Quad;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_SceneObjectsQuadMask))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.QuadWithMask;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_SceneObjectsMesh))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Mesh;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_SceneObjectsWireframe))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Wireframe;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_InferredRegionsOff))
            {
                SUDataProvider.RequestInferredRegions = false;
            }
            else if (arg.Contains(Keyword_InferredRegionsOn))
            {
                SUDataProvider.RequestInferredRegions = true;
            }
            else if (arg.Contains(Keyword_WorldMeshOff))
            {
                SUDisplayManager.RenderWorldMesh = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_WorldMeshOn))
            {
                SUDisplayManager.RenderWorldMesh = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_WorldMeshCoarse))
            {
                SUDataProvider.WorldMeshLOD = SceneUnderstanding.SceneMeshLevelOfDetail.Coarse;
            }
            else if (arg.Contains(Keyword_WorldMeshMedium))
            {
                SUDataProvider.WorldMeshLOD = SceneUnderstanding.SceneMeshLevelOfDetail.Medium;
            }
            else if (arg.Contains(Keyword_WorldMeshFine))
            {
                SUDataProvider.WorldMeshLOD = SceneUnderstanding.SceneMeshLevelOfDetail.Fine;
            }
            else if (arg.Contains(Keyword_PlatformOff))
            {
                SUDisplayManager.RenderPlatformSceneObjects = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_PlatformOn))
            {
                SUDisplayManager.RenderPlatformSceneObjects = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_BackgroundOff))
            {
                SUDisplayManager.RenderBackgroundSceneObjects = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_BackgroundOn))
            {
                SUDisplayManager.RenderBackgroundSceneObjects = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_UnknownOff))
            {
                SUDisplayManager.RenderUnknownSceneObjects = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_UnknownOn))
            {
                SUDisplayManager.RenderUnknownSceneObjects = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_CompletelyInferredOff))
            {
                SUDisplayManager.RenderCompletelyInferredSceneObjects = false;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_CompletelyInferredOn))
            {
                SUDisplayManager.RenderCompletelyInferredSceneObjects = true;
                SUDisplayManager.StartDisplay();
            }
            else if (arg.Contains(Keyword_MinimapOff))
            {
                if (_minimapGO != null)
                {
                    DestroyImmediate(_minimapGO);
                    _minimapGO = null;
                }
                SUDisplayManager.SceneRoot.SetActive(true);
            }
            else if (arg.Contains(Keyword_MinimapOn))
            {
                if (_minimapGO == null)
                {
                    _minimapGO = Instantiate(SUDisplayManager.SceneRoot);
                    _minimapGO.name = "Minimap";
                    _minimapGO.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                    _minimapGO.transform.localScale = new Vector3(MinimapScale, MinimapScale, MinimapScale);

                    SUDisplayManager.SceneRoot.SetActive(false);
                }
            }
            else if (arg.Contains(Keyword_SaveDataToDisk))
            {
                Task.Run(async () => 
                {
                    byte[] serializedScene = SUDataProvider.GetLatestSerializedScene().Item2;
                    await SceneUnderstandingSaveDataUtils.SaveBytesToDiskAsync(serializedScene);
                    await SceneUnderstandingSaveDataUtils.SaveObjsToDiskAsync(serializedScene);
                }).ConfigureAwait(false);
            }
            else if (arg.Contains(Keyword_StatusTextOff))
            {
                StatusText.Hide();
            }
            else if (arg.Contains(Keyword_StatusTextOn))
            {
                StatusText.Show();
            }
            else if (arg.Contains(Keyword_HelpTextOff))
            {
                HelpText.Hide();
            }
            else if (arg.Contains(Keyword_HelpTextOn))
            {
                HelpText.Show();
            }
        }

        /// <summary>
        /// Handles keyboard input, for PC path only.
        /// </summary>
        private void Update()
        {
            // If running on device, nothing to do here.
            if (SUDataProvider.RunOnDevice)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SUDisplayManager.RenderSceneObjects = !SUDisplayManager.RenderSceneObjects;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Quad;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Mesh;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SUDisplayManager.SceneObjectVisualizationMode = VisualizationMode.Wireframe;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SUDisplayManager.RenderPlatformSceneObjects = !SUDisplayManager.RenderPlatformSceneObjects;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SUDisplayManager.RenderBackgroundSceneObjects = !SUDisplayManager.RenderBackgroundSceneObjects;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SUDisplayManager.RenderUnknownSceneObjects = !SUDisplayManager.RenderUnknownSceneObjects;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SUDisplayManager.RenderCompletelyInferredSceneObjects = !SUDisplayManager.RenderCompletelyInferredSceneObjects;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SUDisplayManager.RenderWorldMesh = !SUDisplayManager.RenderWorldMesh;
                SUDisplayManager.StartDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                if (HelpText.gameObject.activeSelf)
                {
                    HelpText.Hide();
                }
                else
                {
                    HelpText.Show();
                }
            }
        }
        
        /// <summary>
        /// Sets the help text on the HelpText component.
        /// </summary>
        private void SetHelpText()
        {
            if (HelpText == null)
            {
                return;
            }

            string helpText = string.Empty;

            if (SUDataProvider.RunOnDevice)
            {
                helpText = string.Format(
                @"
    Welcome to the Scene Understanding App!

    This app displays scene objects from the scene understanding runtime, e.g. walls, floors, ceilings, etc.
        
    Speech Commands:
                
        '{0}' or tap - display the latest data
        '{1}'/'{2}' - enable/disable auto refresh

        '{3}'/'{4}' - enable/disable scene objects
        '{5}' - enable quad mode
        '{6}' - enable default (mesh) mode
        '{7}' - enable wireframe mode

        '{8}'/'{9}' - enable/disable large horizontal surfaces (aka platform)
        '{10}'/'{11}' - enable/disable background objects
        '{12}'/'{13}' - enable/disable unknown objects
        '{14}'/'{15}' - enable/disable completely inferred objects
                
        '{16}'/'{17}' - enable/disable world mesh
        '{18}', '{19}' or '{20}' - change world mesh LOD
                
        '{21}'/'{22}' - enable/disable minimap mode (do try this out :))

        '{23}'/'{24}' - increase/decrease radius of the sphere around the camera, which is used when querying the environment

        '{25}' - save current scene to disk

        '{26}'/'{27}' - enable/disable this help menu
            ",
                Keyword_Update,
                Keyword_AutoRefreshOn,
                Keyword_AutoRefreshOff,
                Keyword_SceneObjectsOn,
                Keyword_SceneObjectsOff,
                Keyword_SceneObjectsQuad,
                Keyword_SceneObjectsMesh,
                Keyword_SceneObjectsWireframe,
                Keyword_PlatformOn,
                Keyword_PlatformOff,
                Keyword_BackgroundOn,
                Keyword_BackgroundOff,
                Keyword_UnknownOn,
                Keyword_UnknownOff,
                Keyword_CompletelyInferredOn,
                Keyword_CompletelyInferredOff,
                Keyword_WorldMeshOn,
                Keyword_WorldMeshOff,
                Keyword_WorldMeshCoarse,
                Keyword_WorldMeshMedium,
                Keyword_WorldMeshFine,
                Keyword_MinimapOn,
                Keyword_MinimapOff,
                Keyword_IncreaseRadius,
                Keyword_DecreaseRadius,
                Keyword_SaveDataToDisk,
                Keyword_HelpTextOn,
                Keyword_HelpTextOff
                );
            }
            else
            {
                helpText =
                @"
    Welcome to the Scene Understanding App!

    This app displays scene objects from the scene understanding runtime, e.g. walls, floors, ceilings, etc.
        
    Input Controls:
                
        'W', 'A', 'S', 'D', 'Q', 'E' - change camera position (hold Shift to speed up)
        Mouse primary button + move mouse - change camera orientation
        'F' - focus on a scene object
        'R' - move camera back to origin
                    
        '1' - enable/disable scene objects
        '2' - enable quad mode
        '3' - enable default (mesh) mode
        '4' - enable wireframe mode
                    
        '5' - enable/disable large horizontal surfaces (aka platform)
        '6' - enable/disable background objects
        '7' - enable/disable unknown objects
        '8' - enable/disable completely inferred objects
                
        '9' - enable/disable world mesh

        'H' - enable/disable this help menu
            ";
            }

            HelpText.Clear();
            HelpText.Append(helpText);
        }
    }
}