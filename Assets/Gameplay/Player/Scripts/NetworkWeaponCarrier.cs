using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkWeaponCarrier : OfflineWeaponCarrier
{
	[SyncVar]
	private bool sniperIsScoped;

	private NetworkPlayer net;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		net = GetComponent<NetworkPlayer>();
	}

	protected override bool Ready()
	{
		return net.Initialized;
	}

	protected override void DelayedAwake()
	{
		base.DelayedAwake();

		primaryWeapon = null;
		secondaryWeapon = null;
	}

	protected override void DelayedStart()
	{
		InitializeStartingWeapons();

		firstPersonHands.SwapTime = swapTime;
		thirdPersonModel.SwapTime = swapTime;
		firstPersonHands.PickUpTime = pickUpTime;
		thirdPersonModel.PickUpTime = pickUpTime;
		playerModel.OnChangeWeapon(settings.StartingPrimaryWeapon);
	}

	public override void Update()
	{
		SafeInitUpdate();

		if (initialized && primaryWeapon)
		{
			if (net.hasAuthority)
			{
				UpdateFirstPerson();
				CheckEnemyInCrosshairs();
			}
			else
			{
				primaryWeapon.ThirdPersonUpdate();
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (net.hasAuthority && PrimaryWeapon && SecondaryWeapon)
		{
			if (other.tag == "Drop")
			{
				WeaponDrop drop = other.GetComponentInParent<WeaponDrop>();
				if (PrimaryWeapon.GetWeaponType() == drop.WeaponType || SecondaryWeapon.GetWeaponType() == drop.WeaponType)
				{
					net.CmdPickUpWeapon(drop.ID, -1, 0);
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	protected override void InitializeWeapon(Weapon weapon)
	{
		base.InitializeWeapon(weapon);

		if (net.hasAuthority)
		{
			weapon.LoadGameSettings();
			weapon.OnStartLocalPlayer();
		}
	}

	public void SetPrimaryWeapon(WeaponType type, int id)
	{
		DropPrimaryWeapon();

		GameObject model = Utility.FindChild(gameObject, net.hasAuthority ? "FirstPersonHands" : "ThirdPersonModel");
		primaryWeapon = (Instantiate(weaponPrefabs[(int)type], Utility.FindChild(model, "Bro_RightHand").transform)).GetComponent<Weapon>();
		InitializeWeapon(primaryWeapon);
		primaryWeapon.ID = id;
		firstPersonHands.ReloadTime = primaryWeapon.ReloadTime;
		thirdPersonModel.ReloadTime = primaryWeapon.ReloadTime;
		if (net.hasAuthority)
		{
			primaryWeapon.OnSwapTo();
			playerModel.OnChangeWeapon(type);
			hud.AmmoDisplay.PrimaryWeapon = type;
		}
	}

	public void SetSecondaryWeapon(WeaponType type, int id)
	{
		DropSecondaryWeapon();

		if (type != WeaponType.None)
		{
			secondaryWeapon = (Instantiate(weaponPrefabs[(int)type], Utility.FindChild(gameObject, "Bro_RightHand").transform)).GetComponent<Weapon>();
			InitializeWeapon(secondaryWeapon);
			secondaryWeapon.ID = id;
		}

		hud.AmmoDisplay.SecondaryWeapon = type;
	}

	public void AddAmmo(int id, int ammo)
	{
		if (ammo > 0)
		{
			if (PrimaryWeapon.ID == id)
			{
				PrimaryWeapon.Ammo += ammo;
				PrimaryWeapon.UpdateAmmoHUD();
				CheckForReload();
			}
			else if (SecondaryWeapon.ID == id)
			{
				SecondaryWeapon.Ammo += ammo;
			}

			aud.PlayOneShot(AmmoPickUpSound);
		}
	}

	public override void OnRespawn()
	{
		base.OnRespawn();

		foreach (Weapon w in GetComponentsInChildren<Weapon>())
		{
			w.Visible = false;
		}
		primaryWeapon.Visible = true;
		playerModel.OnChangeWeapon(primaryWeapon.GetWeaponType());

		if (net.hasAuthority)
		{
			playerModel.OnPickUp();
		}
	}

	protected override void ChangeWeapons()
	{
		base.ChangeWeapons();

		net.CmdSwapWeapon(primaryWeapon.GetWeaponType());
	}

	protected override void AttemptWeaponPickUp(WeaponDrop drop)
	{
		net.CmdPickUpWeapon(drop.ID, primaryWeapon.ID, primaryWeapon.AmmoInClip);
	}

	public void OnPickUpWeapon(WeaponType type, int weaponID, int swappingWeaponID, int ammo, int ammoInClip)
	{
		if (swappingWeaponID == PrimaryWeapon.ID)
		{
			SetPrimaryWeapon(type, weaponID);
			primaryWeapon.Ammo = ammo;
			primaryWeapon.AmmoInClip = ammoInClip;
			primaryWeapon.UpdateAmmoHUD();
			playerModel.OnPickUp();
			Reloading = false;
			hud.AmmoDisplay.PrimaryWeapon = primaryWeapon.GetWeaponType();
		}
		else if (swappingWeaponID == SecondaryWeapon.ID)
		{
			SetSecondaryWeapon(type, weaponID);
			secondaryWeapon.Ammo = ammo;
			secondaryWeapon.AmmoInClip = ammoInClip;
			primaryWeapon.UpdateAmmoHUD();
			playerModel.OnPickUp();
			Reloading = false;
			hud.AmmoDisplay.SecondaryWeapon = secondaryWeapon.GetWeaponType();
		}
		else
		{
			Debug.LogError("Swapping weapon ID not found for primary or secondary weapon!!!");
		}
	}

	/**********************************************************/
	// Third-person Functions

	public void InitializeStartingWeapons()
	{
		if (!primaryWeapon)
		{
			SetThirdPersonWeapon(settings.StartingPrimaryWeapon);
		}
		primaryWeapon.Visible = true;
	}

	public void SetThirdPersonWeapon(WeaponType type)
	{
		DropPrimaryWeapon();
		SetPrimaryWeapon(type, -1);
	}

	/**********************************************************/
	// Commands

	[Command]
	public void CmdSetSniperIsScoped(bool isScoped)
	{
		sniperIsScoped = isScoped;
	}

	/**********************************************************/
	// Helper Functions

	private void CheckEnemyInCrosshairs()
	{
		bool inCrosshairs = false;
		if (PlayerManager.Instance)
		{
			foreach (NetworkPlayer p in PlayerManager.PlayerList)
			{
				if (net != p && !PartyManager.SameTeam(net.ID, p.ID) && (net.transform.position - p.transform.position).sqrMagnitude < PrimaryWeapon.CrosshairRange * PrimaryWeapon.CrosshairRange)
				{
					RaycastHit hit = primaryWeapon.GetRaycastHit(cam.transform.forward);
					if (hit.collider && hit.collider.GetComponent<BodyPartCollider>())
					{
						inCrosshairs = true;
						break;
					}
				}
			}
		}
		hud.Crosshairs.EnemyInCrosshairs = inCrosshairs;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool SniperIsScoped
	{
		get
		{
			return sniperIsScoped;
		}
		set
		{
			sniperIsScoped = value;
		}
	}
}
