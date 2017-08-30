using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MenuState
{
	private VideoSettingsMenu videoMenu;
	private AudioSettingsMenu audioMenu;
	private ControlSettingsMenu controlMenu;
	private RectTransform settingsWindowContent;
	private ScrollRect scrollRect;
	private InGameMenu inGameMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		videoMenu = GetComponentInChildren<VideoSettingsMenu>();
		audioMenu = GetComponentInChildren<AudioSettingsMenu>();
		controlMenu = GetComponentInChildren<ControlSettingsMenu>();
		settingsWindowContent = GameObject.Find("SettingsWindowContent").GetComponent<RectTransform>();
		scrollRect = GetComponentInChildren<ScrollRect>();
		inGameMenu = GetComponentInParent<InGameMenu>();

		base.Awake();

		JP.Event.Register(this, "OnVideoButtonClick");
		JP.Event.Register(this, "OnAudioButtonClick");
		JP.Event.Register(this, "OnControlsButtonClick");
		JP.Event.Register(this, "OnSettingsBackButtonClick");
	}

	/**********************************************************/
	// Interface

	public override MenuType GetMenuType()
	{
		return MenuType.Settings;
	}

	public override void StateBegin()
	{
		base.StateBegin();

		Vector2 size = settingsWindowContent.sizeDelta;
		size.y = videoMenu.GetComponent<RectTransform>().sizeDelta.y;
		settingsWindowContent.sizeDelta = size;

		audioMenu.Visible = false;
		controlMenu.Visible = false;

		scrollRect.verticalNormalizedPosition = 1.0f;
	}

	public override void StateEnd()
	{
		base.StateEnd();

		VideoSettings.Save();
		AudioSettings.Save();
		ControlSettings.Save();
	}

	/**********************************************************/
	// Button Callbacks

	private void OnVideoButtonClick()
	{
		Vector2 size = settingsWindowContent.sizeDelta;
		size.y = videoMenu.GetComponent<RectTransform>().sizeDelta.y;
		settingsWindowContent.sizeDelta = size;

		videoMenu.Visible = true;
		audioMenu.Visible = false;
		controlMenu.Visible = false;

		scrollRect.verticalNormalizedPosition = 1.0f;
	}

	private void OnAudioButtonClick()
	{
		Vector2 size = settingsWindowContent.sizeDelta;
		size.y = audioMenu.GetComponent<RectTransform>().sizeDelta.y;
		settingsWindowContent.sizeDelta = size;

		videoMenu.Visible = false;
		audioMenu.Visible = true;
		controlMenu.Visible = false;

		scrollRect.verticalNormalizedPosition = 1.0f;
	}

	private void OnControlsButtonClick()
	{
		Vector2 size = settingsWindowContent.sizeDelta;
		size.y = controlMenu.GetComponent<RectTransform>().sizeDelta.y;
		settingsWindowContent.sizeDelta = size;

		videoMenu.Visible = false;
		audioMenu.Visible = false;
		controlMenu.Visible = true;

		scrollRect.verticalNormalizedPosition = 1.0f;
	}

	private void OnSettingsBackButtonClick()
	{
		if (mgr)
		{
			mgr.GoToMenu(MenuType.Main);
		}
		else
		{
			StateEnd();

			inGameMenu.Visible = true;
			Visible = false;
		}
	}
}
