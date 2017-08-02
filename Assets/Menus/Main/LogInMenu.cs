using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LogInMenu : MonoBehaviour
{
	private UserManager userMgr;
	private NotificationPanel notificationPanel;
	private InputField userNameInputField;
	private InputField passwordInputField;
	private Button logInButton;
	private MainMenu multiplayerMenu;
	private CreateAccountMenu createAccountMenu;
	private Text messageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		userMgr = GetComponent<UserManager>();
		notificationPanel = GameObject.Find("NotificationCanvas").GetComponent<NotificationPanel>();
		userNameInputField = Utility.FindChild(gameObject, "UsernameInputField").GetComponent<InputField>();
		passwordInputField = Utility.FindChild(gameObject, "PasswordInputField").GetComponent<InputField>();
		logInButton = Utility.FindChild(gameObject, "LogInButton").GetComponent<Button>();
		multiplayerMenu = GameObject.Find("Menus").GetComponent<MainMenu>();
		createAccountMenu = GameObject.Find("CreateAccountPanel").GetComponent<CreateAccountMenu>();
		messageText = transform.Find("MessageText").GetComponent<Text>();

		if (GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>().LocalUser != null)
		{
			multiplayerMenu.GetComponent<Canvas>().enabled = true;
		}

		string fileName = Utility.GetSettingsDirectory() + "/RecentLogIn.txt";
		if (File.Exists(fileName))
		{
			StreamReader file = new StreamReader(fileName);
			userNameInputField.text = file.ReadLine();
			//passwordInputField.text = file.ReadLine();
			file.Close();
		}
	}

	public void Update()
	{
		logInButton.interactable = userNameInputField.text != "" && multiplayerMenu.State == MenuState.LogIn;

		if (userNameInputField.isFocused)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				passwordInputField.Select();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return) && logInButton.interactable)
		{
			OnLogInClick();
		}
	}

	/**********************************************************/
	// Interface

	public void OnLogInClick()
	{
		string fileName = Utility.GetSettingsDirectory();
		Directory.CreateDirectory(fileName);
		fileName += "/RecentLogIn.txt";

		StreamWriter file = new StreamWriter(fileName);
		file.WriteLine(userNameInputField.text);
		//file.WriteLine(passwordInputField.text);
		file.Close();

		messageText.text = "";
		multiplayerMenu.OnLogIn(userNameInputField.text, -1);
	}

	public void OnCreateAccountClick()
	{
		multiplayerMenu.TransitionToState(MenuState.CreateAccount);
	}

	public void OnLogInResponse(string response)
	{
		if (response.Length > 5)
		{
			messageText.text = response;
		}
		else
		{
			string fileName = Utility.GetSettingsDirectory();
			Directory.CreateDirectory(fileName);
			fileName += "/RecentLogIn.txt";

			StreamWriter file = new StreamWriter(fileName);
			file.WriteLine(userNameInputField.text);
			//file.WriteLine(passwordInputField.text);
			file.Close();

			messageText.text = "";
			multiplayerMenu.OnLogIn(userNameInputField.text, System.Convert.ToInt32(response));
		}
	}

	public void OnLogInError(string error)
	{
		messageText.text = "ERROR: " + error;
	}
}