// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Simple Scene Manager to have at the Home Scene, from here you can navigate to the
    /// example scene that you need
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        public void LoadUnderstanding()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Understanding-Simple");
        }

        public void LoadPlacement()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Placement-Simple");
        }

        public void LoadNavMesh()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("NavMesh-Simple");
        }
    }
}
