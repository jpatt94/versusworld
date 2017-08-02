using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
	[SerializeField]
	protected int ammo;
	[SerializeField]
	protected int clipSize;
	[SerializeField]
	protected float fireRate;
	[SerializeField]
	protected float reloadTime;
	[SerializeField]
	protected Vector3 idlePosition;
	[SerializeField]
	protected Vector3 idleRotation;
	[SerializeField]
	protected Vector3 aimDownSightsPosition;
	[SerializeField]
	protected Vector3 aimDownSightsRotation;
	[SerializeField]
	protected float aimDownSightsDuration;
	[SerializeField]
	protected AnimationCurve aimDownSightsTransform;
	[SerializeField]
	protected float aimDownSightsFOV;
	[SerializeField]
	protected float aimDownSightsSensitity;
	[SerializeField]
	protected float crosshairRange;
	[SerializeField]
	protected GameObject shellPrefab;
	[SerializeField]
	protected float shellExitForce;
	[SerializeField]
	protected float viewShakeAmount;
	[SerializeField]
	protected float viewShakeDuration;
	[SerializeField]
	protected float adsViewShakeAmount;
	[SerializeField]
	protected float adsViewShakeDuration;

	[SerializeField]
	protected AudioClip[] shootSounds;
	[SerializeField]
	protected AudioClip reloadSound;
	[SerializeField]
	protected AudioClip exposeClipSound;
	[SerializeField]
	protected AudioClip removeMagSound;
	[SerializeField]
	protected AudioClip changeMagSound;
	[SerializeField]
	protected AudioClip insertClipSound;
	[SerializeField]
	protected AudioClip slidePullSound;

	protected int id;
	protected bool firstPerson;
	protected int ammoInClip;
	protected float canShoot;
	protected bool adsEnabled;
	protected float adsTime;
	protected bool canADS;
	protected bool canCreateFX;
	protected int reloadState;

	protected Transform barrel;
	protected HUD hud;
	protected CameraManager cam;
	protected AudioSource aud;
	protected AudioSource tailSoundAud;
	protected NetworkPlayer net;
	protected OfflineWeaponCarrier carrier;
	protected OfflineCharacterController controller;
	protected Transform firstPersonCam;
	protected OfflineGrenadeCarrier grenades;
	protected OfflineMelee melee;
	protected OfflinePlayerModel model;
	protected Transform shellExit;

	/**********************************************************/
	// Interface

	public virtual WeaponType GetWeaponType()
	{
		return WeaponType.None;
	}

	public virtual void OnStart()
	{
		barrel = transform.Find("Barrel");
		hud = GameObject.Find("HUD").GetComponent<HUD>();
		cam = GetComponentInParent<CameraManager>();
		aud = GetComponent<AudioSource>();
		net = GetComponentInParent<NetworkPlayer>();
		carrier = GetComponentInParent<OfflineWeaponCarrier>();
		controller = GetComponentInParent<OfflineCharacterController>();
		firstPersonCam = Utility.FindChild(carrier.gameObject, "FirstPersonCamera").transform;
		grenades = GetComponentInParent<OfflineGrenadeCarrier>();
		melee = GetComponentInParent<OfflineMelee>();
		model = GetComponentInParent<OfflinePlayerModel>();
		shellExit = transform.Find("ShellExit");

		Transform tail = transform.Find("TailSound");
		if (tail)
		{
			tailSoundAud = tail.GetComponent<AudioSource>();
		}

		transform.localPosition = idlePosition;
		transform.localRotation = Quaternion.identity;

		Visible = false;

		reloadState = 4;
	}

	public virtual void OnStartLocalPlayer()
	{
		firstPerson = true;

		ammoInClip = clipSize;
		canShoot = 0.0f;
		adsEnabled = false;
		adsTime = 0.0f;

		aud.spatialBlend = 0.0f;

		foreach (Transform t in GetComponentsInChildren<Transform>())
		{
			if (t.gameObject.name == "Barrel")
			{
				continue;
			}

			t.gameObject.layer = LayerMask.NameToLayer("FirstPerson");
		}
	}

	public virtual void LoadGameSettings()
	{
	}

	public virtual void FirstPersonUpdate()
	{
		UpdateAimDownSights();

		if (!carrier.Swapping)
		{
			if (!carrier.Reloading)
			{
				UpdateShooting();
			}
		}
	}

	public virtual void ThirdPersonUpdate()
	{
		if (!firstPerson)
		{
			transform.localRotation = Quaternion.Euler(idleRotation);
			canCreateFX = true;
		}
	}

	public virtual void OnSwapTo()
	{
		UpdateAmmoHUD();

		Visible = true;
		canADS = true;

		hud.Crosshairs.SetType(GetWeaponType());

		CheckAutoReload();
	}

	public virtual void OnSwapFrom()
	{
		Visible = false;
		canShoot = 0.0f;
	}

	public virtual void OnReloadStart()
	{
		reloadState = 0;

		aud.PlayOneShot(reloadSound);
	}

	public virtual void OnClipExpose()
	{
		if (exposeClipSound)
		{
			aud.PlayOneShot(exposeClipSound);
		}
	}

	public virtual void OnRemoveMag()
	{
		reloadState = 1;

		if (removeMagSound)
		{
			aud.PlayOneShot(removeMagSound);
		}
	}

	public virtual void OnChangeMag()
	{
		if (changeMagSound)
		{
			aud.PlayOneShot(changeMagSound);
		}
	}

	public virtual void OnSliderPull()
	{
		reloadState = 3;

		if (slidePullSound)
		{
			aud.PlayOneShot(slidePullSound);
		}
	}

	public virtual void OnReload()
	{
		reloadState = 2;

		ammoInClip = Mathf.Min(clipSize, ammo);
		UpdateAmmoHUD();

		aud.PlayOneShot(insertClipSound);
	}

	public virtual void OnReloadFinish()
	{
		canShoot = 0.0f;
		reloadState = 4;
	}

	public virtual void UpdateAmmoHUD()
	{
		hud.AmmoDisplay.AmmoInReserves = ammo - ammoInClip;
		hud.AmmoDisplay.AmmoInClip = ammoInClip;
	}

	public void AimDownSights(bool enabled)
	{
		adsEnabled = enabled;
	}

	public void ThirdPersonShoot(NetworkPlayer victim, GameObject damageable, Vector3 position, Vector3 normal, SurfaceType surfaceType, bool noHit = false)
	{
		if (canCreateFX)
		{
			CreateShootFX();
			PlayShootSound(true);
			canCreateFX = false;
		}

		hud.Radar.SetPlayerDidShoot(carrier.GetComponent<NetworkPlayer>());

		if (victim)
		{
			CreateHitPlayerFX(victim.transform.position + position, normal, BodyPart.None);
		}
		else if (!noHit)
		{
			Transform parent = null;
			if (damageable)
			{
				parent = damageable.transform;
			}

			CreateHitEnvironmentFX(position, normal, surfaceType, parent);
		}
		else
		{
			CreateNoHitFX(position);
		}
	}

	public virtual void OnFinishSwap()
	{
		if (ammoInClip <= 0 && CanReload())
		{
			carrier.Reload();
		}
	}

	public virtual bool CheckReloadInput()
	{
		return PlayerInput.Reload(ButtonStatus.Pressed);
	}

	public virtual bool CanReload()
	{
		return ammo > 0 && ammoInClip != clipSize && ammoInClip != ammo && !carrier.Reloading;
	}

	public virtual void OnFinishShoot()
	{
		//CheckAutoReload();
	}

	public virtual void OnTakeDamage(float damage, Vector3 position, BodyPart bodyPart)
	{
	}

	public virtual void CheckAutoReload()
	{
		if (ammoInClip <= 0 && CanReload())
		{
			carrier.Reload();
		}
	}

	public virtual void SetMagHold(float magHold)
	{
	}

	/**********************************************************/
	// Child Interface

	protected virtual void UpdateAimDownSights()
	{
		if (firstPerson)
		{
			bool aimingDownSights = adsEnabled && canADS && !carrier.Reloading && !carrier.Swapping && grenades.CanThrow && !melee.Active;

			adsTime += Time.deltaTime * (aimingDownSights ? 1 : -1);
			adsTime = Mathf.Clamp(adsTime, 0.0f, aimDownSightsDuration);

			float adsAlpha = AimDownSightsAlpha;
			hud.Crosshairs.Alpha = 1.0f - adsAlpha;
			cam.SetFOV(Mathf.Lerp(cam.DefaultFOV, aimDownSightsFOV, adsAlpha));
			PlayerInput.MouseLookSensitivity = Mathf.Lerp(ControlSettings.MouseSensitivity, ControlSettings.MouseSensitivity * aimDownSightsSensitity, adsAlpha);
		}

		transform.localRotation = Quaternion.Euler(idleRotation);
	}

	protected virtual void UpdateShooting()
	{
		canShoot += Time.deltaTime;

		if (!controller.Sprinting || !controller.IsGrounded)
		{
			if (CheckShootInput() && CanShoot())
			{
				Shoot();
				SendMessageUpwards("OnShoot");
			}
			else if (PlayerInput.Shoot(ButtonStatus.Pressed) && ammo <= 0)
			{
				OnDryFire();
			}
		}
		else if (PlayerInput.Shoot(ButtonStatus.Pressed))
		{
			controller.Sprinting = false;
			//carrier.NeedsShootingReset = true;
		}
	}

	protected virtual bool CheckShootInput()
	{
		return PlayerInput.Shoot(ButtonStatus.Down);
	}

	protected virtual bool CanShoot()
	{
		return canShoot >= 0.0f && ammoInClip > 0 && !carrier.Reloading && grenades.CanThrow && !melee.Active && !carrier.NeedsShootingReset;
	}

	protected virtual void Shoot()
	{
		DecreaseAmmo();
		CheckAutoReload();

		CreateShootFX();
		PlayShootSound();
		UpdateAmmoHUD();
		hud.Crosshairs.Pop();

		canShoot = -(1.0f / fireRate);
	}

	protected virtual void OnDryFire()
	{
		aud.PlayOneShot(carrier.DeadMansClickSound);
	}

	protected virtual void HitPlayer(RaycastHit hit)
	{
		NetworkPlayer otherNet = hit.collider.GetComponentInParent<NetworkPlayer>();
		BodyPartCollider bodyPart = hit.collider.GetComponent<BodyPartCollider>();
		if (bodyPart)
		{
			if (PartyManager.SameTeam(net.ID, otherNet.ID))
			{
				hud.HitMarker.Trigger(HitMarkerType.Friendly);
			}
			else
			{
				if (otherNet.WeaponCarrier.PrimaryWeapon.GetWeaponType() == WeaponType.Sniper && otherNet.WeaponCarrier.SniperIsScoped)
				{
					hud.HitMarker.Trigger(HitMarkerType.SuppressSniper);
				}
				else
				{
					hud.HitMarker.Trigger(bodyPart.type == BodyPart.Head ? HitMarkerType.HeadShot : HitMarkerType.Default);
				}
			}
		}
		else
		{
			Debug.LogError("HitPlayer(): no body part attached to collider " + hit.collider);
		}

		int playerID = -1;
		if (otherNet)
		{
			playerID = otherNet.ID;
		}

		if (net)
		{
			net.ShootPlayer(id, playerID, bodyPart.type, hit.point - hit.collider.transform.position, hit.normal);
		}
	}

	protected virtual void HitDamageable(RaycastHit hit)
	{
		if (net)
		{
			net.ShootDamageable(id, hit.collider.gameObject, hit.point, hit.normal, GetSurfaceType(hit));
			if (hit.collider.GetComponentInParent<Damageable>().ShowsHitMarker)
			{
				hud.HitMarker.Trigger(HitMarkerType.Default);
			}
		}
	}

	protected virtual void HitEnvironment(RaycastHit hit)
	{
		if (net)
		{
			net.Shoot(id, hit.point, hit.normal, GetSurfaceType(hit));
		}
	}

	protected virtual void PlayShootSound(bool thirdPerson = false)
	{
		aud.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
		if (tailSoundAud)
		{
			tailSoundAud.Play();
		}
	}

	protected virtual void CreateShootFX()
	{
		if (shellPrefab && shellExit)
		{
			GameObject obj = Instantiate(shellPrefab, shellExit.position, shellExit.rotation);
			Rigidbody rig = obj.GetComponent<Rigidbody>();
			rig.AddForce(shellExit.forward * shellExitForce * Random.Range(0.5f, 1.5f), ForceMode.VelocityChange);
			rig.AddTorque(Random.Range(0, 2.0f), Random.Range(0, 2.0f), Random.Range(0, 2.0f));
			Destroy(obj, 3.0f);
		}

		if (cam)
		{
			float a = AimDownSightsAlpha;
			cam.ViewShake(Mathf.Lerp(viewShakeAmount, adsViewShakeAmount, a), Mathf.Lerp(viewShakeDuration, adsViewShakeDuration, a));
		}
	}

	protected virtual void CreateHitPlayerFX(Vector3 position, Vector3 normal, BodyPart bodyPart)
	{
	}

	protected virtual void CreateHitEnvironmentFX(Vector3 position, Vector3 normal, SurfaceType surfaceType, Transform parent = null)
	{
		if (surfaceType != SurfaceType.None)
		{
			GameObject obj = Instantiate(EnvironmentManager.GetHitEffect(surfaceType));
			obj.transform.position = position;
			obj.transform.rotation = Quaternion.LookRotation(normal);

			if (parent)
			{
				obj.transform.SetParent(parent);
			}
		}
	}

	protected virtual void CreateNoHitFX(Vector3 position)
	{
	}

	public virtual RaycastHit GetRaycastHit(Vector3 forward)
	{
		RaycastHit hit;
		int mask = net ? NetworkPlayer.BodyPartLayerMask : 1;
		Physics.Raycast(new Ray(cam.transform.position, forward), out hit, float.MaxValue, mask);
		return hit;
	}

	protected virtual SurfaceType GetSurfaceType(RaycastHit hit)
	{
		if (hit.collider && hit.collider.sharedMaterial)
		{
			return EnvironmentManager.ConvertSurfaceNameToType(hit.collider.sharedMaterial.name);
		}

		return SurfaceType.None;
	}

	protected void DecreaseAmmo()
	{
		if (carrier.Settings.InfiniteAmmo != InfiniteAmmoType.BottomlessClip)
		{
			ammoInClip--;
		}
		if (carrier.Settings.InfiniteAmmo == InfiniteAmmoType.None)
		{
			ammo--;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public float AimDownSightsAlpha
	{
		get
		{
			return adsTime / aimDownSightsDuration;
		}
	}

	public float AimDownSightsAlphaCurved
	{
		get
		{
			return aimDownSightsTransform.Evaluate(AimDownSightsAlpha);
		}
	}

	public Vector3 AimDownSightsPosition
	{
		get
		{
			return aimDownSightsPosition;
		}
	}

	public Vector3 AimDownSightsRotation
	{
		get
		{
			return aimDownSightsRotation;
		}
	}

	public int Ammo
	{
		get
		{
			return ammo;
		}
		set
		{
			ammo = value;
		}
	}

	public int AmmoInClip
	{
		get
		{
			return ammoInClip;
		}
		set
		{
			ammoInClip = value;
		}
	}

	public bool Visible
	{
		set
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = value;
			}
		}
	}

	public float ReloadTime
	{
		get
		{
			return reloadTime;
		}
		set
		{
			reloadTime = value;
		}
	}

	public float CrosshairRange
	{
		get
		{
			return crosshairRange;
		}
	}
}