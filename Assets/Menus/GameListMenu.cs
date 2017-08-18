using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class GameListMenu : MenuState
{
	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private float entrySeparation;

	private Transform listTransform;
	private List<GameListEntry> entries;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		listTransform = transform.Find("Panel/List");

		base.Awake();

		JP.Event.Register(this, "OnRefreshButtonClick");
		JP.Event.Register(this, "OnGameListBackButtonClick");

		entries = new List<GameListEntry>();
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.GameList;
	}

	public override void StateBegin()
	{
		base.StateBegin();

		FindMatches();
	}

	public override void StateEnd()
	{
		base.StateEnd();

		foreach (GameListEntry entry in entries)
		{
			Destroy(entry.gameObject);
		}
		entries.Clear();
	}

	public void OnMatchList(List<MatchInfoSnapshot> matchList)
	{
		foreach (MatchInfoSnapshot match in matchList)
		{
			GameObject obj = Instantiate(entryPrefab);
			obj.transform.SetParent(listTransform);
			obj.transform.localPosition = Vector3.down * entrySeparation * entries.Count;

			GameListEntry entry = obj.GetComponent<GameListEntry>();
			entry.MatchInfo = match;
			entries.Add(entry);
		}
	}

	public void OnEntryClick(GameListEntry entry)
	{
		multiplayer.JoinMatch(entry.MatchInfo);
	}

	/**********************************************************/
	// Button Callbacks

	private void OnRefreshButtonClick()
	{
		FindMatches();
	}

	private void OnGameListBackButtonClick()
	{
		mgr.GoToMenu(MenuType.Main);
	}

	/**********************************************************/
	// Helper Functions

	private void FindMatches()
	{
		foreach (GameListEntry entry in entries)
		{
			Destroy(entry.gameObject);
		}
		entries.Clear();

		multiplayer.ListMatches();
	}
}