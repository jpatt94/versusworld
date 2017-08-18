using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviour
{
	/**********************************************************/
	/**********************************************************/
	/**********************************************************/
	// NOTE: A lot of the team stuff here was fuzzy coded.
	/**********************************************************/
	/**********************************************************/
	/**********************************************************/

	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private GameObject teamEntryPrefab;
	[SerializeField]
	private float yGap;
	[SerializeField]
	private Color[] teamColors;

	private Dictionary<int, ScoreboardEntry> entries;
	private List<TeamScoreboardEntry> teams;
	private float alpha;

	private MultiplayerManager mgr;
	private Canvas canvas;
	private Image background;
	private Text score;
	private Text kills;
	private Text deaths;
	private Text ratio;
	private Text assists;
	private RectTransform entryPlaceholder;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		entries = new Dictionary<int, ScoreboardEntry>();
		teams = new List<TeamScoreboardEntry>();
		alpha = 0.0f;

		GameObject obj = GameObject.Find("MultiplayerManager");
		if (obj)
		{
			mgr = obj.GetComponent<MultiplayerManager>();
		}

		canvas = GetComponent<Canvas>();
		background = Utility.FindChild(gameObject, "Background").GetComponent<Image>();
		score = Utility.FindChild(gameObject, "ScoreLabel").GetComponent<Text>();
		kills = Utility.FindChild(gameObject, "KillsLabel").GetComponent<Text>();
		deaths = Utility.FindChild(gameObject, "DeathsLabel").GetComponent<Text>();
		ratio = Utility.FindChild(gameObject, "RatioLabel").GetComponent<Text>();
		assists = Utility.FindChild(gameObject, "AssistsLabel").GetComponent<Text>();
		entryPlaceholder = Utility.FindChild(gameObject, "EntryPlaceholder").GetComponent<RectTransform>();

		canvas.enabled = false;

		JP.Event.Register(this, "OnPartyMembersChanged");
	}

	public void Start()
	{
		if (mgr)
		{
			BuildEntryList();
		}
	}

	public void Update()
	{
		if (PlayerInput.ShowScoreboard(ButtonStatus.Down))
		{
			alpha = Mathf.Min(1.0f, alpha + Time.deltaTime * 5.0f);
			canvas.enabled = true;
		}
		else
		{
			alpha = Mathf.Max(0.0f, alpha - Time.deltaTime * 5.0f);
		}

		if (alpha <= 0.0f)
		{
			canvas.enabled = false;
		}
		else
		{
			SetAlpha(alpha);
		}

		if (mgr)
		{
			foreach (KeyValuePair<int, ScoreboardEntry> kv in entries)
			{
				PlayerData player = mgr.GetPlayerData(kv.Key);
				if (player)
				{
					kv.Value.SetPing(player.Ping);
				}
			}
		}
	}

	public void OnDestroy()
	{
		JP.Event.Unregister(this, "OnPartyMembersChanged");
	}

	/**********************************************************/
	// Interface

	public void OnPartyMembersChanged()
	{
		BuildEntryList();
	}

	public void AddEntry(int id, string userName)
	{
		bool prevEnabled = canvas.enabled;
		canvas.enabled = true;

		GameObject entry = Instantiate(entryPrefab);
		entry.transform.SetParent(entryPlaceholder.transform, false);
		RectTransform rect = entry.GetComponent<RectTransform>();
		rect.localPosition = Vector3.down * yGap * entries.Count;

		Utility.FindChild(entry, "Name").GetComponent<Text>().text = userName;

		entries[id] = entry.GetComponent<ScoreboardEntry>();

		canvas.enabled = prevEnabled;
	}

	public void SetScore(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].Score = value;
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetScore(id, value);
		}
	}

	public void SetKills(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetKills(value);
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetKills(id, value);
		}
	}

	public void SetDeaths(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetDeaths(value);
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetDeaths(id, value);
		}
	}

	public void SetRatio(int id, float value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetRatio(value);
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetRatio(id, value);
		}
	}

	public void SetAssists(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetAssists(value);
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetAssists(id, value);
		}
	}

	public void SetPing(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetPing(value);
		}
		foreach (TeamScoreboardEntry team in teams)
		{
			team.SetPing(id, value);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void BuildEntryList()
	{
		bool prevEnabled = canvas.enabled;
		canvas.enabled = true;

		if (PartyManager.GameSettings.Generic.Teams)
		{
			foreach (PlayerData p in mgr.Players)
			{
				bool teamExists = false;
				foreach (TeamScoreboardEntry team in teams)
				{
					if (p.Team == team.Team)
					{
						teamExists = true;
						break;
					}
				}
				if (!teamExists)
				{
					GameObject obj = Instantiate(teamEntryPrefab);
					obj.transform.SetParent(entryPlaceholder.transform, false);
					TeamScoreboardEntry team = obj.GetComponent<TeamScoreboardEntry>();
					team.Team = p.Team;
					team.Color = teamColors[p.Team];
					team.YGap = yGap;
					teams.Add(team);
				}
			}

			foreach (TeamScoreboardEntry team in teams)
			{
				team.ClearPlayers();
			}

			foreach (PlayerData p in mgr.Players)
			{
				teams[p.Team].AddPlayer(p.NetworkID);
			}

			float offset = 0.0f;
			foreach (TeamScoreboardEntry team in teams)
			{
				team.BuildList();
				team.transform.localPosition = Vector3.down * offset;

				offset += team.Size;
			}
		}
		else
		{
			foreach (PlayerData p in mgr.Players)
			{
				AddEntry(p.NetworkID, p.Name);
			}
		}

		canvas.enabled = prevEnabled;
	}

	private void SetAlpha(float a)
	{
		Utility.SetAlpha(background, 0.6f * a);
		Utility.SetAlpha(score, a);
		Utility.SetAlpha(kills, a);
		Utility.SetAlpha(deaths, a);
		Utility.SetAlpha(ratio, a);
		Utility.SetAlpha(assists, a);

		foreach (KeyValuePair<int, ScoreboardEntry> entry in entries)
		{
			entry.Value.SetAlpha(a);
		}

		foreach (TeamScoreboardEntry team in teams)
		{
			team.Alpha = a;
		}
	}
}
