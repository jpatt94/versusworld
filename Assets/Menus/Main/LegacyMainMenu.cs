using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LegacyMainMenu : MonoBehaviour
{
	[SerializeField]
	private float transitionDuration;
	[SerializeField]
	private AnimationCurve transitionCurve;

	private MenuType state;

	private MultiplayerManager mgr;
	private InputField nameInputField;
	private InputField ipInputField;
	private Text infoText;
	private PlayerCustomizer customizer;
	private NotificationPanel notificationPanel;
	private Text accountNameText;
	private GameListMenu gameListMenu;
	private Text randomMessageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		state = MenuType.Main;

		mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
		nameInputField = GameObject.Find("NameInputField").GetComponent<InputField>();
		ipInputField = GameObject.Find("IPInputField").GetComponent<InputField>();
		customizer = GameObject.Find("PlayerCustomizer").GetComponent<PlayerCustomizer>();
		notificationPanel = GameObject.Find("NotificationCanvas").GetComponent<NotificationPanel>();
		accountNameText = Utility.FindChild(gameObject, "AccountNameText").GetComponent<Text>();
		gameListMenu = GetComponentInChildren<GameListMenu>();

		ipInputField.text = "localhost";

		if (mgr.LocalUser != null)
		{
			nameInputField.text = mgr.LocalUser.userName;
			accountNameText.text = "Logged in as <b>" + mgr.LocalUser.userName + "</b>";
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		JP.Event.Register(this, "OnEnterLobby");

		if (PartyManager.Get && (PartyManager.Get.Status == PartyStatus.ReturningToLobby || PartyManager.Get.Status == PartyStatus.InLobby))
		{
			TransitionToState(MenuType.Lobby);
		}
	}

	public void Update()
	{
		if (mgr.NeedsDisconnectNotification)
		{
			if (mgr.StatusOnDisconnect == MultiplayerStatus.CreatingGame)
			{
				notificationPanel.Show("Failed to create game", true);
			}
			else if (mgr.StatusOnDisconnect == MultiplayerStatus.HostingLocalGame)
			{
				notificationPanel.Show("Failed to host local game", true);
			}
			else if (mgr.StatusOnDisconnect == MultiplayerStatus.JoiningGame)
			{
				notificationPanel.Show("Failed to join game", true);
			}
			else if (mgr.StatusOnDisconnect == MultiplayerStatus.JoiningLocalGame)
			{
				notificationPanel.Show("Failed to join local game", true);
			}
			else if (mgr.StatusOnDisconnect == MultiplayerStatus.Connected)
			{
				switch (mgr.PartyRejectionReason)
				{
					case PartyRejectionReason.None: notificationPanel.Show("Lost connection to server", true); break;
					case PartyRejectionReason.GameInProgress: notificationPanel.Show("Can't join game in progress", true); break;
					case PartyRejectionReason.NoRoomInLobby: notificationPanel.Show("Room in already full", true); break;
				}
			}

			mgr.NeedsDisconnectNotification = false;
		}

		

		if (Application.isEditor)
		{
			//if (Input.GetKeyDown(KeyCode.H))
			//{
			//	OnHostClick();
			//}
			//if (Input.GetKeyDown(KeyCode.J))
			//{
			//	OnJoinClick();
			//}
		}
	}

	public void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public void TransitionToState(MenuType state)
	{
		this.state = state;
	}

	public void OnHostClick()
	{
		notificationPanel.Show("Hosting local game...", false);

		mgr.Status = MultiplayerStatus.HostingLocalGame;
		mgr.StartLocalHost(mgr.LocalUser != null ? mgr.LocalUser.userName : nameInputField.text, customizer.Options);
	}

	public void OnJoinClick()
	{
		notificationPanel.Show("Joining local game...", false);

		mgr.Status = MultiplayerStatus.JoiningLocalGame;
		mgr.networkAddress = ipInputField.text;
		mgr.StartLocalClient(nameInputField.text, customizer.Options);
	}

	public void OnEnterLobby()
	{
		TransitionToState(MenuType.Lobby);
	}

	public void OnCustomizeClick()
	{
		TransitionToState(MenuType.Customize);
	}

	public void OnCustomizeGoBackClick()
	{
		TransitionToState(MenuType.Main);
	}

	public void OnSettingsClick()
	{
		TransitionToState(MenuType.Settings);
	}

	public void OnCreateGameClick()
	{
		notificationPanel.Show("Creating game...", false);

		mgr.Status = MultiplayerStatus.CreatingGame;
		mgr.CreateMatch();
	}

	public void OnFindGameClick()
	{
		//mgr.Status = MultiplayerStatus.FindingGame;
		////mgr.MatchMaker.GetGameList();
		//
		//TransitionToState(MenuType.GameList);

		mgr.JoinFirstMatch();
	}

	public void OnLogOutClick()
	{
		mgr.OnLogOut();

		//TransitionToState(MenuType.LogIn);
	}

	public void OnLogIn(string userName, int id)
	{
		nameInputField.text = userName;
		accountNameText.text = "Logged in as <b>" + userName + "</b>";

		mgr.OnLogIn(userName, id);

		TransitionToState(MenuType.Main);
	}

	public void OnExitClick()
	{
		Application.Quit();
	}

	/**********************************************************/
	// Accessors/Mutators

	public MenuType State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public string Name
	{
		get
		{
			return nameInputField.text;
		}
	}

	public string IP
	{
		get
		{
			return ipInputField.text;
		}
	}

	public PlayerCustomizationOptions CustomizationOptions
	{
		get
		{
			return customizer.Options;
		}
	}

	public NotificationPanel NotificationPanel
	{
		get
		{
			return notificationPanel;
		}
	}

	public MultiplayerManager Manager
	{
		get
		{
			return mgr;
		}
	}

	public GameListMenu GameListMenu
	{
		get
		{
			return gameListMenu;
		}
	}
}