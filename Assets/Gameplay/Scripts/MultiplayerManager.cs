using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NATTraversal.NetworkManager
{
	[SerializeField]
	private GameObject partyManagerPrefab;
	[SerializeField]
	private GameObject inGamePlayerPrefab;

	private AccountData localUser;
	private string localPlayerUserName;
	private PlayerCustomizationOptions localPlayerCustomizationOptions;
	private List<PlayerData> players;
	private int localPlayerIndex;
	private Dictionary<int, int> networkIDIndexMap;

	private MultiplayerStatus status;
	private MultiplayerStatus statusOnDisconnect;
	private bool needsDisconnectNotification;
	private int clientError;
	private PartyRejectionReason partyRejectionReason;
	private bool initializedServer;
	private bool joinFirstMatch;

	private MenuManager menu;
	private GameListMenu gameListMenu;
	private PartyManager party;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		if (useSimulator)
		{
			connectionConfig.NetworkDropThreshold = 90;
		}

		status = MultiplayerStatus.NotConnected;
		clientError = -1;

		SceneManager.sceneLoaded += OnSceneLoaded;

		PlayerPrefs.SetString("CloudNetworkingId", "1674651");
		StartMatchMaker();

		gameListMenu = FindObjectOfType<GameListMenu>();
	}

	public override void Start()
	{
		base.Start();

		Reset();
	}

	public override void Update()
	{
		base.Update();

		if (!initializedServer && NetworkServer.active)
		{
			InitializeServer();
			initializedServer = true;
		}
	}

	/**********************************************************/
	// NetworkManager Interface

	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		base.OnMatchCreate(success, extendedInfo, matchInfo);

		if (success)
		{
			menu.CancelPassiveNotification();
		}
		else
		{
			menu.ShowNotification("Failed to create match");
		}
	}

	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo info)
	{
		base.OnMatchJoined(success, extendedInfo, info);

		if (success)
		{
			menu.CancelPassiveNotification();
		}
		else
		{
			menu.ShowNotification("Failed to join match");
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		status = MultiplayerStatus.Connected;
		partyRejectionReason = PartyRejectionReason.None;

		conn.RegisterHandler(CustomMessageType.PartyRejection, OnClientPartyRejection);
		conn.RegisterHandler(CustomMessageType.GameSettings, OnClientGameSettings);
		conn.RegisterHandler(CustomMessageType.StartGame, OnClientStartGame);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		SetDisconnect();

		if (statusOnDisconnect == MultiplayerStatus.Connected)
		{
			Party.ReturnToMainMenu();
		}
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		base.OnClientError(conn, errorCode);

		clientError = errorCode;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);

		SetDisconnect();
	}

	public override void OnServerError(NetworkConnection conn, int errorCode)
	{
		base.OnServerError(conn, errorCode);

		print("Server error: " + errorCode);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		initializedServer = false;
	}

	/**********************************************************/
	// Interface

	public void Reset()
	{
		players = new List<PlayerData>();

		localPlayerIndex = -1;
		networkIDIndexMap = new Dictionary<int, int>();
	}

	public void OnLogIn(string userName, int id)
	{
		//localUser = new AccountData();
		//localUser.userName = userName;
		//localUser.id = id;

		localPlayerUserName = userName;
	}

	public void OnLogOut()
	{
		localUser = null;
	}

	public void StartLocalHost(string username, PlayerCustomizationOptions options)
	{
		localPlayerUserName = username;
		localPlayerCustomizationOptions = options;
		CreateMatch();
	}

	public void StartLocalClient(string username, PlayerCustomizationOptions options)
	{
		localPlayerUserName = username;
		localPlayerCustomizationOptions = options;
		JoinFirstMatch();
	}

	public void CreateMatch()
	{
		status = MultiplayerStatus.CreatingGame;

		//localPlayerCustomizationOptions = Menu.CustomizationOptions;
		localPlayerCustomizationOptions = new PlayerCustomizationOptions();

		if (matchMaker == null)
		{
			matchMaker = gameObject.AddComponent<NetworkMatch>();
		}
		StartHostAll(localPlayerUserName + "'s Game", customConfig ? (uint)(maxConnections + 1) : matchSize);

		menu.ShowPassiveNotification("Creating match...");
	}

	public void ListMatches()
	{
		joinFirstMatch = false;

		if (matchMaker == null)
		{
			matchMaker = gameObject.AddComponent<NetworkMatch>();
		}
		matchMaker.ListMatches(0, 8, "", true, 0, 0, OnMatchList);
	}

	public void JoinMatch(MatchInfoSnapshot match)
	{
		localPlayerCustomizationOptions = new PlayerCustomizationOptions();
		Status = MultiplayerStatus.JoiningGame;

		if (matchMaker == null)
		{
			matchMaker = gameObject.AddComponent<NetworkMatch>();
		}
		StartClientAll(match);
	}

	public void JoinFirstMatch()
	{
		ListMatches();
		joinFirstMatch = true;
	}

	public void AddPlayer(PlayerData player)
	{
		players.Add(player);
		networkIDIndexMap.Clear();
		localPlayerIndex = -1;
	}

	public void RemovePlayer(PlayerData player)
	{
		players.Remove(player);
		networkIDIndexMap.Clear();
		localPlayerIndex = -1;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (sceneName == Party.MapName)
		{
			Party.ServerSceneIsLoaded = true;
		}

		print("Server Scene Loaded: " + sceneName);
		base.OnServerSceneChanged(sceneName);
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		print("Client Scene Loaded" + SceneManager.GetActiveScene().name);
		base.OnClientSceneChanged(conn);
	}

	public void SetDisconnect()
	{
		statusOnDisconnect = status;
		status = MultiplayerStatus.NotConnected;
		needsDisconnectNotification = true;
	}

	/**********************************************************/
	// Messages

	public void OnClientPartyRejection(NetworkMessage msg)
	{
		partyRejectionReason = msg.ReadMessage<PartyRejectionMessage>().Reason;
		StopClient();
	}

	public void OnClientGameSettings(NetworkMessage msg)
	{
		GameSettingsMessage m = msg.ReadMessage<GameSettingsMessage>();
		Party.OnGameSettingsMessage(m);
	}

	public void OnClientStartGame(NetworkMessage msg)
	{
		StartGameMessage m = msg.ReadMessage<StartGameMessage>();
		Party.OnStartGameMessage(m);
	}

	/**********************************************************/
	// Callbacks

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
	}

	public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		base.OnMatchList(success, extendedInfo, matchList);

		if (success)
		{
			gameListMenu.OnMatchList(matchList);

			if (joinFirstMatch)
			{
				if (matchList.Count > 0)
				{
					JoinMatch(matchList[0]);
				}
			}
		}
	}

	/**********************************************************/
	// Helper Functions

	private void InitializeServer()
	{
		GameObject obj = Instantiate(partyManagerPrefab);
		NetworkServer.Spawn(obj);
	}

	/**********************************************************/
	// Accessors/Mutators

	public GameObject InGamePlayerPrefab
	{
		get
		{
			return inGamePlayerPrefab;
		}
	}

	public string LocalPlayerUserName
	{
		get
		{
			return localPlayerUserName;
		}
		set
		{
			localPlayerUserName = value;
		}
	}

	public PlayerCustomizationOptions LocalPlayerCustomizationOptions
	{
		get
		{
			return localPlayerCustomizationOptions;
		}
		set
		{
			localPlayerCustomizationOptions = value;
		}
	}

	public List<PlayerData> Players
	{
		get
		{
			return players;
		}
	}

	public PlayerData GetLocalPlayerData()
	{
		if (localPlayerIndex < 0)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].isLocalPlayer)
				{
					localPlayerIndex = i;
					break;
				}
			}
		}

		if (localPlayerIndex < 0)
		{
			return null;
		}

		return players[localPlayerIndex];
	}

	public int GetLocalPlayerNetworkID()
	{
		return GetLocalPlayerData().NetworkID;
	}

	public PlayerData GetPlayerData(int networkID)
	{
		if (networkIDIndexMap.ContainsKey(networkID))
		{
			return players[networkIDIndexMap[networkID]];
		}

		for (int i = 0; i < players.Count; i++)
		{
			if (players[i].NetworkID == networkID)
			{
				networkIDIndexMap[networkID] = i;
				return players[i];
			}
		}

		return null;
	}

	public StatsManager Stats
	{
		get
		{
			return party.Stats;
		}
	}

	public PartyManager Party
	{
		get
		{
			return party;
		}
		set
		{
			party = value;
		}
	}

	public MenuManager Menu
	{
		get
		{
			return menu;
		}
		set
		{
			menu = value;
		}
	}

	public bool NeedsDisconnectNotification
	{
		get
		{
			return needsDisconnectNotification;
		}
		set
		{
			needsDisconnectNotification = value;
		}
	}

	public int ClientError
	{
		get
		{
			return clientError;
		}
		set
		{
			clientError = value;
		}
	}

	public MultiplayerStatus Status
	{
		get
		{
			return status;
		}
		set
		{
			status = value;
		}
	}

	public MultiplayerStatus StatusOnDisconnect
	{
		get
		{
			return statusOnDisconnect;
		}
		set
		{
			statusOnDisconnect = value;
		}
	}

	public PartyRejectionReason PartyRejectionReason
	{
		get
		{
			return partyRejectionReason;
		}
	}

	public AccountData LocalUser
	{
		get
		{
			return localUser;
		}
	}
}

public enum MultiplayerStatus
{
	NotConnected,
	CreatingGame,
	FindingGame,
	JoiningGame,
	HostingLocalGame,
	JoiningLocalGame,
	Connected,
}

public class AccountData
{
	public string userName;
	public int id;
}