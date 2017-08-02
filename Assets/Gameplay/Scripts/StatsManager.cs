using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class StatsManager : NetworkBehaviour
{
	private Dictionary<int, PlayerStats> players;

	private Scoreboard scoreboard;
	private MultiplayerManager mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Start()
	{
		players = new Dictionary<int, PlayerStats>();

		scoreboard = null;
		mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();

		DontDestroyOnLoad(gameObject);
	}

	/**********************************************************/
	// Interface

	public void RegisterPlayer(int id)
	{
		players[id] = new PlayerStats();
	}

	[Server]
	public void OnKill(KillData kill)
	{
		if (kill.shooter > -1 && kill.shooter != kill.victim && !PartyManager.SameTeam(kill.shooter, kill.victim))
		{
			AddKill(kill.shooter);
		}
		AddDeath(kill.victim);

		foreach (int id in kill.assisters)
		{
			AddAssist(id);
		}
	}

	[Server]
	public void AddKill(int id)
	{
		if (players.ContainsKey(id))
		{
			players[id].kills++;
			players[id].streak++;

			RpcSetKills(id, players[id].kills);
		}
	}

	[Server]
	public void AddDeath(int id)
	{
		if (players.ContainsKey(id))
		{
			players[id].deaths++;
			players[id].streak = 0;

			RpcSetDeaths(id, players[id].deaths);
		}
	}

	[Server]
	public void AddAssist(int id)
	{
		if (players.ContainsKey(id))
		{
			players[id].assists++;

			RpcSetAssists(id, players[id].assists);
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcSetKills(int id, int kills)
	{
		if (players.ContainsKey(id))
		{
			players[id].kills = kills;
			Scoreboard.SetKills(id, kills);
			SetRatio(id);
		}
	}

	[ClientRpc]
	private void RpcSetDeaths(int id, int deaths)
	{
		if (players.ContainsKey(id))
		{
			players[id].deaths = deaths;
			Scoreboard.SetDeaths(id, deaths);
			SetRatio(id);
		}
	}

	[ClientRpc]
	private void RpcSetAssists(int id, int assists)
	{
		if (players.ContainsKey(id))
		{
			players[id].assists = assists;
			Scoreboard.SetAssists(id, assists);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void SetRatio(int id)
	{
		if (players[id].deaths == 0)
		{
			Scoreboard.SetRatio(id, players[id].kills);
		}
		else
		{
			Scoreboard.SetRatio(id, (float)players[id].kills / players[id].deaths);
		}
	}

	/**********************************************************/
	// Accessors

	public PlayerStats GetPlayerStats(int id)
	{
		return players[id];
	}

	public Scoreboard Scoreboard
	{
		get
		{
			if (scoreboard)
			{
				return scoreboard;
			}
			GameObject obj = GameObject.Find("Scoreboard");
			if (obj)
			{
				scoreboard = obj.GetComponent<Scoreboard>();
				return scoreboard;
			}
			return null;
		}
	}
}

public class PlayerStats
{
	public int kills;
	public int deaths;
	public int streak;
	public int assists;

	public PlayerStats()
	{
		kills = 0;
		deaths = 0;
		streak = 0;
	}
}
