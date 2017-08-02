using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class WeaponManager : NetworkBehaviour
{
	[SerializeField]
	private float minimumPickUpDistance;
	[SerializeField]
	private GameObject[] dropPrefabs;
	[SerializeField]
	private GameObject rocketPrefab;
	[SerializeField]
	private Color weaponSpawnMessageColor;
	[SerializeField]
	private Sprite[] weaponIcons;

	private Dictionary<int, WeaponData> weapons;
	private int nextWeaponID;
	private List<WeaponSpawner> weaponSpawners;

	private WeaponSettings settings;
	private HUD hud;

	private static WeaponManager instance;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		hud = GameObject.Find("HUD").GetComponent<HUD>();

		instance = this;
	}

	public void Update()
	{
		if (isServer)
		{
			foreach (WeaponSpawner spawner in weaponSpawners)
			{
				if (spawner.ReadyToSpawn)
				{
					CreateWeaponFromSpawner(spawner, false);
					spawner.ReadyToSpawn = false;
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public static string GetWeaponName(WeaponType type)
	{
		switch (type)
		{
			case WeaponType.AssaultRifle: return "Assault Rifle";
			case WeaponType.SMG: return "SMG";
			case WeaponType.Sniper: return "Sniper";
			case WeaponType.RocketLauncher: return "Rocket Launcher";
			case WeaponType.Shotgun: return "Shotgun";
			case WeaponType.Pistol: return "Pistol";
			case WeaponType.MachineGun: return "Machine Gun";
			case WeaponType.Blaster: return "Blaster";
		}

		return "N/A";
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		weapons = new Dictionary<int, WeaponData>();
		nextWeaponID = 0;

		settings = PartyManager.GameSettings.Weapons;

		weaponSpawners = new List<WeaponSpawner>();
		if (settings.SpawnOnMap)
		{
			GameObject[] weaponSpawnerObjs = GameObject.FindGameObjectsWithTag("WeaponSpawner");
			foreach (GameObject obj in weaponSpawnerObjs)
			{
				WeaponSpawner weaponSpawner = obj.GetComponent<WeaponSpawner>();
				weaponSpawners.Add(weaponSpawner);

				if (weaponSpawner.SpawnOnStart)
				{
					CreateWeaponFromSpawner(weaponSpawner, true);
				}
			}
		}
	}

	public void RequestStartingWeapons(int ownerID, out WeaponAssignment weapons)
	{
		PlayerTraits defaultPlayerTraits = PartyManager.GameSettings.GetPlayerTraits(PlayerTraitsType.Default);

		if (defaultPlayerTraits.Weapons.StartingPrimaryWeapon != WeaponType.None)
		{
			WeaponData primaryWeapon = CreateWeapon(defaultPlayerTraits.Weapons.StartingPrimaryWeapon, ownerID);
			weapons.primaryType = primaryWeapon.type;
			weapons.primaryID = primaryWeapon.id;
		}
		else
		{
			weapons.primaryType = WeaponType.None;
			weapons.primaryID = -1;
		}

		if (defaultPlayerTraits.Weapons.StartingSecondaryWeapon != WeaponType.None)
		{
			WeaponData secondaryWeapon = CreateWeapon(defaultPlayerTraits.Weapons.StartingSecondaryWeapon, ownerID);
			weapons.secondaryType = secondaryWeapon.type;
			weapons.secondaryID = secondaryWeapon.id;
		}
		else
		{
			weapons.secondaryType = WeaponType.None;
			weapons.secondaryID = -1;
		}
	}

	public void Shoot(int weaponID, int shooter, PlayerTraitsType traitsType)
	{
		if (weapons.ContainsKey(weaponID))
		{
			PlayerTraits traits = PartyManager.GameSettings.GetPlayerTraits(traitsType);

			WeaponData weapon = weapons[weaponID];
			if ((weapon.ammo > 0 || traits.Weapons.InfiniteAmmo != InfiniteAmmoType.None) && CanShootWeapon(weapon, shooter))
			{
				if (traits.Weapons.InfiniteAmmo == InfiniteAmmoType.None)
				{
					weapon.ammo--;
				}
			}
		}
	}

	public void ShootPlayer(int weaponID, int shooter, int victim, BodyPart bodyPart, PlayerTraitsType traitsType)
	{
		if (weapons.ContainsKey(weaponID))
		{
			PlayerTraits traits = PartyManager.GameSettings.GetPlayerTraits(traitsType);

			WeaponData weapon = weapons[weaponID];
			if ((weapon.ammo > 0 || traits.Weapons.InfiniteAmmo != InfiniteAmmoType.None) && CanShootWeapon(weapon, shooter))
			{
				NetworkPlayer victimPlayer = PlayerManager.GetPlayer(victim);
				if (victimPlayer)
				{
					print("Body part: " + bodyPart);
					DamageType damageType = (DamageType)((int)weapon.type + (bodyPart == BodyPart.Head ? (int)DamageType.AssaultRifleHeadShot : (int)DamageType.AssaultRifle));
					PlayerManager.GetPlayer(shooter).DealDamage(victimPlayer, GetDamage(weapon.type, bodyPart == BodyPart.Head), PlayerManager.GetPlayer(shooter).transform.position, bodyPart, damageType);
				}
				if (traits.Weapons.InfiniteAmmo == InfiniteAmmoType.None)
				{
					weapon.ammo--;
				}
			}
		}
	}

	public void ShootDamageable(int weaponID, int shooter, GameObject other, PlayerTraitsType traitsType)
	{
		if (weapons.ContainsKey(weaponID) && other)
		{
			PlayerTraits traits = PartyManager.GameSettings.GetPlayerTraits(traitsType);

			WeaponData weapon = weapons[weaponID];
			if ((weapon.ammo > 0 || traits.Weapons.InfiniteAmmo != InfiniteAmmoType.None) && CanShootWeapon(weapon, shooter))
			{
				other.GetComponent<Damageable>().TakeDamage(GetDamage(weapon.type, false), shooter, PlayerManager.GetPlayer(shooter).transform.position, (DamageType)((int)weapon.type + (int)DamageType.AssaultRifle));
				if (traits.Weapons.InfiniteAmmo == InfiniteAmmoType.None)
				{
					weapon.ammo--;
				}
			}
		}
	}

	public void ShootRocket(int weaponID, int shooter, Vector3 position, Vector3 direction, PlayerTraitsType traitsType)
	{
		if (weapons.ContainsKey(weaponID))
		{
			PlayerTraits traits = PartyManager.GameSettings.GetPlayerTraits(traitsType);

			WeaponData weapon = weapons[weaponID];
			if ((weapon.ammo > 0 || traits.Weapons.InfiniteAmmo != InfiniteAmmoType.None) && CanShootWeapon(weapon, shooter))
			{
				GameObject obj = Instantiate(rocketPrefab);
				obj.transform.position = position;
				obj.transform.forward = direction;

				Rocket rocket = obj.GetComponent<Rocket>();
				rocket.ShooterID = shooter;
				rocket.Mgr = this;

				NetworkServer.Spawn(obj);

				if (traits.Weapons.InfiniteAmmo == InfiniteAmmoType.None)
				{
					weapon.ammo--;
				}
			}
		}
	}

	public void ShootShotgun(int weaponID, int shooter, ShotgunShotData data, PlayerTraitsType traitsType)
	{
		if (weapons.ContainsKey(weaponID))
		{
			PlayerTraits traits = PartyManager.GameSettings.GetPlayerTraits(traitsType);

			WeaponData weapon = weapons[weaponID];
			if ((weapon.ammo > 0 || traits.Weapons.InfiniteAmmo != InfiniteAmmoType.None) && CanShootWeapon(weapon, shooter) && data.x.Length == settings.Shotgun.NumPellets)
			{
				Dictionary<Damageable, float> damageDealt = new Dictionary<Damageable, float>();
				bool headShot = false;

				for (int i = 0; i < data.x.Length; i++)
				{
					if (data.victim[i] >= 0)
					{
						NetworkPlayer victimPlayer = PlayerManager.GetPlayer(data.victim[i]);
						if (victimPlayer)
						{
							PlayerHealth ph = victimPlayer.GetComponent<PlayerHealth>();
							if (!damageDealt.ContainsKey(ph))
							{
								damageDealt[ph] = 0.0f;
							}
							damageDealt[victimPlayer.GetComponent<PlayerHealth>()] += GetDamage(weapon.type, (BodyPart)data.bodyPart[i] == BodyPart.Head);
							headShot = headShot || (BodyPart)data.bodyPart[i] == BodyPart.Head;
						}
					}
					else
					{
						GameObject obj = NetworkServer.FindLocalObject(new NetworkInstanceId(data.damageable[i]));
						if (obj)
						{
							Damageable damageable = obj.GetComponent<Damageable>();
							if (!damageDealt.ContainsKey(damageable))
							{
								damageDealt[damageable] = 0.0f;
							}
							damageDealt[damageable] += GetDamage(weapon.type, false);
						}
					}
				}

				foreach (KeyValuePair<Damageable, float> kv in damageDealt)
				{
					if (kv.Key.GetComponent<NetworkPlayer>())
					{
						PlayerManager.GetPlayer(shooter).DealDamage(kv.Key.GetComponent<NetworkPlayer>(), kv.Value, PlayerManager.GetPlayer(shooter).transform.position, headShot ? BodyPart.Head : BodyPart.None, headShot ? DamageType.ShotgunHeadShot : DamageType.Shotgun);
					}
					else
					{
						kv.Key.TakeDamage(kv.Value, shooter, PlayerManager.GetPlayer(shooter).transform.position, DamageType.Shotgun);
					}
				}

				if (traits.Weapons.InfiniteAmmo == InfiniteAmmoType.None)
				{
					weapon.ammo--;
				}
			}
		}
	}

	public void DropWeapons(NetworkPlayer owner)
	{
		List<int> weaponsToRemove = new List<int>();
		foreach (KeyValuePair<int, WeaponData> kv in weapons)
		{
			if (kv.Value.ownerID == owner.ID)
			{
				if (kv.Value.ammo > 0 && settings.DropWhenKilled)
				{
					CreateWeaponDrop(kv.Value, owner.Model.BodyPartTransforms[BodyPart.RightForearm].position, Quaternion.identity);
				}
				else
				{
					weaponsToRemove.Add(kv.Key);
				}
			}
		}

		foreach (int i in weaponsToRemove)
		{
			weapons.Remove(i);
		}
	}

	public void PickUpWeapon(NetworkPlayer player, int weaponID, int swappingWeaponID, int ammoInClip)
	{
		if (weapons.ContainsKey(weaponID) && !player.Respawner.enabled)
		{
			WeaponData weapon = weapons[weaponID];
			if (WithinPickUpDistance(player, weapon))
			{
				if (swappingWeaponID < 0)
				{
					PickUpAmmo(player, weapon);
				}
				else
				{
					OnPickUpWeapon(player, weapon, swappingWeaponID, ammoInClip);
				}
			}
		}
	}

	public void DespawnWeapon(WeaponDrop drop)
	{
		if (!weapons.ContainsKey(drop.ID))
		{
			Debug.LogError("No weapon to despawn for id " + drop.ID);
			return;
		}

		print("Despawning weapon " + drop.ID);
		weapons.Remove(drop.ID);
		OnWeaponDropDestroy(drop);
		NetworkServer.UnSpawn(drop.gameObject);
		Destroy(drop.gameObject);
	}

	public void RefillPlayersWeapons(NetworkPlayer player)
	{
		foreach (KeyValuePair<int, WeaponData> kv in weapons)
		{
			if (kv.Value.ownerID == player.ID)
			{
				int ammoAdded;
				RefillWeapon(kv.Value, out ammoAdded);
				if (ammoAdded > 0)
				{
					player.AddAmmo(kv.Value.id, ammoAdded);
				}
			}
		}
	}

	public void OnRocketHit(Rocket rocket, bool directHit, Vector3 pos)
	{
		if (directHit)
		{
			PlayerManager.Instance.MedalTracker.AwardMedal(rocket.ShooterID, MedalType.DirectRocketHit);
		}

		DealExplosiveDamage(rocket.ShooterID, rocket.gameObject, settings.RocketLauncher.Damage, settings.RocketLauncher.Radius, DamageType.RocketLauncher, pos);

		NetworkServer.UnSpawn(rocket.gameObject);
		Destroy(rocket.gameObject);
	}

	public void DealExplosiveDamage(int shooter, GameObject obj, float damage, float radius, DamageType type)
	{
		DealExplosiveDamage(shooter, obj, damage, radius, type, obj.transform.position);
	}

	public void DealExplosiveDamage(int shooter, GameObject obj, float damage, float radius, DamageType type, Vector3 position)
	{
		Collider[] cols = Physics.OverlapSphere(position, radius);
		bool hitOther = false;
		bool hitFriendly = false;
		foreach (Collider col in cols)
		{
			NetworkPlayer netPlayer = col.GetComponent<NetworkPlayer>();
			Damageable damageable = col.GetComponent<Damageable>();
			if (col.gameObject != obj && (netPlayer || damageable))
			{
				float thisDamage = (1.0f - (col.transform.position - obj.transform.position).magnitude / radius) * damage;
				if (thisDamage > 0.0f)
				{
					if (netPlayer)
					{
						//int hits = 0;
						//hits += CheckExplosiveRaycast(position, netPlayer.Center, radius, netPlayer.ID) ? 1 : 0;
						//hits += CheckExplosiveRaycast(position, netPlayer.Center + Vector3.up * 0.5f, radius, netPlayer.ID) ? 1 : 0;
						//if (hits < 2)
						//{
						//	hits += CheckExplosiveRaycast(position, netPlayer.Center + Vector3.up * 0.9f, radius, netPlayer.ID) ? 1 : 0;
						//	if (hits < 2)
						//	{
						//		hits += CheckExplosiveRaycast(position, netPlayer.Center + Vector3.down * 0.5f, radius, netPlayer.ID) ? 1 : 0;
						//		if (hits < 2)
						//		{
						//			hits += CheckExplosiveRaycast(position, netPlayer.Center + Vector3.down * 0.9f, radius, netPlayer.ID) ? 1 : 0;
						//		}
						//	}
						//}

						//if (hits > 0)
						//{
						PlayerManager.GetPlayer(shooter).DealDamage(netPlayer, thisDamage /*(hits > 1 ? 1.0f : 0.67f)*/, position, BodyPart.None, type);

						if (shooter != netPlayer.ID)
						{
							if (PartyManager.SameTeam(shooter, netPlayer.ID))
							{
								hitFriendly = true;
							}
							else
							{
								hitOther = true;
							}
						}
						//}

						//print("Hit " + hits + " on " + netPlayer);
					}
					else
					{
						if (damageable.TakeDamage(thisDamage, shooter, position, type) > 0.0f && damageable.ShowsHitMarker)
						{
							hitOther = true;
						}
					}
				}
			}
		}
		if (hitOther && shooter > -1)
		{
			PlayerManager.PlayerMap[shooter].MarkHit(HitMarkerType.Explosive);
		}
		if (hitFriendly && shooter > -1)
		{
			PlayerManager.PlayerMap[shooter].MarkHit(HitMarkerType.Friendly);
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcOnCreateWeaponFromSpawner(WeaponType type)
	{
		hud.KillFeed.ShowMessage(GetWeaponName(type) + " spawned", weaponSpawnMessageColor);
	}

	/**********************************************************/
	// Helper Functions

	private WeaponData CreateWeapon(WeaponType type, int ownerID)
	{
		WeaponData weapon = new WeaponData();

		weapon.type = type;
		weapon.id = nextWeaponID++;
		weapon.ownerID = ownerID;
		weapon.ammo = GetAmmo(type);
		weapon.ammoInClip = GetClipSize(type);
		weapon.drop = null;

		weapons[weapon.id] = weapon;

		return weapon;
	}

	private void CreateWeaponFromSpawner(WeaponSpawner spawner, bool initial)
	{
		WeaponData data = CreateWeapon(spawner.Type, -1);
		CreateWeaponDrop(data, spawner.transform.position, spawner.transform.rotation);
		data.drop.Spawner = spawner;
		data.drop.Despawns = false;
		data.drop.HasWaypoint = spawner.HasWaypoint;

		if (!initial && spawner.HasWaypoint)
		{
			RpcOnCreateWeaponFromSpawner(data.type);
		}
	}

	private void CreateWeaponDrop(WeaponData data, Vector3 position, Quaternion rotation)
	{
		GameObject obj = Instantiate(dropPrefabs[(int)data.type]);
		obj.transform.position = position;
		obj.transform.rotation = rotation;

		data.drop = obj.GetComponent<WeaponDrop>();
		data.drop.Data = data;
		data.drop.ID = data.id;
		data.drop.Manager = this;
		data.drop.Spawner = null;

		data.ownerID = -1;

		NetworkServer.Spawn(obj);
	}

	private bool WithinPickUpDistance(NetworkPlayer player, WeaponData weapon)
	{
		return (player.transform.position - weapon.drop.transform.position).sqrMagnitude <= minimumPickUpDistance * minimumPickUpDistance;
	}

	private void PickUpAmmo(NetworkPlayer player, WeaponData weapon)
	{
		foreach (KeyValuePair<int, WeaponData> kv in weapons)
		{
			if (weapon.id != kv.Value.id && player.ID == kv.Value.ownerID && weapon.type == kv.Value.type)
			{
				int ammoToAdd = Mathf.Min(weapon.ammo, GetMaxAmmo(weapon.type) - kv.Value.ammo);
				kv.Value.ammo += ammoToAdd;
				weapon.ammo -= ammoToAdd;
				if (ammoToAdd > 0)
				{
					player.AddAmmo(kv.Value.id, ammoToAdd);
				}

				if (weapon.ammo <= 0)
				{
					print("Picking up ammo from " + weapon.id);
					weapons.Remove(weapon.id);
					OnWeaponDropDestroy(weapon.drop);
					NetworkServer.UnSpawn(weapon.drop.gameObject);
					Destroy(weapon.drop.gameObject);
				}

				break;
			}
		}
	}

	private void OnPickUpWeapon(NetworkPlayer player, WeaponData weapon, int swappingWeaponID, int ammoInClip)
	{
		WeaponData swappingWeapon = weapons[swappingWeaponID];
		if (swappingWeapon.ownerID == player.ID && weapon.ownerID < 0)
		{
			print("Player " + player.ID + " swapping out " + swappingWeaponID + " for " + weapon.id);

			swappingWeapon.ammoInClip = ammoInClip;

			if (swappingWeapon.ammo > 0)
			{
				CreateWeaponDrop(swappingWeapon, weapon.drop.transform.position + Vector3.up, Quaternion.identity);
			}
			else
			{
				weapons.Remove(swappingWeapon.id);
			}

			OnWeaponDropDestroy(weapon.drop);

			weapon.ownerID = player.ID;
			player.PickUpWeapon(weapon.type, weapon.id, swappingWeaponID, weapon.ammo, Mathf.Min(weapon.ammoInClip, weapon.ammo));
			NetworkServer.UnSpawn(weapon.drop.gameObject);
			Destroy(weapon.drop.gameObject);
		}
	}

	private void OnWeaponDropDestroy(WeaponDrop drop)
	{
		if (drop.Spawner)
		{
			drop.Spawner.Enable();
		}
	}

	private void RefillWeapon(WeaponData data, out int ammoAdded)
	{
		int previousAmmo = data.ammo;
		data.ammo = GetMaxAmmo(data.type);
		ammoAdded = data.ammo - previousAmmo;
	}

	private bool CanShootWeapon(WeaponData data, int shooter)
	{
		return true; // data.ownerID == shooter; this is to prevent cheating, but it currently ruins the game
	}

	private int GetAmmo(WeaponType type)
	{
		switch (type)
		{
			case WeaponType.AssaultRifle: return settings.AssaultRifle.Ammo;
			case WeaponType.SMG: return settings.Smg.Ammo;
			case WeaponType.Sniper: return settings.Sniper.Ammo;
			case WeaponType.RocketLauncher: return settings.RocketLauncher.Ammo;
			case WeaponType.Shotgun: return settings.Shotgun.Ammo;
			case WeaponType.Pistol: return settings.Pistol.Ammo;
			case WeaponType.MachineGun: return settings.MachineGun.Ammo;
		}

		return 0;
	}

	private int GetMaxAmmo(WeaponType type)
	{
		switch (type)
		{
			case WeaponType.AssaultRifle: return settings.AssaultRifle.MaxAmmo;
			case WeaponType.SMG: return settings.Smg.MaxAmmo;
			case WeaponType.Sniper: return settings.Sniper.MaxAmmo;
			case WeaponType.RocketLauncher: return settings.RocketLauncher.MaxAmmo;
			case WeaponType.Shotgun: return settings.Shotgun.MaxAmmo;
			case WeaponType.Pistol: return settings.Pistol.MaxAmmo;
			case WeaponType.MachineGun: return settings.MachineGun.MaxAmmo;
		}

		return 0;
	}

	private int GetClipSize(WeaponType type)
	{
		switch (type)
		{
			case WeaponType.AssaultRifle: return settings.AssaultRifle.ClipSize;
			case WeaponType.SMG: return settings.Smg.ClipSize;
			case WeaponType.Sniper: return settings.Sniper.ClipSize;
			case WeaponType.RocketLauncher: return settings.RocketLauncher.ClipSize;
			case WeaponType.Shotgun: return settings.Shotgun.ClipSize;
			case WeaponType.Pistol: return settings.Pistol.ClipSize;
			case WeaponType.MachineGun: return settings.MachineGun.ClipSize;
		}

		return 0;
	}

	private float GetDamage(WeaponType type, bool headShot)
	{
		switch (type)
		{
			case WeaponType.AssaultRifle: return settings.AssaultRifle.Damage * (headShot ? settings.AssaultRifle.HeadShotDamageMultiplier : 1.0f);
			case WeaponType.SMG: return settings.Smg.Damage * (headShot ? settings.Smg.HeadShotDamageMultiplier : 1.0f);
			case WeaponType.Sniper: return settings.Sniper.Damage * (headShot ? settings.Sniper.HeadShotDamageMultiplier : 1.0f);
			case WeaponType.RocketLauncher: return settings.RocketLauncher.Damage;
			case WeaponType.Shotgun: return settings.Shotgun.Damage;
			case WeaponType.Pistol: return settings.Pistol.Damage * (headShot ? settings.Pistol.HeadShotDamageMultiplier : 1.0f);
			case WeaponType.MachineGun: return settings.MachineGun.Damage * (headShot ? settings.MachineGun.HeadShotDamageMultiplier : 1.0f);
		}

		return 0.0f;
	}

	private bool CheckExplosiveRaycast(Vector3 origin, Vector3 dest, float range, int playerID)
	{
		Vector3 direction = (dest - origin).normalized;
		RaycastHit[] hits = Physics.RaycastAll(new Ray(origin, direction), range, NetworkPlayer.PlayerControllerMask);

		NetworkPlayer player = null;
		float closest = float.MaxValue;
		bool closestIsPlayer = false;
		foreach (RaycastHit hit in hits)
		{
			float dist = (hit.point - origin).sqrMagnitude;
			if (hit.collider && Vector3.Dot(direction, hit.normal) < 0.0f && dist < closest)
			{
				closest = dist;
				player = hit.collider.GetComponent<NetworkPlayer>();
				closestIsPlayer = player && player.ID == playerID;
			}
		}

		return closestIsPlayer;
	}

	/**********************************************************/
	// Accessors/Mutators

	public static Sprite GetWeaponIcon(WeaponType type)
	{
		return instance.weaponIcons[(int)type];
	}
}

public class WeaponData
{
	public WeaponType type;
	public int id;
	public int ownerID;
	public int ammo;
	public int ammoInClip;
	public WeaponDrop drop;
}

public enum WeaponType
{
	AssaultRifle,
	SMG,
	Sniper,
	RocketLauncher,
	Shotgun,
	Pistol,
	MachineGun,
	Blaster,
	None,
}

public struct WeaponAssignment
{
	public WeaponType primaryType;
	public WeaponType secondaryType;
	public int primaryID;
	public int secondaryID;
}

public enum InfiniteAmmoType
{
	None,
	InfiniteAmmo,
	BottomlessClip,
}