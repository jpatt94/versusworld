using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySettingsMenu : MenuState
{
	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		JP.Event.Register(this, "OnCreateGameButtonClick");
		JP.Event.Register(this, "OnLobbySettingsBackButtonClick");
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.LobbySettings;
	}

	/**********************************************************/
	// Button Callbacks

	private void OnCreateGameButtonClick()
	{
		multiplayer.CreateMatch();
	}

	private void OnLobbySettingsBackButtonClick()
	{
		mgr.GoToMenu(MenuType.Main);
	}
}
