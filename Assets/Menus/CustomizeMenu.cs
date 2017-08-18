using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeMenu : MenuState
{
	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		JP.Event.Register(this, "OnCustomizeBackButtonClick");
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.Customize;
	}

	/**********************************************************/
	// Button Callbacks

	private void OnCustomizeBackButtonClick()
	{
		mgr.GoToMenu(MenuType.Main);
	}
}
