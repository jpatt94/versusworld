using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MenuState
{
	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
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
		mgr.GoToMenu(MenuType.LobbySettings);
	}

	private void OnJoinButtonClick()
	{

	}

	private void OnCustomizeButtonClick()
	{

	}

	private void OnSettingsButtonClick()
	{

	}

	private void OnExitButtonClick()
	{

	}
}
