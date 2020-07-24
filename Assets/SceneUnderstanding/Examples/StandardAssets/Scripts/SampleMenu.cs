namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using TMPro;
    using UnityEngine;

    public class SampleMenu : MonoBehaviour
    {
        #region Unity Inspector Fields
        public SampleInputManager suInput;
        public TextMeshPro menutext;
        public GameObject visuals;
        public Transform actionContainer;
        public GameObject editorInputPrefab;
        public GameObject actionItemPrefab;
        #endregion // Unity Inspector Fields

        private GameObject AddEntry(GameObject prefab, ref float top)
        {
            // Create the prefab
            GameObject entry = GameObject.Instantiate(prefab);

            // Parent it
            entry.transform.SetParent(actionContainer, worldPositionStays: false);

            // Get the rect transform
            RectTransform rTrans = entry.GetComponent<RectTransform>();

            // Move the top
            rTrans.localPosition += new Vector3(0, top, 0);

            // Update where the next top will be (with some padding between each item)
            top -= rTrans.rect.height + 3f;

            // Return the created item
            return entry;
        }

        private void Start()
        {
            if (suInput == null) { suInput = this.gameObject.GetComponent<SampleInputManager>(); }
            if (menutext == null) { menutext = this.gameObject.GetComponentInChildren<TextMeshPro>(); }

            // Listen to future changes to commands
            suInput.InputEnabled.AddListener(UpdateCommands);

            // Update commands now
            UpdateCommands();

            // Appear on start
            Show();
        }

        public void Show()
        {
            visuals.SetActive(true);
            visuals.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.75f);

            //Visuals foward vector is reversed, do a look from Camera to visuals to fix it.
            visuals.transform.rotation = Quaternion.LookRotation(visuals.transform.position - Camera.main.transform.position);
        }

        public void Hide()
        {
            visuals.SetActive(false);
        }

        public void Toggle()
        {
            if (IsMenuActive)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void UpdateCommands()
        {
            // Clear any existing actions
            while (actionContainer.childCount > 0)
            {
                Transform child = actionContainer.GetChild(0);
                Destroy(child.gameObject);
                child.SetParent(null, false);
            }

            // If there's no manager, bail
            if (suInput.SuManager == null) { return; }

            // Are we running on device?
            bool onDevice = suInput.SuManager.QuerySceneFromDevice;

            // The location of the top of the next item
            float nextTop = 0f;

            if (!onDevice)
            {
                // If not running on the device, add the special menu item to show editor-only input
                AddEntry(editorInputPrefab, ref nextTop);

                // Add a gap
                nextTop -= 2f;
            }

            // Now display all input actions
            foreach (InputAction ia in suInput.InputActions)
            {
                // Create the menu item
                GameObject actionEntry = AddEntry(actionItemPrefab, ref nextTop);

                // Update it's name, just to be nice
                actionEntry.name = ia.Phrase;

                // Update it to point at the input action. This will update the text to match.
                actionEntry.GetComponent<ActionMenuItem>().Action = ia;
            }
        }

        public bool IsMenuActive { get { return (visuals.activeSelf); } }
    }
}