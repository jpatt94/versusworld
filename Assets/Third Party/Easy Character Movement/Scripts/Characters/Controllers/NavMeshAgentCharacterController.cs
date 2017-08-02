using ECM.Common;
using UnityEngine;
using UnityEngine.AI;

namespace ECM.Characters.Controllers
{
    /// <summary>
    /// NavMeshAgent Character Controller
    /// 
    /// Inherits from 'BaseCharacterController' and extends it to control a 'NavMeshAgent'
    /// and intelligently move in response to mouse right click (Click to move).
    /// </summary>

    public sealed class NavMeshAgentCharacterController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Navigation")]
        [SerializeField]
        private bool _autoBraking = true;

        [SerializeField]
        private float _stoppingDistance = 1.0f;

        [SerializeField]
        public LayerMask groundMask = 1;            // Default layer

        #endregion

        #region FIELDS

        private NavMeshAgent _navMeshAgent;

        private Vector3 _desiredVelocity;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Should the agent brake automatically to avoid overshooting the destination point?
        /// </summary>

        public bool autoBraking
        {
            get { return _autoBraking; }
            set
            {
                _autoBraking = value;

                if (_navMeshAgent != null)
                    _navMeshAgent.autoBraking = _autoBraking;
            }
        }

        /// <summary>
        /// Stop within this distance from the target position.
        /// </summary>

        public float stoppingDistance
        {
            get { return _stoppingDistance; }
            set
            {
                _stoppingDistance = Mathf.Max(0.0f, value);

                if (_navMeshAgent != null)
                    _navMeshAgent.stoppingDistance = _stoppingDistance;
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Update the NavMeshAgent simulation position directly,
        /// this is important if the GO transform is controlled by something else - e.g. animator, physics.
        /// </summary>

        private void SyncNavMeshAgent()
        {
            _navMeshAgent.speed = speed;
            _navMeshAgent.acceleration = acceleration;

            _navMeshAgent.velocity = movement.velocity.onlyXZ();

            _navMeshAgent.nextPosition = transform.position;
        }

        private void UpdateNavMeshAgent()
        {
            // Sync NavMeshAgent with the character movement,
            // WE CONTROL the NavMeshAgent

            SyncNavMeshAgent();

            // Compute desired velocity

            _desiredVelocity = Vector3.zero;

            if (!_navMeshAgent.hasPath)
                return;

            // If the agent has a path to the desired destination,
            // move us using agent's desired velocity

            _desiredVelocity = _navMeshAgent.desiredVelocity;

            // Is destination reached?

            var toTarget = (_navMeshAgent.destination - transform.position).onlyXZ();
            if (!(toTarget.sqrMagnitude <= stoppingDistance * stoppingDistance))
                return;

            // If yes,
            // reset desiredVelocity (stop us) and clear agent's path

            _desiredVelocity = Vector3.zero;

            _navMeshAgent.ResetPath();
        }

        #endregion

        #region MONOBEHAVIOUR

        public override void OnValidate()
        {
            base.OnValidate();

            autoBraking = _autoBraking;
            stoppingDistance = _stoppingDistance;
        }

        public override void Awake()
        {
            base.Awake();

            // Cache and initialize components

            _navMeshAgent = GetComponent<NavMeshAgent>();

            if (_navMeshAgent == null)
            {
                Debug.LogError(
                    string.Format(
                        "NavMeshAgentCharacterController: There is no 'NavMeshAgent' attached to {0} game object.\nYou need to add a 'NavMeshAgent' to the game object {0}",
                        name));
            }
            else
            {
                _navMeshAgent.autoBraking = autoBraking;
                _navMeshAgent.stoppingDistance = _stoppingDistance;

                // Turn-off NavMeshAgent control, we control it

                _navMeshAgent.updatePosition = false;
                _navMeshAgent.updateRotation = false;
            }
        }

        public override void FixedUpdate()
        {
            // Move using NavMeshAgent desired velocity

            movement.Move(_desiredVelocity, acceleration, deceleration);

            // Apply braking drag

            if (Mathf.Approximately(_desiredVelocity.sqrMagnitude, 0.0f))
                movement.ApplyDrag(brakingFriction);
        }

        public override void Update()
        {
            if (_navMeshAgent == null)
                return;

            // Handle input

            if (Input.GetButton("Fire2"))
            {
                // If mouse right click, found click position in the world

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, groundMask.value))
                {
                    // Set NavMeshAgent destination to ground hit point

                    _navMeshAgent.destination = hitInfo.point;
                }
            }

            // Update agent

            UpdateNavMeshAgent();

            // Rotate towards desired velocity

            movement.Rotate(_desiredVelocity, angularSpeed);
        }

        #endregion
    }
}