using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Deathmatch : Game
{
	public class DeathmatchPlayersList : SyncListStruct<DeathmatchPlayer> { }
	private DeathmatchPlayersList players = new DeathmatchPlayersList();

	private DeathmatchSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		settings = PartyManager.GameSettings as DeathmatchSettings;
	}

	public override void Start()
	{
		base.Start();

		players.Callback = OnPlayersChanged;
	}

	public override void Update()
	{
		base.Update();
	}

	/**********************************************************/
	// Interface

	public override GameType GetGameType()
	{
		return GameType.Deathmatch;
	}

	public override void PopulatePlayers()
	{
		base.PopulatePlayers();

		foreach (PlayerData p in PartyManager.Players)
		{
			DeathmatchPlayer dm = new DeathmatchPlayer();
			dm.id = p.NetworkID;
			dm.team = p.Team;
			dm.score = 0;
			players.Add(dm);
		}

		dirtyHUD = true;
	}

	/**********************************************************/
	// Helper Functions

	private void OnPlayersChanged(SyncListStruct<DeathmatchPlayer>.Operation op, int index)
	{
		dirtyHUD = true;
	}

	/**********************************************************/
	// Accessors/Mutators

	public override int GetPlayerScore(int player)
	{
		foreach (DeathmatchPlayer p in players)
		{
			if (p.id == player)
			{
				return p.score;
			}
		}

		Debug.LogWarning("Deathmatch.GetPlayerScore(" + player + "): Did not find player");
		return 0;
	}

	public override void SetPlayerScore(int player, int score)
	{
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i].id == player)
			{
				DeathmatchPlayer p = players[i];
				p.score = score;
				players[i] = p;
				return;
			}
		}

		Debug.LogWarning("Deathmatch.SetPlayerScore(" + player + ", " + score + "): Did not find player");
	}

	public override int GetTeamScore(int team)
	{
		int score = 0;

		foreach (DeathmatchPlayer p in players)
		{
			if (p.team == team)
			{
				score += p.score;
			}
		}

		return score;
	}
}

public struct DeathmatchPlayer
{
	public int id;
	public int team;
	public int score;
}