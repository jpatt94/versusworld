using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkCharacterController : OfflineCharacterController
{
	/**********************************************************/
	// MonoBehaviour Interface

	public override void OnStart()
	{
		base.OnStart();

		Traits = PlayerTraitsType.Default;
	}

	public override void Update()
	{
		if (hasAuthority)
		{
			base.Update();
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
	// Accessors/Mutators

	public PlayerTraitsType Traits
	{
		set
		{
			MovementSettings settings = PartyManager.GameSettings.GetPlayerTraits(value).Movement;

			speed = settings.Speed;
			acceleration = settings.Acceleration;
			jumpHeight = settings.JumpHeight;
			airControl = settings.AirControl;
			sprintSpeedMultiplier = settings.SprintSpeedMultiplier;
			crouchSpeedMultiplier = settings.CrouchSpeedMultiplier;
			controller.gravity = 25.0f * settings.GravityMultiplier;
			controller.maxMoveSpeed = speed * sprintSpeedMultiplier * 10.0f;
			thrustEnabled = settings.ThrustEnabled;
			thrustHorizontalForce = settings.ThrustHorizontalForce;
			thrustVerticalForce = settings.ThrustVerticalForce;
			thrustDelay = settings.ThrustDelay;
		}
	}
}
