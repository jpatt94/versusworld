using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	private MenuState[] menus;
	private MenuState currentMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		menus = new MenuState[(int)MenuType.NumTypes];
		foreach (MenuState state in GetComponentsInChildren<MenuState>())
		{
			menus[(int)state.GetMenuType()] = state;
		}
	}

	public void Start()
	{
		GoToMenu(MenuType.Main);
	}

	public void Update()
	{
		currentMenu.StateUpdate();
	}

	/**********************************************************/
	// Interface

	public void GoToMenu(MenuType menu)
	{
		MenuState prevMenu = currentMenu;
		currentMenu = menus[(int)menu];

		if (prevMenu)
		{
			prevMenu.StateEnd();
		}
		currentMenu.StateBegin();
	}
}
