using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using ECM.Characters;

public class OfflineCharacterController : SafeNetworkBehaviour
{
	[SerializeField]
	protected float speed;
	[SerializeField]
	protected float acceleration;
	[SerializeField]
	protected float jumpHeight;
	[SerializeField]
	protected float airControl;
	[SerializeField]
	protected float sprintSpeedMultiplier;
	[SerializeField]
	protected float sprintTransitionSpeed;
	[SerializeField]
	protected float sprintInputBufferTime;
	[SerializeField]
	protected float gravityMultiplier;
	[SerializeField]
	protected float slopeLimit;
	[SerializeField]
	protected float stickToGroundForce;
	[SerializeField]
	protected float stickToGroundDistance;
	[SerializeField]
	protected float jumpHandsLagDuration;
	[SerializeField]
	protected float jumpHandsLagAmount;
	[SerializeField]
	protected float crouchSpeedMultiplier;
	[SerializeField]
	protected float crouchColliderHeight;
	[SerializeField]
	protected float jumpInputBufferTime;
	[SerializeField]
	protected bool thrustEnabled;
	[SerializeField]
	protected float thrustHorizontalForce;
	[SerializeField]
	protected float thrustVerticalForce;
	[SerializeField]
	protected float thrustDelay;
	[SerializeField]
	protected float thrustViewShakeAmount;
	[SerializeField]
	protected float thrustViewShakeDuration;
	[SerializeField]
	protected Vector3 jumpViewShake;
	[SerializeField]
	protected Vector3 landViewShake;
	[SerializeField]
	protected float jumpViewShakeDuration;
	[SerializeField]
	protected float landViewShakeDuration;

	protected Vector3 inputVelocity;
	protected float jumpInput;
	protected bool previouslyGrounded;
	protected bool jumping;
	protected bool moving;
	protected bool sprinting;
	protected float currentSpeedMultiplier;
	protected float sprintInputTimeLeft;
	protected float jumpHandsLagTime;
	protected bool crouching;
	protected float canThrust;
	protected int freeThrusts;

	protected CharacterMovement controller;
	protected CameraManager cam;
	protected Rigidbody rig;
	protected FirstPersonHands firstPersonHands;
	protected OfflinePlayerModel playerModel;
	protected ThrustIcon thrustIcon;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		inputVelocity = Vector3.zero;
		previouslyGrounded = false;
		jumping = false;
		sprinting = false;
		currentSpeedMultiplier = 1.0f;
		sprintInputTimeLeft = 0.0f;

