using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MenuState
{
	private InputField nameInputField;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		nameInputField = GameObject.Find("NameInputField").GetComponent<InputField>();

		string fileName = Utility.GetSettingsDirectory() + "/RecentLogIn.txt";
		if (File.Exists(fileName))
		{
			StreamReader file = new StreamReader(fileName);
			nameInputField.text = file.ReadLine();
			file.Close();
		}

		base.Awake();

		JP.Event.Register(this, "OnHostButtonClick");
		JP.Event.Register(this, "OnJoinButtonClick");
		JP.Event.Register(this, "OnCustomizeButtonClick");
		JP.Event.Register(this, "OnSettingsButtonClick");
		JP.Event.Register(this, "OnExitButtonClick");
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.Main;
	}

	/**********************************************************/
	// Button Callbacks

	private void OnHostButtonClick()
	{
		SaveRecentLogIn();
		mgr.GoToMenu(MenuType.LobbySettings);
	}

	private void OnJoinButtonClick()
	{
		SaveRecentLogIn();
		mgr.GoToMenu(MenuType.GameList);
	}

	private void OnCustomizeButtonClick()
	{
		mgr.ShowNotification("Customization is currently not available.");
		//mgr.GoToMenu(MenuType.Customize);
	}

	private void OnSettingsButtonClick()
	{
		mgr.GoToMenu(MenuType.Settings);
	}

	private void OnExitButtonClick()
	{
		Application.Quit();
	}

	/**********************************************************/
	// Helper Functions

	private void SaveRecentLogIn()
	{
		string fileName = Utility.GetSettingsDirectory();
		Directory.CreateDirectory(fileName);
		fileName += "/RecentLogIn.txt";

		StreamWriter file = new StreamWriter(fileName);
		file.WriteLine(nameInputField.text);
		file.Close();

		multiplayer.OnLogIn(nameInputField.text, -1);
	}
}
