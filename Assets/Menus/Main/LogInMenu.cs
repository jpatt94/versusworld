using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LogInMenu : MonoBehaviour
{
	private InputField userNameInputField;
	private InputField passwordInputField;
	private Button logInButton;
	private LegacyMainMenu multiplayerMenu;
	private Text messageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		userNameInputField = Utility.FindChild(gameObject, "UsernameInputField").GetComponent<InputField>();
		passwordInputField = Utility.FindChild(gameObject, "PasswordInputField").GetComponent<InputField>();
		logInButton = Utility.FindChild(gameObject, "LogInButton").GetComponent<Button>();
		multiplayerMenu = GameObject.Find("Menus").GetComponent<LegacyMainMenu>();
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
		//logInButton.interactable = userNameInputField.text != "" && multiplayerMenu.State == MenuType.LogIn;

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
		//multiplayerMenu.TransitionToState(MenuType.CreateAccount);
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