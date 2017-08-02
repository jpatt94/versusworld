using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Siege : Game
{
	[SerializeField]
	private GameObject hudPrefab;
	[SerializeField]
	private AudioClip capturedPointSound;
	[SerializeField]
	private AudioClip lostPointSound;

	public class SiegePlayersList : SyncListStruct<SiegePlayer> { }
	private SiegePlayersList players = new SiegePlayersList();
	public class SiegeTeamsList : SyncListStruct<SiegeTeam> { }
	private SiegeTeamsList teams = new SiegeTeamsList();

	private SyncListInt numPlayersInPoint;
	[SyncVar]
	private float captureTime;
	[SyncVar]
	private bool speedMult;

	private List<CapturePoint> capturePoints;
	private CapturePoint activePoint;
	private List<int> unusedPoints;

	private SiegeSettings settings;
	private SiegeHUD siegeHUD;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		numPlayersInPoint = new SyncListInt();

		settings = PartyManager.GameSettings as SiegeSettings;

		capturePoints = new List<CapturePoint>();
		unusedPoints = new List<int>();
	}

	public override void Start()
	{
		base.Start();

		players.Callback = OnPlayersChanged;
		teams.Callback = OnTeamsChanged;
	}

	public override void Update()
	{
		base.Update();

		if (!(gameStatus == GameStatus.InGame))
		{
			return;
		}

		CheckPlayersInPoint();

		float captureAmount = Mathf.InverseLerp(-settings.CaptureDuration, settings.CaptureDuration, captureTime);
		siegeHUD.CaptureAmount = captureAmount;
		if (activePoint)
		{
			activePoint.CaptureAmount = captureAmount;
		}
		siegeHUD.SpeedMult = speedMult;

		if (isServer && !gameOver)
		{
			CheckCapture();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!(gameStatus == GameStatus.InGame))
		{
			return;
		}

		if (isServer && activePoint)
		{
			for (int i = 0; i < numTeams; i++)
			{
				numPlayersInPoint[i] = activePoint.NumPlayers[i];
			}
		}
	}

	/**********************************************************/
	// Interface

	public override GameType GetGameType()
	{
		return GameType.Siege;
	}

	public override void PopulatePlayers()
	{
		base.PopulatePlayers();

		foreach (PlayerData p in PartyManager.Players)
		{
			SiegePlayer sp = new SiegePlayer();
			sp.id = p.NetworkID;
			sp.team = p.Team;
			sp.score = 0;
			players.Add(sp);
		}

		for (int i = 0; i < numTeams; i++)
		{
			SiegeTeam t = new SiegeTeam();
			t.team = i;
			t.score = 0;
			teams.Add(t);
		}

		dirtyHUD = true;
	}

	public override void OnGameStart()
	{
		base.OnGameStart();

		if (isServer)
		{
			for (int i = 0; i < numTeams; i++)
			{
				numPlayersInPoint.Add(0);
			}
			ChangeCapturePoint(0, -1);
		}
	}

	public override void OnGameOver()
	{
		base.OnGameOver();

		activePoint.enabled = false;
		activePoint = null;

		siegeHUD.OnGameOver();
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcChangeCapturePoint(int order, int capturingTeam)
	{
		if (!isServer)
		{
			ChangeCapturePoint(order, capturingTeam);
		}

		if (capturingTeam == localTeam)
		{
			HUD.KillFeed.ShowMessage("Your team captured the point", 1);
			aud.PlayOneShot(capturedPointSound);
		}
		else if (capturingTeam != -1)
		{
			HUD.KillFeed.ShowMessage("Enemy team captured the point", -1);
			aud.PlayOneShot(lostPointSound);
		}
	}

	[ClientRpc]
	private void RpcAwardCapture(int[] capturers, int numCapturers)
	{
		for (int i = 0; i < numCapturers; i++)
		{
			if (localPlayer == capturers[i])
			{
				HUD.ScorePopUp.Trigger("+250 Capture", Color.white);
			}
		}
	}

	/**********************************************************/
	// Callbacks

	private void OnPlayersChanged(SyncListStruct<SiegePlayer>.Operation op, int index)
	{
		dirtyHUD = true;
	}

	private void OnTeamsChanged(SyncListStruct<SiegeTeam>.Operation op, int index)
	{
		dirtyHUD = true;
	}

	protected override void OnMapLoaded()
	{
		base.OnMapLoaded();

		siegeHUD = Instantiate(hudPrefab).GetComponent<SiegeHUD>();
		siegeHUD.transform.SetParent(GameObject.Find("HUD").transform, false);
		siegeHUD.transform.localPosition = Vector3.zero;
		siegeHUD.CaptureStatus = CaptureStatus.None;

		aud = HUD.GetComponent<AudioSource>();

		capturePoints.Clear();
		unusedPoints.Clear();
		foreach (CapturePoint c in FindObjectsOfType<CapturePoint>())
		{
			c.enabled = false;
			capturePoints.Add(c);
		}
		for (int i = 0; i < capturePoints.Count; i++)
		{
			unusedPoints.Add(i);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void ChangeCapturePoint(int order, int capturingTeam)
	{
		if (activePoint)
		{
			activePoint.enabled = false;
		}

		activePoint = FindCapturePoint(order);
		activePoint.enabled = true;

		captureTime = 0.0f;

		if (isServer)
		{
			RpcChangeCapturePoint(order, capturingTeam);
		}
	}

	private CapturePoint FindCapturePoint(int order)
	{
		for (int i = 0; i < capturePoints.Count; i++)
		{
			if (capturePoints[i].Order == order)
			{
				unusedPoints.Remove(i);
				if (unusedPoints.Count <= 0)
				{
					for (int j = 0; j < capturePoints.Count; j++)
					{
						unusedPoints.Add(j);
					}
				}

				return capturePoints[i];
			}
		}

		return null;
	}

	private void CheckPlayersInPoint()
	{
		int playerBalance = numPlayersInPoint[0];
		playerBalance -= numPlayersInPoint[1];

		if (isServer)
		{
			speedMult = false;

			if ((numPlayersInPoint[0] > 0 || numPlayersInPoint[1] > 0) && !(numPlayersInPoint[0] > 0 && numPlayersInPoint[1] > 0))
			{
				speedMult = Mathf.Abs(playerBalance) > 1;
				float mult = speedMult ? settings.MultipleCapturersSpeed : 1.0f;
				if (playerBalance > 0)
				{
					captureTime += Time.deltaTime * mult;
				}
				else if (playerBalance < 0)
				{
					captureTime -= Time.deltaTime * mult;
				}
			}
		}

		if (numPlayersInPoint[0] > 0 || numPlayersInPoint[1] > 0)
		{
			if (numPlayersInPoint[0] > 0 && numPlayersInPoint[1] > 0)
			{
				siegeHUD.CaptureStatus = CaptureStatus.Contesting;
			}
			else
			{
				bool capturing = (localTeam == 0 && playerBalance > 0) || (localTeam == 1 && playerBalance < 0);
				siegeHUD.CaptureStatus = capturing ? CaptureStatus.Capturing : CaptureStatus.LosingPoint;
			}
		}
		else
		{
			siegeHUD.CaptureStatus = CaptureStatus.None;
		}
	}

	private void CheckCapture()
	{
		if (Mathf.Abs(captureTime) >= settings.CaptureDuration)
		{
			int team = captureTime >= 0.0f ? 0 : 1;
			AddTeamScore(team, settings.ScorePerCaptureSuccess);
			AddTeamScore(team == 0 ? 1 : 0, settings.ScorePerCaptureFailure);

			for (int i = 0; i < numPlayersInPoint[team]; i++)
			{
				AddPlayerScore(activePoint.PlayersInPoint[team][i], settings.ScorePerCaptureSuccess);
			}
			RpcAwardCapture(activePoint.PlayersInPoint[team].ToArray(), numPlayersInPoint[team]);

			CheckForWin();

			if (!gameOver)
			{
				ChangeCapturePoint(GetRandomNewCapturePoint(), team);
			}
		}
	}

	private int GetRandomNewCapturePoint()
	{
		while (true)
		{
			int i = unusedPoints[Random.Range(0, unusedPoints.Count)];
			if (capturePoints[i].Order != activePoint.Order)
			{
				return capturePoints[i].Order;
			}
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public override int GetPlayerScore(int player)
	{
		foreach (SiegePlayer p in players)
		{
			if (p.id == player)
			{
				return p.score;
			}
		}

		Debug.LogWarning("Siege.GetPlayerScore(" + player + "): Did not find player");
		return 0;
	}

	public override void SetPlayerScore(int player, int score)
	{
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i].id == player)
			{
				SiegePlayer p = players[i];
				p.score = score;
				players[i] = p;
				return;
			}
		}

		Debug.LogWarning("Siege.SetPlayerScore(" + player + ", " + score + "): Did not find player");
	}

	public override int GetTeamScore(int team)
	{
		return teams[team].score;
	}

	public override void SetTeamScore(int team, int score)
	{
		SiegeTeam st = teams[team];
		st.score = score;
		teams[team] = st;
	}
}

public struct SiegeTeam
{
	public int team;
	public int score;
}

public struct SiegePlayer
{
	public int id;
	public int team;
	public int score;
}