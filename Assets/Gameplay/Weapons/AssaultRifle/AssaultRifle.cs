using UnityEngine;
using System.Collections;

public class AssaultRifle : Weapon
{
	[SerializeField]
	private int burstAmount;
	[SerializeField]
	private float burstRate;
	[SerializeField]
	private float jerkFactor;
	[SerializeField]
	private float adsJerkFactor;
	[SerializeField]
	private Vector3 magHoldPosition;
	[SerializeField]
	private Vector3 magHoldRotation;
	[SerializeField]
	private float sliderClosedPos;
	[SerializeField]
	private float sliderOpenPos;
	[SerializeField]
	private float sliderSpeed;
	[SerializeField]
	private AnimationCurve sliderCurve;
	[SerializeField]
	private float sliderReloadTime;

	public GameObject impactEffect;
	public GameObject bloodEffect;

	private int burstsRemaining;
	private float burstTime;
	private bool storedShootInput;
	private bool storedReloadInput;
	private float trailTime;
	private float sliderTime;

	private ParticleSystem muzzleFlash;
	private LineRenderer trail;
	private Transform magTransform;
	private Vector3 magIdlePosition;
	private Vector3 magIdleRotation;
	private Transform sliderTransform;

	private AssaultRifleSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public void OnDestroy()
	{
		trail.enabled = false;
	}

	/**********************************************************/
	// Interface

	public override WeaponType GetWeaponType()
	{
		return WeaponType.AssaultRifle;
	}

	public override void OnStart()
	{
		base.OnStart();

		burstsRemaining = 0;
		burstTime = 0.0f;
		storedShootInput = false;
		storedReloadInput = false;

		muzzleFlash = barrel.Find("MuzzleFlash").GetComponent<ParticleSystem>();
		trail = barrel.GetComponent<LineRenderer>();
		magTransform = transform.Find("AssaultRifleMesh/Magazine");
		magIdlePosition = magTransform.localPosition;
		magIdleRotation = magTransform.localRotation.eulerAngles;
		sliderTransform = transform.Find("AssaultRifleMesh/Slider");

		sliderTime = 1.0f;

		settings = PartyManager.GameSettings.Weapons.AssaultRifle;
	}

	public override void FirstPersonUpdate()
	{
		storedShootInput = storedShootInput || (PlayerInput.Shoot(ButtonStatus.Pressed) && !carrier.Reloading);

		if (burstsRemaining > 0)
		{
			if (ammoInClip > 0)
			{
				burstTime -= Time.deltaTime;
				if (burstTime <= 0.0f)
				{
					Burst();
					burstsRemaining--;
					burstTime = (1.0f / burstRate);
				}
			}
			else
			{
				burstsRemaining = 0;
			}
		}

		base.FirstPersonUpdate();
	}

	public override void ThirdPersonUpdate()
	{
		base.ThirdPersonUpdate();

		trailTime -= Time.deltaTime;
		trail.SetPosition(0, trail.transform.position);
		if (trail.enabled && trailTime <= 0.0f)
		{
			trail.enabled = false;
		}

		sliderTime += Time.deltaTime * sliderSpeed;
		if (reloadState < 3 || ammo <= 0)
		{
			sliderTime = Mathf.Min(sliderTime, sliderReloadTime);
		}

		Vector3 newPos = sliderTransform.localPosition;
		newPos.z = Mathf.Lerp(sliderClosedPos, sliderOpenPos, sliderCurve.Evaluate(sliderTime));
		sliderTransform.localPosition = newPos;
	}

	public override void LoadGameSettings()
	{
		ammo = settings.Ammo;
		clipSize = settings.ClipSize;
		fireRate = settings.FireRate;
		reloadTime = settings.ReloadTime;
	}

	public override void OnSwapFrom()
	{
		base.OnSwapFrom();

		trail.enabled = false;
	}

	public override void OnFinishSwap()
	{
		base.OnFinishSwap();

		storedShootInput = PlayerInput.Shoot(ButtonStatus.Down);
	}

	public override void OnReloadStart()
	{
		base.OnReloadStart();

		storedReloadInput = false;
		burstsRemaining = 0;
		if (sliderTime >= 1.0f)
		{
			sliderTime = 0.0f;
		}
	}

