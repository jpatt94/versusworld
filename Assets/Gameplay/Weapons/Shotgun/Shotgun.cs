using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Weapon
{
	[SerializeField]
	private GameObject impactEffect;
	[SerializeField]
	private GameObject bloodEffect;
	[SerializeField]
	private float range;
	[SerializeField]
	private int numPellets;
	[SerializeField]
	private float spread;
	[SerializeField]
	private float trailParticlesPerUnit;
	[SerializeField]
	private GameObject trailEffect;
	[SerializeField]
	private Vector2 jerk;
	[SerializeField]
	private float jerkDuration;

	private float canAutoReload;

	private ParticleSystem muzzleFlash;

	/**********************************************************/
	// Weapon Interface

	public override WeaponType GetWeaponType()
	{
		return WeaponType.Shotgun;
	}

	public override void OnStart()
	{
		base.OnStart();

		muzzleFlash = barrel.Find("MuzzleFlash").GetComponent<ParticleSystem>();
	}

	public override void LoadGameSettings()
	{
		ShotgunSettings settings = PartyManager.GameSettings.Weapons.Shotgun;

		ammo = settings.Ammo;
		clipSize = settings.ClipSize;
		fireRate = settings.FireRate;
		reloadTime = settings.ReloadTime;
		range = settings.Range;
		numPellets = settings.NumPellets;
		spread = settings.Spread;
	}

	public override void FirstPersonUpdate()
	{
		canAutoReload -= Time.deltaTime;

		UpdateAimDownSights();

		if (!carrier.Swapping)
		{
			UpdateShooting();
		}
	}

	public override void OnReload()
	{
		ammoInClip += 1;
		UpdateAmmoHUD();

		aud.PlayOneShot(insertClipSound);
	}

	public override void OnReloadFinish()
	{
		if (!carrier.Swapping && CanReload() && !adsEnabled && canAutoReload <= 0.0f)
		{
			carrier.Reload();
		}
	}

	protected override bool CanShoot()
	{
		return canShoot >= 0.0f && ammoInClip > 0 && grenades.CanThrow && !melee.Active && !carrier.NeedsShootingReset;
	}

	protected override void Shoot()
	{
		base.Shoot();

		cam.Jerk(jerk, jerkDuration, true);
		cam.ZShake(1.0f, jerkDuration);

		ShotgunShotData data = new ShotgunShotData();
		data.x = new float[numPellets];
		data.y = new float[numPellets];
		data.z = new float[numPellets];
		data.victim = new int[numPellets];
		data.bodyPart = new int[numPellets];
		data.damageable = new uint[numPellets];

		int markHit = 0;
		bool friendlyFire = false;

		for (int i = 0; i < numPellets; i++)
		{
			data.victim[i] = -2;
			data.bodyPart[i] = (int)BodyPart.None;
			data.damageable[i] = uint.MaxValue;

			Vector3 shootVector = GetShootVector();
			RaycastHit hit = GetRaycastHit(shootVector);
			if (hit.transform && hit.distance <= range)
			{
				data.x[i] = hit.point.x;
				data.y[i] = hit.point.y;
				data.z[i] = hit.point.z;

				if (hit.collider.GetComponent<BodyPartCollider>())
				{
					NetworkPlayer otherNet = hit.collider.GetComponentInParent<NetworkPlayer>();
					if (otherNet)
					{
						data.victim[i] = otherNet.ID;
						data.bodyPart[i] = (int)hit.collider.GetComponent<BodyPartCollider>().type;
					}

					CreateHitPlayerFX(hit.point, hit.normal, hit.collider.GetComponent<BodyPartCollider>().type);

					if (PartyManager.SameTeam(net.ID, otherNet ? otherNet.ID : -1))
					{
						friendlyFire = true;
					}
					else
					{
						if ((BodyPart)data.bodyPart[i] == BodyPart.Head)
						{
							markHit = 2;
						}
						else
						{
							markHit = Mathf.Max(1, markHit);
						}
					}
				}
				else if (hit.collider.GetComponentInParent<Damageable>())
				{
					data.damageable[i] = hit.collider.GetComponentInParent<Damageable>().netId.Value;
					CreateHitEnvironmentFX(hit.point, hit.normal, GetSurfaceType(hit), hit.collider.transform);
					if (hit.collider.GetComponentInParent<Damageable>().ShowsHitMarker)
					{
						markHit = Mathf.Max(1, markHit);
					}
				}
				else
				{
					data.victim[i] = -1;
					CreateHitEnvironmentFX(hit.point, hit.normal, GetSurfaceType(hit));
				}
			}
			else
			{
				Vector3 hitPoint = cam.transform.position + shootVector * range;
				data.x[i] = hitPoint.x;
				data.y[i] = hitPoint.y;
				data.z[i] = hitPoint.z;

				CreateNoHitFX(hitPoint);
			}
		}

		if (net)
		{
			net.ShootShotgun(id, data);
		}

		if (markHit > 0)
		{
			hud.HitMarker.Trigger(markHit > 1 ? HitMarkerType.HeadShot : HitMarkerType.Default);
		}
		if (friendlyFire)
		{
			hud.HitMarker.Trigger(HitMarkerType.Friendly);
		}

		if (ammoInClip > 0)
		{
			canAutoReload = reloadTime * 0.9f;
		}
	}

	protected override void CreateShootFX()
	{
		base.CreateShootFX();

		muzzleFlash.Play();
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

	protected override void CreateNoHitFX(Vector3 position)
	{
		CreateTrailFX(position);
	}

	/**********************************************************/
	// Helper Functions

	private Vector3 GetShootVector()
	{
		Vector3 forward = cam.transform.forward;

		forward.x += Random.Range(-spread, spread);
		forward.y += Random.Range(-spread, spread);
		forward.z += Random.Range(-spread, spread);

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

public struct ShotgunShotData
{
	public float[] x;
	public float[] y;
	public float[] z;
	public int[] victim;
	public int[] bodyPart;
	public uint[] damageable;
}