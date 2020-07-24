namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This component holds all the relevant SceneUnderstanding Information in a Mono Behaviour
    /// Attached to the GameObject that represents its corresponding Scene Understanding Object
    /// </summary>
    public class SceneUnderstandingProperties : MonoBehaviour
    {
        [HideInInspector]
        public SceneUnderstanding.SceneObjectKind suKind;
        public SceneUnderstanding.SceneObject suObject;
    }
}
