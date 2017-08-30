using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstPersonHands : MonoBehaviour
{
	[SerializeField]
	private RuntimeAnimatorController[] weaponControllers;
	[SerializeField]
	private Vector3 positionLagMin;
	[SerializeField]
	private Vector3 positionLagMax;
	[SerializeField]
	private float positionLagCorrectionSpeed;
	[SerializeField]
	private float runRatioChangeSpeed;
	[SerializeField]
	private float adsPositionLagModifier;

	private Vector3 idlePosition;
	private int waitingForReload;
	private int waitingForShoot;
	private Vector3 positionLag;
	private float runRatio;

	private OfflinePlayerModel mgr;
	private Animator ani;
	private SkinnedMeshRenderer mesh;
	private OfflineCharacterController controller;
	private OfflineWeaponCarrier weaponCarrier;
	private OfflineMelee melee;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		idlePosition = transform.localPosition;
		waitingForReload = 0;
		positionLag = Vector3.zero;

		mgr = GetComponentInParent<OfflinePlayerModel>();
		ani = GetComponent<Animator>();
		mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		controller = GetComponentInParent<OfflineCharacterController>();
		weaponCarrier = GetComponentInParent<OfflineWeaponCarrier>();
		melee = GetComponentInParent<OfflineMelee>();
		aud = GetComponent<AudioSource>();

		OfflinePlayerModel.FirstPersonHandsLI = ani.GetLayerIndex("FirstPersonHands");
		OfflinePlayerModel.AimDownSightsLI = ani.GetLayerIndex("AimDownSights");

		ani.SetLayerWeight(OfflinePlayerModel.FirstPersonHandsLI, 1.0f);
	}

	public void Update()
	{
		UpdatePositionLag();
		UpdateRunRatio();

		ani.SetBool("Running", controller.IsRunning);

		Weapon weapon = weaponCarrier.PrimaryWeapon;
		if (weapon)
		{
			float adsCurved = weapon.AimDownSightsAlphaCurved;
			transform.localPosition = Vector3.Lerp(idlePosition + positionLag, weapon.AimDownSightsPosition + positionLag * adsPositionLagModifier, adsCurved);
			transform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(weapon.AimDownSightsRotation), adsCurved);

			ani.SetLayerWeight(OfflinePlayerModel.AimDownSightsLI, Mathf.Clamp01(weapon.AimDownSightsAlpha * 1.5f));

			weapon.SetMagHold(ani.GetFloat("MagHold"));
		}
		else
		{
			ani.SetLayerWeight(OfflinePlayerModel.AimDownSightsLI, 0.0f);
		}
	}

	public void LateUpdate()
	{
		int layer = OfflinePlayerModel.FirstPersonHandsLI;

		if (weaponCarrier.Swapping && !ani.GetCurrentAnimatorStateInfo(layer).IsName("Swap") && !ani.GetNextAnimatorStateInfo(layer).IsName("Swap"))
		{
			weaponCarrier.OnFinishSwap();
		}

		if (waitingForReload == 2 && (ani.GetCurrentAnimatorStateInfo(layer).IsName("Reload") || ani.GetNextAnimatorStateInfo(layer).IsName("Reload")))
		{
			waitingForReload = 1;
		}
		else if (waitingForReload == 1 && (!ani.GetCurrentAnimatorStateInfo(layer).IsName("Reload") && !ani.GetNextAnimatorStateInfo(layer).IsName("Reload")))
		{
			waitingForReload = 0;
		}

		if (weaponCarrier.Reloading && waitingForReload == 0)
		{
			weaponCarrier.OnFinishReload();
		}

		if (waitingForShoot == 2 && (ani.GetCurrentAnimatorStateInfo(layer).IsName("Shoot") || ani.GetNextAnimatorStateInfo(layer).IsName("Shoot")))
		{
			waitingForShoot = 1;
		}
		else if (waitingForShoot == 1 && (!ani.GetCurrentAnimatorStateInfo(layer).IsName("Shoot") && !ani.GetNextAnimatorStateInfo(layer).IsName("Shoot")))
		{
			waitingForShoot = 0;
			weaponCarrier.OnFinishShoot();
		}
	}

	/**********************************************************/
	// Interface

	public void OnChangeWeapon(WeaponType type)
	{
		ani.runtimeAnimatorController = weaponControllers[(int)type];
	}

	public void OnSwapAnimation()
	{
		weaponCarrier.OnSwap();
		aud.PlayOneShot(mgr.Swap2Sound);
	}

	public void OnClipExpose()
	{
		weaponCarrier.OnClipExpose();
	}

	public void OnRemoveMag()
	{
		weaponCarrier.OnRemoveMag();
	}

	public void OnChangeMag()
	{
		weaponCarrier.OnChangeMag();
	}

	public void OnSliderPull()
	{
		weaponCarrier.PrimaryWeapon.OnSliderPull();
	}

	public void OnReloadAnimation()
	{
		weaponCarrier.OnReload();
	}

	public void OnMeleeHitAnimation()
	{
		melee.OnMeleeHit();
	}

	public void OnMeleeReadyAnimation()
	{
		melee.OnMeleeReady();
	}

	public void OnMeleeDoneAnimation()
	{
		melee.OnMeleeDone();
	}

	public void Shoot()
	{
		ani.Play("Shoot", OfflinePlayerModel.FirstPersonHandsLI, 0.0f);
		ani.Play("ADSShoot", OfflinePlayerModel.AimDownSightsLI, 0.0f);
		waitingForShoot = 2;
	}

	public void Reload()
	{
		ani.SetTrigger("Reload");
		waitingForReload = 2;
	}

	public void Swap()
	{
		if (!ani.GetNextAnimatorStateInfo(OfflinePlayerModel.FirstPersonHandsLI).IsName("Swap") && !ani.GetCurrentAnimatorStateInfo(OfflinePlayerModel.FirstPersonHandsLI).IsName("Swap"))
		{
			ani.SetTrigger("Swap");
			weaponCarrier.Swapping = true;
			aud.PlayOneShot(mgr.Swap1Sound);
		}
	}

	public void AddPositionLag(Vector3 amount)
	{
		positionLag += amount;
	}

	public void Throw()
	{
		ani.Play("Throw");
		aud.PlayOneShot(mgr.ThrowSound);
	}

	public void Melee()
	{
		//if (!ani.GetCurrentAnimatorStateInfo(OfflinePlayerModel.FirstPersonHandsLI).IsName("Melee"))
		//{
			ani.Play("Melee");
		//}
		//else
		//{
		//	ani.SetTrigger("Melee");
		//}
		aud.PlayOneShot(mgr.MeleeSound);
	}

	public void PickUp()
	{
		ani.Play("PickUp");
		aud.PlayOneShot(mgr.PickUpSound);
	}

	/**********************************************************/
	// Helper Functions

	private void UpdatePositionLag()
	{
		positionLag.x = Mathf.Clamp(positionLag.x, positionLagMin.x, positionLagMax.x);
		positionLag.y = Mathf.Clamp(positionLag.y, positionLagMin.y, positionLagMax.y);
		positionLag.z = Mathf.Clamp(positionLag.z, positionLagMin.z, positionLagMax.z);

		positionLag = Vector3.Lerp(positionLag, Vector3.zero, positionLagCorrectionSpeed * Time.deltaTime);
	}

	private void UpdateRunRatio()
	{
		if (controller.IsRunning && !weaponCarrier.Reloading)
		{
			if (controller.Sprinting)
			{
				runRatio += Time.deltaTime * runRatioChangeSpeed;
			}
			else
			{
				if (runRatio > 0.5f)
				{
					runRatio -= Time.deltaTime * runRatioChangeSpeed;
				}
				else
				{
					float add = Time.deltaTime * runRatioChangeSpeed;
					if (runRatio + add > 0.5f)
					{
						add = 0.5f - runRatio;
					}
					runRatio += add;
				}
			}
		}
		else
		{
			runRatio -= Time.deltaTime * runRatioChangeSpeed;
		}
		runRatio = Mathf.Clamp01(runRatio);
		mgr.SetRunRatio(runRatio);
	}

	/**********************************************************/
	// Accessors/Mutators

	public Material Material
	{
		get
		{
			return mesh.material;
		}
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

	public WeaponType WeaponType
	{
		set
		{
			//ani.SetInteger("Weapon", (int)value);
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

	public AudioSource AudioSource
	{
		get
		{
			return aud;
		}
	}

	public float RunRatio
	{
		get
		{
			return runRatio;
		}
	}
}
