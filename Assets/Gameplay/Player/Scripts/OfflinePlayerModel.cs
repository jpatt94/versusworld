using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OfflinePlayerModel : NetworkBehaviour
{
	public static int FirstPersonHandsLI;
	public static int AimDownSightsLI;
	public static int ThirdPersonLegsLI;
	public static int ThirdPersonHandsLI = 1;

	public AudioClip ThrowSound;
	public AudioClip[] JumpSounds;
	public AudioClip Swap1Sound;
	public AudioClip Swap2Sound;
	public AudioClip PickUpSound;
	public AudioClip MeleeSound;
	public AudioClip HeadGrowSound;
	public AudioClip HeadShrinkSound;
	public AudioClip ThrustSound;

	[SerializeField]
	protected float[] shootSpeed;
	[SerializeField]
	protected AnimationCurve handsRunSpeedCurve;
	[SerializeField]
	private Color[] powerUpColors;

	protected FirstPersonHands firstPersonHands;
	protected FirstPersonLegs firstPersonLegs;
	protected ThirdPersonModel thirdPersonModel;
	protected CameraManager cam;
	protected Transform leftHand;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public virtual void Awake()
	{
		firstPersonHands = GetComponentInChildren<FirstPersonHands>();
		firstPersonLegs = GetComponentInChildren<FirstPersonLegs>();
		thirdPersonModel = GetComponentInChildren<ThirdPersonModel>();
		cam = GetComponentInChildren<CameraManager>();
		leftHand = Utility.FindChild(firstPersonHands.gameObject, "Bro_LeftHand").transform;
	}

	public virtual void Update()
	{
		thirdPersonModel.LookRotationAlpha = cam.GetLookRotationAlpha();

		float speed = handsRunSpeedCurve.Evaluate(RunRatio);
		firstPersonHands.Animator.SetFloat("HandsRunSpeed", speed);
		thirdPersonModel.Animator.SetFloat("HandsRunSpeed", speed);
	}

	/**********************************************************/
	// Interface

	public virtual void OnShoot()
	{
		firstPersonHands.Shoot();
		thirdPersonModel.Shoot();
	}

	public virtual void OnReload()
	{
		firstPersonHands.Reload();
		thirdPersonModel.Reload();
	}

	public virtual void OnStartSwap()
	{
		firstPersonHands.Swap();
		thirdPersonModel.Swap();
	}

	public virtual void OnChangeWeapon(WeaponType type)
	{
		firstPersonHands.OnChangeWeapon(type);
		thirdPersonModel.OnChangeWeapon(type);

		firstPersonHands.Animator.SetFloat("ShootSpeed", shootSpeed[(int)type]);
		thirdPersonModel.Animator.SetFloat("ShootSpeed", shootSpeed[(int)type]);
	}

	public virtual void OnThrow()
	{
		firstPersonHands.Throw();
		thirdPersonModel.Throw();
	}

	public virtual void OnMelee()
	{
		firstPersonHands.Melee();
		thirdPersonModel.Melee();
	}

	public virtual void OnJump()
	{
		firstPersonLegs.Jump();
		thirdPersonModel.Jump();
	}

	public virtual void OnThrust()
	{
		thirdPersonModel.Thrust();
	}

	public virtual void OnPickUp()
	{
		firstPersonHands.PickUp();
		thirdPersonModel.PickUp();
	}

	public virtual void SetForward(float forward)
	{
		firstPersonHands.Animator.SetFloat("Forward", forward);
		firstPersonLegs.Animator.SetFloat("Forward", forward);
		thirdPersonModel.Animator.SetFloat("Forward", forward);
	}

	public virtual void SetStrafe(float strafe)
	{
		firstPersonHands.Animator.SetFloat("Strafe", strafe);
		firstPersonLegs.Animator.SetFloat("Strafe", strafe);
		thirdPersonModel.Animator.SetFloat("Strafe", strafe);
	}

	public virtual void SetJumping(bool jumping)
	{
		firstPersonLegs.Animator.SetBool("Jumping", jumping);
		thirdPersonModel.Animator.SetBool("Jumping", jumping);
	}

	public virtual void SetCrouching(bool crouching)
	{
		firstPersonLegs.Animator.SetBool("Crouching", crouching);
		thirdPersonModel.Animator.SetBool("Crouching", crouching);
	}

	public virtual void SetRunSpeed(float speed)
	{
		firstPersonLegs.Animator.SetFloat("RunSpeed", speed);
		thirdPersonModel.Animator.SetFloat("RunSpeed", speed);
	}

	public virtual void SetRunRatio(float ratio)
	{
		firstPersonHands.Animator.SetFloat("RunRatio", ratio);
		thirdPersonModel.Animator.SetFloat("RunRatio", ratio);
	}

	public virtual void SetPowerUpMaterial(PowerUpType type)
	{
		if (type == PowerUpType.None)
		{
			Material.color = Color.white;
		}
		else
		{
			Material.color = powerUpColors[type - PowerUpType.DamageResist];
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public Texture2D Texture
	{
		set
		{
			Material mat = firstPersonHands.GetComponentInChildren<SkinnedMeshRenderer>().material;
			mat.mainTexture = value;
			firstPersonLegs.Material = mat;
			thirdPersonModel.Material = mat;
		}
	}

	public Material Material
	{
		get
		{
			return firstPersonHands.Material;
		}
	}

	public FirstPersonHands FirstPersonHands
	{
		get
		{
			return firstPersonHands;
		}
	}

	public FirstPersonLegs FirstPersonLegs
	{
		get
		{
			return firstPersonLegs;
		}
	}

	public ThirdPersonModel ThirdPersonModel
	{
		get
		{
			return thirdPersonModel;
		}
	}

	public Transform LeftHand
	{
		get
		{
			return leftHand;
		}
	}

	public virtual float Forward
	{
		get
		{
			return 0.0f;
		}
	}

	public virtual float Strafe
	{
		get
		{
			return 0.0f;
		}
	}

	public virtual bool Jumping
	{
		get
		{
			return false;
		}
	}

	public virtual bool Crouching
	{
		get
		{
			return false;
		}
	}

	public virtual float RunSpeed
	{
		get
		{
			return 4.0f;
		}
	}

	public virtual float RunRatio
	{
		get
		{
			return firstPersonHands.RunRatio;
		}
	}
}