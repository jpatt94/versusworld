using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class GameListEntry : MonoBehaviour
{
	private MatchInfoSnapshot matchInfo;
	private string matchName;

	private GameListMenu menu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		menu = FindObjectOfType<GameListMenu>();
	}

	/**********************************************************/
	// Interface

	public void OnClick()
	{
		menu.OnEntryClick(this);
	}

	/**********************************************************/
	// Accessors/Mutators

	public MatchInfoSnapshot MatchInfo
	{
		get
		{
			return matchInfo;
		}
		set
		{
			matchInfo = value;
			matchName = matchInfo.name.Split('|')[0];
			transform.Find("NameButton").GetComponent<Text>().text = matchName;
		}
	}

	public string MatchName
	{
		get
		{
			return matchName;
		}
	}
}