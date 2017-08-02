using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{
	private static PlayerManager instance;
	private Dictionary<int, NetworkPlayer> playerMap;
	private List<NetworkPlayer> playerList;
	private int localPlayerID;

	private HUD hud;
	private MedalTracker medals;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		instance = this;
		playerMap = new Dictionary<int, NetworkPlayer>();
		playerList = new List<NetworkPlayer>();
		localPlayerID = -1;

		hud = GameObject.Find("HUD").GetComponent<HUD>();
		medals = GetComponent<MedalTracker>();
	}

	/**********************************************************/
	// Interface

	public override void OnStartClient()
	{
		print("OnClientStart PlayerManager");
		base.OnStartClient();
	}

	public static void AddPlayer(NetworkPlayer player)
	{
		if (!instance.playerMap.ContainsValue(player))
		{
			instance.playerMap[player.ID] = player;
			instance.playerList.Add(player);
			instance.medals.AddPlayer(player.ID);
		}
	}

	public static void RemovePlayer(int networkID)
	{
		if (!instance.playerMap.ContainsKey(networkID))
		{
			Debug.LogError("PlayerManager.RemovePlayer(" + networkID + "): No player found for id " + networkID);
			return;
		}

		instance.playerList.Remove(instance.playerMap[networkID]);
		instance.playerMap.Remove(networkID);
	}

	/**********************************************************/
	// Accessors/Mutators

	public static PlayerManager Instance
	{
		get
		{
			return instance;
		}
	}

	public static NetworkPlayer GetPlayer(int networkID)
	{
		if (instance.playerMap.ContainsKey(networkID))
		{
			return instance.playerMap[networkID];
		}

		return null;
	}

	public static Dictionary<int, NetworkPlayer> PlayerMap
	{
		get
		{
			return instance.playerMap;
		}
	}

	public static List<NetworkPlayer> PlayerList
	{
		get
		{
			return instance.playerList;
		}
	}

	public static NetworkPlayer LocalPlayer
	{
		get
		{
			if (instance.playerMap.ContainsKey(instance.localPlayerID))
			{
				return instance.playerMap[instance.localPlayerID];
			}

			return null;
		}
	}

	public static int LocalPlayerID
	{
		get
		{
			if (!instance)
			{
				return -1;
			}

			return instance.localPlayerID;
		}
		set
		{
			instance.localPlayerID = value;
		}
	}

	public MedalTracker MedalTracker
	{
		get
		{
			return medals;
		}
	}
}

public struct KillData
{
	public int shooter;
	public int victim;
	public DamageType damageType;
	public Vector3 force;
	public int[] assisters;
	public float[] assistDamages;
}