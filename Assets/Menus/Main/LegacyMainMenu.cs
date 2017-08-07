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
	private LogInMenu logInMenu;
	private SettingsMenu settingsMenu;
	private GameListMenu gameListMenu;
	private Camera cam;
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
		logInMenu = GameObject.Find("LogInMenu").GetComponent<LogInMenu>();
		settingsMenu = GetComponentInChildren<SettingsMenu>();
		gameListMenu = GetComponentInChildren<GameListMenu>();
		cam = GameObject.Find("Camera").GetComponent<Camera>();

		nameInputField.text = GetRandomName();
		ipInputField.text = "localhost";

		randomMessageText = GameObject.Find("RandomMessageText").GetComponent<Text>();
		randomMessageText.text = GetRandomMessage();

		mgr.Menu = this;
		if (mgr.LocalUser != null)
		{
			nameInputField.text = mgr.LocalUser.userName;
			accountNameText.text = "Logged in as <b>" + mgr.LocalUser.userName + "</b>";
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		JP.Event.Register(this, "OnRandomizeClick");
		JP.Event.Register(this, "OnNextFaceClick");
		JP.Event.Register(this, "OnNextHatClick");
		JP.Event.Register(this, "OnNextSkinClick");
		JP.Event.Register(this, "OnNextShirtClick");
		JP.Event.Register(this, "OnNextPantsClick");
		JP.Event.Register(this, "OnNextShoesClick");
		JP.Event.Register(this, "OnPrevFaceClick");
		JP.Event.Register(this, "OnPrevHatClick");
		JP.Event.Register(this, "OnPrevSkinClick");
		JP.Event.Register(this, "OnPrevShirtClick");
		JP.Event.Register(this, "OnPrevPantsClick");
		JP.Event.Register(this, "OnPrevShoesClick");
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

		if (Input.GetKeyDown(KeyCode.M))
		{
			randomMessageText.text = GetRandomMessage();
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
		mgr.Menu = null;

		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public void TransitionToState(MenuType state)
	{
		MenuType prevState = this.state;

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

	public void OnGenerateRandomNameClick()
	{
		nameInputField.text = GetRandomName();
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

	public void OnRandomizeClick()
	{
		customizer.Randomize();
	}

	public void OnNextFaceClick()
	{
		customizer.NextFace();
	}

	public void OnPrevFaceClick()
	{
		customizer.PrevFace();
	}

	public void OnNextSkinClick()
	{
		customizer.NextSkin();
	}

	public void OnPrevSkinClick()
	{
		customizer.PrevSkin();
	}

	public void OnNextHairClick()
	{
		customizer.NextHair();
	}

	public void OnPrevHairClick()
	{
		customizer.PrevHair();
	}

	public void OnNextShirtClick()
	{
		customizer.NextShirt();
	}

	public void OnPrevShirtClick()
	{
		customizer.PrevShirt();
	}

	public void OnNextPantsClick()
	{
		customizer.NextPants();
	}

	public void OnPrevPantsClick()
	{
		customizer.PrevPants();
	}

	public void OnNextHatClick()
	{
	}

	public void OnPrevHatClick()
	{
	}

	public void OnNextShoesClick()
	{
		customizer.NextShoes();
	}

	public void OnPrevShoesClick()
	{
		customizer.PrevShoes();
	}

	/**********************************************************/
	// Helper Functions

	private string GetRandomName()
	{
		string[] cons =
		{
			"qu", "w", "r", "t", "y", "p", "s", "d", "f", "g", "h", "j", "k", "l", "z", "c", "v", "b", "n", "m",
			"tr", "th", "pr", "sh", "st", "gh", "fr", "dr", "kl", "zh", "cr", "ch", "br",
		};

		string[] vowels =
		{
			"a", "e", "i", "o", "u", "ou", "oo", "ue", "ee", "ea", "io", "ie", "ei",
		};

		string name = "";

		bool vowel = Random.Range(0, 2) == 0;
		int length = Random.Range(3, 6);
		for (int i = 0; i < length; i++)
		{
			if (vowel)
			{
				name += vowels[Random.Range(0, vowels.Length)];
			}
			else
			{
				name += cons[Random.Range(0, cons.Length)];
			}

			vowel = !vowel;
		}

		name = name.ToCharArray()[0].ToString().ToUpper() + name.Substring(1);

		return name;
	}

	private string GetRandomMessage()
	{
		switch (Random.Range(-1, 71))
		{
			case -1: return "Collect all 71!";
			case 0: return "Lost? Call 481-516-2342";
			case 1: return "Don't attack a straw man";
			case 2: return "Flying is easier with wings";
			case 3: return "They weren't dead the whole time";
			case 4: return "01100011 problems but a bit ain't one";
			case 5: return "One is outside, one is inside";
			case 6: return "Zoo driven rules system";
			case 7: return "Imported Exitrons";
			case 8: return "It's int max, don't you ever forget it";
			case 9: return "I'll get you my little pretty";
			case 10: return "And your little dog, too!";
			case 11: return "Pen attackers, beware!";
			case 12: return "Throw it out and start over";
			case 13: return "So big that I can't even see it";
			case 14: return "This is a warning";
			case 15: return "Who's gonna play with me this time?";
			case 16: return "Stole your kill, stole your life";
			case 17: return "Casually mentioning something";
			case 18: return "Is this the 4th wall?";
			case 19: return "Clean as a pickle";
			case 20: return "The lizard who never learned to love";
			case 21: return "Royal flush? Nah, too political";
			case 22: return "The cosmos of jinx land";
			case 23: return "Spaghetti in my hair";
			case 24: return "Always check expiration dates";
			case 25: return "The namer of Moon Base is unknown";
			case 26: return "Here at Senwan's gathering";
			case 27: return "Rearrange me 'till I'm sane";
			case 28: return "Sunlight through the blanket";
			case 29: return "Pass the torch, gotta get lit";
			case 30: return "This is positive reinforcement";
			case 31: return "We'll never find the tower";
			case 32: return "On a riverboat to Mars";
			case 33: return "Captions left at the alter";
			case 34: return "The 100 yard leap";
			case 35: return "There's gotta be something hidden in one of these, right?";
			case 36: return "Playing with sludge";
			case 37: return "Foreshadowing four shadows";
			case 38: return "Area to play made of synthetic materials";
			case 39: return "Case 39";
			case 40: return "Pop vs. Soda";
			case 41: return "The lord of the valley";
			case 42: return "Testing the limits of your neurons";
			case 43: return "Dealing chips, but not too salty";
			case 44: return "Genius captured in a bottle";
			case 45: return "A 12 pack of numbers";
			case 46: return "Sequential words perceived as a sentence";
			case 47: return "Haunted by ghost gorillas";
			case 48: return "Can't even with these odds";
			case 49: return "A charging bullfighter";
			case 50: return "sean coded this";
			case 51: return "Brainwashed into neutralism";
			case 52: return "This one fits the formula";
			case 53: return "Rock, paper, scissors to decide who's right";
			case 54: return "This wall agrees with you";
			case 55: return "Drinkin' food and smokin' water";
			case 56: return "Case 39 is a lie";
			case 57: return "It's time for clocks";
			case 58: return "Programmatic literature for kids";
			case 59: return "Approaching infinity with class";
			case 60: return "Manual auto repair";
			case 61: return "737 down over ABQ";
			case 62: return "Committed and wit it";
			case 63: return "Acting like it's my job";
			case 64: return "Stale words on a chalkboard";
			case 65: return "All hail the Deathbat";
			case 66: return "Religious salad in the cookie kingdom";
			case 67: return "Tacos without the crust";
			case 68: return "The girl from East Hampton";
			case 69: return "The sphinx cat has returned";
			case 70: return "Good morning, Worm your honor";
		}

		return "";
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