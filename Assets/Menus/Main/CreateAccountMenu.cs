using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateAccountMenu : MonoBehaviour
{
	private UserManager userMgr;
	private NotificationPanel notificationPanel;
	private InputField emailInputField;
	private InputField userNameInputField;
	private InputField passwordInputField;
	private Button createAccountButton;
	private LegacyMainMenu multiplayerMenu;
	private LogInMenu logInMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		userMgr = GetComponent<UserManager>();
		notificationPanel = GameObject.Find("NotificationCanvas").GetComponent<NotificationPanel>();
		emailInputField = Utility.FindChild(gameObject, "EmailInputField").GetComponent<InputField>();
		userNameInputField = Utility.FindChild(gameObject, "UsernameInputField").GetComponent<InputField>();
		passwordInputField = Utility.FindChild(gameObject, "PasswordInputField").GetComponent<InputField>();
		createAccountButton = Utility.FindChild(gameObject, "CreateAccountButton").GetComponent<Button>();
		multiplayerMenu = GameObject.Find("Menus").GetComponent<LegacyMainMenu>();
	}

	public void Update()
	{
		createAccountButton.interactable = userNameInputField.text != "" && passwordInputField.text != "" && emailInputField.text != "";
	}

	/**********************************************************/
	// Interface

	public void OnCreateAccountClick()
	{
		notificationPanel.Show("Creating account...", false);
		userMgr.CreateUser(userNameInputField.text, passwordInputField.text, emailInputField.text);
	}

	public void OnGoBackClick()
	{
		//multiplayerMenu.TransitionToState(MenuType.LogIn);
	}

	public void OnCreateAccountResponse(string response)
	{
		if (response.Length > 5)
		{
			notificationPanel.Show("ERROR: " + response, true);
		}
		else
		{
			notificationPanel.Show("Account created successfully", true);
			multiplayerMenu.OnLogIn(userNameInputField.text, System.Convert.ToInt32(response));
			notificationPanel.Hide();

			multiplayerMenu.TransitionToState(MenuType.Main);
		}
	}

	public void OnCreateAccountError(string error)
	{
		notificationPanel.Show("ERROR: " + error, true);
	}
}