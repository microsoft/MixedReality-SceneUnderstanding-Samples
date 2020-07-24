// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.MixedReality.SceneUnderstanding.Samples.Unity
{
    using UnityEngine;

    /// <summary>
    /// Simple, first person camera for the PC path.
    /// </summary>
    public class CameraMovement : MonoBehaviour
    {
        /// <summary>
        /// Speed at which the camera will translate.
        /// </summary>
        [Tooltip("Speed at which the camera will translate.")]
        public float TranslationSpeed = 0.24f;
        
        /// <summary>
        /// Translation speed multiplier when Shift key is pressed.
        /// </summary>
        [Tooltip("Translation speed multiplier when Shift key is pressed.")]
        public float TranslationShiftMultiplier = 3.0f;

        /// <summary>
        /// Speed at which the camera will rotate.
        /// </summary>
        [Tooltip("Speed at which the camera will rotate.")]
        public float RotationSpeed = 5f;

        /// <summary>
        /// Controls the magnitude of the step taken during the lerp.
        /// </summary>
        private readonly float _translationLerpSpeed = 5f;

        /// <summary>
        /// Controls the magnitude of the step taken during the lerp.
        /// </summary>
        private readonly float _rotationLerpSpeed = 7f;

        /// <summary>
        /// Temporary variable used to hold the translation data.
        /// </summary>
        private Vector3 _desiredPosition;
        
        // Temporary variables used to hold the rotation data.
        private float _desiredRotationX = 0;
        private float _desiredRotationY = 0;
        private Quaternion _desiredRotation;

        // Minimum, maximum rotation angles for the X and Y axes.
        private readonly float _minimumXRotation = -360.0f;
        private readonly float _maximumXRotation = 360.0f;
        private readonly float _minimumYRotation = -90.0f;
        private readonly float _maximumYRotation = 90.0f;

        /// <summary>
        /// Distance from the object where the camera will stop, when the focus 'F' key is pressed.
        /// </summary>
        private readonly float _focusDistance = 0.5f;
        
        /// <summary>
        /// Ray cast distance used during focus.
        /// </summary>
        private readonly float _raycastMaxDistance = 100.0f;
        
        /// <summary>
        /// Holds the camera position at the start, to allow resetting to the original position.
        /// </summary>
        private Vector3 _originalCameraPosition;

        /// <summary>
        /// Holds the camera orientation at the start, to allow resetting to the original orientation.
        /// </summary>
        private Quaternion _originalCameraRotation;

        private void Start()
        {
            _desiredPosition = transform.localPosition;
            _desiredRotation = transform.localRotation;
            // Hold the original position and rotation to allow for resetting later.
            _originalCameraPosition = transform.localPosition;
            _originalCameraRotation = transform.localRotation;
        }

        /// <summary>
        /// Updates the camera based on user input.
        /// </summary>
        private void LateUpdate()
        {
            UpdateDesiredPosition();
            UpdateDesiredRotation();
            CheckForFocus();
            CheckForReset();

            transform.localPosition = Vector3.Lerp(transform.localPosition, _desiredPosition, Time.deltaTime * _translationLerpSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _desiredRotation, Time.deltaTime * _rotationLerpSpeed);
        }

        /// <summary>
        /// Computes the new desired position, based on user input.
        /// </summary>
        private void UpdateDesiredPosition()
        {
            float shiftMultiplier = 1.0f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                shiftMultiplier = TranslationShiftMultiplier;
            }

            Vector3 forward = Vector3.zero;
            Vector3 up = Vector3.zero;
            Vector3 right = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                forward = transform.localRotation * Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                forward = transform.localRotation * Vector3.back;
            }
            if (Input.GetKey(KeyCode.E))
            {
                up = Vector3.up;    // World up
            }
            if (Input.GetKey(KeyCode.Q))
            {
                up = -Vector3.up;   // World down
            }
            if (Input.GetKey(KeyCode.D))
            {
                right = transform.right;
            }
            if (Input.GetKey(KeyCode.A))
            {
                right = -transform.right;
            }

            if (forward != Vector3.zero || up != Vector3.zero || right != Vector3.zero)
            {
                Vector3 inputMovement = forward + up + right;
                _desiredPosition = transform.localPosition + (inputMovement * shiftMultiplier * TranslationSpeed);
            }
        }

        /// <summary>
        /// Computes the new desired orientation, based on user input.
        /// </summary>
        private void UpdateDesiredRotation()
        {
            if (Input.GetMouseButton(0))
            {
                _desiredRotationX += Input.GetAxis("Mouse X") * RotationSpeed;
                _desiredRotationY += Input.GetAxis("Mouse Y") * RotationSpeed;

                _desiredRotationX = ClampAngle(_desiredRotationX, _minimumXRotation, _maximumXRotation);
                _desiredRotationY = ClampAngle(_desiredRotationY, _minimumYRotation, _maximumYRotation);
                
                _desiredRotation = Quaternion.AngleAxis(_desiredRotationX, Vector3.up) * Quaternion.AngleAxis(_desiredRotationY, Vector3.left);
            }
        }

        /// <summary>
        /// Handles quick movement across scene objects, using the 'F' key.
        /// </summary>
        private void CheckForFocus()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, _raycastMaxDistance))
                {
                    _desiredPosition = hit.point - (ray.direction * _focusDistance);
                }
            }
        }

        /// <summary>
        /// Handles resetting the camera to the original position and orientation.
        /// </summary>
        private void CheckForReset()
        {
            // Reset to original position and orientation.
            if (Input.GetKeyDown(KeyCode.R))
            {
                _desiredPosition = _originalCameraPosition;
                _desiredRotation = _originalCameraRotation;
                _desiredRotationX = 0.0f;
                _desiredRotationY = 0.0f;

                transform.localPosition = _originalCameraPosition;
                transform.localRotation = _originalCameraRotation;
            }
        }

        /// <summary>
        /// Helper to clamp an angle between min and max values.
        /// </summary>
        /// <param name="angle">Angle to clamp.</param>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max Value.</param>
        /// <returns>Clamped angle.</returns>
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }

            return Mathf.Clamp(angle, min, max);
        }
    }
}
