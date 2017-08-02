using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{
	[SerializeField]
	private float jerkFactor;
	[SerializeField]
	private float adsJerkFactor;
	[SerializeField]
	private Vector3 magHoldPosition;
	[SerializeField]
	private Vector3 magHoldRotation;
	[SerializeField]
	private float slideClosedPos;
	[SerializeField]
	private float slideOpenPos;
	[SerializeField]
	private float slideSpeed;
	[SerializeField]
	private AnimationCurve slideCurve;
	[SerializeField]
	private float slideReloadTime;

	public GameObject impactEffect;
	public GameObject bloodEffect;

	private bool storedShootInput;
	private float trailTime;
	private float slideTime;

	private ParticleSystem muzzleFlash;
	private Transform magTransform;
	private Vector3 magIdlePosition;
	private Vector3 magIdleRotation;
	private LineRenderer trail;
	private Transform slideTransform;

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
		return WeaponType.Pistol;
	}

	public override void OnStart()
	{
		base.OnStart();

		storedShootInput = false;

		muzzleFlash = barrel.Find("MuzzleFlash").GetComponent<ParticleSystem>();
		trail = barrel.GetComponent<LineRenderer>();
		magTransform = transform.Find("PistolMesh/Magazine");
		magIdlePosition = magTransform.localPosition;
		magIdleRotation = magTransform.localRotation.eulerAngles;
		slideTransform = transform.Find("PistolMesh/Slide");

		slideTime = 1.0f;
	}

	public override void LoadGameSettings()
	{
		PistolSettings settings = PartyManager.GameSettings.Weapons.Pistol;

		ammo = settings.Ammo;
		clipSize = settings.ClipSize;
		fireRate = settings.FireRate;
		reloadTime = settings.ReloadTime;
	}

	public override void FirstPersonUpdate()
	{
		storedShootInput = storedShootInput || (PlayerInput.Shoot(ButtonStatus.Pressed) && !carrier.Reloading);

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

		slideTime += Time.deltaTime * slideSpeed;
		if (reloadState < 3 || ammo <= 0)
		{
			slideTime = Mathf.Min(slideTime, slideReloadTime);
		}

		Vector3 newPos = slideTransform.localPosition;
		newPos.z = Mathf.Lerp(slideClosedPos, slideOpenPos, slideCurve.Evaluate(slideTime));
		slideTransform.localPosition = newPos;
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

		if (slideTime >= 1.0f)
		{
			slideTime = 0.0f;
		}
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
		base.Shoot();

		cam.Jerk(new Vector2(Random.Range(-1.0f, -0.2f), Random.Range(-0.1f, 0.1f)) * Mathf.Lerp(jerkFactor, adsJerkFactor, AimDownSightsAlpha), 0.25f, true);

		storedShootInput = false;

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
	}

	protected override void CreateShootFX()
	{
		base.CreateShootFX();

		muzzleFlash.Play();
		slideTime = 0.0f;
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

	private Vector3 GetShootVector()
	{
		return cam.transform.forward;
	}

	private void CreateTrailFX(Vector3 position)
	{
		trail.enabled = true;
		trail.SetPosition(0, trail.transform.position);
		trail.SetPosition(1, position);
		trailTime = 0.05f;
	}
}