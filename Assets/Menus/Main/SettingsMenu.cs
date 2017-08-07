using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
	private LegacyMainMenu menus;
	private VideoSettingsMenu videoMenu;
	private AudioSettingsMenu audioMenu;
	private ControlSettingsMenu controlsMenu;

	/**********************************************************/
	// MonoBehaviour Interface
	
	public void Awake()
	{
		menus = GetComponentInParent<LegacyMainMenu>();
		videoMenu = GetComponentInChildren<VideoSettingsMenu>();
		audioMenu = GetComponentInChildren<AudioSettingsMenu>();
		controlsMenu = GetComponentInChildren<ControlSettingsMenu>();
	}

	public void Start()
	{
		//videoMenu.Visible = true;
		//audioMenu.Visible = false;
		//controlsMenu.Visible = false;

		VideoSettings.Load();
		AudioSettings.Load();
		ControlSettings.Load();
	}

	/**********************************************************/
	// Interface

	public void OnVideoClick()
	{
		videoMenu.Visible = true;
		audioMenu.Visible = false;
		controlsMenu.Visible = false;
	}

	public void OnAudioClick()
	{
		videoMenu.Visible = false;
		audioMenu.Visible = true;
		controlsMenu.Visible = false;
	}

	public void OnControlsClick()
	{
		videoMenu.Visible = false;
		audioMenu.Visible = false;
		controlsMenu.Visible = true;
	}

	public void OnGoBackClick()
	{
		menus.TransitionToState(MenuType.Main);
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = value;
			}
			foreach (CanvasRenderer r in GetComponentsInChildren<CanvasRenderer>())
			{
				r.cull = !value;
			}
		}
	}
}