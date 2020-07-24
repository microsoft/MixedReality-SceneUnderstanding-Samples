using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    public class ActionMenuItem : MonoBehaviour
    {
        #region Unity Inspector Fields
        [Tooltip("The action that the menu item represents.")]
        [SerializeField]
        private InputAction action;

        [Tooltip("The label for the keyboard key to press.")]
        public TextMeshPro keyText;

        [Tooltip("The label for the phrase to speak.")]
        public TextMeshPro phraseText;

        [Tooltip("The label for a description of the action.")]
        public TextMeshPro descriptionText;
        #endregion // Unity Inspector Fields

        #region Internal Methods
        /// <summary>
        /// Gets a friendly name for the specified key.
        /// </summary>
        static private string GetKeyName(KeyCode? key)
        {
            switch (key)
            {
                case null:
                case KeyCode.None:
                    return string.Empty;
                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    return key.ToString().Substring(5);
                default:
                    return key.ToString();
            }
        }

        /// <summary>
        /// Updates UI controls to match the action.
        /// </summary>
        private void UpdateUI()
        {
            keyText.text = GetKeyName(action?.Key);
            phraseText.text = action?.Phrase;
            descriptionText.text = action?.Description;
        }
        #endregion // Internal Methods

        #region Unity Overrides
        // Start is called before the first frame update
        private void Start()
        {
            if (action != null) { UpdateUI(); }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the action that the menu item represents.
        /// </summary>
        public InputAction Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    UpdateUI();
                }
            }
        }
        #endregion // Public Properties
    }
}