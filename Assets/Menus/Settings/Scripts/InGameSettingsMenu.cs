using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGameSettingsMenu : MonoBehaviour
{
	private InGameMenu inGameMenu;
	private Canvas canvas;
	private Text tabLabel;
	private ControlSettingsMenu controlSettings;
	private VideoSettingsMenu videoSettings;
	private AudioSettingsMenu audioSettings;

	void Awake()
	{
		inGameMenu = GameObject.Find("InGameMenu").GetComponent<InGameMenu>();
		canvas = GetComponent<Canvas>();
		controlSettings = GetComponentInChildren<ControlSettingsMenu>();
		videoSettings = GetComponentInChildren<VideoSettingsMenu>();
		audioSettings = GetComponentInChildren<AudioSettingsMenu>();
	}

	void Start()
	{
		tabLabel = Utility.FindChild(gameObject, "TabLabel").GetComponent<Text>();
	}

	/**********************************************************/
	// Interface

	public void Enable(bool enable)
	{
		canvas.enabled = enable;
	}

	public void OnControlsTabClick()
	{
		tabLabel.text = "Control Settings";
		controlSettings.Visible = true;
		videoSettings.Visible = false;
		audioSettings.Visible = false;
	}

	public void OnVideoTabClick()
	{
		tabLabel.text = "Video Settings";
		controlSettings.Visible = false;
		videoSettings.Visible = true;
		audioSettings.Visible = false;
	}

	public void OnAudioTabClick()
	{
		tabLabel.text = "Audio Settings";
		controlSettings.Visible = false;
		videoSettings.Visible = false;
		audioSettings.Visible = true;
	}

	public void OnApplyClick()
	{
		SaveSettings();
	}

	public void OnAcceptClick()
	{
		SaveSettings();
		inGameMenu.Enable(true);
		Enable(false);
	}

	public void OnCancelClick()
	{
		controlSettings.OnControlSettingsLoad();
		inGameMenu.Enable(true);
		Enable(false);
	}

	/**********************************************************/
	// Helper Functions

	private void SaveSettings()
	{
		controlSettings.Save();
		videoSettings.Save();
		audioSettings.Save();
	}
}
