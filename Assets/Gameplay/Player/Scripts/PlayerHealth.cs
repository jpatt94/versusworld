using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : Damageable
{
	[SerializeField]
	private float[] forceModifiers;
	[SerializeField]
	private float[] separatesHeadThreshold;

	private float regenDelay;
	private float regenRate;
	//private float lifeSteal;
	private float regening;
	private bool alive;
	private Vector3 damageForce;
	private float lastDamageAmount;
	protected int lastEnemyShooter;
	private Dictionary<int, float> damageHistory;

	private HUD hud;
	private NetworkPlayer net;
	private CameraManager cam;
	private OfflineWeaponCarrier weaponCarrier;

	/**********************************************************/
	// MonoBehaviour Interface

	protected override bool Ready()
	{
		return net.Initialized;
	}

	public override void Awake()
	{
		base.Awake();

		hud = GameObject.Find("HUD").GetComponent<HUD>();
		net = GetComponent<NetworkPlayer>();
		cam = GetComponentInChildren<CameraManager>();
		weaponCarrier = GetComponent<OfflineWeaponCarrier>();

		regening = 0.0f;
		lastShooter = -1;
		lastEnemyShooter = -1;
		alive = true;
	}

	protected override void DelayedStart()
	{
		base.DelayedStart();

		health = maxHealth;
	}

	protected override void DelayedOnStartServer()
	{
		base.DelayedOnStartServer();

		damageHistory = new Dictionary<int, float>();
	}

	public override void Update()
	{
		base.Update();

		if (!initialized)
		{
			return;
		}

		regening += Time.deltaTime;
		if (regening >= 0.0f && health < maxHealth)
		{
			if (isServer && damageHistory.Count > 0)
			{
				damageHistory.Clear();
			}

			health = Mathf.Min(health + regenRate * Time.deltaTime, maxHealth);
			if (net.hasAuthority)
			{
				hud.SetHealth(health / maxHealth);
			}
			else
			{
				hud.SetPlayerNameTagHealth(net.ID, health);
			}
		}

		if (isServer)
		{
			if (health >= maxHealth)
			{
				lastShooter = -1;
				lastEnemyShooter = -1;
			}
		}
	}

	/**********************************************************/
	// Interface

	public void ResetHealth()
	{
		health = maxHealth;
		lastShooter = -1;
		lastEnemyShooter = -1;
		alive = true;

		if (isServer)
		{
			damageHistory.Clear();
		}

		if (net && net.hasAuthority)
		{
			hud.SetHealth(1.0f);
		}
	}

	public void TakeDamage(float damage, int shooter, Vector3 position, BodyPart bodyPart, DamageType type)
	{
		if (damage > 0.0f)
		{
			health = Mathf.Max(health - damage, 0.0f);
			regening = -regenDelay;
			lastShooter = shooter;
			lastDamageType = type;
			lastDamageAmount = damage;

			if (shooter != net.ID && !PartyManager.SameTeam(shooter, net.ID))
			{
				lastEnemyShooter = shooter;
			}

			if (isServer)
			{
				if (damageHistory.ContainsKey(shooter))
				{
					damageHistory[shooter] += damage;
				}
				else
				{
					damageHistory[shooter] = damage;
				}
			}

			if (net.hasAuthority)
			{
				hud.SetHealth(health / maxHealth);
				if (shooter >= 0)
				{
					hud.DamageIndicator.Trigger(shooter, position, cam);
				}
				weaponCarrier.OnTakeDamage(damage, position, bodyPart);
				cam.OnTakeDamage(damage, position, bodyPart);
			}
			else
			{
				hud.SetPlayerNameTagHealth(net.ID, health);
			}

			Vector3 forceDirection = Vector3.Slerp((transform.position - position).normalized, Vector3.up, 0.3f);
			damageForce = forceDirection * forceModifiers[(int)type];
		}

		if (alive && health <= 0.0f && net.isServer)
		{
			net.Die();
			alive = false;
		}
	}

	public bool SeparatesHead(DamageType damageType)
	{
		return lastDamageAmount >= separatesHeadThreshold[(int)damageType];
	}

	public void PopulateAssisters(out int[] assisters, out float[] assistDamages)
	{
		List<int> assistersList = new List<int>();
		List<float> assistDamagesList = new List<float>();

		foreach (KeyValuePair<int, float> kv in damageHistory)
		{
			if (kv.Key > -1 && kv.Key != net.ID && kv.Key != lastShooter && kv.Key != lastEnemyShooter && !PartyManager.SameTeam(kv.Key, net.ID))
			{
				assistersList.Add(kv.Key);
				assistDamagesList.Add(kv.Value);
			}
		}

		assisters = assistersList.ToArray();
		assistDamages = assistDamagesList.ToArray();
	}

	/**********************************************************/
	// Accessors/Mutators

	public Vector3 DamageForce
	{
		get
		{
			return damageForce;
		}
	}

	public int LastEnemyShooter
	{
		get
		{
			return lastEnemyShooter;
		}
	}

	public HealthSettings Traits
	{
		set
		{
			maxHealth = 100.0f * value.Modifier;
			regenDelay = value.RegenDelay;
			regenRate = value.RegenRate;
			//lifeSteal = settings.LifeSteal;
		}
	}
}