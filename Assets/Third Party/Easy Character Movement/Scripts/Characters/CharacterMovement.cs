using System;
using ECM.Common;
using UnityEngine;
 
namespace ECM.Characters
{
    /// <summary>
    /// Character Movement
    /// 
    /// 'CharacterMovement' component do all the heavy work, such as apply forces, impulses,
    /// platforms interaction, and more.
    /// 
    /// The controller determines how the Character should be moved, such as in response from user input,
    /// AI, animation, etc. and pass this information to the CharacterMovement component, which do the movement.
    /// 
    /// </summary>
    
    public sealed class CharacterMovement : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Tooltip("The maximum horizontal (xz plane) speed this character can move," +
                 "including movement from external forces like sliding, collisions, etc.")]
        [SerializeField]
        private float _maxMoveSpeed = 8.0f;

        [Tooltip("The maximum rising speed. Effective terminal velocity along Y+ axis.")]
        [SerializeField]
        private float _maxRiseSpeed = 15.0f;

        [Tooltip("The maximum falling speed. Effective terminal velocity along Y- axis.")]
        [SerializeField]
        private float _maxFallSpeed = 15.0f;

        [Tooltip("Enable / disable character's custom gravity." +
                 "If enabled the character will be affected by this gravity force.")]
        [SerializeField]
        private bool _useGravity = true;
        
        [Tooltip("The amount of gravity to be applied to this character.")]
        [SerializeField]
        private float _gravity = 25.0f;
        
        [Tooltip("The maximum angle (in degrees) the slope (under the actor) " +
                 "needs to be before the character starts to slide.")]
        [SerializeField]
        private float _slopeLimit = 45.0f;
        
        [Tooltip("The percentage of gravity that will be applied to the slide.")]
        [SerializeField]
        private float _slideGravityMultiplier = 1.0f;

        #endregion

        #region FIELDS

        private Rigidbody _rigidbody;

        private GroundDetection _groundDetection;
        
        private Vector3 _groundNormal = Vector3.up;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// The maximum speed this character can move, including movement from external forces like sliding, collisions, etc.
        /// Effective terminal velocity along XZ plane.
        /// </summary>

        public float maxMoveSpeed
        {
            get { return _maxMoveSpeed; }
            set { _maxMoveSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The maximum rising speed.
        /// Effective terminal velocity along Y axis, when moving up (rising).
        /// NOTE: This value MUST be higher than your jumpImpulse (initial jump velocity).
        /// </summary>

        public float maxRiseSpeed
        {
            get { return _maxRiseSpeed; }
            set { _maxRiseSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The maximum falling speed.
        /// Effective terminal velocity along Y axis, when moving down (falling).
        /// </summary>

        public float maxFallSpeed
        {
            get { return _maxFallSpeed; }
            set { _maxFallSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Enable / disable character's custom gravity.
        /// If enabled the character will be affected by its custom gravity force.
        /// </summary>

        public bool useGravity
        {
            get { return _useGravity; }
            set
            {
                _useGravity = value;

                if (!_useGravity)
                    return;

#if UNITY_EDITOR

                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();

#endif
                // If our gravity is enabled,
                // make sure global gravity don't affect this rigidbody

                _rigidbody.useGravity = false;
            }
        }

        /// <summary>
        /// The amount of gravity to be applied to this character.
        /// We apply gravity manually for more tuning control.
        /// </summary>

        public float gravity
        {
            get { return _gravity; }
            set { _gravity = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The maximum angle (in degrees) the slope (under the actor) needs to be before the character starts to slide. 
        /// </summary>

        public float slopeLimit
        {
            get { return _slopeLimit; }
            set { _slopeLimit = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The percentage of gravity that will be applied to the slide.
        /// </summary>

        public float slideGravityMultiplier
        {
            get { return _slideGravityMultiplier; }
            set { _slideGravityMultiplier = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The amount of gravity to apply when sliding.
        /// </summary>

        public float slideGravity
        {
            get { return gravity * slideGravityMultiplier; }
        }

        /// <summary>
        /// The slope angle (in degrees) the character is standing on.
        /// </summary>

        public float slopeAngle { get; private set; }

        /// <summary>
        /// Is a valid slope to walk without slide?
        /// </summary>

        public bool isValidSlope
        {
            get { return slopeAngle < slopeLimit; }
        }

        /// <summary>
        /// The velocity of the platform the character is standing on,
        /// zero (Vector3.zero) if not on a platform.
        /// </summary>

        public Vector3 platformVelocity { get; private set; }

        /// <summary>
        /// Character's velocity vector.
        /// </summary>

        public Vector3 velocity
        {
            get { return _rigidbody.velocity - platformVelocity; }
            set { _rigidbody.velocity = value + platformVelocity; }
        }

        /// <summary>
        /// The character forward speed (along its forward vector).
        /// </summary>

        public float forwardSpeed
        {
            get { return Vector3.Dot(velocity, transform.forward); }
        }

        /// <summary>
        /// The character's current rotation.
        /// Setting it comply with the Rigidbody's interpolation setting.
        /// </summary>

        public Quaternion rotation
        {
            get { return _rigidbody.rotation; }
            set { _rigidbody.MoveRotation(value); }
        }

        /// <summary>
        /// Is this character standing on "ground".
        /// </summary>

        public bool isGrounded
        {
            get { return _groundDetection.isGrounded; }
        }

        /// <summary>
        /// The impact point in world space where the cast hit the collider.
        /// If the character is not on ground, it represent a point at character's base.
        /// </summary>

        public Vector3 groundPoint
        {
            get { return _groundDetection.groundPoint; }
        }

        /// <summary>
        /// The normal of the surface the cast hit.
        /// If the character is not grounded, it will point up (Vector3.up).
        /// </summary>

        public Vector3 groundNormal
        {
            get { return _groundNormal; }
            private set { _groundNormal = value; }
        }

        /// <summary>
        /// The distance from the ray's origin to the impact point.
        /// </summary>

        public float groundDistance
        {
            get { return _groundDetection.groundDistance; }
        }

        /// <summary>
        /// The Collider that was hit.
        /// This property is null if the cast hit nothing (not grounded) and not-null if it hit a Collider.
        /// </summary>

        public Collider groundCollider
        {
            get { return _groundDetection.groundCollider; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Rotates the character to face the given direction.
        /// </summary>
        /// <param name="direction">The target direction vector.</param>
        /// <param name="angularSpeed">Maximum turning speed in (deg/s).</param>
        /// <param name="onlyXZ">Should it be restricted to XZ only.</param>

        public void Rotate(Vector3 direction, float angularSpeed, bool onlyXZ = true)
        {
            if (onlyXZ)
                direction.y = 0.0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            var targetRotation = Quaternion.LookRotation(direction);
            var newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation,
                angularSpeed * Mathf.Deg2Rad * Time.deltaTime);

            _rigidbody.MoveRotation(newRotation);
        }

        /// <summary>
        /// Apply a force to the character's rigidbody.
        /// </summary>
        /// <param name="force">The force to be applied.</param>
        /// <param name="forceMode">Option for how to apply the force.</param>

        public void ApplyForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        /// <summary>
        /// Apply a vertical impulse (along world's up vector).
        /// E.g. Use this to make character jump.
        /// </summary>
        /// <param name="impulse">The magnitude of the impulse to be applied.</param>

        public void ApplyVerticalImpulse(float impulse)
        {
            var verticalVelocityChange = impulse - velocity.y;
            _rigidbody.AddForce(0.0f, verticalVelocityChange, 0.0f, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Apply a drag to character, an opposing force that scales with current velocity.
        /// Drag reduces the effective maximum speed of the character.
        /// </summary>
        /// <param name="drag">The amount of drag to be applied.</param>
        /// <param name="onlyXZ">Should it be restricted to XZ plane.</param>

        public void ApplyDrag(float drag, bool onlyXZ = true)
        {
            var v = onlyXZ ? velocity.onlyXZ() : velocity;

            var d = -drag * v.sqrMagnitude * v.normalized;
            if (onlyXZ)
                d = Vector3.ProjectOnPlane(d, groundNormal);

            _rigidbody.AddForce(d, ForceMode.Acceleration);
        }

        /// <summary>
        /// Perform ground detection.
        /// </summary>

        private void GroundCheck()
        {
            var up = Vector3.up;

            if (_groundDetection.DetectGround())
            {
                // Cache ground normal

                groundNormal = _groundDetection.groundNormal;

                // Check if we are over other rigidbody...

                var otherRigidbody = _groundDetection.groundCollider.attachedRigidbody;

				// If other rigidbody is a dynamic platform (KINEMATIC rigidbody), get its velocity
				// GetPointVelocity will take the angularVelocity of the rigidbody into account when calculating the velocity,
				// allowing for rotating platforms

                platformVelocity = (otherRigidbody != null && otherRigidbody.isKinematic)
                    ? otherRigidbody.GetPointVelocity(transform.position).onlyXZ()
                    : Vector3.zero;

                // If other is a non-kinematic rigidbody, reset groundNormal (point up)
                
                if (otherRigidbody != null && !otherRigidbody.isKinematic)
                    groundNormal = up;

                // Apply slide if steep slope

                slopeAngle = Vector3.Angle(up, groundNormal);
                if (slopeAngle > slopeLimit)
                    _rigidbody.AddForce(up * -slideGravity, ForceMode.Acceleration);

            }
            else
            {
                // Reset info

                slopeAngle = 0.0f;
                groundNormal = up;
                
                platformVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Perform character's movement.
        /// </summary>
        /// <param name="desiredVelocity">Target velocity vector.</param>
        /// <param name="onlyXZ">Is the movement restricted to XZ plane.</param>

        private void ApplyMovement(Vector3 desiredVelocity, bool onlyXZ)
        {
            // Rigidbody's velocity

            var groundVelocity = _rigidbody.GetPointVelocity(transform.position);

            // Computes velocity change

            var velocityChange = desiredVelocity - groundVelocity;
            if (onlyXZ)
                velocityChange = Vector3.ProjectOnPlane(velocityChange.onlyXZ(), groundNormal);

            // Apply velocity change PLUS any platform velocity

            _rigidbody.AddForce(velocityChange + platformVelocity, ForceMode.VelocityChange);

			//print("Applying movement of " + desiredVelocity + "   " + Time.time);
        }

        /// <summary>
        /// Perform character's movement.
        /// </summary>
        /// <param name="desiredVelocity">Target velocity vector.</param>
        /// <param name="acceleration">The rate of change of velocity.</param>
        /// <param name="deceleration">The rate at which the character's slows down.</param>
        /// <param name="onlyXZ">Is the movement restricted to XZ plane.</param>
        
        private void ApplyMovement(Vector3 desiredVelocity, float acceleration, float deceleration, bool onlyXZ)
        {
            // Computes desired velocity accelerating / decelerating character's velocity towards target velocity

            desiredVelocity = desiredVelocity.sqrMagnitude > 0.0001f
                ? Vector3.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime)
                : Vector3.MoveTowards(velocity, desiredVelocity, deceleration * Time.deltaTime);

            // Performs movement

            ApplyMovement(desiredVelocity, onlyXZ);
        }

        /// <summary>
        /// Make sure we don't move any faster than our maxMoveSpeed.
        /// </summary>

        private void LimitMovementVelocity()
        {
            var hVelocity = velocity.onlyXZ();
            if (hVelocity.sqrMagnitude <= maxMoveSpeed * maxMoveSpeed)
                return;

            var velocityChange = hVelocity.normalized * maxMoveSpeed - hVelocity;
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Apply custom gravity acceleration.
        /// </summary>

        private void ApplyGravity()
        {
            var gravityForce = groundNormal * -gravity;
            _rigidbody.AddForce(gravityForce, ForceMode.Acceleration);
        }

        /// <summary>
        /// Make sure we don't fall any faster than maxFallSpeed.
        /// </summary>

        [Obsolete("Replaced by LimitVerticalVelocity to limit rising speed too.")]
        private void LimitFallSpeed()
        {
            var verticalVelocity = velocity.y;
            if (_groundDetection.isGrounded || verticalVelocity > -maxFallSpeed)
                return;

            var verticalVelocityChange = -maxFallSpeed - verticalVelocity;
            _rigidbody.AddForce(0.0f, verticalVelocityChange, 0.0f, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Limit vertical velocity along Y axis.
        /// Make sure we cant fall faster than maxFallSpeed, 
        /// and cant rise faster than maxRiseSpeed.
        /// </summary>

        private void LimitVerticalVelocity()
        {
            if (_groundDetection.isGrounded)
                return;

            var verticalVelocity = velocity.y;
            if (verticalVelocity < -maxFallSpeed)
            {
                var verticalVelocityChange = -maxFallSpeed - verticalVelocity;
                _rigidbody.AddForce(0.0f, verticalVelocityChange, 0.0f, ForceMode.VelocityChange);
            }
            else if (verticalVelocity > maxRiseSpeed)
            {
                var verticalVelocityChange = maxRiseSpeed - verticalVelocity;
                _rigidbody.AddForce(0.0f, verticalVelocityChange, 0.0f, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// Performs character's movement, will apply our custom gravity if useGravity == true.
        /// This function will apply the velocity change instantly (no acceleration / deceleration).
        /// 
        /// Must be called in FixedUpdate.
        /// 
        /// </summary>
        /// <param name="desiredVelocity">Target velocity vector.</param>
        /// <param name="onlyXZ">Is the movement restricted to XZ plane.</param>

        public void Move(Vector3 desiredVelocity, bool onlyXZ = true)
        {
            GroundCheck();

            ApplyMovement(desiredVelocity, onlyXZ);
            LimitMovementVelocity();

            if (useGravity)
                ApplyGravity();

            LimitVerticalVelocity();
        }

        /// <summary>
        /// Perform character's movement, will apply our custom gravity if useGravity == true.
        /// 
        /// Must be called in FixedUpdate.
        /// 
        /// </summary>
        /// <param name="desiredVelocity">Target velocity vector.</param>
        /// <param name="acceleration">The rate of change of velocity.</param>
        /// <param name="deceleration">The rate at which the character's slows down.</param>
        /// <param name="onlyXZ">Is the movement restricted to XZ plane.</param>

        public void Move(Vector3 desiredVelocity, float acceleration, float deceleration, bool onlyXZ = true)
        {
            GroundCheck();

            ApplyMovement(desiredVelocity, acceleration, deceleration, onlyXZ);
            LimitMovementVelocity();

            if (useGravity)
                ApplyGravity();

            LimitVerticalVelocity();
        }

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            maxMoveSpeed = _maxMoveSpeed;

            maxRiseSpeed = _maxRiseSpeed;
            maxFallSpeed = _maxFallSpeed;

            useGravity = _useGravity;
            gravity = _gravity;

            slopeLimit = _slopeLimit;
            slideGravityMultiplier = _slideGravityMultiplier;
        }

        public void Awake()
        {
            // Cache an initialize components

            _groundDetection = GetComponent<GroundDetection>();
            if (_groundDetection == null)
            {
                Debug.LogError(
                    string.Format(
                        "CharacterMovement: No 'GroundDetection' found for {0} game object.\nPlease add a 'GroundDetection' component to {0} game object",
                        name));

                return;
            }

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError(
                    string.Format(
                        "CharacterMovement: No 'Rigidbody' found for {0} game object.\nPlease add a 'Rigidbody' component to {0} game object",
                        name));

                return;
            }
            
            if (useGravity)
                _rigidbody.useGravity = false;

            _rigidbody.isKinematic = false;
            _rigidbody.freezeRotation = true;

            // Attempt to validate frictionless material (if collider found in this gameobject)

            var aCollider = GetComponent<CapsuleCollider>();
            if (aCollider == null)
                return;

            var physicMaterial = aCollider.sharedMaterial;
            if (physicMaterial != null)
                return;

            physicMaterial = new PhysicMaterial("Frictionless")
            {
                dynamicFriction = 0.0f,
                staticFriction = 0.0f,
                bounciness = 0.0f,
                frictionCombine = PhysicMaterialCombine.Multiply,
                bounceCombine = PhysicMaterialCombine.Multiply
            };

            aCollider.material = physicMaterial;

            Debug.LogWarning(
                string.Format(
                    "CharacterMovement: No 'PhysicMaterial' found for {0}'s Collider, a frictionless one has been created and assigned.\n You should add a Frictionless 'PhysicMaterial' to '{0}' game object.",
                    name));
        }
        
        #endregion
    }
}