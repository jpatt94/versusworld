using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OfflineWeaponCarrier : SafeNetworkBehaviour
{
	[SerializeField]
	protected WeaponType offlinePrimaryType;
	[SerializeField]
	protected WeaponType offlineSecondaryType;
	[SerializeField]
	protected float swapTime;
	[SerializeField]
	protected float pickUpTime;
	[SerializeField]
	protected float pickUpRate;

	[SerializeField]
	protected GameObject[] weaponPrefabs;

	public AudioClip AmmoPickUpSound;
	public AudioClip DeadMansClickSound;
	public AudioClip AimDownSightsSound;

	protected PlayerWeaponSettings settings;
	protected Weapon primaryWeapon;
	protected Weapon secondaryWeapon;
	protected float canPickUp;
	protected bool aimingDownSights;
	protected bool wasSprinting;
	protected bool swapping;
	protected bool reloading;
	protected bool needsShootingReset;

	protected HUD hud;
	protected OfflineCharacterController controller;
	protected FirstPersonHands firstPersonHands;
	protected ThirdPersonModel thirdPersonModel;
	protected OfflinePlayerModel playerModel;
	protected AudioSource aud;
	protected CameraManager cam;
	protected MultiplayerMap map;
	protected PlayerInteractor interactor;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		hud = GameObject.Find("HUD").GetComponent<HUD>();
		controller = GetComponent<OfflineCharacterController>();
		firstPersonHands = GetComponentInChildren<FirstPersonHands>();
		thirdPersonModel = GetComponentInChildren<ThirdPersonModel>();
		playerModel = GetComponent<OfflinePlayerModel>();
		aud = firstPersonHands.GetComponent<AudioSource>();
		cam = GetComponentInChildren<CameraManager>();
		interactor = GetComponent<PlayerInteractor>();

		needsShootingReset = false;
	}

	protected override void DelayedAwake()
	{
	}

	protected override void DelayedStart()
	{
		if (settings == null) // Should only happen when offline
		{
			settings = PartyManager.GameSettings.GetPlayerTraits(PlayerTraitsType.Default).Weapons;
		}

		primaryWeapon = (Instantiate(weaponPrefabs[(int)offlinePrimaryType], Utility.FindChild(gameObject, "Bro_RightHand").transform)).GetComponent<Weapon>();
		secondaryWeapon = (Instantiate(weaponPrefabs[(int)offlineSecondaryType], Utility.FindChild(gameObject, "Bro_RightHand").transform)).GetComponent<Weapon>(); ;
		canPickUp = 0.0f;
		aimingDownSights = false;
		wasSprinting = false;
		swapping = false;

		InitializeWeapon(primaryWeapon);
		primaryWeapon.OnStartLocalPlayer();
		primaryWeapon.OnSwapTo();

		firstPersonHands.WeaponType = primaryWeapon.GetWeaponType();
		firstPersonHands.SwapTime = swapTime;
		thirdPersonModel.SwapTime = swapTime;
		firstPersonHands.ReloadTime = primaryWeapon.ReloadTime;
		thirdPersonModel.ReloadTime = primaryWeapon.ReloadTime;
		firstPersonHands.PickUpTime = pickUpTime;
		thirdPersonModel.PickUpTime = pickUpTime;
		playerModel.OnChangeWeapon(primaryWeapon.GetWeaponType());

		InitializeWeapon(secondaryWeapon);
		secondaryWeapon.OnStartLocalPlayer();
	}

	public override void Update()
	{
		base.Update();

		if (initialized)
		{
			UpdateFirstPerson();
		}
	}

	/**********************************************************/
	// Interface

	public void Enable()
	{
		enabled = true;
	}

	public void Reload()
	{
		if (reloading)
		{
			return;
		}

		reloading = true;
		primaryWeapon.OnReloadStart();
		playerModel.OnReload();
	}

	public void OnSwap()
	{
		if (secondaryWeapon)
		{
			ChangeWeapons();
			playerModel.OnChangeWeapon(primaryWeapon.GetWeaponType());
		}
	}

	public void OnFinishSwap()
	{
		swapping = false;
	}

	public void OnClipExpose()
	{
		primaryWeapon.OnClipExpose();
	}

	public void OnRemoveMag()
	{
		primaryWeapon.OnRemoveMag();
	}

	public void OnChangeMag()
	{
		primaryWeapon.OnChangeMag();
	}

	public void OnReload()
	{
		primaryWeapon.OnReload();
	}

	public void OnFinishReload()
	{
		reloading = false;
		primaryWeapon.OnReloadFinish();
	}

	public void CheckForReload()
	{
		if (primaryWeapon.AmmoInClip == 0)
		{
			Reload();
		}
	}

	public virtual void OnRespawn()
	{
		aimingDownSights = false;
	}

	public virtual void OnTakeDamage(float damage, Vector3 position, BodyPart bodyPart)
	{
		if (PrimaryWeapon)
		{
			primaryWeapon.OnTakeDamage(damage, position, bodyPart);
		}
	}

	public virtual void OnMeleeDone()
	{
		primaryWeapon.CheckAutoReload();
	}

	public void OnFinishShoot()
	{
		primaryWeapon.OnFinishShoot();
	}

	/**********************************************************/
	// Child Interface

	protected virtual void UpdateFirstPerson()
	{
		if (primaryWeapon == null)
		{
			return;
		}

		if (needsShootingReset)
		{
			if (PlayerInput.Shoot(ButtonStatus.Released))
			{
				needsShootingReset = false;
			}
		}

		if (PlayerInput.AimDownSights(ButtonStatus.Pressed))
		{
			primaryWeapon.AimDownSights(true);
			if (secondaryWeapon)
			{
				secondaryWeapon.AimDownSights(true);
			}
			wasSprinting = controller.Sprinting;
			aimingDownSights = true;
			aud.PlayOneShot(AimDownSightsSound);
		}
		else if (PlayerInput.AimDownSights(ButtonStatus.Released))
		{
			controller.Sprinting = wasSprinting;
			primaryWeapon.AimDownSights(false);
			if (secondaryWeapon)
			{
				secondaryWeapon.AimDownSights(false);
			}
			aimingDownSights = false;
			aud.PlayOneShot(AimDownSightsSound);
		}

		controller.Sprinting = controller.Sprinting && !aimingDownSights;
		wasSprinting = wasSprinting && !controller.CheckCancelSprint();

		if (PlayerInput.Swap() && secondaryWeapon && !interactor.InteractableAvailable)
		{
			StartSwap();
		}
		else if (primaryWeapon.CheckReloadInput() && primaryWeapon.CanReload())
		{
			Reload();
		}

		primaryWeapon.FirstPersonUpdate();
		primaryWeapon.ThirdPersonUpdate();

		CheckWeaponPickUp();
	}

	protected virtual void InitializeWeapon(Weapon weapon)
	{
		weapon.OnStart();
	}

	public void DropPrimaryWeapon()
	{
		if (primaryWeapon)
		{
			Destroy(primaryWeapon.gameObject);
			primaryWeapon = null;
		}
	}

	public void DropSecondaryWeapon()
	{
		if (secondaryWeapon)
		{
			Destroy(secondaryWeapon.gameObject);
			secondaryWeapon = null;
		}
	}

	public void DropWeapons()
	{
		DropPrimaryWeapon();
		DropSecondaryWeapon();
	}

	protected virtual void StartSwap()
	{
		SendMessageUpwards("OnStartSwap");
	}

	protected virtual void ChangeWeapons()
	{
		Weapon temp = primaryWeapon;
		primaryWeapon = secondaryWeapon;
		secondaryWeapon = temp;

		primaryWeapon.OnSwapTo();
		secondaryWeapon.OnSwapFrom();

		firstPersonHands.WeaponType = primaryWeapon.GetWeaponType();
		firstPersonHands.ReloadTime = primaryWeapon.ReloadTime;
		thirdPersonModel.ReloadTime = primaryWeapon.ReloadTime;

		hud.AmmoDisplay.PrimaryWeapon = primaryWeapon.GetWeaponType();
		hud.AmmoDisplay.SecondaryWeapon = secondaryWeapon.GetWeaponType();
	}

	protected virtual void CheckWeaponPickUp()
	{
		canPickUp += Time.deltaTime;

		bool foundDrop = false;

		if (!interactor.InteractableAvailable)
		{
			Collider[] cols = Physics.OverlapCapsule(transform.position + Vector3.up, transform.position + Vector3.down, 0.5f, Physics.IgnoreRaycastLayer, QueryTriggerInteraction.Collide);
			foreach (Collider col in cols)
			{
				WeaponDrop drop = col.GetComponentInParent<WeaponDrop>();
				if (drop && drop.WeaponType != primaryWeapon.GetWeaponType() && drop.WeaponType != secondaryWeapon.GetWeaponType())
				{
					if (canPickUp >= 0.0f && PlayerInput.PickUp())
					{
						AttemptWeaponPickUp(drop);
						canPickUp = -(1.0f / pickUpRate);
						PlayerInput.NeedsSwapRelease();
					}

					hud.WeaponPickUp.Enable(drop.WeaponType);
					foundDrop = true;
					break;
				}
			}
		}

		if (!foundDrop)
		{
			hud.WeaponPickUp.Disable();
		}
	}

	protected virtual void AttemptWeaponPickUp(WeaponDrop drop)
	{
	}

	/**********************************************************/
	// Accessors/Mutators

	public PlayerWeaponSettings Traits
	{
		set
		{
			settings = value;

			hud.AmmoDisplay.InfiniteAmmoType = settings.InfiniteAmmo;
		}
	}

	public PlayerWeaponSettings Settings
	{
		get
		{
			return settings;
		}
	}

	public Weapon PrimaryWeapon
	{
		get
		{
			return primaryWeapon;
		}
	}

	public Weapon SecondaryWeapon
	{
		get
		{
			return secondaryWeapon;
		}
	}

	public bool Swapping
	{
		get
		{
			return swapping;
		}
		set
		{
			swapping = value;
		}
	}

	public bool Reloading
	{
		get
		{
			return reloading;
		}
		set
		{
			reloading = value;
		}
	}

	public bool NeedsShootingReset
	{
		get
		{
			return needsShootingReset;
		}
		set
		{
			needsShootingReset = value;
		}
	}
}
