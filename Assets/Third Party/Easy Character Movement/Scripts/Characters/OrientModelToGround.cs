using UnityEngine;

namespace ECM.Characters
{
    /// <summary>
    /// OrientModelToGround
    /// 
    /// Helper component used to orient a model to ground.
    /// This must be attached to the game object with 'CharacterMovement' component.
    /// 
    /// </summary>

    [RequireComponent(typeof(CharacterMovement))]
    public sealed class OrientModelToGround : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [SerializeField]
        private bool _orientToGround = true;

        /// <summary>
        /// The model's transform to orient.
        /// It must be child of the game object with 'CharacterMovement' component.
        /// </summary>

        public Transform modelTransform;

        [SerializeField]
        private float _minAngle = 5.0f;

        [SerializeField]
        private float _rotationSpeed = 240.0f;

        #endregion

        #region FIELDS

        private CharacterMovement _movement;

        private Quaternion _groundRotation = Quaternion.identity;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Determines if the character will change its up vector to match the ground normal.
        /// </summary>

        public bool orientToGround
        {
            get { return _orientToGround; }
            set
            {
                _orientToGround = value;

                // Reset model transform

                if (modelTransform)
                    modelTransform.rotation = transform.rotation;
            }
        }

        /// <summary>
        /// Minimum slope angle (in degrees) to cause an orientation change. 
        /// </summary>

        public float minAngle
        {
            get { return _minAngle; }
            set { _minAngle = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        /// <summary>
        /// Maximum turning speed (in deg/s).
        /// </summary>

        public float rotationSpeed
        {
            get { return _rotationSpeed; }
            set { _rotationSpeed = Mathf.Max(0.0f, value); }
        }

        #endregion

        public void OnValidate()
        {
            orientToGround = _orientToGround;
            rotationSpeed = _rotationSpeed;
            minAngle = _minAngle;
        }

        public void Awake()
        {
            _movement = GetComponent<CharacterMovement>();

            _groundRotation = modelTransform.rotation;
        }

        public void Update()
        {
            if (!orientToGround)
                return;

            // Compute target ground normal rotation

            var targetGroundRotation = _movement.slopeAngle < minAngle
                ? Quaternion.identity
                : Quaternion.FromToRotation(Vector3.up, _movement.groundNormal);

            // Interpolate ground orientation

            var maxRadiansDelta = rotationSpeed * Mathf.Deg2Rad * Time.deltaTime;
            _groundRotation = Quaternion.Slerp(_groundRotation, targetGroundRotation, maxRadiansDelta);

            // Concatenate ground and parent facing rotation

            modelTransform.rotation = _groundRotation * transform.rotation;
        }
    }
}