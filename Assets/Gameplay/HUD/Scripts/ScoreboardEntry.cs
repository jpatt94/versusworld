using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreboardEntry : MonoBehaviour
{
	private int score;
	private int team;

	private Image background;
	private Text userName;
	private Text scoreText;
	private Text kills;
	private Text deaths;
	private Text ratio;
	private Text assists;
	private Text ping;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		background = GetComponent<Image>();
		userName = Utility.FindChild(gameObject, "Name").GetComponent<Text>();
		scoreText = Utility.FindChild(gameObject, "Score").GetComponent<Text>();
		kills = Utility.FindChild(gameObject, "Kills").GetComponent<Text>();
		deaths = Utility.FindChild(gameObject, "Deaths").GetComponent<Text>();
		ratio = Utility.FindChild(gameObject, "Ratio").GetComponent<Text>();
		assists = Utility.FindChild(gameObject, "Assists").GetComponent<Text>();
		ping = Utility.FindChild(gameObject, "Ping").GetComponent<Text>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public string Name
	{
		get
		{
			return userName.text;
		}
		set
		{
			userName.text = value;
		}
	}

	public int Score
	{
		get
		{
			return score;
		}
		set
		{
			score = value;
			scoreText.text = value.ToString();
		}
	}

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

	public void SetColor(Color c)
	{
		Utility.SetRGB(background, c);
	}

	public void SetAlpha(float a)
	{
		Utility.SetAlpha(background, a);
		Utility.SetAlpha(userName, a);
		Utility.SetAlpha(scoreText, a);
		Utility.SetAlpha(kills, a);
		Utility.SetAlpha(deaths, a);
		Utility.SetAlpha(ratio, a);
		Utility.SetAlpha(assists, a);
	}

	public void SetKills(int value)
	{
		kills.text = value.ToString();
	}

	public void SetDeaths(int value)
	{
		deaths.text = value.ToString();
	}

	public void SetRatio(float value)
	{
		ratio.text = value.ToString("0.00");
	}

	public void SetAssists(int value)
	{
		assists.text = value.ToString();
	}

	public void SetPing(int value)
	{
		ping.text = value.ToString();
	}
}
