using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberEntry : MonoBehaviour
{
	[SerializeField]
	private Color defaultColor;
	[SerializeField]
	private List<Sprite> testPlayerIcons;

	private int id;
	private int team;

	private Image backgroundPanel;
	private Text nameText;
	private Image iconImage;
	private Text pingText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		backgroundPanel = GetComponent<Image>();
		nameText = transform.Find("NameText").GetComponent<Text>();
		iconImage = transform.Find("PlayerIcon").GetComponent<Image>();
		pingText = transform.Find("PingText").GetComponent<Text>();
	}

	public void Update()
	{
		Color newColor = defaultColor;
		if (PartyManager.GameSettings.Generic.Teams)
		{
			newColor = PartyManager.GetTeamColor(team);
		}
		backgroundPanel.color = new Color(newColor.r, newColor.g, newColor.b, backgroundPanel.color.a);

		PlayerData player = PartyManager.GetPlayer(id);
		if (player)
		{
			pingText.text = player.Ping.ToString();
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;

			iconImage.sprite = testPlayerIcons[id % testPlayerIcons.Count];
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

	public string Name
	{
		get
		{
			return nameText.text;
		}
		set
		{
			nameText.text = value;
		}
	}
}
