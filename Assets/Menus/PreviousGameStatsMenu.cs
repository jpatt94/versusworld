using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousGameStatsMenu : StackedMenuState
{
	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		JP.Event.Register(this, "OnPreviousGameStatsBackButtonClick");
	}

	/**********************************************************/
	// Button Callbacks

	public void OnPreviousGameStatsBackButtonClick()
	{
		StateEnd();
	}
}
