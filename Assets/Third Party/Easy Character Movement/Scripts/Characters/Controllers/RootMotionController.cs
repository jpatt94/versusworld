using UnityEngine;

namespace ECM.Characters.Controllers
{
    /// <summary>
    /// RootMotionController
    /// 
    /// Helper component to get 'Animator' root-motion velocity vector (animVelocity).
    /// This must be attached to the game object with the 'Animator' component.
    /// </summary>

    public sealed class RootMotionController : MonoBehaviour
    {
        #region FIELDS

        private Animator _animator;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The animation velocity vector.
        /// </summary>

        public Vector3 animVelocity { get; private set; }

        #endregion

        #region MONOBEHAVIOUR

        public void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError(
                    string.Format(
                        "RootMotionController: There is no 'Animator' attached to the {0} game object.\nYou need to attach a 'Animator' to the game object {0}",
                        name));
            }
        }

        public void OnAnimatorMove()
        {
            // Compute movement velocity from animation

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0.0f)
                return;

            animVelocity = _animator.deltaPosition / deltaTime;
        }

        #endregion
    }
}