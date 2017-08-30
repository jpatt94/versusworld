using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PowerUpManager : NetworkBehaviour
{
	[SerializeField]
	private float distanceThreshold;
	[SerializeField]
	private GameObject grenadeCloudPrefab;

	private Dictionary<int, PowerUp> powerUps;
	private List<PowerUpAward> awardedPowerUps;
	private int weightSum;

	private PowerUpSettings settings;
	private MultiplayerMap map;

	private static PowerUpManager instance;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		instance = this;

		settings = PartyManager.GameSettings.PowerUps;
		weightSum = 0;

		foreach (int i in settings.PowerUpWeights)
		{
			weightSum += i;
		}
	}

	public void Update()
	{
		if (isServer)
		{
			for (int i = 0; i < awardedPowerUps.Count; i++)
			{
				awardedPowerUps[i].triggerTime -= Time.deltaTime;
				if (awardedPowerUps[i].triggerTime <= 0.0f)
				{
					ProcessPowerUp(awardedPowerUps[i].player, awardedPowerUps[i].type);
					awardedPowerUps.RemoveAt(i);
					i--;
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public override void OnStartServer()
	{
		powerUps = new Dictionary<int, PowerUp>();
		int nextID = 0;

		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("PowerUp"))
		{
			PowerUp p = obj.GetComponent<PowerUp>();
			powerUps[nextID] = p;
			p.ID = nextID;
			nextID++;

			p.Available = settings.SpawnOnMap;
			p.Respawns = p.Available;
		}

		awardedPowerUps = new List<PowerUpAward>();

		map = FindObjectOfType<MultiplayerMap>();
	}

	public void PickUp(NetworkPlayer player, int powerUpID)
	{
		PowerUp powerUp = powerUps[powerUpID];
		if (powerUp.Available && (powerUp.transform.position - player.transform.position).sqrMagnitude <= distanceThreshold * distanceThreshold)
		{
			powerUp.Available = false;
			powerUp.RespawnTime = settings.RespawnTime;

			PowerUpType type = GetRandomPowerUpType();
			PowerUpAward award = new PowerUpAward();
			award.player = player;
			award.type = type;
			award.triggerTime = settings.SpinnerDuration;
			awardedPowerUps.Add(award);

			player.PowerUpCarrier.RpcOnPickUpPowerUp(type);
		}
	}

	public static PowerUpType GetRandomPowerUpType()
	{
		if (instance && instance.settings.PowerUpWeights != null)
		{
			int[] weights = instance.settings.PowerUpWeights;
			int roll = Random.Range(0, instance.weightSum);
			int runningWeight = 0;
			for (int i = 0; i < weights.Length; i++)
			{
				runningWeight += weights[i];
				if (roll < runningWeight)
				{
					return (PowerUpType)i;
				}
			}
		}

		return (PowerUpType)Random.Range(0, (int)PowerUpType.NumTypes);
	}

	public static bool PowerUpNeedsActivation(PowerUpType type)
	{
		switch (type)
		{
			case PowerUpType.AmmoRefill: return false;
			case PowerUpType.PowerPlay: return true;
			case PowerUpType.TeslaGrenade: return false;
			case PowerUpType.GrenadeCloud: return true;
			case PowerUpType.SpikeGrenade: return false;
			case PowerUpType.BigHeads: return true;
			case PowerUpType.DamageResist: return true;
			case PowerUpType.SpeedBoost: return true;
			case PowerUpType.DamageBoost: return true;
		}

		return false;
	}

	public void OnDeath(NetworkPlayer player)
	{
		for (int i = 0; i < awardedPowerUps.Count; i++)
		{
			if (awardedPowerUps[i].player == player)
			{
				awardedPowerUps.RemoveAt(i);
				i--;
			}
		}
	}

	public void UseGrenadeCloud(NetworkPlayer player)
	{
		GrenadeCloudSettings gcs = settings.GrenadeCloud;

		GameObject obj = Instantiate(grenadeCloudPrefab);
		GrenadeCloud cloud = obj.GetComponent<GrenadeCloud>();
		cloud.OwnerID = player.ID;

		Vector3 startPos = Vector3.zero;
		Vector3 endPos = Vector3.zero;
		switch (Random.Range(0, 4))
		{
			case 0:
				startPos = map.TopLeft;
				endPos = map.BottomRight;
				break;
			case 1:
				startPos = map.TopLeft + Vector3.right * map.Width;
				endPos = map.BottomRight + Vector3.left * map.Width;
				break;
			case 2:
				startPos = map.BottomRight;
				endPos = map.TopLeft;
				break;
			case 3:
				startPos = map.BottomRight + Vector3.left * map.Width;
				endPos = map.TopLeft + Vector3.right * map.Width;
				break;
		}

		cloud.Duration = (endPos - startPos).magnitude / gcs.Speed;

		Vector3 direction = (endPos - startPos).normalized;
		cloud.Velocity = direction * gcs.Speed;

		startPos -= direction * gcs.Speed * gcs.IntroDuration;
		obj.transform.position = new Vector3(startPos.x, map.GrenadeCloudY, startPos.z);

		NetworkServer.Spawn(obj);
	}

	public void UseBigHeads(NetworkPlayer player)
	{
		foreach (NetworkPlayer p in PlayerManager.PlayerList)
		{
			if (!PartyManager.SameTeam(player.ID, p.ID))
			{
				p.Model.RpcActivateBigHead(settings.BigHeads.Duration);
			}
		}

		RpcOnUseBigHeads(player.ID);
	}

	public void UseTraitsPowerUp(NetworkPlayer player, PowerUpType type)
	{
		switch (type)
		{
			case PowerUpType.DamageResist:
				player.PowerUpCarrier.TraitsPowerUpTime = settings.DamageResistDuration;
				break;
			case PowerUpType.SpeedBoost:
				player.PowerUpCarrier.TraitsPowerUpTime = settings.SpeedBoostDuration;
				break;
			case PowerUpType.DamageBoost:
				player.PowerUpCarrier.TraitsPowerUpTime = settings.DamageBoostDuration;
				break;
		}

		player.PowerUpCarrier.StartTraitsPowerUp(type);
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcOnUseBigHeads(int playerID)
	{
		HUD.Instance.KillFeed.ShowMessage(PlayerManager.GetPlayer(playerID).Name + ": Big heads!", PartyManager.SameTeam(playerID, PlayerManager.LocalPlayerID) ? 1 : -1);
	}

	/**********************************************************/
	// Helper Functions

	private void ProcessPowerUp(NetworkPlayer player, PowerUpType type)
	{
		switch (type)
		{
			case PowerUpType.AmmoRefill: ProcessAmmoRefill(player); break;
			case PowerUpType.PowerPlay: ProcessPowerPlay(player); break;
			case PowerUpType.TeslaGrenade: ProcessTeslaGrenade(player); break;
			case PowerUpType.GrenadeCloud: ProcessGrenadeCloud(player); break;
			case PowerUpType.SpikeGrenade: ProcessSpikeGrenade(player); break;
			case PowerUpType.BigHeads: ProcessBigHeads(player); break;
			case PowerUpType.DamageResist: ProcessDamageResist(player); break;
			case PowerUpType.SpeedBoost: ProcessSpeedBoost(player); break;
			case PowerUpType.DamageBoost: ProcessSpeedBoost(player); break;
		}
	}

	private void ProcessAmmoRefill(NetworkPlayer player)
	{
		player.WeaponManager.RefillPlayersWeapons(player);
	}

	private void ProcessPowerPlay(NetworkPlayer player)
	{
	}

	private void ProcessTeslaGrenade(NetworkPlayer player)
	{
		player.GrenadeManager.GiveGrenade(player, GrenadeType.Tesla);
	}

	private void ProcessGrenadeCloud(NetworkPlayer player)
	{
	}

	private void ProcessSpikeGrenade(NetworkPlayer player)
	{
		player.GrenadeManager.GiveGrenade(player, GrenadeType.Spike);
	}

	private void ProcessBigHeads(NetworkPlayer player)
	{

	}

	private void ProcessDamageResist(NetworkPlayer player)
	{
	}

	private void ProcessSpeedBoost(NetworkPlayer player)
	{
	}

	private void ProcessDamageBoost(NetworkPlayer player)
	{
	}

	/**********************************************************/
	// Accessors/Mutators

	public PowerUpSettings Settings
	{
		get
		{
			return settings;
		}
	}
}

public class PowerUpAward
{
	public NetworkPlayer player;
	public PowerUpType type;
	public float triggerTime;
}

public enum PowerUpType
{
	AmmoRefill,
	PowerPlay,
	TeslaGrenade,
	GrenadeCloud,
	SpikeGrenade,
	BigHeads,
	DamageResist,
	SpeedBoost,
	DamageBoost,
	NumTypes,
	None,
}