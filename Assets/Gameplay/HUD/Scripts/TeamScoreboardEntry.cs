using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScoreboardEntry : MonoBehaviour
{
	[SerializeField]
	private float headerGap;
	[SerializeField]
	private float teamGap;
	[SerializeField]
	private GameObject entryPrefab;

	private int team;
	private float yGap;
	private Color color;
	private List<int> players;
	private Dictionary<int, ScoreboardEntry> entries;

	private Text headerText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		players = new List<int>();
		entries = new Dictionary<int, ScoreboardEntry>();

		headerText = transform.Find("HeaderText").GetComponent<Text>();
	}

	/**********************************************************/
	// Interface

	public void ClearPlayers()
	{
		players.Clear();
	}

	public void AddPlayer(int player)
	{
		players.Add(player);
	}

	public void BuildList()
	{
		foreach (int player in players)
		{
			if (!entries.ContainsKey(player))
			{
				GameObject obj = Instantiate(entryPrefab);
				obj.transform.SetParent(transform, false);

				ScoreboardEntry entry = obj.GetComponent<ScoreboardEntry>();
				entry.Name = PartyManager.GetPlayer(player).Name;
				entry.SetColor(color);
				entries[player] = entry;
			}
		}

		Sort();
	}

	/**********************************************************/
	// Accessors/Mutators

	private void Sort()
	{
		int i = 0;
		foreach (KeyValuePair<int, ScoreboardEntry> s in entries)
		{
			s.Value.transform.localPosition = Vector3.down * (headerGap + yGap * i);
			i++;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int Team
	{
		get
		{
			return team;
		}
		set
		{
			team = value;
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			headerText.color = value;
		}
	}

	public float Alpha
	{
		set
		{
			Utility.SetAlpha(headerText, value);
			foreach (KeyValuePair<int, ScoreboardEntry> entry in entries)
			{
				entry.Value.SetAlpha(value);
			}
		}
	}

	public float YGap
	{
		get
		{
			return yGap;
		}
		set
		{
			yGap = value;
		}
	}

	public void SetScore(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].Score = value;
			Sort();
		}
	}

	public void SetKills(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetKills(value);
		}
	}

	public void SetDeaths(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetDeaths(value);
		}
	}

	public void SetRatio(int id, float value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetRatio(value);
		}
	}

	public void SetAssists(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetAssists(value);
		}
	}

	public void SetPing(int id, int value)
	{
		if (entries.ContainsKey(id))
		{
			entries[id].SetPing(value);
		}
	}

	public float Size
	{
		get
		{
			return entries.Count * yGap + headerGap + teamGap;
		}
	}
}