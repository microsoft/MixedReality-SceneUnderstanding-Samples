// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEngine;
    
    /// <summary>
    /// Helps display UI text.
    /// </summary>
    public class UITextDisplay : MonoBehaviour
    {
        /// <summary>
        /// TextMesh component for displaying the text.
        /// </summary>
        [Tooltip("TextMesh component for displaying the text.")]
        public TextMesh MainTextMesh = null;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Awake()
        {
            MainTextMesh = gameObject.GetComponent<TextMesh>();
            if (MainTextMesh == null)
            {
                MainTextMesh = gameObject.AddComponent<TextMesh>();
            }
        }

        /// <summary>
        /// Activates the game object associated with this component and places it in front of the camera.
        /// </summary>
        public void Show()
        {
            gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 1.5f);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates the game object associated with this component.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Clears all text.
        /// </summary>
        public void Clear()
        {
            MainTextMesh.text = string.Empty;
        }

        /// <summary>
        /// Appends to existing text.
        /// </summary>
        /// <param name="text">String to append.</param>
        public void Append(string text)
        {
            MainTextMesh.text += text;
            MainTextMesh.text += System.Environment.NewLine;
            Logger.Log(text);
        }
    }
}