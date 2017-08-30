using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MedalTracker : MonoBehaviour
{
	[SerializeField]
	private float multiKillTime;

	private Dictionary<int, PlayerMedalData> players;

	private bool awardedFirstBlood;

	private StatsManager stats;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		players = new Dictionary<int, PlayerMedalData>();

		stats = FindObjectOfType<StatsManager>(); // Should only be null when offline
	}

	public void Update()
	{
		foreach (KeyValuePair<int, PlayerMedalData> player in players)
		{
			player.Value.multiKillTime -= Time.deltaTime;
			if (player.Value.multiKillTime <= 0.0f)
			{
				player.Value.multiKills = 0;
			}
		}
	}

	/**********************************************************/
	// Interface

	public static string GetMedalName(MedalType type)
	{
		switch (type)
		{
			case MedalType.DoubleKill: return "Double Kill";
			case MedalType.TripleKill: return "Triple Kill";
			case MedalType.QuadraKill: return "Quadra Kill";
			case MedalType.PentaKill: return "Pentakill";
			case MedalType.HexaKill: return "Hexakill";
			case MedalType.SeptaKill: return "Septakill";
			case MedalType.OctaKill: return "Octakill";
			case MedalType.NonaKill: return "Nonakill";
			case MedalType.DecaKill: return "Decakill";
			case MedalType.KillingSpree5: return "Killing Spree";
			case MedalType.KillingSpree10: return "Killing Spree (10)";
			case MedalType.KillingSpree15: return "Killing Spree (15)";
			case MedalType.KillingSpree20: return "Killing Spree (20)";
			case MedalType.KillingSpree25: return "Killing Spree (25)";
			case MedalType.KillingSpree30: return "Killing Spree (30)";
			case MedalType.BuzzKill: return "Buzz Kill";
			case MedalType.PostMortemKill: return "Post-Mortem Kill";
			case MedalType.DirectRocketHit: return "Direct Hit";
			case MedalType.StickyKill: return "Stuck!";
			case MedalType.Scumped: return "Scumped!";
			case MedalType.SYKSYL: return "S.Y.K.S.Y.L";
			case MedalType.FirstBlood: return "First Blood";
		}

		return "N/A";
	}

	public void AddPlayer(int id)
	{
		PlayerMedalData player = new PlayerMedalData();
		player.net = PlayerManager.GetPlayer(id);
		players[id] = player;
	}

	public int[] OnKill(KillData data)
	{
		List<int> awardedMedals = new List<int>();

		if (data.shooter > -1)
		{
			PlayerStats shooterStats = stats.GetPlayerStats(data.shooter);
			PlayerStats victimStats = stats.GetPlayerStats(data.victim);
			PlayerMedalData shooterData = players[data.shooter];
			PlayerMedalData victimData = players[data.victim];

			if (data.shooter != data.victim && !PartyManager.SameTeam(data.shooter, data.victim))
			{
				int streak = shooterStats.Streak;
				if (streak > 0 && streak % 5 == 0 && streak <= 30)
				{
					awardedMedals.Add((int)(MedalType.KillingSpree5 + (streak / 5) - 1));
				}

				if (victimData.previousStreak >= 5)
				{
					awardedMedals.Add((int)MedalType.BuzzKill);
				}

				if (shooterData.net.Respawner.enabled)
				{
					awardedMedals.Add((int)MedalType.PostMortemKill);
				}

				shooterData.multiKills++;
				shooterData.multiKillTime = multiKillTime;
				if (shooterData.multiKills >= 2 && shooterData.multiKills <= 10)
				{
					awardedMedals.Add((int)(MedalType.DoubleKill + (shooterData.multiKills - 2)));
				}

				if (!awardedFirstBlood)
				{
					awardedMedals.Add((int)MedalType.FirstBlood);
					awardedFirstBlood = true;
				}
			}

			shooterData.previousStreak = shooterStats.Streak;
			victimData.previousStreak = victimStats.Streak;
		}

		return awardedMedals.ToArray();
	}

	public void AwardMedal(int player, MedalType type)
	{
		PlayerManager.GetPlayer(player).RpcAwardMedal(type);
	}
}

public class PlayerMedalData
{
	public NetworkPlayer net;
	public int previousStreak;
	public int multiKills;
	public float multiKillTime;
}

public enum MedalType
{
	DoubleKill,
	TripleKill,
	QuadraKill,
	PentaKill,
	HexaKill,
	SeptaKill,
	OctaKill,
	NonaKill,
	DecaKill,
	KillingSpree5,
	KillingSpree10,
	KillingSpree15,
	KillingSpree20,
	KillingSpree25,
	KillingSpree30,
	BuzzKill,
	PostMortemKill,
	DirectRocketHit,
	StickyKill,
	Scumped,
	SYKSYL,
	FirstBlood,
	NumMedals,
}