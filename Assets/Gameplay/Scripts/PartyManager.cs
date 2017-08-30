using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PartyManager : NetworkBehaviour
{
	[SerializeField]
	private List<GameObject> gamePrefabs;
	[SerializeField]
	private List<GameObject> gameSettingsPrefabs;
	[SerializeField]
	private Color[] teamColors;
	[SerializeField]
	private GameObject loadingMapCanvas;
	[SerializeField]
	private int[] teamShirts;
	[SerializeField]
	private int[] teamHats;
	[SerializeField]
	private string[] internalMapNames;

	[SyncVar]
	private bool serverSceneIsReady;
	[SyncVar]
	private bool allPlayerObjectsReady;

	private int nextNetworkID;
	private PartyStatus status;

	private AsyncOperation loadOp;
	private int mapIndex;
	private string mapName;
	private bool sentMapLoadCommand;
	private bool serverSceneIsLoaded;
	private bool allPlayersMapLoaded;

	private List<GameVariantMetaData> gameVariants;
	private int selectedGameVariant;
	private GameType gameType;
	private Game game;

	private StatsManager stats;
	private MultiplayerManager mgr;
	private MenuManager menus;
	private LobbyMenu lobbyMenu;
	private static PartyManager instance;
	private static GameSettings gameSettings;

	private bool initialized;
	private void SafeInitialize()
	{
		if (initialized)
		{
			return;
		}

		if (gameSettings == null)
		{
			gameType = GameType.Deathmatch;
			gameSettings = Instantiate(gameSettingsPrefabs[(int)gameType]).GetComponent<GameSettings>();
		}
		mapName = internalMapNames[0];

		DontDestroyOnLoad(gameObject);

		initialized = true;
	}

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		if (FindObjectsOfType<PartyManager>().Length > 1)
		{
			Destroy(gameObject);
			return;
		}

		SafeInitialize();

		mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
		mgr.Party = this;
		stats = GetComponent<StatsManager>();
		menus = FindObjectOfType<MenuManager>();

		instance = this;

		status = PartyStatus.NotConnected;
	}

	public void Update()
	{
		if (isClient)
		{
			UpdateClient();
		}
		if (isServer)
		{
			UpdateServer();
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		FindAllGameVariants();
		selectedGameVariant = 0;
		ServerChangeGameType();
	}

	/**********************************************************/
	// Interface

	public PartyRejectionReason IsNewConnectionAllowed()
	{
		if (status == PartyStatus.InGame || status == PartyStatus.LoadingGame || status == PartyStatus.LoadingMap)
		{
			return PartyRejectionReason.GameInProgress;
		}
		else if (Players.Count >= 10)
		{
			return PartyRejectionReason.NoRoomInLobby;
		}

		return PartyRejectionReason.None;
	}

	public void OnServerNewConnection(PlayerData dataObj, string name, PlayerCustomizationOptions options, int accountID)
	{
		SafeInitialize();

		PartyRejectionReason reason = IsNewConnectionAllowed();
		if (reason == PartyRejectionReason.None)
		{
			dataObj.Name = name;
			dataObj.CustomizationOptions = options;
			dataObj.Team = GetUnevenTeam();
			dataObj.NetworkID = nextNetworkID++;
			dataObj.AccountID = accountID;

			ServerShowChatMessage(dataObj.Name + " connected");
		}
		else
		{
			dataObj.RpcOnPartyRejection(reason);
		}
	}

	public void Register(PlayerData player)
	{
		if (mgr.GetPlayerData(player.NetworkID) == null)
		{
			mgr.AddPlayer(player);
			if (lobbyMenu)
			{
				lobbyMenu.OnRegisterPartyMember(player);
			}

			if (isServer)
			{
				GameSettingsMessage msg = new GameSettingsMessage();
				gameSettings.Serialize(out msg.Settings);
				msg.Type = gameType;
				player.connectionToClient.SendByChannel(CustomMessageType.GameSettings, msg, 2);

				RpcChangeMap(mapIndex);
			}
		}
		else
		{
			Debug.LogWarning("Trying to register " + player.ToString() + " again");
		}

		JP.Event.Trigger("OnPartyMembersChanged");
	}

	public void Unregister(PlayerData player)
	{
		if (mgr.GetPlayerData(player.NetworkID))
		{
			mgr.RemovePlayer(player);
			if (lobbyMenu)
			{
				lobbyMenu.OnUnregisterPartyMember(player);
			}

			ServerShowChatMessage(player.Name + " disconnected");
		}
		else
		{
			Debug.LogWarning("Trying to unregister non-registered " + player.ToString());
		}

		JP.Event.Trigger("OnPartyMembersChanged");
	}

	public void EnterLobby()
	{
		menus.GoToMenu(MenuType.Lobby);
		LobbyMenu.OnChangeGameMode(gameSettings.MetaData.Name, gameSettings.MetaData.Description);
		LobbyMenu.OnChangeMap(mapIndex);
	}

	public void OnGameSettingsMessage(GameSettingsMessage msg)
	{
		ChangeGameTypeFromBytes(msg.Type, msg.Settings);
		LobbyMenu.OnChangeGameMode(gameSettings.MetaData.Name, gameSettings.MetaData.Description);
	}

	[Server]
	public void ServerStartGame()
	{
		status = PartyStatus.LoadingMap;

		foreach (PlayerData p in Players)
		{
			p.ResetLoadingFlags();
		}
		allPlayersMapLoaded = false;
		allPlayerObjectsReady = false;

		if (game)
		{
			NetworkServer.UnSpawn(game.gameObject);
			Destroy(game.gameObject);
			game = null;
			print("Destroying game");
		}

		GameObject obj = Instantiate(gamePrefabs[(int)gameType]);
		game = obj.GetComponent<Game>();
		game.PopulatePlayers();
		game.MapName = mapName;
		NetworkServer.Spawn(obj);

		StartGameMessage msg = new StartGameMessage();
		msg.MapName = mapName;
		gameSettings.Serialize(out msg.GameSettings);
		msg.GameType = gameType;
		NetworkServer.SendByChannelToAll(CustomMessageType.StartGame, msg, 2);

		lobbyMenu = null;
	}

	public void OnStartGameMessage(StartGameMessage msg)
	{
		ChangeGameTypeFromBytes(msg.GameType, msg.GameSettings);

		loadOp = SceneManager.LoadSceneAsync(msg.MapName, LoadSceneMode.Additive);
		loadOp.allowSceneActivation = false;
		sentMapLoadCommand = false;

		lobbyMenu = null;

		Instantiate(loadingMapCanvas);

		if (!isServer)
		{
			status = PartyStatus.LoadingMap;
		}
	}

	public void ReturnToMainMenu()
	{
		status = PartyStatus.NotConnected;

		if (SceneManager.GetActiveScene().name != "MainMenu")
		{
			Destroy(gameObject);
			SceneManager.LoadScene("MainMenu");
		}

		FindObjectOfType<MenuManager>().GoToMenu(MenuType.Main);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		mgr.Reset();
		NetworkServer.SetAllClientsNotReady();
		mgr.StopHost();
	}

	public void ReturnToLobby()
	{
		status = PartyStatus.ReturningToLobby;

		mgr.ServerChangeScene("MainMenu");
		serverSceneIsLoaded = false;
		foreach (PlayerData p in Players)
		{
			p.ResetLoadingFlags();
		}
	}

	public void SendChatMessage(string str)
	{
		mgr.GetLocalPlayerData().CmdSendChatMessage(str);
	}

	public void OnReceiveChatMessage(int id, string str)
	{
		RpcOnReceiveChatMessage(mgr.GetPlayerData(id).Name + ": " + str);
	}

	public void ServerShowChatMessage(string str)
	{
		RpcOnReceiveChatMessage(str);
	}

	public void ServerChangeMap(int mapIndex)
	{
		mapName = internalMapNames[mapIndex];
		this.mapIndex = mapIndex;
		RpcChangeMap(mapIndex);
	}

	public void ServerChangeGameType()
	{
		selectedGameVariant++;
		if (selectedGameVariant >= gameVariants.Count)
		{
			selectedGameVariant = 0;
		}

		GameType prevGameType = gameType;
		gameType = gameVariants[selectedGameVariant].Type;
		if (gameType != prevGameType && gameSettings)
		{
			Destroy(gameSettings.gameObject);
			gameSettings = null;
		}
		if (!gameSettings)
		{
			gameSettings = Instantiate(gameSettingsPrefabs[(int)gameType]).GetComponent<GameSettings>();
		}
		gameSettings.Load(gameVariants[selectedGameVariant].FileName);

		GameSettingsMessage msg = new GameSettingsMessage();
		gameSettings.Serialize(out msg.Settings);
		msg.Type = gameType;
		NetworkServer.SendByChannelToAll(CustomMessageType.GameSettings, msg, 2);
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcChangeScene()
	{
		lobbyMenu = null;
		loadOp.allowSceneActivation = true;

		status = PartyStatus.LoadingGame;
	}

	[ClientRpc]
	private void RpcOnReceiveChatMessage(string str)
	{
		if (lobbyMenu)
		{
			lobbyMenu.ShowChatMessage(str);
		}
	}

	[ClientRpc]
	private void RpcOnAllPlayerObjectsReady()
	{

	}

	[ClientRpc]
	private void RpcChangeMap(int mapIndex)
	{
		LobbyMenu.OnChangeMap(mapIndex);
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateClient()
	{
		if (status == PartyStatus.LoadingMap && loadOp != null && loadOp.progress >= 0.9f && !sentMapLoadCommand && mgr.GetLocalPlayerData())
		{
			mgr.GetLocalPlayerData().CmdMapLoaded();
			sentMapLoadCommand = true;
		}

		//if (status == PartyStatus.ReturningToLobby && LobbyMenu)
		//{
		//	menus.GoToMenu(MenuType.Lobby);
		//	LobbyMenu.OnChangeGameMode(gameSettings.MetaData.Name, gameSettings.MetaData.Description);
		//	LobbyMenu.OnChangeMap(mapIndex);
		//	//LobbyMenu.OnEnterLobby();
		//}
	}

	private void UpdateServer()
	{
		if (status == PartyStatus.LoadingMap)
		{
			if (!allPlayersMapLoaded)
			{
				allPlayersMapLoaded = true;
				foreach (PlayerData p in Players)
				{
					if (!p.LoadedMap)
					{
						allPlayersMapLoaded = false;
						break;
					}
				}
			}

			if (allPlayersMapLoaded)
			{
				status = PartyStatus.LoadingGame;
				RpcChangeScene();
				mgr.ServerChangeScene(mapName);
			}
		}
		else if (status == PartyStatus.LoadingGame)
		{
			if (serverSceneIsLoaded && !serverSceneIsReady)
			{
				serverSceneIsReady = GameObject.Find("MultiplayerMap");
			}

			if (serverSceneIsReady)
			{
				bool temp = true;
				foreach (PlayerData p in Players)
				{
					if (!p.PlayerObjectReady)
					{
						temp = false;
					}
				}
				if (temp != allPlayerObjectsReady)
				{
					allPlayerObjectsReady = temp;
				}

				if (allPlayerObjectsReady)
				{
					status = PartyStatus.InGame;
				}
			}
		}
	}

	private int GetUnevenTeam()
	{
		int team = 0;
		int playersOnTeam = int.MaxValue;

		for (int i = 0; i < 2; i++)
		{
			int num = 0;
			foreach (PlayerData p in Players)
			{
				if (p.Team == i)
				{
					num++;
				}
			}
			if (num < playersOnTeam)
			{
				team = i;
				playersOnTeam = num;
			}
		}

		return team;
	}

	private void FindAllGameVariants()
	{
		gameVariants = new List<GameVariantMetaData>();

		string[] files = System.IO.Directory.GetFiles("GameVariants", "*.xml");
		foreach (string file in files)
		{
			GameVariantMetaData meta = new GameVariantMetaData();
			GameSettings.LoadMetaData(file, meta);
			gameVariants.Add(meta);
		}
	}

	private void ChangeGameTypeFromBytes(GameType type, byte[] settings)
	{
		GameType prevGameType = gameType;
		gameType = type;
		if (gameType != prevGameType && gameSettings)
		{
			Destroy(gameSettings.gameObject);
			gameSettings = null;
		}
		if (!gameSettings)
		{
			gameSettings = Instantiate(gameSettingsPrefabs[(int)gameType]).GetComponent<GameSettings>();
		}
		gameSettings.Deserialize(settings);
	}

	/**********************************************************/
	// Accessors/Mutators

	public static PartyManager Get
	{
		get
		{
			return instance;
		}
	}

	public StatsManager Stats
	{
		get
		{
			return stats;
		}
	}

	public MenuManager Menus
	{
		get
		{
			return menus;
		}
		set
		{
			menus = value;
		}
	}

	public LobbyMenu LobbyMenu
	{
		get
		{
			if (!lobbyMenu)
			{
				GameObject obj = GameObject.Find("LobbyMenu");
				if (obj)
				{
					lobbyMenu = obj.GetComponent<LobbyMenu>();
				}
			}

			return lobbyMenu;
		}
	}

	public PartyStatus Status
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

	public string MapName
	{
		get
		{
			return mapName;
		}
	}

	public int MapIndex
	{
		get
		{
			return mapIndex;
		}
	}

	public bool ServerSceneIsLoaded
	{
		get
		{
			return serverSceneIsLoaded;
		}
		set
		{
			serverSceneIsLoaded = value;
		}
	}

	public bool ServerSceneIsReady
	{
		get
		{
			return serverSceneIsReady;
		}
	}

	public static List<PlayerData> Players
	{
		get
		{
			return instance.mgr.Players;
		}
	}

	public static PlayerData GetPlayer(int id)
	{
		return instance.mgr.GetPlayerData(id);
	}

	public static PlayerData LocalPlayer
	{
		get
		{
			return instance.mgr.GetLocalPlayerData();
		}
	}

	public Game Game
	{
		get
		{
			return game;
		}
	}

	public static GameSettings GameSettings
	{
		get
		{
			return gameSettings;
		}
		set
		{
			gameSettings = value;
		}
	}

	public bool AllPlayerObjectsReady
	{
		get
		{
			return allPlayerObjectsReady;
		}
	}

	public bool AllPlayersMapLoaded
	{
		get
		{
			return allPlayersMapLoaded;
		}
	}

	public static bool SameTeam(int player1, int player2)
	{
		if (player1 < 0 || player2 < 0)
		{
			return false;
		}

		if (player1 == player2)
		{
			return true;
		}

		if (!gameSettings.Generic.Teams)
		{
			return false;
		}

		return GetPlayer(player1).Team == GetPlayer(player2).Team;
	}

	public int[] TeamShirts
	{
		get
		{
			return teamShirts;
		}
	}

	public int[] TeamHats
	{
		get
		{
			return teamHats;
		}
	}

	public static Color GetTeamColor(int team)
	{
		return instance.teamColors[team];
	}
}

public enum PartyStatus
{
	NotConnected,
	InLobby,
	LoadingMap,
	LoadingGame,
	InGame,
	ReturningToLobby,
}

public enum PartyRejectionReason
{
	NoRoomInLobby,
	GameInProgress,
	None,
}