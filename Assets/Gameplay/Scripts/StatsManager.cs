using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class StatsManager : NetworkBehaviour
{
	private Dictionary<int, PlayerStats> players;

	private Scoreboard scoreboard;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Start()
	{
		players = new Dictionary<int, PlayerStats>();

		scoreboard = null;

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
			players[id].Kills++;
			players[id].Streak++;

			RpcSetKills(id, players[id].Kills);
		}
	}

	[Server]
	public void AddDeath(int id)
	{
		if (players.ContainsKey(id))
		{
			players[id].Deaths++;
			players[id].Streak = 0;

			RpcSetDeaths(id, players[id].Deaths);
		}
	}

	[Server]
	public void AddAssist(int id)
	{
		if (players.ContainsKey(id))
		{
			players[id].Assists++;

			RpcSetAssists(id, players[id].Assists);
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcSetKills(int id, int kills)
	{
		if (players.ContainsKey(id))
		{
			players[id].Kills = kills;
			Scoreboard.SetKills(id, kills);
			SetRatio(id);
		}
	}

	[ClientRpc]
	private void RpcSetDeaths(int id, int deaths)
	{
		if (players.ContainsKey(id))
		{
			players[id].Deaths = deaths;
			Scoreboard.SetDeaths(id, deaths);
			SetRatio(id);
		}
	}

	[ClientRpc]
	private void RpcSetAssists(int id, int assists)
	{
		if (players.ContainsKey(id))
		{
			players[id].Assists = assists;
			Scoreboard.SetAssists(id, assists);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void SetRatio(int id)
	{
		if (players[id].Deaths == 0)
		{
			Scoreboard.SetRatio(id, players[id].Kills);
		}
		else
		{
			Scoreboard.SetRatio(id, (float)players[id].Kills / players[id].Deaths);
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
	public int Kills;
	public int Deaths;
	public int Streak;
	public int Assists;

	public PlayerStats()
	{
		Kills = 0;
		Deaths = 0;
		Streak = 0;
		Assists = 0;
	}
}
