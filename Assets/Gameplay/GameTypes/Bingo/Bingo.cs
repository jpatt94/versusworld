using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bingo : Game
{

	public class BingoPlayersList : SyncListStruct<BingoPlayer> { }
	private BingoPlayersList players = new BingoPlayersList();
	public class BingoTeamsList : SyncListStruct<BingoTeam> { }
	private BingoTeamsList teams = new BingoTeamsList();

	private List<BingoTask> tasks;

	private BingoSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		settings = PartyManager.GameSettings as BingoSettings;
	}

	public override void Start()
	{
		base.Start();

		players.Callback = OnPlayersChanged;
		teams.Callback = OnTeamsChanged;
	}

	/**********************************************************/
	// Interface

	public override GameType GetGameType()
	{
		return GameType.Bingo;
	}

	public override void PopulatePlayers()
	{
		base.PopulatePlayers();

		foreach (PlayerData p in PartyManager.Players)
		{
			BingoPlayer bp = new BingoPlayer();
			bp.id = p.NetworkID;
			bp.taskProgress = new int[24]; 
			players.Add(bp);
		}

		dirtyHUD = true;
	}

	/**********************************************************/
	// Client RPCs


	/**********************************************************/
	// Callbacks

	private void OnPlayersChanged(SyncListStruct<BingoPlayer>.Operation op, int index)
	{
		dirtyHUD = true;
	}

	private void OnTeamsChanged(SyncListStruct<BingoTeam>.Operation op, int index)
	{
		dirtyHUD = true;
	}

	/**********************************************************/
	// Accessors/Mutators

}

public struct BingoTeam
{
	public int team;
	public int[] taskProgress;
}

public struct BingoPlayer
{
	public int id;
	public int[] taskProgress;
}
