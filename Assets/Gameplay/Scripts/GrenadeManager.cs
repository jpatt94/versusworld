using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GrenadeManager : NetworkBehaviour
{
	[SerializeField]
	private float minimumPickUpDistance;
	[SerializeField]
	private List<GameObject> grenadePrefabs;
	[SerializeField]
	private List<GameObject> grenadeDropPrefabs;

	private Dictionary<int, GrenadeData> grenades;
	private int nextGrenadeID;
	private List<SpikeGrenade> stuckSpikeGrenades;

	private WeaponManager weaponMgr;
	private GrenadeSettings settings;

	/**********************************************************/
	// Interface

	public override void OnStartServer()
	{
		grenades = new Dictionary<int, GrenadeData>();
		nextGrenadeID = 0;
		stuckSpikeGrenades = new List<SpikeGrenade>();

		settings = PartyManager.GameSettings.Grenades;
	}

	public void RequestStartingGrenades(int ownerID, out List<GrenadeAssignment> grenadeAssignments)
	{
		grenadeAssignments = new List<GrenadeAssignment>();

		for (int i = 0; i < settings.Quantities.Length; i++)
		{
			int startingAmount = settings.Quantities[i].StartingAmount;
			if (startingAmount > 0)
			{
				GrenadeAssignment g;
				g.id = CreateGrenades((GrenadeType)i, startingAmount, ownerID);
				g.type = (GrenadeType)i;
				g.amount = startingAmount;
				grenadeAssignments.Add(g);
			}
		}
	}

	public void DropGrenades(NetworkPlayer player)
	{
		foreach (KeyValuePair<int, GrenadeData> kv in grenades)
		{
			if (kv.Value.ownerID == player.ID)
			{
				CreateGrenadeDrop(kv.Value, player.transform.position);
			}
		}

		foreach (SpikeGrenade s in stuckSpikeGrenades)
		{
			if (s.StuckPlayer == player.ID)
			{
				s.Detonate();
			}
		}
	}

	public void ThrowGrenade(NetworkPlayer player, GrenadeType type, Vector3 position, Vector3 forward, float forceRatio)
	{
		foreach (KeyValuePair<int, GrenadeData> kv in grenades)
		{
			if (kv.Value.ownerID == player.ID && kv.Value.grenade == null && kv.Value.type == type)
			{
				GameObject obj = Instantiate(grenadePrefabs[(int)type], position, Quaternion.LookRotation(forward));
				Grenade grenade = obj.GetComponent<Grenade>();
				grenade.ID = kv.Value.id;
				grenade.Manager = this;
				grenade.Data = kv.Value;
				grenade.FriendlyFire = true;
				kv.Value.grenade = grenade;
				kv.Value.throwerID = kv.Value.ownerID;
				kv.Value.ownerID = -1;

				Rigidbody rig = grenade.GetComponent<Rigidbody>();
				rig.AddForce(forward * Mathf.Lerp(settings.MinThrowForce, settings.MaxThrowForce, forceRatio), ForceMode.VelocityChange);
				rig.angularVelocity = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
				Physics.IgnoreCollision(grenade.GetComponent<Collider>(), player.GetComponent<CapsuleCollider>());

				NetworkServer.Spawn(obj);

				break;
			}
		}
	}

	public Grenade CreateLiveGrenade(GrenadeType type, Vector3 position, Vector3 forward, float speed, int throwerID = -1, bool friendlyFire = true)
	{
		int i = CreateGrenades(type, 1, throwerID);
		GrenadeData data = grenades[i];
		
		GameObject obj = Instantiate(grenadePrefabs[(int)type], position, Quaternion.LookRotation(forward));
		Grenade grenade = obj.GetComponent<Grenade>();
		grenade.ID = data.id;
		grenade.Manager = this;
		grenade.Data = data;
		grenade.FriendlyFire = friendlyFire;
		data.grenade = grenade;
		data.throwerID = data.ownerID;
		data.ownerID = -1;

		Rigidbody rig = grenade.GetComponent<Rigidbody>();
		rig.AddForce(forward * speed, ForceMode.VelocityChange);
		rig.angularVelocity = new Vector3(10.0f, 0.0f, 0.0f);

		NetworkServer.Spawn(obj);

		return grenade;
	}

	public void OnDetonate(int id)
	{
		if (!grenades.ContainsKey(id))
		{
			Debug.LogError("No grenade to detonate for id " + id);
			return;
		}

		Grenade grenade = grenades[id].grenade;
		NetworkServer.UnSpawn(grenade.gameObject);
		Destroy(grenade.gameObject);
		grenades.Remove(id);
		print("Removing grenade " + id);
	}

	public void PickUpGrenade(NetworkPlayer player, int id)
	{
		print("Attempting to pick up " + id);
		if (grenades.ContainsKey(id) && !player.Respawner.enabled)
		{
			GrenadeData data = grenades[id];
			if (data.drop && WithinPickUpDistance(player, data) && NumberOfOwnedGrenades(player, data.type) < settings.Quantities[(int)data.type].MaxAmount)
			{
				print("Picking up " + data.type);
				NetworkServer.UnSpawn(data.drop.gameObject);
				Destroy(data.drop.gameObject);
				data.drop = null;
				data.ownerID = player.ID;
				player.PickUpGrenade(data.type);
			}
		}
	}

	public void DespawnGrenade(GrenadeDrop drop)
	{
		if (!grenades.ContainsKey(drop.ID))
		{
			Debug.LogError("No grenade to despawn for id " + drop.ID);
			return;
		}

		print("Despawning grenade " + drop.Type);
		NetworkServer.UnSpawn(drop.gameObject);
		Destroy(drop.gameObject);
		grenades.Remove(drop.ID);
	}

	public int CreateGrenades(GrenadeType type, int amount, int ownerID)
	{
		int retval = nextGrenadeID;

		for (int i = 0; i < amount; i++)
		{
			GrenadeData data = new GrenadeData();

			data.type = type;
			data.id = nextGrenadeID++;
			data.ownerID = ownerID;
			data.grenade = null;
			data.drop = null;
			data.throwerID = -1;

			grenades[data.id] = data;
		}

		return retval;
	}

	public int NumberOfOwnedGrenades(NetworkPlayer player, GrenadeType type)
	{
		int ownedGrenades = 0;

		foreach (KeyValuePair<int, GrenadeData> kv in grenades)
		{
			if (kv.Value.ownerID == player.ID && kv.Value.type == type)
			{
				ownedGrenades++;
			}
		}

		return ownedGrenades;
	}

	public void GiveGrenade(NetworkPlayer player, GrenadeType type)
	{
		if (player.GrenadeManager.NumberOfOwnedGrenades(player, type) < settings.Quantities[(int)type].MaxAmount)
		{
			player.GrenadeManager.CreateGrenades(type, 1, player.ID);
			player.PickUpGrenade(type);
		}
	}

	public void DealExplosiveDamage(int thrower, GameObject obj, float damage, float radius, DamageType type, bool friendlyFire = true)
	{
		WeaponManager.DealExplosiveDamage(thrower, obj, damage, radius, type, friendlyFire);
	}

	/**********************************************************/
	// Helper Functions

	private void CreateGrenadeDrop(GrenadeData data, Vector3 position)
	{
		GameObject obj = Instantiate(grenadeDropPrefabs[(int)data.type]);
		obj.transform.position = position;

		data.drop = obj.GetComponent<GrenadeDrop>();
		data.drop.Data = data;
		data.drop.ID = data.id;
		data.drop.Manager = this;

		data.ownerID = -1;
		data.throwerID = -1;

		NetworkServer.Spawn(obj);
	}

	private bool WithinPickUpDistance(NetworkPlayer player, GrenadeData grenade)
	{
		return (player.transform.position - grenade.drop.transform.position).sqrMagnitude <= minimumPickUpDistance * minimumPickUpDistance;
	}

	/**********************************************************/
	// Accessors/Mutators

	public GrenadeSettings Settings
	{
		get
		{
			return settings;
		}
	}

	public WeaponManager WeaponManager
	{
		get
		{
			if (!weaponMgr)
			{
				weaponMgr = GameObject.Find("WeaponManager").GetComponent<WeaponManager>();
			}

			return weaponMgr;
		}
	}

	public List<SpikeGrenade> StuckSpikeGrenades
	{
		get
		{
			return stuckSpikeGrenades;
		}
	}
}

public class GrenadeData
{
	public GrenadeType type;
	public int id;
	public int ownerID;
	public int throwerID;
	public Grenade grenade;
	public GrenadeDrop drop;
}

public enum GrenadeType
{
	Frag,
	Tesla,
	Spike,
	NumTypes,
	None,
}

public struct GrenadeAssignment
{
	public GrenadeType type;
	public int amount;
	public int id;
}