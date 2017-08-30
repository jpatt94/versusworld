using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ThirdPersonModel : MonoBehaviour
{
	[SerializeField]
	private RuntimeAnimatorController[] weaponControllers;
	[SerializeField]
	private float maxLookRotation;
	[SerializeField]
	private float minLookRotation;
	[SerializeField]
	private float maxFootstepVolumeSpeed;
	[SerializeField]
	private float thrustTrailDuration;
	[SerializeField]
	private AnimationCurve thrustCurve;
	[SerializeField]
	private AnimationCurve landingSoundVolumeCurve;

	[SerializeField]
	private AudioClip[] concreteFootstepSounds;
	[SerializeField]
	private AudioClip[] grassFootstepSounds;
	[SerializeField]
	private AudioClip[] woodFootstepSounds;
	[SerializeField]
	private AudioClip[] concreteLandSounds;
	[SerializeField]
	private AudioClip[] grassLandSounds;
	[SerializeField]
	private AudioClip[] woodLandSounds;

	private float lookRotationAlpha;
	private int previousFootstepSound;
	private float canPlayFootstep;
	private bool prevKnifeVisible;
	private float thrustTrailTime;
	private int previousJumpSound;

	private OfflinePlayerModel mgr;
	private SkinnedMeshRenderer mesh;
	private Animator ani;
	private Transform chestJoint;
	private AudioSource aud;
	private AudioSource footstepAudio;
	private OfflineCharacterController controller;
	private TrailRenderer thrustTrail;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mgr = GetComponentInParent<OfflinePlayerModel>();
		mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		ani = GetComponent<Animator>();
		chestJoint = Utility.FindChild(gameObject, "Bro_Spine2").transform;
		aud = GetComponent<AudioSource>();
		footstepAudio = transform.Find("FootstepAudio").GetComponent<AudioSource>();
		controller = GetComponentInParent<OfflineCharacterController>();
		thrustTrail = GetComponentInChildren<TrailRenderer>();
	}

	public void Start()
	{
	}

	public void Update()
	{
		canPlayFootstep += Time.deltaTime;

		thrustTrailTime -= Time.deltaTime;
		thrustTrail.time = thrustCurve.Evaluate(thrustTrailTime / thrustTrailDuration);
	}

	public void LateUpdate()
	{
		chestJoint.localRotation = Quaternion.Euler(Mathf.Lerp(minLookRotation, maxLookRotation, lookRotationAlpha), 0.0f, 0.0f);

		//bool knifeVisible = false;
		//if (ani.GetCurrentAnimatorStateInfo(OfflinePlayerModel.ThirdPersonHandsLI).IsName("Melee") || ani.GetNextAnimatorStateInfo(OfflinePlayerModel.ThirdPersonHandsLI).IsName("Melee"))
		//{
		//	knifeVisible = true;
		//}
		//
		//if (knifeVisible != prevKnifeVisible)
		//{
		//	melee.KnifeVisible = knifeVisible;
		//	prevKnifeVisible = knifeVisible;
		//}
	}

	/**********************************************************/
	// Interface

	public void OnChangeWeapon(WeaponType type)
	{
		ani.runtimeAnimatorController = weaponControllers[(int)type];
	}

	public void Shoot()
	{
		ani.SetTrigger("Shoot");
	}

	public void Reload()
	{
		ani.SetTrigger("Reload");
	}

	public void Swap()
	{
		ani.SetTrigger("Swap");
		if (!mgr.hasAuthority)
		{
			aud.PlayOneShot(mgr.Swap1Sound);
		}
	}

	public void OnClipExpose()
	{
	}

	public void OnRemoveMag()
	{
	}

	public void OnChangeMag()
	{
	}

	public void OnSliderPull()
	{
	}

	public void OnReloadAnimation()
	{
	}

	public void OnSwapAnimation()
	{
		if (!mgr.hasAuthority)
		{
			aud.PlayOneShot(mgr.Swap2Sound);
		}
	}

	public void OnMeleeHitAnimation()
	{

	}

	public void OnMeleeReadyAnimation()
	{

	}

	public void OnMeleeDoneAnimation()
	{
	}

	public void Throw()
	{
		ani.SetTrigger("Throw");
		if (!mgr.hasAuthority)
		{
			aud.PlayOneShot(mgr.ThrowSound);
		}
	}

	public void Melee()
	{
		ani.SetTrigger("Melee");
		if (!mgr.hasAuthority)
		{
			aud.PlayOneShot(mgr.MeleeSound);
		}
	}

	public void Jump()
	{
		if (!mgr.hasAuthority)
		{
			int jumpSound = Random.Range(0, mgr.JumpSounds.Length);
			if (jumpSound == previousJumpSound)
			{
				jumpSound++;
				if (jumpSound >= mgr.JumpSounds.Length)
				{
					jumpSound = 0;
				}
			}
			previousJumpSound = jumpSound;

			aud.PlayOneShot(mgr.JumpSounds[jumpSound]);
		}
	}

	public void Thrust()
	{
		aud.PlayOneShot(mgr.ThrustSound);

		thrustTrailTime = thrustTrailDuration;
	}

	public void PickUp()
	{
		ani.Play("PickUp");
		if (!mgr.hasAuthority)
		{
			aud.PlayOneShot(mgr.PickUpSound);
		}
	}

	public void OnFootstepAnimation()
	{
		if (canPlayFootstep < 0.0f || mgr.Jumping)
		{
			return;
		}

		AudioClip[] footstepSounds = concreteFootstepSounds;
		SurfaceType groundSurfaceType = controller.GroundSurfaceType;
		switch (groundSurfaceType)
		{
			case SurfaceType.Grass: footstepSounds = grassFootstepSounds; break;
			case SurfaceType.Wood: footstepSounds = woodFootstepSounds; break;
		}

		int i = Random.Range(0, footstepSounds.Length);
		if (i == previousFootstepSound)
		{
			i++;
			if (i >= footstepSounds.Length)
			{
				i = 0;
			}
		}

		footstepAudio.volume = Mathf.Clamp01(mgr.RunSpeed / maxFootstepVolumeSpeed);
		footstepAudio.PlayOneShot(footstepSounds[i]);

		previousFootstepSound = i;
		canPlayFootstep = -0.08f;
	}

	public void OnLand(float landingVelocity)
	{
		AudioClip[] landSounds = concreteLandSounds;
		SurfaceType groundSurfaceType = controller.GroundSurfaceType;
		switch (groundSurfaceType)
		{
			case SurfaceType.Grass: landSounds = grassLandSounds; break;
			case SurfaceType.Wood: landSounds = woodLandSounds; break;
		}

		footstepAudio.volume = landingSoundVolumeCurve.Evaluate(-landingVelocity);
		footstepAudio.PlayOneShot(landSounds[Random.Range(0, landSounds.Length)]);
	}

	/**********************************************************/
	// Accessors/Mutators

	public Material Material
	{
		set
		{
			mesh.material = value;
		}
	}

	public Animator Animator
	{
		get
		{
			return ani;
		}
	}

	public SkinnedMeshRenderer Mesh
	{
		get
		{
			return mesh;
		}
	}

	public bool Visible
	{
		get
		{
			return mesh.enabled;
		}
		set
		{
			mesh.enabled = value;
		}
	}

	public float SwapTime
	{
		get
		{
			return 1.0f / ani.GetFloat("SwapSpeed");
		}
		set
		{
			ani.SetFloat("SwapSpeed", 1.0f / value);
		}
	}

	public float ReloadTime
	{
		get
		{
			return 1.0f / ani.GetFloat("ReloadSpeed");
		}
		set
		{
			ani.SetFloat("ReloadSpeed", 1.0f / value);
		}
	}

	public float PickUpTime
	{
		set
		{
			ani.SetFloat("PickUpSpeed", 1.0f / value);
		}
	}

	public float LookRotationAlpha
	{
		get
		{
			return lookRotationAlpha;
		}
		set
		{
			lookRotationAlpha = value;
		}
	}

	public AudioSource AudioSource
	{
		get
		{
			return aud;
		}
	}
}