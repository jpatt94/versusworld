namespace ECM.Characters.Controllers
{
    /// <summary>
    /// Platformer Character Controller
    /// 
    /// Inherits from 'BaseCharacterController' and extends it to allow a variable height DOUBLE jump.
    /// </summary>

    public sealed class PlatformerCharacterController : BaseCharacterController
    {
        #region FIELDS

        private bool _canDoubleJump = true;

        #endregion

        #region PROPERTIES

        private bool canDoubleJump
        {
            get
            {
                if (!_canDoubleJump && movement.isGrounded)
                    _canDoubleJump = true;

                return _canDoubleJump;
            }
        }

        #endregion

        #region METHODS

        private void DoubleJump()
        {
            // If already double jump, return

            if (!canDoubleJump)
                return;

            // Is jump button released?

            if (!_canJump)
                return;

            // jump button not pressed or in ground, return

            if (!jump || movement.isGrounded)
                return;

            // Apply in air jump impulse

            _canJump = false;
            _canDoubleJump = false;
            _updateJumpTimer = true; // allows second jump to be variable height based on button hold duration

            movement.ApplyVerticalImpulse(jumpImpulse);
        }

        #endregion

        #region MONOBEHAVIOUR

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            DoubleJump();
        }

        #endregion
    }
}