using UnityEngine;

namespace ECM.Characters.Controllers
{
    /// <summary>
    /// Base Character Controller
    /// 
    /// This is the default controller and base of the other controllers I've included.
    /// It handles keyboard input, and allows for a variable height jump.
    /// 
    /// </summary>

    public class BaseCharacterController : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Steering")]
        [Tooltip("Maximum movement speed (in m/s).")]
        [SerializeField]
        private float _speed = 5.0f;

        [Tooltip("Maximum turning speed (in deg/s).")]
        [SerializeField]
        private float _angularSpeed = 240.0f;

        [Tooltip("The rate of change of velocity.")]
        [SerializeField]
        private float _acceleration = 20.0f;

        [Tooltip("The rate at which the character's slows down.")]
        [SerializeField]
        private float _deceleration = 20.0f;

        [Tooltip("Friction (drag) coefficient applied when braking (whenever desiredVelocity ~ 0).\n" +
                 "Braking is composed of friction (velocity-dependent drag) and constant deceleration.")]
        [Range(0.0f, 2.0f)]
        [SerializeField]
        private float _brakingFriction = 0.6f;

        [Tooltip("When not grounded, the amount of lateral movement control available to the character.\n" +
                 "0 == no control, 1 == full control.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float _airControl = 0.2f;

        [Header("Jump")]
        [Tooltip("The initial jump height (in meters).")]
        [SerializeField]
        private float _baseJumpHeight = 2.0f;

        [Tooltip("The extra jump time (e.g. holding jump button) in seconds.")]
        [SerializeField]
        private float _extraJumpTime = 0.5f;

        [Tooltip("Acceleration while jump button is held down, given in meters / sec^2.")]
        [SerializeField]
        private float _extraJumpPower = 10.0f;

        [Tooltip("How early before hitting the ground you can press jump, and still perform the jump.\n" +
                 "Typical values goes from 0.05f to 0.5f.")]
        [SerializeField]
        private float _jumpToleranceTime = 0.15f;

        #endregion

        #region FIELDS

        private bool _jump;
        protected bool _canJump = true;
        private bool _isJumping;

        protected bool _updateJumpTimer;
        protected float _jumpTimer;

        protected float _jumpButtonHeldDownTimer;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>

        protected CharacterMovement movement { get; private set; }

        /// <summary>
        /// Maximum movement speed (in m/s).
        /// </summary>

        public float speed
        {
            get { return _speed; }
            set { _speed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Maximum turning speed (in deg/s).
        /// </summary>

        public float angularSpeed
        {
            get { return _angularSpeed; }
            set { _angularSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The rate of change of velocity.
        /// </summary>

        public float acceleration
        {
            get { return movement.isGrounded ? _acceleration : _acceleration * airControl; }
            set { _acceleration = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The rate at which the character's slows down.
        /// </summary>

        public float deceleration
        {
            get { return movement.isGrounded ? _deceleration : _deceleration * airControl; }
            set { _deceleration = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Friction (drag) coefficient applied when braking (whenever desiredVelocity ~ 0).
        /// When braking, this property allows you to control how much friction is applied when moving across the ground,
        /// applying an opposing force that scales with current velocity.
        /// 
        /// Braking is composed of friction (velocity-dependent drag) and constant deceleration.
        /// </summary>

        public float brakingFriction
        {
            get { return movement.isGrounded ? _brakingFriction : 0.0f; }
            set { _brakingFriction = Mathf.Clamp(value, 0.0f, 2.0f); }
        }

        /// <summary>
        /// When not grounded, the amount of lateral movement control available to the character.
        /// 0 == no control, 1 == full control.
        /// </summary>

        public float airControl
        {
            get { return _airControl; }
            set { _airControl = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// The initial jump height (in meters).
        /// </summary>

        public float baseJumpHeight
        {
            get { return _baseJumpHeight; }
            set { _baseJumpHeight = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Computed jump impulse.
        /// </summary>

        public float jumpImpulse
        {
            get { return Mathf.Sqrt(2.0f * baseJumpHeight * movement.gravity); }
        }

        /// <summary>
        /// The extra jump time (e.g. holding jump button) in seconds.
        /// </summary>

        public float extraJumpTime
        {
            get { return _extraJumpTime; }
            set { _extraJumpTime = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Acceleration while jump button is held down, given in meters / sec^2.
        /// </summary>

        public float extraJumpPower
        {
            get { return _extraJumpPower; }
            set { _extraJumpPower = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// How early before hitting the ground you can press jump, and still perform the jump.
        /// Typical values goes from 0.05f to 0.5f, the higher the value, the easier to "chain" jumps and vice-versa.
        /// </summary>

        public float jumpToleranceTime
        {
            get { return _jumpToleranceTime; }
            set { _jumpToleranceTime = Mathf.Clamp(value, 0.0f, 1.0f); }
        }

        /// <summary>
        /// Jump input command.
        /// </summary>

        public bool jump
        {
            get { return _jump; }
            set
            {
                // If jump is released, allow to jump again

                if (_jump && value == false)
                {
                    _canJump = true;
                    _jumpButtonHeldDownTimer = 0.0f;
                }

                // Update jump value; if pressed, update held down timer

                _jump = value;
                if (_jump)
                    _jumpButtonHeldDownTimer += Time.deltaTime;
            }
        }

        /// <summary>
        /// True if character is jumping, false if not.
        /// </summary>

        public bool isJumping
        {
            get
            {
                // We are in jump mode but just became grounded

                if (_isJumping)
                    return !movement.isGrounded;

                return _isJumping;
            }
        }

        /// <summary>
        /// Movement input command.
        /// </summary>

        public Vector3 moveDirection { get; set; }

        #endregion

        #region METHODS

        protected void Jump()
        {
            // Is jump button released?

            if (!_canJump)
                return;

            // Jump button not pressed or not in ground, return

            if (!jump || !movement.isGrounded)
                return;

            // Is jump button pressed within jump tolerance?

            if (_jumpButtonHeldDownTimer > jumpToleranceTime)
                return;

            // Apply jump impulse

            _canJump = false;
            _isJumping = true;
            _updateJumpTimer = true;

            movement.ApplyVerticalImpulse(jumpImpulse);
        }

        protected void UpdateJumpTimer()
        {
            if (!_updateJumpTimer)
                return;

            // If jump button is held down and extra jump time is not exceeded...

            if (jump && _jumpTimer < extraJumpTime)
            {
                // Calculate how far through the extra jump time we are (jumpProgress),

                var jumpProgress = _jumpTimer / _extraJumpTime;

                // Apply proportional extra jump power (acceleration) to simulate variable height jump,
                // thi method offers better control and less 'floaty' feel.

                var proportionalJumpPower = Mathf.Lerp(extraJumpPower, 0f, jumpProgress);
                movement.ApplyForce(Vector3.up * proportionalJumpPower, ForceMode.Acceleration);

                // Update jump timer

                _jumpTimer = Mathf.Min(_jumpTimer + Time.deltaTime, extraJumpTime);
            }
            else
            {
                // Button released or extra jump time ends, reset info

                _jumpTimer = 0.0f;
                _updateJumpTimer = false;
            }
        }

        #endregion

        #region MONOBEHAVIOUR

        public virtual void OnValidate()
        {
            speed = _speed;
            angularSpeed = _angularSpeed;

            acceleration = _acceleration;
            deceleration = _deceleration;
            brakingFriction = _brakingFriction;

            airControl = _airControl;

            baseJumpHeight = _baseJumpHeight;
            extraJumpTime = _extraJumpTime;
            extraJumpPower = _extraJumpPower;
            jumpToleranceTime = _jumpToleranceTime;
        }

        public virtual void Awake()
        {
            // Cache components

            movement = GetComponent<CharacterMovement>();
        }

        public virtual void FixedUpdate()
        {
            // Apply movement

            var desiredVelocity = Vector3.ClampMagnitude(moveDirection * speed, speed);
            movement.Move(desiredVelocity, acceleration, deceleration);

            // Apply braking drag

            if (Mathf.Approximately(moveDirection.sqrMagnitude, 0.0f))
                movement.ApplyDrag(brakingFriction);

            // Apply Jump

            Jump();
            UpdateJumpTimer();
        }

        public virtual void Update()
        {
            // Handle input

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            jump = Input.GetButton("Jump");

            // Rotate towards input movement direction

            movement.Rotate(moveDirection, angularSpeed);
        }

        #endregion
    }
}