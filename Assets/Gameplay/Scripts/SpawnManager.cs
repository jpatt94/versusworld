using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	private bool requiresInitialSpawn;
	private List<PendingSpawn> pending;

	private List<SpawnPoint> spawnPoints;
	private MultiplayerManager mgr;
	private PartyManager party;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mgr = FindObjectOfType<MultiplayerManager>();
		party = mgr.Party;

		requiresInitialSpawn = true;

		spawnPoints = new List<SpawnPoint>();
		GameObject[] objs = GameObject.FindGameObjectsWithTag("SpawnPoint");
		foreach (GameObject o in objs)
		{
			spawnPoints.Add(o.GetComponent<SpawnPoint>());
		}

		pending = new List<PendingSpawn>();
	}

	public void Update()
	{
		if (party.isClient)
		{
			if (requiresInitialSpawn && party.ServerSceneIsReady)
			{
				mgr.GetLocalPlayerData().SpawnPlayer();
				requiresInitialSpawn = false;
			}
		}
	}

	/**********************************************************/
	// Interface

	public SpawnPoint InitialSpawn(int player)
	{
		PlayerData playerData = PartyManager.GetPlayer(player);

		int order = -1;
		for (int i = 0; i < PartyManager.Players.Count; i++)
		{
			if (!PartyManager.GameSettings.Generic.Teams || PartyManager.SameTeam(PartyManager.Players[i].NetworkID, player))
			{
				order++;
			}
			if (PartyManager.Players[i].NetworkID == player)
			{
				break;
			}
		}

		if (order < 0)
		{
			Debug.LogError("SpawnManager.InitialSpawn(" + player + "): Couldn't find player in list");
			return null;
		}

		foreach (SpawnPoint s in spawnPoints)
		{
			if ((!PartyManager.GameSettings.Generic.Teams && s.InitialSpawnIndex == order) ||
				(PartyManager.GameSettings.Generic.Teams && playerData.Team == s.TeamIndex && s.InitialSpawnTeamIndex == order))
			{
				pending.Add(new PendingSpawn(player, s));
				return s;
			}
		}

		Debug.LogWarning("Not enough initial spawns set up");

		SpawnPoint bestSpawn = GetBestSpawnPoint(player);
		pending.Add(new PendingSpawn(player, bestSpawn));
		return bestSpawn;
	}

	public SpawnPoint StartRespawn(NetworkPlayer player)
	{
		SpawnPoint bestSpawn = GetBestSpawnPoint(player.ID);
		pending.Add(new PendingSpawn(player.ID, bestSpawn));
		return bestSpawn;
	}

	public void OnSpawn(NetworkPlayer player)
	{
		for (int i = 0; i < pending.Count; i++)
		{
			if (player.ID == pending[i].id)
			{
				pending.RemoveAt(i);
				break;
			}
		}
	}

	public void OnRespawn(NetworkPlayer player)
	{
		OnSpawn(player);
	}

	/**********************************************************/
	// Helper Fucntions

	private SpawnPoint GetBestSpawnPoint(int player)
	{
		NetworkPlayer spawningPlayer = PlayerManager.GetPlayer(player);

		float furthest = float.MinValue;
		int index = 0;
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			float closestEnemy = float.MaxValue;
			foreach (NetworkPlayer p in PlayerManager.PlayerList)
			{
				if (!PartyManager.SameTeam(player, p.ID))
				{
					closestEnemy = Mathf.Min(closestEnemy, (spawnPoints[i].Top - p.Cam.transform.position).magnitude);
				}
			}

			foreach (PendingSpawn p in pending)
			{
				if (!PartyManager.SameTeam(player, p.id))
				{
					closestEnemy = Mathf.Min(closestEnemy, (spawnPoints[i].Top - p.spawnPoint.Top).magnitude);
				}
			}

			if (closestEnemy > furthest)
			{
				furthest = closestEnemy;
				index = i;
			}
		}

		return spawnPoints[index];
	}
}

public struct PendingSpawn
{
	public int id;
	public SpawnPoint spawnPoint;

	public PendingSpawn(int id, SpawnPoint spawnPoint)
	{
		this.id = id;
		this.spawnPoint = spawnPoint;
	}
}