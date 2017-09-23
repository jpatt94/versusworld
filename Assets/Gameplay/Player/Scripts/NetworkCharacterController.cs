using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkCharacterController : OfflineCharacterController
{
	[SerializeField]
	private float rogiBallTeleportDuration;

	private float rogiBallTeleportTime;
	private Vector3 rogiBallTeleportPos;

	/**********************************************************/
	// MonoBehaviour Interface

	protected override bool Ready()
	{
		return GetComponent<NetworkPlayer>().Initialized;
	}

	protected override void DelayedStart()
	{
		base.DelayedStart();

		if (thrustEnabled)
		{
			thrustIcon = HUD.Instance.AbilityDisplay.AddAbilityIcon(AbilityType.Thrust) as ThrustIcon;
		}
	}

	public override void Update()
	{
		if (hasAuthority)
		{
			base.Update();

			if (rogiBallTeleportTime > 0.0f)
			{
				rogiBallTeleportTime -= Time.deltaTime;

				cam.TwirlDegrees = Mathf.Lerp(360.0f, 0.0f, rogiBallTeleportTime / rogiBallTeleportDuration);

				if (rogiBallTeleportTime < rogiBallTeleportDuration * 0.5f)
				{
					transform.position = rogiBallTeleportPos;
					rig.position = rogiBallTeleportPos;
					GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
				}

				if (rogiBallTeleportTime < 0.0f)
				{
					cam.TwirlDegrees = 0.0f;
				}
			}
		}
	}

	public override void FixedUpdate()
	{
		if (hasAuthority)
		{
			base.FixedUpdate();
		}
	}

	/**********************************************************/
	// Interface

	public void OnDeath()
	{
		freeThrusts = 0;
		thrustIcon.FreeThrusts = 0;
	}

	public void RogiBallTeleport(Vector3 pos)
	{
		rogiBallTeleportPos = pos;
		rogiBallTeleportTime = rogiBallTeleportDuration;
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	public void RpcAddFreeThrusts(int amount)
	{
		freeThrusts += amount;
		thrustIcon.FreeThrusts = freeThrusts;
		thrustIcon.Pop();

		canThrust = 0.0f;
	}

	/**********************************************************/
	// Accessors/Mutators

	public MovementSettings Traits
	{
		set
		{
			speed = value.Speed;
			acceleration = value.Acceleration;
			jumpHeight = value.JumpHeight;
			airControl = value.AirControl;
			sprintSpeedMultiplier = value.SprintSpeedMultiplier;
			crouchSpeedMultiplier = value.CrouchSpeedMultiplier;
			controller.gravity = 25.0f * value.GravityMultiplier;
			controller.maxMoveSpeed = speed * sprintSpeedMultiplier * 10.0f;
			thrustEnabled = value.ThrustEnabled;
			thrustHorizontalForce = value.ThrustHorizontalForce;
			thrustVerticalForce = value.ThrustVerticalForce;
			thrustDelay = value.ThrustDelay;
		}
	}
}
