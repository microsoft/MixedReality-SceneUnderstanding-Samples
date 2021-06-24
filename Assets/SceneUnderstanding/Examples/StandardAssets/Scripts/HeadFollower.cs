using UnityEngine;

namespace Scanning.Utils
{
    /// <summary>
    /// Implements a basic follower that tracks the users head smoothly.
    /// </summary>
    public class HeadFollower : MonoBehaviour
    {
        [SerializeField]
        private Transform headTransform = null;

        [SerializeField]
        private Transform offsetTransform = null;

        [SerializeField]
        private float distanceToHead = 1.5f;

        [SerializeField]
        private float interpolateSpeed = 10.0f;

        [SerializeField]
        private bool ignoreRollRotation = true;

        private bool snapToTarget;
        private float currentDistance;

        public Transform HeadTransform
        {
            get { return headTransform; }
        }

        private void OnEnable()
        {
            snapToTarget = true;
        }

        private void Update()
        {
            if (headTransform == null && Camera.main != null)
            {
                headTransform = Camera.main.transform;
            }

            UpdateTransform();
        }

        public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt)
        {
            return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }

        public static Quaternion Damp(Quaternion source, Quaternion target, float smoothing, float dt)
        {
            return Quaternion.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }

        public void UpdateTransform()
        {
            if (headTransform != null)
            {
                Quaternion targetRotation = headTransform.rotation;
                if (ignoreRollRotation)
                {
                    Vector3 cameraForward = (targetRotation * Vector3.forward);
                    cameraForward.y = 0;
                    targetRotation = Quaternion.LookRotation(cameraForward);
                }

                transform.position = Damp(transform.position, headTransform.position, 0.5f, Time.deltaTime * interpolateSpeed);
                transform.rotation = Damp(transform.rotation, targetRotation, 0.5f, Time.deltaTime * interpolateSpeed);

                if (snapToTarget)
                {
                    snapToTarget = false;
                    transform.position = headTransform.position;
                    transform.rotation = headTransform.rotation;
                }
            }
            
            if (offsetTransform != null)
            {
                offsetTransform.localPosition = new Vector3(0, 0, distanceToHead);
            }
        }
    }
}
