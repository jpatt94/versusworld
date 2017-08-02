using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TeslaGrenade : Grenade
{
	[SerializeField]
	private float hitMarkerThreshold;
	[SerializeField]
	private AudioClip igniteSound;
	[SerializeField]
	private GameObject shockPrefab;
	[SerializeField]
	private GameObject explosionPrefab;
	[SerializeField]
	private float postIgnitionLightRange;
	[SerializeField]
	private float postIgnitionLightIntensity;

	private const int MaxConductors = 16;

	private int raycastMask;
	private bool ignited;
	private float sparkTime;
	private float range;
	private float damagePerSecond;
	private SyncListInt victims;
	private SyncListInt origins;
	private ConductorData[] conductors;
	private Dictionary<int, float> nextDamageTick;
	private Dictionary<int, TeslaShock> shocks;

	private Transform sparkTransform;
	private ParticleSystem ignitionSparks;
	private ParticleSystem ignitionSmoke;
	private ParticleSystem constantSpark;
	private Light lt;
	private TeslaSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		settings = PartyManager.GameSettings.Grenades.Tesla;
		FuseTime = settings.FuseTime;
		range = settings.Range;
		damagePerSecond = settings.DamagePerSecond;
		ignited = false;
		victims = new SyncListInt();
		origins = new SyncListInt();
		conductors = new ConductorData[MaxConductors];
		nextDamageTick = new Dictionary<int, float>();
		shocks = new Dictionary<int, TeslaShock>();

		aud = GetComponent<AudioSource>();
		sparkTransform = transform.Find("Mesh/Spark");
		ignitionSparks = Utility.FindChild(gameObject, "IgnitionSparks").GetComponent<ParticleSystem>();
		ignitionSmoke = Utility.FindChild(gameObject, "IgnitionSmoke").GetComponent<ParticleSystem>();
		constantSpark = Utility.FindChild(gameObject, "ConstantSpark").GetComponent<ParticleSystem>();
		lt = sparkTransform.GetComponent<Light>();

		raycastMask = ~((1 << LayerMask.NameToLayer("LocalBodyPartCollider")) |
			(1 << LayerMask.NameToLayer("Ignore Raycast")) |
			(1 << LayerMask.NameToLayer("Drop")) |
			(1 << LayerMask.NameToLayer("BodyPartCollider")));
	}

	public override void Update()
	{
		if (ignited)
		{
			sparkTime -= Time.deltaTime;
			if (sparkTime < 1.0f)
			{
				lt.intensity = postIgnitionLightIntensity * sparkTime;
			}
		}

		if (isServer)
		{
			if (!ignited)
			{
				FuseTime -= Time.deltaTime;
				if (FuseTime <= 0.0f)
				{
					ignited = true;
					sparkTime = settings.SparkTime;
					RpcIgnite(transform.position);

					mgr.DealExplosiveDamage(Thrower, gameObject, settings.ExplosiveDamage, settings.Range, DamageType.TeslaGrenade);
				}
			}
			else
			{
				CheckForVictims();
				if (sparkTime <= 0.0f)
				{
					mgr.OnDetonate(ID);
				}
			}
		}

		UpdateShockEffects();
	}

	public void OnCollisionEnter(Collision col)
	{
		if (settings.IgnitedOnCollision && isServer && !ignited)
		{
			ignited = true;
			sparkTime = settings.SparkTime;
			RpcIgnite(transform.position);

			mgr.DealExplosiveDamage(Thrower, gameObject, settings.ExplosiveDamage, settings.Range, DamageType.TeslaGrenade);
		}
	}

	/**********************************************************/
	// Grenade Interface

	public override GrenadeType GetGrenadeType()
	{
		return GrenadeType.Tesla;
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcIgnite(Vector3 pos)
	{
		aud.PlayOneShot(igniteSound);
		ignitionSparks.Play();
		ignitionSmoke.Play();
		constantSpark.Play();

		Destroy(Instantiate(explosionPrefab, pos, Quaternion.identity), 2.0f);

		lt.range = postIgnitionLightRange;
		lt.intensity = postIgnitionLightIntensity;

		if (!isServer)
		{
			sparkTime = settings.SparkTime;
		}
	}

	/**********************************************************/
	// Helper Functions

	private void CheckForVictims()
	{
		int numConductors = 1;
		conductors[0] = new ConductorData(-1, -1, transform.position);

		for (int i = 0; i < numConductors && i < MaxConductors; i++)
		{
			foreach (NetworkPlayer player in PlayerManager.PlayerList)
			{
				if (numConductors >= MaxConductors)
				{
					break;
				}

				if (conductors[i].victimID == player.ID)
				{
					continue;
				}

				bool isAlreadyConductor = false;
				for (int j = i; j < numConductors && j < MaxConductors; j++)
				{
					if (player.ID == conductors[j].victimID)
					{
						isAlreadyConductor = true;
						break;
					}
				}

				if (WithinRange(conductors[i].pos, player))
				{
					if (!isAlreadyConductor)
					{
						conductors[numConductors] = new ConductorData(player.ID, conductors[i].victimID, player.Center);
						numConductors++;
					}
					else
					{
						Vector3 parentPos = conductors[i].parentID == -1 ? transform.position : PlayerManager.GetPlayer(conductors[i].parentID).Model.HeadPosition;
						if ((parentPos - player.Center).sqrMagnitude > (conductors[i].pos - player.Center).sqrMagnitude)
						{
							conductors[i].parentID = player.ID;
						}
					}
				}
			}
		}

		for (int i = 0; i < victims.Count; i++)
		{
			bool isConductor = false;
			for (int j = 0; j < numConductors; j++)
			{
				if (victims[i] == conductors[j].victimID)
				{
					isConductor = true;
					origins[i] = conductors[j].parentID;
					break;
				}
			}

			if (!isConductor)
			{
				victims.RemoveAt(i);
				origins.RemoveAt(i);
				i--;
			}
		}

		for (int i = 1; i < numConductors; i++)
		{
			if (!victims.Contains(conductors[i].victimID))
			{
				victims.Add(conductors[i].victimID);
				origins.Add(conductors[i].parentID);
			}
		}

		foreach (int i in victims)
		{
			if (!nextDamageTick.ContainsKey(i))
			{
				nextDamageTick[i] = hitMarkerThreshold;
			}
			nextDamageTick[i] -= damagePerSecond * Time.deltaTime;

			if (nextDamageTick[i] <= 0.0f)
			{
				PlayerManager.GetPlayer(data.throwerID).DealDamage(PlayerManager.GetPlayer(i), hitMarkerThreshold, transform.position, BodyPart.None, DamageType.TeslaGrenade);
				nextDamageTick[i] += hitMarkerThreshold;

				if (data.throwerID > -1 && i != data.throwerID)
				{
					PlayerManager.GetPlayer(data.throwerID).MarkHit(PartyManager.SameTeam(data.throwerID, i) ? HitMarkerType.Friendly : HitMarkerType.Default);
				}
			}
		}
	}

	private bool WithinRange(Vector3 fromPos, NetworkPlayer player)
	{
		if (player.Respawner.enabled)
		{
			return false;
		}

		Vector3 toPlayer = (player.Center) - fromPos;
		if (toPlayer.sqrMagnitude > range * range)
		{
			return false;
		}

		RaycastHit hit;
		Physics.Raycast(new Ray(fromPos, toPlayer), out hit, toPlayer.magnitude, raycastMask);
		if (hit.distance < toPlayer.magnitude - 1.0f)
		{
			return false;
		}

		return true;
	}

	private void UpdateShockEffects()
	{
		List<int> removed = new List<int>();
		foreach (KeyValuePair<int, TeslaShock> kv in shocks)
		{
			int key = kv.Key;
			if (!victims.Contains(key) && kv.Value)
			{
				Destroy(kv.Value.gameObject);
				removed.Add(key);
			}
		}
		foreach (int i in removed)
		{
			shocks[i] = null;
		}

		for (int i = 0; i < victims.Count; i++)
		{
			if (!shocks.ContainsKey(victims[i]) || shocks[victims[i]] == null)
			{
				GameObject obj = Instantiate(shockPrefab);
				obj.transform.SetParent(sparkTransform);
				obj.transform.localPosition = Vector3.zero;
				TeslaShock shock = obj.GetComponent<TeslaShock>();
				shock.Victim = PlayerManager.PlayerMap[victims[i]];
				if (origins[i] > -1)
				{
					shock.Origin = PlayerManager.GetPlayer(origins[i]);
				}
				shocks[victims[i]] = shock;
			}

			if (origins[i] > -1)
			{
				shocks[victims[i]].Origin = PlayerManager.GetPlayer(origins[i]);
			}
			else
			{
				shocks[victims[i]].Origin = null;
			}
		}
	}
}

public struct ConductorData
{
	public int victimID;
	public int parentID;
	public Vector3 pos;

	public ConductorData(int victimID, int parentID, Vector3 pos)
	{
		this.victimID = victimID;
		this.parentID = parentID;
		this.pos = pos;
	}
}