	public override bool CheckReloadInput()
	{
		return base.CheckReloadInput() || storedReloadInput;
	}

	public override void SetMagHold(float magHold)
	{
		if (magHold < Mathf.Epsilon)
		{
			magTransform.localPosition = magIdlePosition;
			magTransform.localRotation = Quaternion.Euler(magIdleRotation);
		}
		else
		{
			Vector3 idlePos = transform.TransformPoint(magIdlePosition);
			Quaternion idleRot = Quaternion.Euler(transform.TransformDirection(magIdleRotation));

			Vector3 holdPos = model.LeftHand.TransformPoint(magHoldPosition);
			Quaternion holdRot = model.LeftHand.rotation * Quaternion.Euler(magHoldRotation);

			magTransform.position = Vector3.Lerp(idlePos, holdPos, magHold);
			magTransform.rotation = Quaternion.Lerp(idleRot, holdRot, magHold);
		}
	}

	protected override bool CheckShootInput()
	{
		return PlayerInput.Shoot(ButtonStatus.Pressed) || (storedShootInput && PlayerInput.Shoot(ButtonStatus.Down));
	}

	protected override void Shoot()
	{
		canShoot = -(1.0f / fireRate);

		Burst();
		burstsRemaining = burstAmount - 1;
		burstTime = 1.0f / burstRate;

		storedShootInput = false;
		storedReloadInput = false;

		hud.Crosshairs.Pop();
	}

	protected override void CreateShootFX()
	{
		base.CreateShootFX();

		muzzleFlash.Play();

		sliderTime = 0.0f;
	}

	protected override void CreateHitPlayerFX(Vector3 position, Vector3 normal, BodyPart bodyPart)
	{
		GameObject effect = Instantiate(bloodEffect);
		effect.transform.position = position;
		effect.transform.forward = Vector3.Slerp(normal, Vector3.up, 0.5f);
		Destroy(effect, 0.5f);

		CreateTrailFX(position);
	}

	protected override void CreateHitEnvironmentFX(Vector3 position, Vector3 normal, SurfaceType surfaceType, Transform parent = null)
	{
		base.CreateHitEnvironmentFX(position, normal, surfaceType, parent);

		CreateTrailFX(position);
	}

	/**********************************************************/
	// Helper Functions

	private void Burst()
	{
		DecreaseAmmo();
		CheckAutoReload();

		if (ammoInClip <= 0 && CanReload())
		{
			burstsRemaining = 0;
			carrier.Reload();
		}

		Vector3 shootVector = GetShootVector();
		RaycastHit hit = GetRaycastHit(shootVector);
		if (hit.transform)
		{
			if (hit.collider.GetComponent<BodyPartCollider>())
			{
				HitPlayer(hit);
				CreateHitPlayerFX(hit.point, hit.normal, hit.collider.GetComponent<BodyPartCollider>().type);
			}
			else if (hit.collider.GetComponentInParent<Damageable>())
			{
				HitDamageable(hit);
				CreateHitEnvironmentFX(hit.point, hit.normal, GetSurfaceType(hit), hit.collider.transform);
			}
			else
			{
				HitEnvironment(hit);
				CreateHitEnvironmentFX(hit.point, hit.normal, GetSurfaceType(hit));
			}
		}
		else
		{
			if (net)
			{
				net.Shoot(id, shootVector * 150.0f, shootVector, GetSurfaceType(hit));
			}
			CreateTrailFX(cam.transform.transform.position + shootVector * 150.0f);
		}

		CreateShootFX();
		PlayShootSound();
		UpdateAmmoHUD();

		cam.Jerk(Random.insideUnitCircle * Mathf.Lerp(jerkFactor, adsJerkFactor, AimDownSightsAlpha), 0.1f, true);
	}

	private Vector3 GetShootVector()
	{
		Vector3 forward = cam.transform.forward;

		float variance = 0.005f * (1.0f - AimDownSightsAlpha) + 0.001f;
		forward.x += Random.Range(-variance, variance);
		forward.y += Random.Range(-variance, variance);
		forward.z += Random.Range(-variance, variance);

		return forward.normalized;
	}

	private void CreateTrailFX(Vector3 position)
	{
		trail.enabled = true;
		trail.SetPosition(0, trail.transform.position);
		trail.SetPosition(1, position);
		trailTime = 0.05f;
	}
}
