using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerData : SafeNetworkBehaviour
{
	[SerializeField]
	private float pingQueryInterval;

	[SyncVar]
	private int networkID;
	[SyncVar]
	private string userName;
	[SyncVar]
	private int team;
	[SyncVar]
	private PlayerCustomizationOptions customizationOptions;
	[SyncVar]
	private int ping;

	private bool registeredWithParty;
	private bool loadedMap;
	private bool playerObjectReady;
	private int accountID;
	private int pingQueryIndex;
	private float pingQuerySendTime;
	private float nextPingQuery;

	private MultiplayerManager mgr;
	private PartyManager party;
	private NetworkPlayer networkPlayer;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		networkID = -1;
		team = -1;
		userName = "N/A";

		ResetLoadingFlags();

		DontDestroyOnLoad(gameObject);
	}

	public override void Update()
	{
		base.Update();

		if (!initialized)
		{
			return;
		}

		if (!registeredWithParty && networkID > -1 && userName != "N/A")
		{
			Party.Register(this);
			registeredWithParty = true;

			if (hasAuthority)
			{
				Party.EnterLobby();
			}
		}

		if (isServer)
		{
			nextPingQuery -= Time.deltaTime;
			if (nextPingQuery <= 0.0f)
			{
				pingQueryIndex++;
				pingQuerySendTime = Time.time;
				RpcQueryPing(pingQueryIndex);
				nextPingQuery = 10.0f;
			}
		}
	}

	public override void OnNetworkDestroy()
	{
		base.OnNetworkDestroy();

		Party.Unregister(this);
	}

	/**********************************************************/
	// Safe Initialization

	protected override bool Ready()
	{
		return Party;
	}

	protected override void DelayedAwake()
	{
		mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
	}

	protected override void DelayedOnStartLocalPlayer()
	{
		CmdOnNewConnection(mgr.LocalPlayerUserName, mgr.LocalPlayerCustomizationOptions, mgr.LocalUser == null ? -1 : mgr.LocalUser.id);
	}

	/**********************************************************/
	// Interface

	public override string ToString()
	{
		return userName + " (" + networkID + ")";
	}

	public void SpawnPlayer()
	{
		CmdSpawnPlayer();
	}

	public void ResetLoadingFlags()
	{
		loadedMap = false;
		playerObjectReady = false;
	}

	/**********************************************************/
	// Commands

	[Command]
	public void CmdOnNewConnection(string userName, PlayerCustomizationOptions options, int accountID)
	{
		Party.OnServerNewConnection(this, userName, options, accountID);
	}

	[Command]
	public void CmdSpawnPlayer()
	{
		SpawnPoint spawnPoint = GameObject.Find("MultiplayerMap").GetComponent<SpawnManager>().InitialSpawn(networkID);

		GameObject player = Instantiate(mgr.InGamePlayerPrefab);
		player.GetComponent<Rigidbody>().position = spawnPoint.transform.position;
		player.transform.position = spawnPoint.transform.position;
		player.transform.rotation = spawnPoint.transform.rotation;
		player.GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();

		NetworkPlayer net = player.GetComponent<NetworkPlayer>();
		net.ID = networkID;
		net.Name = userName;
		net.Team = team;

		NetworkServer.SpawnWithClientAuthority(player, gameObject);
	}

	[Command]
	public void CmdMapLoaded()
	{
		loadedMap = true;
	}

	[Command]
	public void CmdPlayerObjectReady()
	{
		playerObjectReady = true;
	}

	[Command]
	public void CmdSendChatMessage(string str)
	{
		Party.OnReceiveChatMessage(networkID, str);
	}

	[Command]
	private void CmdQueuryPing(int index)
	{
		if (index == pingQueryIndex)
		{
			nextPingQuery = pingQueryInterval;
			ping = (int)((Time.time - pingQuerySendTime) * 1000);
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	public void RpcOnPartyRejection(PartyRejectionReason reason)
	{

	}

	[ClientRpc]
	private void RpcQueryPing(int index)
	{
		if (isLocalPlayer)
		{
			CmdQueuryPing(index);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public PartyManager Party
	{
		get
		{
			if (!party)
			{
				party = FindObjectOfType<PartyManager>();
			}

			return party;
		}
	}

	public int NetworkID
	{
		get
		{
			return networkID;
		}
		set
		{
			networkID = value;
		}
	}

	public string Name
	{
		get
		{
			return userName;
		}
		set
		{
			userName = value;
		}
	}

	public int Team
	{
		get
		{
			return team;
		}
		set
		{
			team = value;

			PlayerCustomizationOptions c = customizationOptions;
			c.Shirt = PartyManager.Get.TeamShirts[team];
			customizationOptions = c;
		}
	}

	public PlayerCustomizationOptions CustomizationOptions
	{
		get
		{
			return customizationOptions;
		}
		set
		{
			customizationOptions = value;
		}
	}

	public int Ping
	{
		get
		{
			return ping;
		}
		set
		{
			ping = value;
		}
	}

	public NetworkPlayer NetworkPlayer
	{
		get
		{
			return networkPlayer;
		}
		set
		{
			networkPlayer = value;
		}
	}

	public int AccountID
	{
		get
		{
			return accountID;
		}
		set
		{
			accountID = value;
		}
	}

	public bool LoadedMap
	{
		get
		{
			return loadedMap;
		}
		set
		{
			loadedMap = value;
		}
	}

	public bool PlayerObjectReady
	{
		get
		{
			return playerObjectReady;
		}
		set
		{
			playerObjectReady = value;
		}
	}
}