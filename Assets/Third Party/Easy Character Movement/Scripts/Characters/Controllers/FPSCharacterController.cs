using System;
using UnityEngine;

namespace ECM.Characters.Controllers
{
    /// <summary>
    /// FPS Character Controller
    /// 
    /// Inherits from 'BaseCharacterController' and extends it to allow a FPS control,
    /// where you moves relative to camera's view direction at different speeds (forward, backward, strafe) based on input.
    /// 
    /// </summary>

    public sealed class FPSCharacterController : BaseCharacterController
    {
        #region NESTED CLASS MouseLook

        [Serializable]
        public class MouseLook
        {
            public float XSensitivity = 2f;
            public float YSensitivity = 2f;
            public bool clampVerticalRotation = true;
            public float MinimumX = -90F;
            public float MaximumX = 90F;
            public bool smooth;
            public float smoothTime = 5f;
            public bool lockCursor = true;

            private Quaternion m_CharacterTargetRot;
            private Quaternion m_CameraTargetRot;
            private bool m_cursorIsLocked = true;

            public void Init(Transform character, Transform camera)
            {
                m_CharacterTargetRot = character.localRotation;
                m_CameraTargetRot = camera.localRotation;
            }

            public void LookRotation(CharacterMovement character, Transform camera)
            {
                var yRot = Input.GetAxis("Mouse X") * XSensitivity;
                var xRot = Input.GetAxis("Mouse Y") * YSensitivity;

                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (clampVerticalRotation)
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

                if (smooth)
                {
                    character.rotation = Quaternion.Slerp(character.rotation, m_CharacterTargetRot,
                        smoothTime * Time.deltaTime);

                    camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                        smoothTime * Time.deltaTime);
                }
                else
                {
                    character.rotation = m_CharacterTargetRot;

                    camera.localRotation = m_CameraTargetRot;
                }

                UpdateCursorLock();
            }

            public void SetCursorLock(bool value)
            {
                lockCursor = value;
                if (lockCursor)
                    return;

                //we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            public void UpdateCursorLock()
            {
                //if the user set "lockCursor" we check & properly lock the cursors
                if (lockCursor)
                    InternalLockUpdate();
            }

            private void InternalLockUpdate()
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    m_cursorIsLocked = false;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    m_cursorIsLocked = true;
                }

                if (m_cursorIsLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else if (!m_cursorIsLocked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            Quaternion ClampRotationAroundXAxis(Quaternion q)
            {
                q.x /= q.w;
                q.y /= q.w;
                q.z /= q.w;
                q.w = 1.0f;

                var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

                angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

                q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

                return q;
            }

        }

        #endregion

        #region EDITOR EXPOSED FIELDS

        [Header("FPS")]
        [SerializeField]
        private float _backwardSpeed = 2.5f;

        [SerializeField]
        private float _strafeSpeed = 3.5f;

        /// <summary>
        /// Unity's Standard Assets MouseLook
        /// </summary>

        [Space(10.0f)]
        public MouseLook mouseLook = new MouseLook();

        #endregion

        #region FIELDS

        private Camera _camera;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Maximum backward movement speed (in m/s).
        /// </summary>

        public float backwardSpeed
        {
            get { return _backwardSpeed; }
            set { _backwardSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Maximum sides movement speed (in m/s).
        /// </summary>

        public float strafeSpeed
        {
            get { return _strafeSpeed; }
            set { _strafeSpeed = Mathf.Max(0.0f, value); }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Get target speed, relative to input.
        /// </summary>

        private float GetTargetSpeed()
        {
            var targetSpeed = speed;

            if (moveDirection.x > 0.0f || moveDirection.x < 0.0f)
                targetSpeed = strafeSpeed;

            if (moveDirection.z < 0.0f)
                targetSpeed = backwardSpeed;
            else if (moveDirection.z > 0.0f)
                targetSpeed = speed;

            return targetSpeed;
        }

        #endregion

        #region MONOBEHAVIOUR

        public override void OnValidate()
        {
            base.OnValidate();

            backwardSpeed = _backwardSpeed;
            strafeSpeed = _strafeSpeed;
        }

        public override void Awake()
        {
            base.Awake();

            // Cache and initialize components

            _camera = GetComponentInChildren<Camera>();
            if (_camera != null)
                mouseLook.Init(transform, _camera.transform);
            else
                Debug.LogError("FPSCharacterController: No 'Camera' found. Please parent a camera to " + name +
                               " game object.");
        }

        public override void FixedUpdate()
        {
            // Apply movement relative to view direction

            var targetSpeed = GetTargetSpeed();

            var desiredVelocity = Vector3.ClampMagnitude(moveDirection * targetSpeed, targetSpeed);
            movement.Move(transform.TransformDirection(desiredVelocity), acceleration, deceleration);

            // Apply braking drag

            if (Mathf.Approximately(desiredVelocity.sqrMagnitude, 0.0f))
                movement.ApplyDrag(brakingFriction);

            // Apply Jump

            Jump();
            UpdateJumpTimer();
        }

        public override void Update()
        {
            // Handle input

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            jump = Input.GetButton("Jump");

            // Handle mouse look

            if (_camera != null)
                mouseLook.LookRotation(movement, _camera.transform);
        }

        #endregion
    }
}