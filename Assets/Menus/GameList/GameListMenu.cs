using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class GameListMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject matchEntryPrefab;
	[SerializeField]
	private float matchEntrySeparation;

	private MultiplayerManager mgr;
	private LegacyMainMenu menus;
	private Button refreshButton;
	private Text findingGamesText;

	/**********************************************************/
	// MonoBehaviour Interface

	private void Awake()
	{
		mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
		menus = GetComponentInParent<LegacyMainMenu>();
		refreshButton = transform.Find("UI/RefreshButton").GetComponent<Button>();
		findingGamesText = transform.Find("UI/FindingGamesText").GetComponent<Text>();
	}

	/**********************************************************/
	// Interface

	public void OnRefreshClick()
	{
		mgr.ListMatches();
	}

	public void OnGoBackClick()
	{
		menus.TransitionToState(MenuType.Main);
	}

	public void OnMatchList(List<MatchInfoSnapshot> matchList)
	{

	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = value;
			}
			foreach (CanvasRenderer r in GetComponentsInChildren<CanvasRenderer>())
			{
				r.cull = !value;
			}
		}
	}
}