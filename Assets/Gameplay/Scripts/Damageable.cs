using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Damageable : NetworkBehaviour
{
	[SerializeField]
	protected float maxHealth;
	[SerializeField]
	protected float forceModifier;
	[SerializeField]
	protected bool showsHitMarker;

	protected float health;
	protected int lastShooter;
	protected DamageType lastDamageType;
	protected Rigidbody rig;

	/**********************************************************/
	// Interface

	public virtual void Awake()
	{
		health = maxHealth;
		rig = GetComponent<Rigidbody>();
	}

	public virtual void Update()
	{
	}

	public virtual float TakeDamage(float damage, int shooter, Vector3 position, DamageType type)
	{
		health -= damage;
		lastShooter = shooter;

		if (forceModifier > 0.0f && rig)
		{
			rig.AddForce((transform.position - position).normalized * damage * forceModifier, ForceMode.Impulse);
		}

		return damage;
	}

	/**********************************************************/
	// Accessors/Mutators

	public float MaxHealth
	{
		get
		{
			return maxHealth;
		}
		set
		{
			maxHealth = value;
		}
	}

	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
		}
	}

	public int LastShooter
	{
		get
		{
			return lastShooter;
		}
	}

	public DamageType LastDamageType
	{
		get
		{
			return lastDamageType;
		}
		set
		{
			lastDamageType = value;
		}
	}

	public bool ShowsHitMarker
	{
		get
		{
			return showsHitMarker;
		}
		set
		{
			showsHitMarker = value;
		}
	}
}

public enum DamageType
{
	AssaultRifle,
	SMG,
	Sniper,
	RocketLauncher,
	Shotgun,
	Pistol,
	MachineGun,
	Blaster,
	AssaultRifleHeadShot,
	SMGHeadShot,
	SniperHeadShot,
	RocketLauncherHeadShot,
	ShotgunHeadShot,
	PistolHeadShot,
	MachineGunHeadShot,
	BlasterHeadShot,
	FragGrenade,
	FragGrenadeShot,
	TeslaGrenade,
	Melee,
	ExplosiveBarrel,
	Nature,
	SpikeGrenade,
	NumTypes,
}