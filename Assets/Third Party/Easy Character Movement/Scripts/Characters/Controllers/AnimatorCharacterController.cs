using UnityEngine;

namespace ECM.Characters.Controllers
{
    /// <summary>
    /// Animator Character Controller
    /// 
    /// Inherits from 'BaseCharacterController' and extends it to animate a character and move with or without root motion.
    /// In order to use root motion, You need to attach a 'RootMotionController' to the game object with the 'Animator' component.
    /// </summary>

    public sealed class AnimatorCharacterController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Animator")]
        public bool useRootMotion = true;

        #endregion

        #region FIELDS

        private Animator _animator;

        private RootMotionController _rootMotionController;

        #endregion

        #region METHODS

        /// <summary>
        /// Animate Ethan character.
        /// </summary>

        private void UpdateAnimator(Vector3 move)
        {
            if (_animator == null)
                return;

            // Compute move vector in local space

            move = transform.InverseTransformDirection(Vector3.ClampMagnitude(move, 1.0f));

            // Update the animator parameters

            var forwardAmount = (_animator.applyRootMotion)
                ? move.z
                : Mathf.InverseLerp(0.0f, speed, movement.forwardSpeed);

            _animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            _animator.SetFloat("Turn", Mathf.Atan2(move.x, move.z), 0.1f, Time.deltaTime);

            _animator.SetBool("OnGround", movement.isGrounded);

            if (!movement.isGrounded)
                _animator.SetFloat("Jump", movement.velocity.y, 0.1f, Time.deltaTime);

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            var runCycle = Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            var jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            if (movement.isGrounded)
                _animator.SetFloat("JumpLeg", jumpLeg);
        }

        #endregion

        #region MONOBEHAVIOUR

        public override void Awake()
        {
            base.Awake();

            // Cache components

            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogError(
                    string.Format(
                        "AnimatorCharacterController: There is no 'Animator' in the {0} game object hierarchy.\nYou need to parent a 'Animator' to the game object {0}",
                        name));
            }

            _rootMotionController = GetComponentInChildren<RootMotionController>();
            if (!useRootMotion || _rootMotionController != null)
                return;

            Debug.LogError(
                string.Format(
                    "AnimatorCharacterController: There is no 'RootMotionController' in the {0} game object hierarchy.\nYou need to attach a 'RootMotionController' to the 'Animator' game object {0} to use root motion.",
                    name));

            useRootMotion = false;
        }

        public override void FixedUpdate()
        {
            if (_animator == null)
                return;

            // Compute desired velocity,
            // if useRootMotion and has RootMotionController component,
            // use animation velocity, else use input velocity

            var desiredVelocity = _animator.applyRootMotion && _rootMotionController != null
                ? Vector3.ClampMagnitude(_rootMotionController.animVelocity, speed)
                : Vector3.ClampMagnitude(moveDirection * speed, speed);

            // Apply movement

            movement.Move(desiredVelocity, acceleration, deceleration);

            // Apply braking drag

            if (Mathf.Approximately(desiredVelocity.sqrMagnitude, 0.0f))
                movement.ApplyDrag(brakingFriction);

            // Apply Jump

            Jump();
            UpdateJumpTimer();

            // Should root motion be used?
            // enable / disable root motion if grounded or not

            _animator.applyRootMotion = useRootMotion && movement.isGrounded;

            // Update animator

            UpdateAnimator(moveDirection);
        }

        #endregion
    }
}