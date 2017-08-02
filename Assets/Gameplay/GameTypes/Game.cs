using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Game : NetworkBehaviour
{
	[SerializeField]
	private float gameOverDuration;
	[SerializeField]
	private GameObject scoreboardPrefab;

	[SyncVar]
	protected string mapName;

	protected float timeRemaining;

	protected int localPlayer;
	protected int localTeam;
	protected int numTeams;
	protected bool dirtyHUD;
	protected bool gameOver;
	protected int winner;
	protected float gameOverTime;
	protected GameStatus gameStatus;

	private HUD hud;
	private PartyManager party;
	private GameSettings baseSettings;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		gameStatus = GameStatus.Loading;

		baseSettings = PartyManager.GameSettings;
		numTeams = 2;

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;

		localPlayer = PartyManager.LocalPlayer.NetworkID;
		localTeam = PartyManager.LocalPlayer.Team;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (gameStatus == GameStatus.Loading)
		{
			if (isServer && Party.AllPlayerObjectsReady)
			{
				OnGameStart();
				RpcOnGameStart();
			}
		}
		else if (gameStatus == GameStatus.InGame)
		{
			if (isServer && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
			{
				KillData killData = new KillData();
				killData.shooter = 0;
				killData.victim = 1;
				OnKill(killData);
			}

			if (baseSettings.Generic.Time > 0.0f)
			{
				timeRemaining -= Time.deltaTime;
				HUD.QuickScoreboard.TimeText = FormatTime(timeRemaining);
			}

			if (dirtyHUD && HUD)
			{
				UpdateHUD();
				dirtyHUD = false;
			}

			if (gameOver)
			{
				gameOverTime -= Time.deltaTime;
				HUD.GameOverPanel.FadeAlpha = 1.0f - gameOverTime / gameOverDuration;

				if (gameOverTime <= 0.0f)
				{
					OnGameOverTimeExpired();
				}
			}
		}
	}

	public virtual void FixedUpdate()
	{
	}

	public void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	/**********************************************************/
	// Interface

	public virtual GameType GetGameType()
	{
		return GameType.None;
	}

	public virtual void OnGameStart()
	{
		timeRemaining = baseSettings.Generic.Time;

		PlayerInput.KeyboardActive = true;
		Destroy(GameObject.Find("StartGameCanvas"));

		gameStatus = GameStatus.InGame;
		winner = -1;
	}

	public virtual void PopulatePlayers()
	{
	}

	public virtual void UpdateHUD()
	{
		HUD.QuickScoreboard.MaxScore = baseSettings.Generic.ScoreToWin;

		int localScore = int.MinValue;
		int otherScore = int.MinValue;

		if (baseSettings.Generic.Teams)
		{
			for (int i = 0; i < numTeams; i++)
			{
				int teamScore = GetTeamScore(i);
				if (localTeam == i)
				{
					localScore = teamScore;
				}
				else if (teamScore > otherScore)
				{
					otherScore = teamScore;
				}
			}
		}

		foreach (PlayerData p in PartyManager.Players)
		{
			int playerScore = GetPlayerScore(p.NetworkID);
			if (!baseSettings.Generic.Teams)
			{
				if (p.NetworkID == localPlayer)
				{
					localScore = playerScore;
				}
				else if (playerScore > otherScore)
				{
					otherScore = playerScore;
				}
			}
			HUD.Scoreboard.SetScore(p.NetworkID, playerScore);
		}

		Color localColor = Color.white;
		Color otherColor = Color.white;
		if (baseSettings.Generic.Teams)
		{
			localColor = PartyManager.GetTeamColor(localTeam);
			otherColor = PartyManager.GetTeamColor(localTeam == 0 ? 1 : 0);
		}

		HUD.QuickScoreboard.SetScores(localScore, otherScore, localColor, otherColor);
	}

	public virtual void OnKill(KillData killData)
	{
		if (gameOver)
		{
			return;
		}

		int shooterTeam = -1;
		if (killData.shooter > -1)
		{
			shooterTeam = PartyManager.GetPlayer(killData.shooter).Team;
		}
		int victimTeam = PartyManager.GetPlayer(killData.victim).Team;

		if (shooterTeam > -1 && PartyManager.SameTeam(killData.shooter, killData.victim))
		{
			if (killData.shooter == killData.victim)
			{
				AddPlayerScore(killData.shooter, baseSettings.Generic.ScorePerSuicide);
				AddTeamScore(shooterTeam, baseSettings.Generic.ScorePerSuicide);
			}
			else
			{
				AddPlayerScore(killData.shooter, baseSettings.Generic.ScorePerBetrayal);
				AddTeamScore(shooterTeam, baseSettings.Generic.ScorePerBetrayal);
			}
		}
		else
		{
			if (shooterTeam > -1)
			{
				AddPlayerScore(killData.shooter, baseSettings.Generic.ScorePerKill);
				AddTeamScore(shooterTeam, baseSettings.Generic.ScorePerKill);
			}
			AddPlayerScore(killData.victim, baseSettings.Generic.ScorePerDeath);
			AddTeamScore(victimTeam, baseSettings.Generic.ScorePerDeath);
		}

		CheckForWin();
	}

	public virtual void OnGameOver()
	{
		gameOverTime = gameOverDuration;
		gameOver = true;

		HUD.GameOverPanel.Visible = true;
		HUD.GameOverPanel.FadeAlpha = 0.0f;

		if (isServer)
		{
			RpcOnGameOver(winner);
		}
	}

	public virtual void OnGameOverTimeExpired()
	{
		if (isServer)
		{
			RpcOnGameOverTimeExpired();
			Party.ReturnToLobby();
		}
	}

	public virtual void EndGame()
	{
		OnGameOver();
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcOnGameStart()
	{
		if (!isServer)
		{
			OnGameStart();
		}
	}

	[ClientRpc]
	private void RpcOnGameOverTimeExpired()
	{
		gameStatus = GameStatus.PostGame;
		PartyManager.Get.Status = PartyStatus.ReturningToLobby;
	}

	[ClientRpc]
	private void RpcOnGameOver(int winnerID)
	{
		if (!isServer)
		{
			OnGameOver();
			winner = winnerID;
		}

		if (winner > -1)
		{
			if (baseSettings.Generic.Teams)
			{
				HUD.GameOverPanel.WinnerText = winnerID == 0 ? "Blue Team wins!" : "Red Team wins!";
			}
			else
			{
				HUD.GameOverPanel.WinnerText = PartyManager.GetPlayer(winnerID).Name + " wins!";
			}
		}
		else
		{
			HUD.GameOverPanel.WinnerText = "Host ended game";
		}
	}

	/**********************************************************/
	// Callbacks

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == mapName)
		{
			OnMapLoaded();
		}
	}

	protected virtual void OnMapLoaded()
	{
		HUD.QuickScoreboard.GameModeText = baseSettings.MetaData.Name;

		if (baseSettings.Generic.Time > 0.0f)
		{
			HUD.QuickScoreboard.TimeText = FormatTime(baseSettings.Generic.Time);
		}
		else
		{
			HUD.QuickScoreboard.TimeText = "";
		}
	}

	/**********************************************************/
	// Helper Functions

	protected string FormatTime(float time)
	{
		int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(time));
		string str = "";

		str += secondsLeft / 60;
		str += ":";

		secondsLeft = secondsLeft % 60;
		if (secondsLeft < 10)
		{
			str += "0";
		}
		str += secondsLeft;

		return str;
	}

	protected virtual void CheckForWin()
	{
		bool isGameOver = false;

		if (baseSettings.Generic.Teams)
		{
			for (int i = 0; i < numTeams; i++)
			{
				if (GetTeamScore(i) >= baseSettings.Generic.ScoreToWin)
				{
					winner = i;
					isGameOver = true;
				}
			}
		}
		else
		{
			foreach (PlayerData p in PartyManager.Players)
			{
				if (GetPlayerScore(p.NetworkID) >= baseSettings.Generic.ScoreToWin)
				{
					winner = p.NetworkID;
					isGameOver = true;
				}
			}
		}

		if (isGameOver)
		{
			OnGameOver();
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public HUD HUD
	{
		get
		{
			if (!hud)
			{
				GameObject obj = GameObject.Find("HUD");
				if (obj)
				{
					hud = obj.GetComponent<HUD>();
				}
			}
			return hud;
		}
	}

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

	public bool IsGameOver
	{
		get
		{
			return gameOver;
		}
	}

	public string MapName
	{
		get
		{
			return mapName;
		}
		set
		{
			mapName = value;
		}
	}

	public virtual int GetPlayerScore(int player)
	{
		return 0;
	}

	public virtual void SetPlayerScore(int player, int score)
	{
	}

	public void AddPlayerScore(int player, int score)
	{
		SetPlayerScore(player, GetPlayerScore(player) + score);
	}

	public virtual int GetTeamScore(int team)
	{
		return 0;
	}

	public virtual void SetTeamScore(int team, int score)
	{
	}

	public void AddTeamScore(int team, int score)
	{
		SetTeamScore(team, GetTeamScore(team) + score);
	}
}

public class GenericPlayerStats
{
	public int id;
	public int team;
	public int score;
}

public enum GameType
{
	Deathmatch,
	Siege,
	Bingo,
	None,
}

public enum GameStatus
{
	Loading,
	InGame,
	PostGame,
}