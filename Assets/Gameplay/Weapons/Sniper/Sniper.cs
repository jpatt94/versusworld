using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sniper : Weapon
{
	[SerializeField]
	private float scopedFOV;
	[SerializeField]
	private GameObject impactEffect;
	[SerializeField]
	private GameObject bloodEffect;
	[SerializeField]
	private GameObject trailEffect;
	[SerializeField]
	private float trailParticlesPerUnit;
	[SerializeField]
	private AudioClip zoomInSound;
	[SerializeField]
	private Vector2 jerk;
	[SerializeField]
	private float jerkDuration;
	[SerializeField]
	private GameObject bulletPrefab;
	[SerializeField]
	private GameObject clipSegmentPrefab;
	[SerializeField]
	private float bulletSeparation;
	[SerializeField]
	private float clipSpeed;
	[SerializeField]
	private AnimationCurve clipCurve;
	[SerializeField]
	private Vector3 clipHoldPosition;
	[SerializeField]
	private Vector3 clipHoldRotation;

	private bool scoped;
	private bool previouslyScoped;
	private bool storedShootInput;
	private Transform clip;
	private Vector3 clipDefaultPos;
	private Vector3 clipDefaultRot;
	private List<Transform> bulletHolders;
	private List<GameObject> bullets;
	private int currentBullet;
	private float clipTime;
	private float clipStartPos;
	private float clipEndPos;

	private ParticleSystem muzzleFlash;
	private FirstPersonHands firstPersonHands;

	/**********************************************************/
	// Interface

	public override WeaponType GetWeaponType()
	{
		return WeaponType.Sniper;
	}

	public override void OnStart()
	{
		base.OnStart();

		scoped = false;
		previouslyScoped = false;
		storedShootInput = false;

		muzzleFlash = barrel.Find("MuzzleFlash").GetComponent<ParticleSystem>();
		firstPersonHands = controller.GetComponentInChildren<FirstPersonHands>();
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		clip = transform.Find("Clip");
		clipDefaultPos = clip.localPosition;
		clipDefaultRot = clip.localRotation.eulerAngles;
		bulletHolders = new List<Transform>();
		bullets = new List<GameObject>();

		for (int i = 0; i < clipSize; i++)
		{
			GameObject obj = Instantiate(clipSegmentPrefab, clip);
			obj.transform.localPosition = Vector3.right * bulletSeparation * i;
			obj.transform.localRotation = Quaternion.identity;
			bulletHolders.Add(obj.transform.Find("BulletPlaceholder"));
			bullets.Add(null);
		}

		OnChangeMag();
	}

	public override void FirstPersonUpdate()
	{
		storedShootInput = storedShootInput || (PlayerInput.Shoot(ButtonStatus.Pressed) && !carrier.Reloading);

		if (AimDownSightsAlpha <= 0.05f)
		{
			canADS = true;
		}

		scoped = adsTime >= aimDownSightsDuration;

		base.FirstPersonUpdate();

		hud.SetSniperScopeVisible(scoped);
		Visible = !scoped;
		firstPersonHands.Visible = !scoped;
		if (cam)
		{
			cam.SetFOV(scoped ? scopedFOV : cam.DefaultFOV);
		}

		if (scoped && !previouslyScoped)
		{
			aud.PlayOneShot(zoomInSound);
			if (net)
			{
				((NetworkWeaponCarrier)carrier).CmdSetSniperIsScoped(true);
			}
		}
		if (!scoped && previouslyScoped)
		{
			if (ammoInClip <= 0 && CanReload())
			{
				carrier.Reload();
			}
			if (net)
			{
				((NetworkWeaponCarrier)carrier).CmdSetSniperIsScoped(false);
			}
		}

		previouslyScoped = scoped;
	}

	public override void LoadGameSettings()
	{
		SniperSettings settings = PartyManager.GameSettings.Weapons.Sniper;

		ammo = settings.Ammo;
		clipSize = settings.ClipSize;
		fireRate = settings.FireRate;
		reloadTime = settings.ReloadTime;
	}

	protected override bool CheckShootInput()
	{
		return PlayerInput.Shoot(ButtonStatus.Pressed) || (storedShootInput && PlayerInput.Shoot(ButtonStatus.Down));
	}

	protected override void Shoot()
	{
		DecreaseAmmo();
		CheckAutoReload();

		CreateShootFX();
		PlayShootSound();
		UpdateAmmoHUD();
		hud.Crosshairs.Pop();
		cam.Jerk(jerk, jerkDuration, true);
		cam.ZShake(-1.0f, jerkDuration * 2.0f);

		canShoot = -(1.0f / fireRate);

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

		storedShootInput = false;

		if (bullets[currentBullet])
		{
			Destroy(bullets[currentBullet]);
			bullets[currentBullet] = null;
		}

		currentBullet++;
		if (currentBullet < clipSize)
		{
			clipStartPos = Mathf.Lerp(clipStartPos, clipEndPos, clipCurve.Evaluate(clipTime));
			clipEndPos = -currentBullet * bulletSeparation;
			clipTime = 0.0f;
		}
		currentBullet = Mathf.Min(clipSize - 1, currentBullet);
	}

	protected override void OnDryFire()
	{
		base.OnDryFire();

		currentBullet++;
		if (currentBullet < clipSize)
		{
			clipStartPos = Mathf.Lerp(clipStartPos, clipEndPos, clipCurve.Evaluate(clipTime));
			clipEndPos = -currentBullet * bulletSeparation;
			clipTime = 0.0f;
		}
		currentBullet = Mathf.Min(clipSize - 1, currentBullet);
	}

	protected override void CreateShootFX()
	{
		base.CreateShootFX();

		if (!scoped)
		{
			muzzleFlash.Play();
		}
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

	public override void OnFinishSwap()
	{
		base.OnFinishSwap();

		storedShootInput = PlayerInput.Shoot(ButtonStatus.Down);
	}

	public override void OnTakeDamage(float damage, Vector3 position, BodyPart bodyPart)
	{
		base.OnTakeDamage(damage, position, bodyPart);

		canADS = false;
	}

	public override void OnSwapTo()
	{
		base.OnSwapTo();

		clipEndPos = -currentBullet * bulletSeparation;
		clipTime = 1.0f;
	}

	public override void OnSwapFrom()
	{
		base.OnSwapFrom();

		if (net)
		{
			((NetworkWeaponCarrier)carrier).CmdSetSniperIsScoped(false);
		}
	}

	public override void OnChangeMag()
	{
		base.OnChangeMag();

		int numInClip = Mathf.Min(clipSize, ammo);
		for (int i = 0; i < clipSize; i++)
		{
			if (i < numInClip)
			{
				if (!bullets[i])
				{
					GameObject obj = Instantiate(bulletPrefab, bulletHolders[i]);
					obj.transform.localPosition = Vector3.zero;
					obj.transform.localRotation = Quaternion.identity;
					bullets[i] = obj;
				}
			}
			else
			{
				if (bullets[i])
				{
					Destroy(bullets[i]);
					bullets[i] = null;
				}
			}
		}

		currentBullet = 0;
		clipStartPos = 0.0f;
		clipEndPos = 0.0f;

		ammoInClip = Mathf.Min(clipSize, ammo);
		UpdateAmmoHUD();
	}

	public override void OnClipExpose()
	{
		base.OnClipExpose();

		clipStartPos = Mathf.Lerp(clipStartPos, clipEndPos, clipCurve.Evaluate(clipTime));
		clipEndPos = 0.0f;
		clipTime = 0.0f;
	}

	public override void SetMagHold(float magHold)
	{
		if (!clip)
		{
			return;
		}

		clipTime += Time.deltaTime * clipSpeed;
		Vector3 clipIdlePos = clipDefaultPos + Vector3.right * Mathf.Lerp(clipStartPos, clipEndPos, clipCurve.Evaluate(clipTime));

		if (magHold < Mathf.Epsilon)
		{
			clip.localPosition = clipIdlePos;
			clip.localRotation = Quaternion.Euler(clipDefaultRot);
		}
		else
		{
			Vector3 idlePos = transform.TransformPoint(clipIdlePos);
			Quaternion idleRot = Quaternion.Euler(transform.TransformDirection(clipDefaultRot));

			Vector3 holdPos = model.LeftHand.TransformPoint(clipHoldPosition);
			Quaternion holdRot = model.LeftHand.rotation * Quaternion.Euler(clipHoldRotation);

			clip.position = Vector3.Lerp(idlePos, holdPos, magHold);
			clip.rotation = Quaternion.Lerp(idleRot, holdRot, magHold);
		}
	}

	/**********************************************************/
	// Helper Functions

	private Vector3 GetShootVector()
	{
		Vector3 forward = cam.transform.forward;

		float variance = 0.002f * (1.0f - AimDownSightsAlpha);
		forward.x += Random.Range(-variance, variance);
		forward.y += Random.Range(-variance, variance);
		forward.z += Random.Range(-variance, variance);

		return forward.normalized;
	}

	private void CreateTrailFX(Vector3 position)
	{
		Vector3 startPos = barrel.position;
		Vector3 toTarget = position - startPos;

		GameObject trail = Instantiate(trailEffect);
		trail.transform.position = startPos + toTarget / 2.0f;
		trail.transform.forward = toTarget.normalized;

		ParticleSystem part = trail.GetComponent<ParticleSystem>();
		ParticleSystem.ShapeModule shape = part.shape;
		shape.box = new Vector3(0.1f, 0.1f, toTarget.magnitude);

		part.Emit((int)(toTarget.magnitude * trailParticlesPerUnit));

		Destroy(trail, 4.0f);
	}
}