		controller = GetComponent<CharacterMovement>();
		cam = GetComponentInChildren<CameraManager>();
		rig = GetComponent<Rigidbody>();
		firstPersonHands = GetComponentInChildren<FirstPersonHands>();
		playerModel = GetComponent<OfflinePlayerModel>();
	}

	protected override void DelayedStart()
	{
		if (thrustEnabled)
		{
			thrustIcon = HUD.Instance.AbilityDisplay.AddAbilityIcon(AbilityType.Thrust) as ThrustIcon;
		}
	}

	public override void Update()
	{
		base.Update();

		if (initialized)
		{
			UpdateInputVelocity(PlayerInput.MoveAxis);
			UpdateSprint();
			UpdateCrouch();

			if (thrustEnabled && thrustIcon)
			{
				canThrust += Time.deltaTime;
				thrustIcon.RechargeAmount = Mathf.Clamp01(-canThrust / thrustDelay);
			}

			jumpInput -= Time.deltaTime;
			if (PlayerInput.Jump(ButtonStatus.Pressed))
			{
				jumpInput = jumpInputBufferTime;
			}

			if (!controller.isGrounded)
			{
				jumpHandsLagTime -= Time.deltaTime;
				if (jumpHandsLagTime >= 0.0f)
				{
					firstPersonHands.AddPositionLag(Vector3.down * jumpHandsLagAmount * Time.deltaTime);
				}
			}
		}
	}

	public virtual void FixedUpdate()
	{
		if (initialized)
		{
			Move();
		}
	}

	/**********************************************************/
	// Interface

	public void Move()
	{
		Vector3 move = inputVelocity;

		moving = move.sqrMagnitude > 0.01f;

		if (jumpInput > 0.0f)
		{
			if (controller.isGrounded)
			{
				controller.ApplyVerticalImpulse(Mathf.Sqrt(controller.gravity * 2.0f * jumpHeight));
				jumping = true;
				jumpHandsLagTime = jumpHandsLagDuration;
				crouching = false;
				playerModel.OnJump();
				cam.ZShake(0.5f, 0.15f);
				cam.ViewShake(jumpViewShake, jumpViewShakeDuration);
				jumpInput = -1.0f;
			}
			else if (((thrustEnabled && canThrust >= 0.0f) || freeThrusts > 0) && !(Physics.Raycast(transform.position, Vector3.down, 1.0f) && controller.velocity.y < 0.0f))
			{
				Thrust(inputVelocity);

				if (freeThrusts > 0)
				{
					freeThrusts--;
					thrustIcon.FreeThrusts = freeThrusts;
					thrustIcon.Pop();
				}
				else
				{
					canThrust = -thrustDelay;
				}

				playerModel.OnThrust();
				jumpInput = -1.0f;
			}
		}

		Vector3 vel = controller.velocity;
		vel.y = 0.0f;
		float currentSpeed = controller.isGrounded ? 0.0f : vel.magnitude;
		controller.Move(move * Mathf.Max(speed * currentSpeedMultiplier, currentSpeed), controller.isGrounded ? acceleration : acceleration * airControl, controller.isGrounded ? acceleration : 0.0f);

		if (!previouslyGrounded && controller.isGrounded)
		{
			OnLand();
		}
		previouslyGrounded = controller.isGrounded;

		playerModel.SetJumping(!controller.isGrounded);
		playerModel.SetCrouching(crouching);
		cam.Crouching = crouching;
	}

	public bool CheckCancelSprint()
	{
		return PlayerInput.MoveAxis.y < 0.5f || PlayerInput.Sprint(ButtonStatus.Pressed);
	}

	public void Reset()
	{
		sprinting = false;
		crouching = false;
		controller.velocity = Vector3.zero;
		canThrust = 1.0f;
	}

	/**********************************************************/
	// Helper Functions

	protected virtual void OnLand()
	{
		jumping = false;
		cam.ViewShake(landViewShake, landViewShakeDuration);
		playerModel.ThirdPersonModel.OnLand(Velocity.y);
	}

	protected void UpdateSprint()
	{
		if (sprinting)
		{
			currentSpeedMultiplier = Mathf.Min(sprintSpeedMultiplier, currentSpeedMultiplier + sprintTransitionSpeed * Time.deltaTime);

			if (CheckCancelSprint())
			{
				sprinting = false;
				sprintInputTimeLeft = 0.0f;
			}
		}
		else
		{
			sprintInputTimeLeft -= Time.deltaTime;
			if (PlayerInput.Sprint(ButtonStatus.Pressed))
			{
				sprintInputTimeLeft = sprintInputBufferTime;
				crouching = false;
			}

			if (controller.isGrounded)
			{
				currentSpeedMultiplier = Mathf.Max(1.0f, currentSpeedMultiplier - sprintTransitionSpeed * Time.deltaTime);
			}

			if (sprintInputTimeLeft > 0.0f && CanSprint())
			{
				sprinting = true;
			}
		}

		playerModel.SetRunSpeed(speed * currentSpeedMultiplier);
		cam.SetHeadBob(sprinting && controller.isGrounded);
	}

	protected bool CanSprint()
	{
		return PlayerInput.MoveAxis.y > 0.5f && Mathf.Approximately(inputVelocity.sqrMagnitude, 1.0f);
	}

	protected void UpdateInputVelocity(Vector2 input)
	{
		inputVelocity = InputToWorldVelocity(input);

		if (inputVelocity.sqrMagnitude > 1.0f)
		{
			inputVelocity.Normalize();
		}

		float speedAlpha = Mathf.Clamp01(controller.velocity.magnitude / speed);

		playerModel.SetForward(Vector3.Dot(transform.forward, inputVelocity) * speedAlpha);
		playerModel.SetStrafe(Vector3.Dot(transform.right, inputVelocity) * speedAlpha);
	}

	protected Vector3 InputToWorldVelocity(Vector2 inputMoveAxis)
	{
		return new Vector3(cam.transform.forward.x, 0.0f, cam.transform.forward.z).normalized * inputMoveAxis.y
			+ new Vector3(cam.transform.right.x, 0.0f, cam.transform.right.z).normalized * inputMoveAxis.x;
	}

	protected void UpdateCrouch()
	{
		if (PlayerInput.Crouch(ButtonStatus.Pressed))
		{
			crouching = !crouching;
		}

		if (crouching)
		{
			currentSpeedMultiplier = crouchSpeedMultiplier;
			sprinting = false;
		}
	}

	protected void Thrust(Vector3 direction)
	{
		if (direction.sqrMagnitude < 0.1f)
		{
			direction = transform.forward;
		}
		controller.ApplyForce(direction * thrustHorizontalForce + Vector3.up * Mathf.Max(thrustVerticalForce, thrustVerticalForce - controller.velocity.y), ForceMode.VelocityChange);

		cam.ViewShake(thrustViewShakeAmount, thrustViewShakeDuration);
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Sprinting
	{
		get
		{
			return sprinting;
		}
		set
		{
			bool wasSprinting = sprinting;
			sprinting = value;
			if (!sprinting && wasSprinting)
			{
				sprintInputTimeLeft = 0.0f;
			}
		}
	}

	public bool IsRunning
	{
		get
		{
			return inputVelocity.sqrMagnitude > float.Epsilon && controller.isGrounded;
		}
	}

	public bool IsGrounded
	{
		get
		{
			return controller.isGrounded;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			return controller.velocity;
		}
		set
		{
			controller.velocity = value;
		}
	}

	public SurfaceType GroundSurfaceType
	{
		get
		{
			if (controller.groundCollider && controller.groundCollider.sharedMaterial)
			{
				return EnvironmentManager.ConvertSurfaceNameToType(controller.groundCollider.sharedMaterial.name);
			}
			return SurfaceType.None;
		}
	}
}
