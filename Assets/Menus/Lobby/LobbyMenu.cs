using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyMenu : MenuState
{
	[SerializeField]
	private float partyMemberSeparationY;
	[SerializeField]
	private GameObject partyMemberEntryPrefab;
	[SerializeField]
	private Sprite[] mapThumbnails;
	[SerializeField]
	private string[] mapNames;

	private bool isHost;
	private List<PartyMemberEntry> partyMemberEntries;

	private PreviousGameStatsMenu previousGameStatsMenu;
	private RectTransform partyMemberList;
	private Text waitingForHostText;
	private Button startGameButton;
	private Text chatBoxText;
	private InputField chatBoxInputField;
	private ScrollRect chatBoxScroll;
	private Text numPlayersText;
	private Text gameModeText;
	private Text gameDescriptionText;
	private Text mapText;
	private Image mapThumbnail;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		previousGameStatsMenu = GetComponentInChildren<PreviousGameStatsMenu>();
		multiplayer = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
		partyMemberList = GameObject.Find("PartyMemberList").GetComponent<RectTransform>();
		waitingForHostText = GameObject.Find("WaitingForHostText").GetComponent<Text>();
		startGameButton = GameObject.Find("StartGameButton").GetComponent<Button>();
		chatBoxText = GameObject.Find("ChatBoxText").GetComponent<Text>();
		chatBoxInputField = GameObject.Find("ChatBoxInputField").GetComponent<InputField>();
		chatBoxScroll = GameObject.Find("ChatBoxPanel").GetComponentInChildren<ScrollRect>();
		numPlayersText = GameObject.Find("NumPlayersText").GetComponent<Text>();
		gameModeText = GameObject.Find("GameText").GetComponent<Text>();
		gameDescriptionText = GameObject.Find("GameDescriptionText").GetComponent<Text>();
		mapText = GameObject.Find("MapText").GetComponent<Text>();
		mapThumbnail = GameObject.Find("MapThumbnail").GetComponent<Image>();

		base.Awake();

		JP.Event.Register(this, "OnStartGameButtonClick");
		JP.Event.Register(this, "OnGameButtonClick");
		JP.Event.Register(this, "OnMapButtonClick");
		JP.Event.Register(this, "OnPreviousGameStatsButtonClick");
		JP.Event.Register(this, "OnLeaveButtonClick");

		partyMemberEntries = new List<PartyMemberEntry>();

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public override void Update()
	{
		base.Update();

		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && chatBoxInputField.text != "" && multiplayer.Party)
		{
			multiplayer.Party.SendChatMessage(chatBoxInputField.text);
			chatBoxInputField.text = "";
		}

		// Delete this shit later
		if (isHost)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				for (int i = 0; i < mapNames.Length; i++)
				{
					if (Input.GetKeyDown(KeyCode.Alpha1 + i))
					{
						multiplayer.Party.ServerChangeMap(i);
						break;
					}
				}
			}
			else
			{
				for (int i = 0; i < PartyManager.Players.Count; i++)
				{
					if (Input.GetKeyDown(KeyCode.Alpha1 + i))
					{
						PlayerData player = PartyManager.Players[i];
						player.Team = player.Team == 0 ? 1 : 0;
					}
				}
			}
		}

		// Delete this shit later
		if (PartyManager.Get)
		{
			for (int i = 0; i < PartyManager.Players.Count; i++)
			{
				PlayerData player = PartyManager.Players[i];
				foreach (PartyMemberEntry p in partyMemberEntries)
				{
					if (player.NetworkID == p.ID)
					{
						p.Team = player.Team;
						break;
					}
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.Lobby;
	}

	public override void StateBegin()
	{
		base.StateBegin();

		isHost = multiplayer.Party.isServer;

		foreach (PlayerData p in multiplayer.Players)
		{
			OnRegisterPartyMember(p);
		}

		if (isHost)
		{
			EnableStartGameButton(true);
		}
		else
		{
			waitingForHostText.enabled = true;
			waitingForHostText.text = "Waiting for host...";
		}

		OnChangeGameMode(PartyManager.GameSettings.MetaData.Name, PartyManager.GameSettings.MetaData.Description);
		OnChangeMap(PartyManager.Get.MapIndex);
	}

	public void OnRegisterPartyMember(PlayerData player)
	{
		foreach (PartyMemberEntry p in partyMemberEntries)
		{
			if (p.ID == player.NetworkID)
			{
				return;
			}
		}

		GameObject obj = Instantiate(partyMemberEntryPrefab);
		obj.transform.SetParent(partyMemberList, false);

		PartyMemberEntry entry = obj.GetComponent<PartyMemberEntry>();
		entry.ID = player.NetworkID;
		entry.Name = player.Name;

		int index = partyMemberEntries.Count;
		for (int i = 0; i < partyMemberEntries.Count; i++)
		{
			if (player.NetworkID < partyMemberEntries[i].ID)
			{
				index = i;
			}
		}
		partyMemberEntries.Insert(index, entry);

		OnPartyMembersChanged();
	}

	public void OnUnregisterPartyMember(PlayerData player)
	{
		foreach (PartyMemberEntry p in partyMemberEntries)
		{
			if (p.ID == player.NetworkID)
			{
				Destroy(p.gameObject);
				partyMemberEntries.Remove(p);
				break;
			}
		}

		OnPartyMembersChanged();
	}

	public void ShowChatMessage(string str)
	{
		if (chatBoxText.text.Length > 0)
		{
			str = "\n" + str;
		}

		chatBoxText.text += str;
		chatBoxScroll.verticalNormalizedPosition = 0.0f;
	}

	public void OnChangeGameMode(string name, string description)
	{
		gameModeText.text = name;
		gameDescriptionText.text = description;
	}

	public void OnChangeMap(int mapIndex)
	{
		mapText.text = mapNames[mapIndex];
		mapThumbnail.sprite = mapThumbnails[mapIndex];
	}

	public override void StateEnd()
	{
		base.StateEnd();

		foreach (PartyMemberEntry entry in partyMemberEntries)
		{
			Destroy(entry.gameObject);
		}
		partyMemberEntries.Clear();
	}

	/**********************************************************/
	// Button Callbacks

	public void OnStartGameButtonClick()
	{
		multiplayer.Party.ServerStartGame();
	}

	public void OnGameButtonClick()
	{
		multiplayer.Party.ServerChangeGameType();
	}

	public void OnMapButtonClick()
	{
	}

	public void OnPreviousGameStatsButtonClick()
	{
		previousGameStatsMenu.StateBegin();
	}

	public void OnLeaveButtonClick()
	{
		multiplayer.Party.ReturnToMainMenu();
	}

	/**********************************************************/
	// Helper Functions

	private void EnableStartGameButton(bool enable)
	{
		startGameButton.enabled = enable;
		startGameButton.GetComponent<Text>().enabled = enable;
	}

	private void OnPartyMembersChanged()
	{
		for (int i = 0; i < partyMemberEntries.Count; i++)
		{
			partyMemberEntries[i].transform.localPosition = Vector3.down * i * partyMemberSeparationY;
		}

		numPlayersText.text = partyMemberEntries.Count.ToString() + " Player" + (partyMemberEntries.Count > 1 ? "s" : "") + " (16 max)";
	}

	/**********************************************************/
	// Accessors/Mutators

	public string[] MapNames
	{
		get
		{
			return mapNames;
		}
	}
}
