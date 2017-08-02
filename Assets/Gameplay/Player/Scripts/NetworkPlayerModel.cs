using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerModel : OfflinePlayerModel
{
	[SerializeField]
	private float netUpdateRate;
	[SerializeField]
	private float headGrowthTime;
	[SerializeField]
	private AnimationCurve headGrowthCurve;
	[SerializeField]
	private float headShrinkTime;
	[SerializeField]
	private AnimationCurve headShrinkCurve;

	[SyncVar(hook = "OnForwardChanged")]
	private float forward;
	[SyncVar(hook = "OnStrafeChanged")]
	private float strafe;
	[SyncVar(hook = "OnJumpingChanged")]
	private bool jumping;
	[SyncVar(hook = "OnCrouchingChanged")]
	private bool crouching;
	[SyncVar(hook = "OnRunSpeedChanged")]
	private float runSpeed;
	[SyncVar(hook = "OnRunRatioChanged")]
	private float runRatio;

	private ThirdPersonMovementState realState;
	private float netUpdateTime;

	private float bigHeadTime;
	private bool needsHeadShrinkSound;

	private Dictionary<BodyPart, Transform> bodyPartTransforms;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		realState = new ThirdPersonMovementState();

		bodyPartTransforms = new Dictionary<BodyPart, Transform>();
	}

	public override void Update()
	{
		base.Update();

		if (hasAuthority)
		{
			netUpdateTime -= Time.deltaTime;
			if (netUpdateTime <= 0.0f)
			{
				netUpdateTime = 1.0f / netUpdateRate;

				ThirdPersonMovementState state = new ThirdPersonMovementState();
				state.forward = realState.forward;
				state.strafe = realState.strafe;
				state.jumping = realState.jumping;
				state.crouching = realState.crouching;
				state.runSpeed = realState.runSpeed;
				state.runRatio = realState.runRatio;
				CmdSetMovementState(state);
			}
		}

		if (bigHeadTime > 0.0f)
		{
			UpdateBigHead();
		}
	}

	/**********************************************************/
	// Interface

	public void PopulateBodyTransformsThirdPerson()
	{
		foreach (BodyPartCollider bp in ThirdPersonModel.GetComponentsInChildren<BodyPartCollider>())
		{
			bodyPartTransforms[bp.type] = bp.transform;
		}
	}

	public void PopulateBodyTransformsFirstPerson()
	{
		Transform hands = FirstPersonHands.transform.Find("Bro_Reference/Bro_Hips");
		bodyPartTransforms[BodyPart.Head] = cam.transform.Find("View/FirstPersonCamera/FirstPersonHeadJoint");

		Transform handsSpine = hands.Find("Bro_Spine/Bro_Spine1/Bro_Spine2");
		bodyPartTransforms[BodyPart.UpperTorso] = handsSpine;
		bodyPartTransforms[BodyPart.LowerTorso] = hands;
		bodyPartTransforms[BodyPart.RightUpperArm] = handsSpine.Find("Bro_RightShoulder/Bro_RightArm");
		bodyPartTransforms[BodyPart.RightForearm] = handsSpine.Find("Bro_RightShoulder/Bro_RightArm/Bro_RightForeArm");
		bodyPartTransforms[BodyPart.LeftUpperArm] = handsSpine.Find("Bro_LeftShoulder/Bro_LeftArm");
		bodyPartTransforms[BodyPart.LeftForearm] = handsSpine.Find("Bro_LeftShoulder/Bro_LeftArm/Bro_LeftForeArm"); 

		Transform legs = FirstPersonLegs.transform.Find("Bro_Reference/Bro_Hips");
		bodyPartTransforms[BodyPart.RightUpperLeg] = legs.Find("Bro_RightUpLeg");
		bodyPartTransforms[BodyPart.RightLowerLeg] = legs.Find("Bro_RightUpLeg/Bro_RightLeg");
		bodyPartTransforms[BodyPart.RightFoot] = legs.Find("Bro_RightUpLeg/Bro_RightLeg/Bro_RightFoot");
		bodyPartTransforms[BodyPart.LeftUpperLeg] = legs.Find("Bro_LeftUpLeg");
		bodyPartTransforms[BodyPart.LeftLowerLeg] = legs.Find("Bro_LeftUpLeg/Bro_LeftLeg");
		bodyPartTransforms[BodyPart.LeftFoot] = legs.Find("Bro_LeftUpLeg/Bro_LeftLeg/Bro_LeftFoot");
	}

	public void OnRespawn()
	{
		bigHeadTime = 0.0f;
		bodyPartTransforms[BodyPart.Head].parent.localScale = Vector3.one;
		needsHeadShrinkSound = false;
	}

	public override void OnShoot()
	{
		firstPersonHands.Shoot();
		CmdShoot();
	}

	public override void OnReload()
	{
		firstPersonHands.Reload();
		CmdReload();
	}

	public override void OnStartSwap()
	{
		firstPersonHands.Swap();
		CmdSwap();
	}

	public override void OnThrow()
	{
		firstPersonHands.Throw();
		CmdThrow();
	}

	public override void OnMelee()
	{
		firstPersonHands.Melee();
		CmdMelee();
	}

	public override void OnJump()
	{
		firstPersonLegs.Jump();
		CmdJump();
	}

	public override void OnThrust()
	{
		CmdThrust();
	}

	public override void OnPickUp()
	{
		firstPersonHands.PickUp();
		CmdPickUp();
	}

	public override void OnChangeWeapon(WeaponType type)
	{
		firstPersonHands.OnChangeWeapon(type);
		if (hasAuthority)
		{
			CmdChangeWeapon(type);
		}

		firstPersonHands.Animator.SetFloat("ShootSpeed", shootSpeed[(int)type]);
		thirdPersonModel.Animator.SetFloat("ShootSpeed", shootSpeed[(int)type]);
	}

	public override void SetForward(float forward)
	{
		base.SetForward(forward);
		realState.forward = forward;
	}

	public override void SetStrafe(float strafe)
	{
		base.SetStrafe(strafe);
		realState.strafe = strafe;
	}

	public override void SetJumping(bool jumping)
	{
		base.SetJumping(jumping);
		realState.jumping = jumping;
	}

	public override void SetCrouching(bool crouching)
	{
		base.SetCrouching(crouching);
		realState.crouching = crouching;
	}

	public override void SetRunSpeed(float speed)
	{
		base.SetRunSpeed(speed);
		realState.runSpeed = speed;
	}

	public override void SetRunRatio(float ratio)
	{
		base.SetRunRatio(ratio);
		realState.runRatio = ratio;
	}

	/**********************************************************/
	// Commands

	[Command]
	private void CmdShoot()
	{
		RpcShoot();
	}

	[Command]
	private void CmdReload()
	{
		RpcReload();
	}

	[Command]
	private void CmdSwap()
	{
		RpcSwap();
	}

	[Command]
	private void CmdChangeWeapon(WeaponType type)
	{
		RpcChangeWeapon(type);
	}

	[Command]
	private void CmdSetMovementState(ThirdPersonMovementState state)
	{
		forward = state.forward;
		strafe = state.strafe;
		jumping = state.jumping;
		crouching = state.crouching;
		runSpeed = state.runSpeed;
	}

	[Command]
	private void CmdThrow()
	{
		RpcThrow();
	}

	[Command]
	private void CmdMelee()
	{
		RpcMelee();
	}

	[Command]
	private void CmdJump()
	{
		RpcJump();
	}

	[Command]
	private void CmdThrust()
	{
		RpcThrust();
	}

	[Command]
	private void CmdPickUp()
	{
		RpcPickUp();
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcShoot()
	{
		thirdPersonModel.Shoot();
	}

	[ClientRpc]
	private void RpcReload()
	{
		thirdPersonModel.Reload();
	}

	[ClientRpc]
	private void RpcSwap()
	{
		thirdPersonModel.Swap();
	}

	[ClientRpc]
	private void RpcChangeWeapon(WeaponType type)
	{
		thirdPersonModel.OnChangeWeapon(type);
	}

	[ClientRpc]
	private void RpcThrow()
	{
		thirdPersonModel.Throw();
	}

	[ClientRpc]
	private void RpcMelee()
	{
		thirdPersonModel.Melee();
	}

	[ClientRpc]
	private void RpcJump()
	{
		thirdPersonModel.Jump();
	}

	[ClientRpc]
	private void RpcThrust()
	{
		thirdPersonModel.Thrust();
	}

	[ClientRpc]
	private void RpcPickUp()
	{
		thirdPersonModel.PickUp();
	}

	[ClientRpc]
	public void RpcActivateBigHead(float time)
	{
		bigHeadTime = time;
		needsHeadShrinkSound = true;

		if (hasAuthority)
		{
			firstPersonHands.AudioSource.PlayOneShot(HeadGrowSound);
		}
		else
		{
			thirdPersonModel.AudioSource.PlayOneShot(HeadGrowSound);
		}
	}

	/**********************************************************/
	// Hooks

	private void OnForwardChanged(float value)
	{
		ThirdPersonModel.Animator.SetFloat("Forward", value);
		forward = value;
	}

	private void OnStrafeChanged(float value)
	{
		ThirdPersonModel.Animator.SetFloat("Strafe", value);
		strafe = value;
	}

	private void OnJumpingChanged(bool value)
	{
		ThirdPersonModel.Animator.SetBool("Jumping", value);
		jumping = value;
	}

	private void OnCrouchingChanged(bool value)
	{
		ThirdPersonModel.Animator.SetBool("Crouching", value);
		crouching = value;
	}

	private void OnRunSpeedChanged(float value)
	{
		thirdPersonModel.Animator.SetFloat("RunSpeed", value);
		runSpeed = value;
	}

	private void OnRunRatioChanged(float value)
	{
		thirdPersonModel.Animator.SetFloat("RunRatio", value);
		runRatio = value;
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateBigHead()
	{
		BigHeadsSettings settings = PartyManager.GameSettings.PowerUps.BigHeads;

		bigHeadTime -= Time.deltaTime;
		if (bigHeadTime <= 0.0f)
		{
			bodyPartTransforms[BodyPart.Head].parent.localScale = Vector3.one;
		}
		else
		{
			float scale = 0.0f;
			if (bigHeadTime > settings.Duration - headGrowthTime)
			{
				scale = headGrowthCurve.Evaluate(Mathf.InverseLerp(settings.Duration, settings.Duration - headGrowthTime, bigHeadTime));
			}
			else if (bigHeadTime > headShrinkTime)
			{
				scale = 1.0f;
			}
			else
			{
				scale = headShrinkCurve.Evaluate(1.0f - (bigHeadTime / headShrinkTime));
				if (needsHeadShrinkSound)
				{
					if (hasAuthority)
					{
						firstPersonHands.AudioSource.PlayOneShot(HeadShrinkSound);
					}
					else
					{
						thirdPersonModel.AudioSource.PlayOneShot(HeadShrinkSound);
					}
					needsHeadShrinkSound = false;
				}
			}

			scale = Mathf.LerpUnclamped(1.0f, settings.Scale, scale);
			bodyPartTransforms[BodyPart.Head].parent.localScale = Vector3.one * scale;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public Dictionary<BodyPart, Transform> BodyPartTransforms
	{
		get
		{
			return bodyPartTransforms;
		}
	}

	public Vector3 HeadPosition
	{
		get
		{
			return bodyPartTransforms[BodyPart.Head].position;
		}
	}

	public override float Forward
	{
		get
		{
			return forward;
		}
	}

	public override float Strafe
	{
		get
		{
			return strafe;
		}
	}

	public override bool Jumping
	{
		get
		{
			return jumping;
		}
	}

	public override bool Crouching
	{
		get
		{
			return crouching;
		}
	}

	public override float RunSpeed
	{
		get
		{
			return runSpeed;
		}
	}

	public override float RunRatio
	{
		get
		{
			if (hasAuthority)
			{
				return firstPersonHands.RunRatio;
			}
			return runRatio;
		}
	}
}

public struct ThirdPersonMovementState
{
	public float forward;
	public float strafe;
	public bool jumping;
	public bool crouching;
	public float runSpeed;
	public float runRatio;
}