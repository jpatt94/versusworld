using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class GameListEntry : MonoBehaviour
{
	private MatchInfoSnapshot matchInfo;

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
			GetComponent<Text>().text = matchInfo.name.Split('|')[0];
		}
	}
}